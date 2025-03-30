using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace DistributedCacheDemo.Services;

public class LockService
{
    private static readonly Random _random = new();
    /// <summary>
    /// Metodo per ottenere o creare un valore in cache con lock distribuito.
    /// Utilizza un approccio iterativo con backoff esponenziale e limite di tentativi.
    /// </summary>
    public async Task<T?> GetOrCreateWithDistributedLockAsync<T>(
        IDistributedCache cache,
        IConnectionMultiplexer redis,
        string cacheKey,
        Func<Task<T?>> dataFactory,
        int maxRetryAttempts = 5,
        int lockTimeoutSeconds = 10,
        DistributedCacheEntryOptions? options = null) where T : class
    {
        // Controllo iniziale della cache prima di tentare il lock
        var cachedValue = await cache.GetAsync(cacheKey);
        if (cachedValue != null)
        {
            return JsonSerializer.Deserialize<T>(cachedValue);
        }

        string lockKey = $"lock:{cacheKey}";
        int attempt = 0;
        int baseDelay = 20; // ms

        while (attempt < maxRetryAttempts)
        {
            attempt++;
            string lockValue = Guid.NewGuid().ToString();
            var db = redis.GetDatabase();

            try
            {
                // Tenta di acquisire il lock
                bool lockAcquired = await db.StringSetAsync(
                    lockKey,
                    lockValue,
                    TimeSpan.FromSeconds(lockTimeoutSeconds),
                    When.NotExists
                );

                if (lockAcquired)
                {
                    try
                    {
                        // Double-check della cache dopo aver acquisito il lock
                        cachedValue = await cache.GetAsync(cacheKey);
                        if (cachedValue != null)
                        {
                            return JsonSerializer.Deserialize<T>(cachedValue);
                        }

                        // Recupera il dato dalla fonte originale
                        var result = await dataFactory();
                        if (result is null)
                            return null;

                        // Salva in cache
                        var serialized = JsonSerializer.SerializeToUtf8Bytes(result);

                        // Usa opzioni personalizzate o predefinite
                        options ??= new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                            SlidingExpiration = TimeSpan.FromMinutes(5)
                        };

                        await cache.SetAsync(cacheKey, serialized, options);
                        return result;
                    }
                    finally
                    {
                        // Rilascia il lock in modo sicuro
                        await ReleaseLockAsync(db, lockKey, lockValue);
                    }
                }

                // Lock non acquisito, calcola il ritardo con backoff esponenziale e jitter
                // Calcolo con fattore di crescita contenuto e cap massimo
                int exponentialDelay = baseDelay * (int)Math.Pow(1.8, attempt - 1);
                int cappedDelay = Math.Min(exponentialDelay, 200); // Cap a 200ms
                int jitter = _random.Next(cappedDelay / 2); // Jitter proporzionale
                int totalDelay = cappedDelay + jitter;

                Console.WriteLine($"Lock non acquisito per {cacheKey}, tentativo {attempt}/{maxRetryAttempts}, attendo {totalDelay}ms");
                await Task.Delay(totalDelay);

                // Prima di riprovare, controlla di nuovo la cache
                cachedValue = await cache.GetAsync(cacheKey);
                if (cachedValue != null)
                {
                    return JsonSerializer.Deserialize<T>(cachedValue);
                }
            }
            catch (RedisConnectionException ex)
            {
                // Gestisci esplicitamente i problemi di connessione a Redis
                Console.WriteLine($"Errore di connessione Redis al tentativo {attempt}: {ex.Message}");

                if (attempt >= maxRetryAttempts)
                {
                    // Dopo tutti i tentativi, recupera i dati direttamente
                    Console.WriteLine("Numero massimo di tentativi raggiunto, recupero diretto dalla fonte originale");
                    return await dataFactory();
                }

                await Task.Delay(baseDelay * (int)Math.Pow(2, attempt));
            }
            catch (Exception ex)
            {
                // Gestisci altre eccezioni
                Console.WriteLine($"Errore inaspettato al tentativo {attempt}: {ex.Message}");
                throw;
            }
        }

        // Se raggiungiamo questo punto, tutti i tentativi sono falliti
        Console.WriteLine($"Impossibile acquisire il lock dopo {maxRetryAttempts} tentativi, recupero diretto");
        return await dataFactory();
    }

    /// <summary>
    /// Implementazione più robusta che combina lock distribuito e negative caching.
    /// Utilizza un approccio iterativo con backoff esponenziale e limite di tentativi.
    /// </summary>
    public async Task<T?> GetOrCreateWithDistributedLockAndNegativeCachingAsync<T>(
        IDistributedCache cache,
        IConnectionMultiplexer redis,
        string cacheKey,
        Func<Task<T?>> dataFactory,
        int maxRetryAttempts = 5,
        int lockTimeoutSeconds = 10,
        DistributedCacheEntryOptions? positiveOptions = null,
        DistributedCacheEntryOptions? negativeOptions = null) where T : class
    {
        // Controllo iniziale della cache (prima del lock)
        var cachedData = await CheckCacheAsync<T>(cache, cacheKey);
        if (cachedData.found)
        {
            return cachedData.result;
        }

        string lockKey = $"lock:{cacheKey}";

        // Approccio iterativo invece di ricorsivo
        int attempt = 0;
        int baseDelay = 20; // ms

        while (attempt < maxRetryAttempts)
        {
            attempt++;

            string lockValue = Guid.NewGuid().ToString();
            var db = redis.GetDatabase();

            try
            {
                // Tenta di acquisire il lock
                bool lockAcquired = await db.StringSetAsync(
                    lockKey,
                    lockValue,
                    TimeSpan.FromSeconds(lockTimeoutSeconds),
                    When.NotExists
                );

                if (lockAcquired)
                {
                    try
                    {
                        // Double-check della cache
                        var doubleCheckData = await CheckCacheAsync<T>(cache, cacheKey);
                        if (doubleCheckData.found)
                        {
                            return doubleCheckData.result;
                        }

                        // Recupera i dati e aggiorna la cache
                        return await UpdateCacheWithDataAsync<T>(
                            cache, cacheKey, dataFactory, positiveOptions, negativeOptions);
                    }
                    finally
                    {
                        // Rilascia il lock in modo sicuro
                        await ReleaseLockAsync(db, lockKey, lockValue);
                    }
                }

                // Lock non acquisito, calcola il ritardo con backoff esponenziale e jitter
                // Calcolo con fattore di crescita contenuto e cap massimo
                int exponentialDelay = baseDelay * (int)Math.Pow(1.8, attempt - 1);
                int cappedDelay = Math.Min(exponentialDelay, 200); // Cap a 200ms
                int jitter = _random.Next(cappedDelay / 2); // Jitter proporzionale
                int totalDelay = cappedDelay + jitter;

                Console.WriteLine($"Lock non acquisito per {cacheKey}, tentativo {attempt}/{maxRetryAttempts}, attendo {totalDelay}ms");

                await Task.Delay(totalDelay);

                // Prima di riprovare, controlla di nuovo la cache
                // (qualcun altro potrebbe aver già completato l'operazione)
                var retryCheckData = await CheckCacheAsync<T>(cache, cacheKey);
                if (retryCheckData.found)
                {
                    return retryCheckData.result;
                }
            }
            catch (RedisConnectionException ex)
            {
                // Gestisci esplicitamente i problemi di connessione a Redis
                Console.WriteLine($"Errore di connessione Redis al tentativo {attempt}: {ex.Message}");

                if (attempt >= maxRetryAttempts)
                {
                    // Dopo tutti i tentativi, prova a recuperare i dati dalla fonte originale senza cache
                    Console.WriteLine("Numero massimo di tentativi raggiunto, recupero diretto dalla fonte originale");
                    return await dataFactory();
                }

                await Task.Delay(baseDelay * (int)Math.Pow(2, attempt)); // Backoff semplice prima di riprovare
            }
            catch (Exception ex)
            {
                // Gestisci altre eccezioni
                Console.WriteLine($"Errore inaspettato al tentativo {attempt}: {ex.Message}");
                throw; // Rilancia l'eccezione per altri tipi di errori
            }
        }

        // Se raggiungiamo questo punto, tutti i tentativi sono falliti
        Console.WriteLine($"Impossibile acquisire il lock dopo {maxRetryAttempts} tentativi, recupero diretto");
        return await dataFactory();
    }

    // Metodi di supporto
    private static async Task<(bool found, T? result)> CheckCacheAsync<T>(
        IDistributedCache cache, string cacheKey) where T : class
    {
        byte[]? cachedData = await cache.GetAsync(cacheKey);

        if (cachedData == null || cachedData.Length == 0)
        {
            return (false, null);
        }

        bool isNegativeResult = cachedData.Length == 1 && cachedData[0] == 1;
        if (isNegativeResult)
        {
            return (true, null);
        }

        if (cachedData.Length > 1)
        {
            byte[] actualData = new byte[cachedData.Length - 1];
            Array.Copy(cachedData, 1, actualData, 0, actualData.Length);
            var result = JsonSerializer.Deserialize<T>(actualData);
            return (true, result);
        }

        return (false, null);
    }

    private static async Task<T?> UpdateCacheWithDataAsync<T>(
        IDistributedCache cache,
        string cacheKey,
        Func<Task<T?>> dataFactory,
        DistributedCacheEntryOptions? positiveOptions,
        DistributedCacheEntryOptions? negativeOptions) where T : class
    {
        T? result = await dataFactory();

        positiveOptions ??= new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            SlidingExpiration = TimeSpan.FromMinutes(5)
        };

        negativeOptions ??= new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
        };

        if (result != null)
        {
            byte[] serialized = JsonSerializer.SerializeToUtf8Bytes(result);
            byte[] withFlag = new byte[serialized.Length + 1];
            withFlag[0] = 0;
            Array.Copy(serialized, 0, withFlag, 1, serialized.Length);

            await cache.SetAsync(cacheKey, withFlag, positiveOptions);
            Console.WriteLine($"Risultato positivo per la chiave {cacheKey} salvato in cache");
        }
        else
        {
            await cache.SetAsync(cacheKey, new byte[] { 1 }, negativeOptions);
            Console.WriteLine($"Risultato negativo per la chiave {cacheKey} salvato in cache con TTL breve");
        }

        return result;
    }

    private static async Task ReleaseLockAsync(IDatabase db, string lockKey, string lockValue)
    {
        await db.ScriptEvaluateAsync(@"
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('del', KEYS[1])
        else
            return 0
        end",
            [lockKey],
            [lockValue]
        );
        Console.WriteLine($"Lock per {lockKey} rilasciato");
    }
}

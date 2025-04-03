using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace HybridCacheDemo.Services;

/// <summary>
/// Servizio che implementa pattern di locking distribuito per cache ibrida
/// Utile per evitare il problema della "Thunder Herd" (molte richieste concorrenti 
/// per un elemento non presente in cache)
/// </summary>
public class CacheLockService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<CacheLockService> _logger;
    private static readonly Random _random = new();

    public CacheLockService(IConnectionMultiplexer redis, ILogger<CacheLockService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    /// <summary>
    /// Recupera un valore dalla cache o lo crea con un lock distribuito se non esiste
    /// </summary>
    /// <typeparam name="T">Tipo dell'oggetto da recuperare</typeparam>
    /// <param name="distributedCache">Cache distribuita</param>
    /// <param name="memoryCache">Cache in memoria</param>
    /// <param name="cacheKey">Chiave della cache</param>
    /// <param name="factory">Factory function da eseguire se il valore non è in cache</param>
    /// <param name="distributedCacheTTL">Durata nella cache distribuita</param>
    /// <param name="memoryCacheTTL">Durata nella cache in memoria</param>
    /// <param name="maxRetryAttempts">Numero massimo di tentativi per acquisire il lock</param>
    /// <param name="lockTimeoutSeconds">Timeout per il lock in secondi</param>
    /// <returns>Il valore memorizzato o creato</returns>
    public async Task<T?> GetOrSetWithLockAsync<T>(
        IDistributedCache distributedCache,
        IMemoryCache memoryCache,
        string cacheKey,
        Func<Task<T?>> factory,
        TimeSpan distributedCacheTTL,
        TimeSpan memoryCacheTTL,
        int maxRetryAttempts = 5,
        int lockTimeoutSeconds = 10) where T : class
    {
        // Prima verifica: controllo diretto nella memoria cache
        if (memoryCache.TryGetValue(cacheKey, out T? cachedValue) && cachedValue != null)
        {
            _logger.LogDebug("Valore trovato in memoria cache per chiave {CacheKey}", cacheKey);
            return cachedValue;
        }

        // Seconda verifica: controllo nella cache distribuita
        byte[]? cachedData = await distributedCache.GetAsync(cacheKey);
        if (cachedData != null && cachedData.Length > 0)
        {
            try
            {
                cachedValue = JsonSerializer.Deserialize<T>(cachedData);
                if (cachedValue != null)
                {
                    // Memorizza anche in memoria per accessi futuri più veloci
                    var memoryCacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(memoryCacheTTL);
                    memoryCache.Set(cacheKey, cachedValue, memoryCacheOptions);

                    _logger.LogDebug("Valore trovato in cache distribuita per chiave {CacheKey}", cacheKey);
                    return cachedValue;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Errore nella deserializzazione dalla cache distribuita per chiave {CacheKey}", cacheKey);
                // Se la deserializzazione fallisce, procedi con il recupero
            }
        }

        string lockKey = $"lock:{cacheKey}";
        int attempt = 0;
        int baseDelay = 20; // ms

        while (attempt < maxRetryAttempts)
        {
            attempt++;
            string lockValue = Guid.NewGuid().ToString();
            var db = _redis.GetDatabase();

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
                        // Un'altra istanza potrebbe aver già popolato la cache

                        // Controlla prima la memoria
                        if (memoryCache.TryGetValue(cacheKey, out cachedValue) && cachedValue != null)
                        {
                            _logger.LogDebug("Valore trovato in memoria cache durante double-check per chiave {CacheKey}", cacheKey);
                            return cachedValue;
                        }

                        // Controlla poi la cache distribuita
                        cachedData = await distributedCache.GetAsync(cacheKey);
                        if (cachedData != null && cachedData.Length > 0)
                        {
                            try
                            {
                                cachedValue = JsonSerializer.Deserialize<T>(cachedData);
                                if (cachedValue != null)
                                {
                                    // Memorizza anche in memoria per accessi futuri più veloci
                                    memoryCache.Set(cacheKey, cachedValue, memoryCacheTTL);

                                    _logger.LogDebug("Valore trovato in cache distribuita durante double-check per chiave {CacheKey}", cacheKey);
                                    return cachedValue;
                                }
                            }
                            catch (JsonException)
                            {
                                // Se la deserializzazione fallisce, procedi con il recupero
                            }
                        }

                        // Recupera il dato dalla fonte originale
                        var result = await factory();

                        // Salva in cache se non è null
                        if (result != null)
                        {
                            // Salva nella cache distribuita
                            var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(distributedCacheTTL);
                            await distributedCache.SetAsync(
                                cacheKey,
                                JsonSerializer.SerializeToUtf8Bytes(result),
                                options);

                            // Salva anche in memoria
                            memoryCache.Set(cacheKey, result, memoryCacheTTL);

                            _logger.LogDebug("Valore recuperato e salvato in cache per chiave {CacheKey}", cacheKey);
                        }
                        else
                        {
                            // Per valori null, memorizziamo un marker speciale
                            string nullMarkerKey = $"{cacheKey}:null-marker";

                            // Memorizza il null marker nella cache distribuita
                            var options = new DistributedCacheEntryOptions()
                                .SetAbsoluteExpiration(TimeSpan.FromMinutes(1)); // TTL più breve per i null

                            await distributedCache.SetAsync(
                                nullMarkerKey,
                                JsonSerializer.SerializeToUtf8Bytes(true),
                                options);

                            // Memorizza anche in memoria
                            memoryCache.Set(nullMarkerKey, true, TimeSpan.FromMinutes(1));

                            _logger.LogDebug("Valore null memorizzato come marker in cache per chiave {CacheKey}", cacheKey);
                        }

                        return result;
                    }
                    finally
                    {
                        // Rilascia il lock in modo sicuro usando Lua script
                        await ReleaseLockAsync(db, lockKey, lockValue);
                    }
                }

                // Lock non acquisito, calcola il ritardo con backoff esponenziale e jitter
                int exponentialDelay = baseDelay * (int)Math.Pow(1.8, attempt - 1);
                int cappedDelay = Math.Min(exponentialDelay, 200); // Cap a 200ms
                int jitter = _random.Next(cappedDelay / 2); // Jitter proporzionale
                int totalDelay = cappedDelay + jitter;

                _logger.LogDebug("Lock non acquisito per {CacheKey}, tentativo {Attempt}/{MaxAttempts}, attendo {Delay}ms",
                    cacheKey, attempt, maxRetryAttempts, totalDelay);

                await Task.Delay(totalDelay);

                // Prima di riprovare, controlla di nuovo la memoria cache
                if (memoryCache.TryGetValue(cacheKey, out cachedValue) && cachedValue != null)
                {
                    _logger.LogDebug("Valore trovato in memoria cache dopo attesa per chiave {CacheKey}", cacheKey);
                    return cachedValue;
                }

                // Controlla anche la cache distribuita
                cachedData = await distributedCache.GetAsync(cacheKey);
                if (cachedData != null && cachedData.Length > 0)
                {
                    try
                    {
                        cachedValue = JsonSerializer.Deserialize<T>(cachedData);
                        if (cachedValue != null)
                        {
                            memoryCache.Set(cacheKey, cachedValue, memoryCacheTTL);
                            _logger.LogDebug("Valore trovato in cache distribuita dopo attesa per chiave {CacheKey}", cacheKey);
                            return cachedValue;
                        }
                    }
                    catch (JsonException)
                    {
                        // Se la deserializzazione fallisce, procedi con il prossimo tentativo
                    }
                }
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogWarning(ex, "Errore di connessione Redis al tentativo {Attempt} per chiave {CacheKey}", attempt, cacheKey);

                if (attempt >= maxRetryAttempts)
                {
                    // Dopo tutti i tentativi, recupera direttamente
                    _logger.LogWarning("Numero massimo di tentativi raggiunto per chiave {CacheKey}, recupero diretto", cacheKey);
                    return await factory();
                }

                // Applica backoff prima di riprovare
                await Task.Delay(baseDelay * (int)Math.Pow(2, attempt));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore inaspettato al tentativo {Attempt} per chiave {CacheKey}", attempt, cacheKey);
                throw;
            }
        }

        // Se raggiungiamo questo punto, tutti i tentativi di lock sono falliti
        _logger.LogWarning("Impossibile acquisire il lock dopo {MaxAttempts} tentativi per chiave {CacheKey}, recupero diretto",
            maxRetryAttempts, cacheKey);

        return await factory();
    }

    /// <summary>
    /// Rilascia un lock distribuito in modo sicuro usando Lua script
    /// </summary>
    private static async Task ReleaseLockAsync(IDatabase db, string lockKey, string lockValue)
    {
        // Lo script Lua garantisce l'atomicità dell'operazione
        // Controlla che il lock sia ancora nostro prima di eliminarlo
        await db.ScriptEvaluateAsync(@"
            if redis.call('get', KEYS[1]) == ARGV[1] then
                return redis.call('del', KEYS[1])
            else
                return 0
            end",
            new RedisKey[] { lockKey },
            new RedisValue[] { lockValue }
        );
    }
}

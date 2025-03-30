using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace DistributedCacheDemo;

public static class DistributedCacheExtensions
{
    public static async Task<T?> GetOrCreateAsync<T>(
        this IDistributedCache cache,
        string key,
        Func<Task<T>> factory,
        DistributedCacheEntryOptions? options = null)
    {
        // Tenta di recuperare l'elemento dalla cache
        byte[]? cachedData = await cache.GetAsync(key);

        if (cachedData != null)
        {
            // Elemento trovato in cache, deserializza
            return JsonSerializer.Deserialize<T>(cachedData);
        }

        // Elemento non trovato, crealo
        T item = await factory();

        // Se l'elemento non è il valore predefinito del tipo, serializza e salvalo in cache
        if (!EqualityComparer<T>.Default.Equals(item, default))
        {
            byte[] serializedData = JsonSerializer.SerializeToUtf8Bytes(item);

            // Usa opzioni predefinite se non specificate
            options ??= new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };

            await cache.SetAsync(key, serializedData, options);
        }

        return item;
    }

    public static async Task<(bool Found, T? Value)> TryGetValueAsync<T>(
        this IDistributedCache cache,
        string key)
    {
        byte[]? cachedData = await cache.GetAsync(key);

        if (cachedData == null)
        {
            return (false, default);
        }

        T? value = JsonSerializer.Deserialize<T>(cachedData);
        return (true, value);
    }
    
    

    /// <summary>
    /// Implementa il pattern "Staggered Renewal" per la cache distribuita.
    /// Restituisce i dati dalla cache se disponibili, altrimenti esegue una factory per recuperarli.
    /// Se i dati sono vicini alla scadenza, avvia un rinnovo asincrono in background.
    /// I dati freschi vengono memorizzati in cache con una scadenza più lunga.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cache"></param>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <param name="absoluteExpiration"></param>
    /// <param name="slidingExpiration"></param>
    /// <param name="renewalPercentage"></param>
    /// <returns></returns>
    public static async Task<T?> GetOrCreateWithStaggeredRenewalAsync<T>(
        this IDistributedCache cache,
        string key,
        Func<Task<T?>> factory,
        TimeSpan absoluteExpiration,
        TimeSpan slidingExpiration,
        double renewalPercentage = 0.75) where T : class
    {
        byte[]? cachedData = await cache.GetAsync(key);

        if (cachedData != null)
        {
            // Verifica se ci sono abbastanza dati per contenere il flag di metadati
            if (cachedData.Length > 0)
            {
                // Estrai il flag di metadati
                byte metadataFlag = cachedData[0];

                // Estrai i dati effettivi (senza il flag)
                byte[] actualData = new byte[cachedData.Length - 1];
                Array.Copy(cachedData, 1, actualData, 0, actualData.Length);

                // Deserializza i dati effettivi
                T? result = JsonSerializer.Deserialize<T>(actualData);

                // Se siamo vicini alla scadenza (flag = 1), c'è una probabilità che questa richiesta rinnovi i dati
                if (metadataFlag == 1 && Random.Shared.NextDouble() < 0.1) // 10% di probabilità
                {
                    Console.WriteLine($"Chiave {key}: Rilevato flag di prossima scadenza, avvio rinnovo asincrono");

                    // Avvia il rinnovo asincrono (non attendiamo il completamento)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            Console.WriteLine($"Rinnovo asincrono iniziato per la chiave {key}");

                            // Recupera nuovi dati
                            T? newData = await factory();
                            if (newData == null) return;

                            // Serializza i nuovi dati
                            byte[] serialized = JsonSerializer.SerializeToUtf8Bytes(newData);

                            // Aggiungi il flag di "freschezza" (0)
                            byte[] withMetadata = new byte[serialized.Length + 1];
                            withMetadata[0] = 0; // Non vicino alla scadenza
                            Array.Copy(serialized, 0, withMetadata, 1, serialized.Length);

                            // Salva in cache con nuova scadenza
                            await cache.SetAsync(key, withMetadata, new DistributedCacheEntryOptions
                            {
                                AbsoluteExpirationRelativeToNow = absoluteExpiration,
                                SlidingExpiration = slidingExpiration
                            });

                            Console.WriteLine($"Rinnovo asincrono completato per la chiave {key}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Errore durante il rinnovo asincrono: {ex.Message}");
                        }
                    });
                }

                return result;
            }

            // Se non c'è il flag di metadati (caso improbabile), deserializza direttamente
            return JsonSerializer.Deserialize<T>(cachedData);
        }

        // Cache miss - recupera i dati normalmente
        Console.WriteLine($"Cache miss per la chiave {key}, recupero dati dalla fonte originale");
        T? freshData = await factory();
        if (freshData == null) return null;

        // Calcola quando impostare il flag di "vicino alla scadenza"
        var renewalTime = TimeSpan.FromTicks((long)(absoluteExpiration.Ticks * renewalPercentage));

        // Prepara dati con metadati
        byte[] serializedData = JsonSerializer.SerializeToUtf8Bytes(freshData);
        byte[] dataWithMetadata = new byte[serializedData.Length + 1];
        dataWithMetadata[0] = 0; // Flag: non vicino alla scadenza
        Array.Copy(serializedData, 0, dataWithMetadata, 1, serializedData.Length);

        // Salva in cache
        await cache.SetAsync(key, dataWithMetadata, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = absoluteExpiration,
            SlidingExpiration = slidingExpiration
        });

        Console.WriteLine($"Dati salvati in cache per la chiave {key} con scadenza tra {absoluteExpiration}");

        // Pianifica un'operazione che imposterà il flag "vicino alla scadenza"
        _ = Task.Delay(renewalTime).ContinueWith(async _ =>
        {
            try
            {
                var existingData = await cache.GetAsync(key);
                if (existingData == null || existingData.Length <= 1) return;

                // Modifica solo il flag di metadati
                existingData[0] = 1; // Ora è vicino alla scadenza

                // Aggiorna la cache senza modificare la scadenza (mantiene la stessa scadenza)
                await cache.SetAsync(key, existingData,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromTicks(absoluteExpiration.Ticks - renewalTime.Ticks),
                        SlidingExpiration = slidingExpiration
                    });

                Console.WriteLine($"Flag 'vicino alla scadenza' impostato per la chiave {key}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore nell'aggiornamento del flag: {ex.Message}");
            }
        });

        return freshData;
    }

    /// <summary>
    /// Implementa il pattern "Stale-While-Revalidate" per la cache distribuita.
    /// Restituisce i dati dalla cache se disponibili, altrimenti esegue una factory per recuperarli.
    /// Se i dati sono obsoleti, restituisce comunque i dati obsoleti e avvia un rinnovo in background.
    /// I dati freschi vengono memorizzati in cache con una scadenza più lunga.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cache"></param>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <param name="freshDuration"></param>
    /// <param name="staleDuration"></param>
    /// <returns></returns>
    public static async Task<T?> GetOrCreateStaleWhileRevalidateAsync<T>(
        this IDistributedCache cache,
        string key,
        Func<Task<T?>> factory,
        TimeSpan freshDuration,
        TimeSpan staleDuration) where T : class
    {
        byte[]? cachedData = await cache.GetAsync(key);
        CachedItem<T>? cachedItem = null;
        bool needsRevalidation = false;

        // Elabora i dati dalla cache se esistono
        if (cachedData != null)
        {
            try
            {
                cachedItem = JsonSerializer.Deserialize<CachedItem<T>>(cachedData);

                if (cachedItem != null)
                {
                    // Controlla se i dati sono ancora "freschi"
                    var age = DateTime.UtcNow - cachedItem.CreatedAt;

                    // I dati sono obsoleti (stale) ma ancora utilizzabili?
                    if (age > freshDuration && age <= freshDuration + staleDuration)
                    {
                        Console.WriteLine($"Dati obsoleti per {key}, restituiscili mentre aggiorniamo");
                        needsRevalidation = true;
                    }
                    // I dati sono completamente scaduti?
                    else if (age > freshDuration + staleDuration)
                    {
                        Console.WriteLine($"Dati troppo obsoleti per {key}, è necessario un nuovo caricamento");
                        cachedItem = null; // Tratta come cache miss
                    }
                    else
                    {
                        Console.WriteLine($"Dati freschi per {key}");
                    }
                }
            }
            catch
            {
                cachedItem = null; // In caso di errore nella deserializzazione
            }
        }

        // Cache miss o dati completamente scaduti
        if (cachedItem == null)
        {
            var freshData = await factory();

            if (freshData != null)
            {
                // Crea un nuovo item con timestamp
                var newCachedItem = new CachedItem<T>
                {
                    Data = freshData,
                    CreatedAt = DateTime.UtcNow
                };

                // Salva in cache
                var serialized = JsonSerializer.SerializeToUtf8Bytes(newCachedItem);
                await cache.SetAsync(key, serialized, new DistributedCacheEntryOptions
                {
                    // Usa una scadenza molto più lunga della staleDuration
                    AbsoluteExpirationRelativeToNow = freshDuration + staleDuration + TimeSpan.FromHours(1)
                });

                Console.WriteLine($"Nuovi dati caricati e salvati in cache per {key}");
                return freshData;
            }

            return null;
        }

        // Avvia la rivalidazione in background se necessario
        if (needsRevalidation)
        {
            // Non attendiamo il completamento (fire and forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    Console.WriteLine($"Avvio rivalidazione in background per {key}");
                    var freshData = await factory();

                    if (freshData != null)
                    {
                        // Crea un nuovo item con timestamp
                        var newCachedItem = new CachedItem<T>
                        {
                            Data = freshData,
                            CreatedAt = DateTime.UtcNow
                        };

                        // Salva in cache
                        var serialized = JsonSerializer.SerializeToUtf8Bytes(newCachedItem);
                        await cache.SetAsync(key, serialized, new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = freshDuration + staleDuration + TimeSpan.FromHours(1)
                        });

                        Console.WriteLine($"Rivalidazione in background completata con successo per {key}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Errore durante la rivalidazione in background: {ex.Message}");
                }
            });
        }

        // Restituisci i dati dalla cache (anche se obsoleti)
        return cachedItem.Data;
    }

    /// <summary>
    /// Implementa il pattern di caching con invalidazione per i risultati negativi.
    /// Memorizza i risultati negativi (null) in cache con un TTL breve per evitare richieste ripetute.
    /// Se il risultato è positivo, lo memorizza normalmente.
    /// Se il risultato è negativo, lo memorizza con un TTL breve.
    /// In questo modo, le richieste successive per lo stesso dato non richiederanno un accesso alla fonte originale per un breve periodo.
    /// Questo è utile per evitare di sovraccaricare la fonte originale con richieste ripetute per dati che non esistono.
    /// Il TTL breve per i risultati negativi consente di rimuovere rapidamente i dati obsoleti dalla cache.
    /// Se il risultato è positivo, lo memorizza normalmente.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cache"></param>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <param name="options"></param>
    /// <param name="negativeOptions"></param>
    /// <returns></returns>
    public static async Task<T?> GetOrCreateWithNegativeCachingAsync<T>(
        this IDistributedCache cache,
        string key,
        Func<Task<T?>> factory,
        DistributedCacheEntryOptions? options = null,
        DistributedCacheEntryOptions? negativeOptions = null) where T : class
    {
        // Tenta di recuperare l'elemento dalla cache
        byte[]? cachedData = await cache.GetAsync(key);

        // Verifica se è un valore normale o un risultato negativo
        if (cachedData != null)
        {
            // Se il primo byte è 0, è un risultato normale
            // Se è 1, è un risultato negativo (null/not found)
            bool isNegativeResult = cachedData.Length == 1 && cachedData[0] == 1;

            if (isNegativeResult)
            {
                Console.WriteLine($"Risultato negativo per la chiave {key} trovato in cache");
                return null; // Restituisci null per il risultato negativo memorizzato
            }

            // Altrimenti è un risultato normale, rimuovi il flag e deserializza
            byte[] actualData = new byte[cachedData.Length - 1];
            Array.Copy(cachedData, 1, actualData, 0, actualData.Length);

            return JsonSerializer.Deserialize<T>(actualData);
        }

        // Elemento non trovato in cache, bisogna recuperalo
        T? item = await factory();

        // Opzioni predefinite se non specificate
        options ??= new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            SlidingExpiration = TimeSpan.FromMinutes(5)
        };

        // Opzioni per i risultati negativi (TTL molto breve)
        negativeOptions ??= new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1), // TTL molto breve
        };

        if (item != null)
        {
            // Serializza con flag = 0 (risultato normale)
            byte[] serialized = JsonSerializer.SerializeToUtf8Bytes(item);
            byte[] withFlag = new byte[serialized.Length + 1];
            withFlag[0] = 0; // Risultato normale
            Array.Copy(serialized, 0, withFlag, 1, serialized.Length);

            await cache.SetAsync(key, withFlag, options);
            Console.WriteLine($"Risultato positivo per la chiave {key} salvato in cache");
            return item;
        }
        else
        {
            // Risultato negativo - lo memorizziamo come marker speciale
            // Un singolo byte con valore 1 indica un risultato negativo
            byte[] negativeMarker = [1];
            await cache.SetAsync(key, negativeMarker, negativeOptions);
            Console.WriteLine($"Risultato negativo per la chiave {key} salvato in cache con TTL breve");
            return null;
        }
    }

    // Metodo per invalidare la cache quando viene creato un nuovo elemento
    public static async Task InvalidateCacheAsync(
        this IDistributedCache cache,
        string keyPattern,
        object identifier)
    {
        string key = string.Format(keyPattern, identifier);
        await cache.RemoveAsync(key);
        Console.WriteLine($"Cache invalidata per la chiave {key} a seguito di creazione/modifica dati");
    }
    
}

// Classe di supporto per memorizzare i dati con metadati
public class CachedItem<T>
{
    public T Data { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}

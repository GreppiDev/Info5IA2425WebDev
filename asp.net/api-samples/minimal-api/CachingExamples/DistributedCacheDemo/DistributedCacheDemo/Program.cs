using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using StackExchange.Redis;
using DistributedCacheDemo;
using DistributedCacheDemo.Models;
using DistributedCacheDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
//aggiunge il servizio che permette ad OpenAPI di leggere i metadati delle API
builder.Services.AddEndpointsApiExplorer();
//configura il servizio OpenAPI
builder.Services.AddOpenApiDocument(config =>
    {
        config.Title = "Distributed Cache Demo v1";
        config.DocumentName = "Distributed Cache Demo API";
        config.Version = "v1";
    }
);

// Registra il servizio Redis per IDistributedCache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetSection("Redis:ConnectionString").Value;
    options.InstanceName = builder.Configuration.GetSection("Redis:InstanceName").Value;
});

// Aggiunge ConnectionMultiplexer per i lock distribuiti
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    string? connectionString = builder.Configuration.GetSection("Redis:ConnectionString").Value;
    if (string.IsNullOrEmpty(connectionString))
    {
        // Fornisci un valore di default o lancia un'eccezione
        connectionString = "localhost:6379"; // Valore di default
        Console.WriteLine("AVVISO: Redis:ConnectionString non trovata nella configurazione. Usando il valore di default: " + connectionString);
    }
    return ConnectionMultiplexer.Connect(connectionString);
});

// Registra il servizio per il lock distribuito
builder.Services.AddSingleton<LockService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    //permette a Swagger (NSwag) di generare un file JSON con le specifiche delle API
    app.UseOpenApi();
    //permette di configurare l'interfaccia SwaggerUI (l'interfaccia grafica web di Swagger (NSwag) che permette di interagire con le API)
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "Distributed Cache Demo v1";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });

    app.UseDeveloperExceptionPage();
}

//app.UseHttpsRedirection();

app.MapGet("/products/{id}", async (int id, IDistributedCache cache) =>
{
    // Chiave univoca per identificare l'elemento nella cache
    string cacheKey = $"product:{id}";

    // Tenta di recuperare il prodotto dalla cache
    var cachedProduct = await cache.GetStringAsync(cacheKey);
    Product? product = null;

    if (string.IsNullOrEmpty(cachedProduct))
    {
        // Se non presente in cache, simula il recupero dal database
        product = await GetProductFromDatabaseAsync(id);

        if (product is not null)
        {
            // Serializza il prodotto in formato JSON
            var serializedProduct = JsonSerializer.Serialize(product);

            // Imposta le opzioni di cache
            var cacheEntryOptions = new DistributedCacheEntryOptions()
                // Scadenza assoluta - rimuove l'elemento dopo 1 ora
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                // Scadenza scorrevole - rimuove l'elemento se non vi si accede per 10 minuti
                .SetSlidingExpiration(TimeSpan.FromMinutes(10));

            // Salva il prodotto nella cache
            await cache.SetStringAsync(cacheKey, serializedProduct, cacheEntryOptions);
            Console.WriteLine($"Prodotto {id} salvato nella cache distribuita");
        }
    }
    else
    {
        // Deserializza il prodotto dal formato JSON
        product = JsonSerializer.Deserialize<Product>(cachedProduct);
        Console.WriteLine($"Prodotto {id} recuperato dalla cache distribuita");
    }

    if (product is null)
    {
        return Results.NotFound();
    }
    return Results.Ok(product);
});

// Endpoint che utilizza un pattern più semplice
app.MapGet("/products-v2/{id}", async (int id, IDistributedCache cache) =>
{
    // Chiave univoca per identificare l'elemento nella cache
    string cacheKey = $"product-v2:{id}";

    // Tenta di recuperare il prodotto dalla cache
    var cachedProduct = await cache.GetStringAsync(cacheKey);
    Product? product;

    // Se non presente in cache, recuperalo dal database
    if (string.IsNullOrEmpty(cachedProduct))
    {
        Console.WriteLine($"Recupero del prodotto {id} dal database (usando pattern v2)");
        product = await GetProductFromDatabaseAsync(id);

        if (product is not null)
        {
            // Serializza il prodotto e salvalo nella cache
            var serializedProduct = JsonSerializer.Serialize(product);
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                .SetSlidingExpiration(TimeSpan.FromMinutes(10));

            await cache.SetStringAsync(cacheKey, serializedProduct, options);
            Console.WriteLine($"Prodotto {id} salvato nella cache distribuita (pattern v2)");
        }
    }
    else
    {
        // Deserializza il prodotto dalla cache
        product = JsonSerializer.Deserialize<Product>(cachedProduct);
        Console.WriteLine($"Prodotto {id} recuperato dalla cache distribuita (pattern v2)");
    }

    if (product is null)
    {
        return Results.NotFound();
    }
    return Results.Ok(product);
});

// Endpoint che utilizza la serializzazione binaria diretta
app.MapGet("/product-v3/{id:int}", async (int id, IDistributedCache cache) =>
{
    string cacheKey = $"product-v3:{id}";

    // Tentativo di ottenere il prodotto dalla cache
    byte[]? cachedProduct = await cache.GetAsync(cacheKey);

    if (cachedProduct == null)
    {
        // Simulazione di un accesso al database
        var product = new Product
        {
            Id = id,
            Name = $"Prodotto {id}",
            Price = 10.99m * id,
            LastUpdated = DateTime.UtcNow
        };

        // Serializzazione dell'oggetto
        byte[] serializedProduct = JsonSerializer.SerializeToUtf8Bytes(product);

        // Salva in cache per 5 minuti
        var options = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

        await cache.SetAsync(cacheKey, serializedProduct, options);
        Console.WriteLine($"Prodotto {id} salvato nella cache distribuita (serializzazione binaria)");

        return Results.Ok(product);
    }
    else
    {
        // Deserializzazione dell'oggetto
        var product = JsonSerializer.Deserialize<Product>(cachedProduct);
        Console.WriteLine($"Prodotto {id} recuperato dalla cache distribuita (serializzazione binaria)");
        return Results.Ok(product);
    }
});

// Endpoint che utilizza GetOrCreateAsync con IDistributedCache
app.MapGet("/products-v4/{id}", async (int id, IDistributedCache cache) =>
{
    // Chiave univoca per identificare l'elemento nella cache
    string cacheKey = $"product-v4:{id}";

    // Utilizzo del metodo di estensione GetOrCreateAsync
    var product = await cache.GetOrCreateAsync(
        cacheKey,
        async () =>
        {
            Console.WriteLine($"Recupero del prodotto {id} dal database (usando GetOrCreateAsync con IDistributedCache)");
            return await GetProductFromDatabaseAsync(id);
        },
        new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromHours(1))
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
    );

    if (product is null)
    {
        return Results.NotFound();
    }

    Console.WriteLine($"Prodotto {id} gestito tramite GetOrCreateAsync con IDistributedCache");
    return Results.Ok(product);
});

// Endpoint che utilizza TryGetValueAsync con IDistributedCache
app.MapGet("/products-v5/{id}", async (int id, IDistributedCache cache) =>
{
    // Chiave univoca per identificare l'elemento nella cache
    string cacheKey = $"product-v5:{id}";

    // Tenta di recuperare il prodotto dalla cache usando TryGetValueAsync
    var (found, product) = await cache.TryGetValueAsync<Product>(cacheKey);

    if (!found)
    {
        // Se non presente in cache, simula il recupero dal database
        product = await GetProductFromDatabaseAsync(id);

        if (product is not null)
        {
            // Serializza il prodotto e salvalo nella cache
            byte[] serializedProduct = JsonSerializer.SerializeToUtf8Bytes(product);
            var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                .SetSlidingExpiration(TimeSpan.FromMinutes(10));

            await cache.SetAsync(cacheKey, serializedProduct, options);
            Console.WriteLine($"Prodotto {id} salvato nella cache distribuita (usando TryGetValueAsync)");
        }
    }
    else
    {
        Console.WriteLine($"Prodotto {id} recuperato dalla cache distribuita (usando TryGetValueAsync)");
    }

    if (product is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(product);
});

// Endpoint che utilizza un lock distribuito per risolvere il problema del "Thundering Herd" o "Cache Stampede"
// Questo approccio è utile quando più richieste concorrenti cercano di accedere a un dato non presente in cache
app.MapGet("/products-with-lock/{id}", async (int id, IDistributedCache cache,
    IConnectionMultiplexer redis, LockService lockService) =>
{
    string cacheKey = $"product-lock-v2:{id}";

    Console.WriteLine($"Richiesta per il prodotto {id} ricevuta (usando lock distribuito migliorato)");

    var product = await lockService.GetOrCreateWithDistributedLockAsync<Product>(
        cache,
        redis,
        cacheKey,
        async () =>
        {
            Console.WriteLine($"Recupero del prodotto {id} dal database (lock distribuito)");
            await Task.Delay(500); // Simulazione database
            return await GetProductFromDatabaseAsync(id);
        },
        5,              // Massimo 5 tentativi
        10,             // 10 secondi di timeout per il lock
        new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
            .SetSlidingExpiration(TimeSpan.FromMinutes(5))
    );

    if (product is null)
    {
        return Results.NotFound();
    }

    Console.WriteLine($"Prodotto {id} restituito (lock distribuito v2)");
    return Results.Ok(product);
});

// Endpoint che utilizza la strategia di rinnovo staggered (scaglionato)
app.MapGet("/products-staggered/{id}", async (int id, IDistributedCache cache) =>
{
    string cacheKey = $"product-staggered:{id}";

    Console.WriteLine($"Richiesta per il prodotto {id} ricevuta (usando rinnovo staggered)");

    var product = await cache.GetOrCreateWithStaggeredRenewalAsync<Product>(
        cacheKey,
        async () =>
        {
            Console.WriteLine($"Recupero del prodotto {id} dal database per il rinnovo staggered");
            await Task.Delay(500); // Simula un accesso al database più lungo
            return await GetProductFromDatabaseAsync(id);
        },
        TimeSpan.FromMinutes(10),  // Scadenza assoluta
        TimeSpan.FromMinutes(2)    // Scadenza scorrevole
    );

    if (product is null)
    {
        return Results.NotFound();
    }

    Console.WriteLine($"Prodotto {id} restituito (usando rinnovo staggered)");
    return Results.Ok(product);
});

// Endpoint che implementa la strategia "Stale While Revalidate" (SWR)
app.MapGet("/products-swr/{id}", async (int id, IDistributedCache cache) =>
{
    string cacheKey = $"product-swr:{id}";

    Console.WriteLine($"Richiesta per il prodotto {id} ricevuta (usando Stale While Revalidate)");

    var product = await cache.GetOrCreateStaleWhileRevalidateAsync<Product>(
        cacheKey,
        async () =>
        {
            Console.WriteLine($"Recupero del prodotto {id} dal database per SWR");
            await Task.Delay(500); // Simula un accesso al database più lungo
            return await GetProductFromDatabaseAsync(id);
        },
        TimeSpan.FromSeconds(30),  // Durata di "freschezza" (30 secondi per demo, normalmente più lungo)
        TimeSpan.FromMinutes(5)    // Durata di "obsolescenza" (5 minuti)
    );

    if (product is null)
    {
        return Results.NotFound();
    }

    // Aggiungiamo un timestamp per mostrare quando è stato generato il prodotto
    var result = new
    {
        product.Id,
        product.Name,
        product.Price,
        product.LastUpdated,
        ServedAt = DateTime.UtcNow
    };

    Console.WriteLine($"Prodotto {id} restituito (usando Stale While Revalidate)");
    return Results.Ok(result);
});

// Endpoint che utilizza la strategia Negative Caching
app.MapGet("/products-negative/{id}", async (int id, IDistributedCache cache) =>
{
    string cacheKey = $"product-negative:{id}";

    Console.WriteLine($"Richiesta per il prodotto {id} ricevuta (usando Negative Caching)");

    var product = await cache.GetOrCreateWithNegativeCachingAsync<Product>(
        cacheKey,
        async () =>
        {
            Console.WriteLine($"Recupero del prodotto {id} dal database per Negative Caching");
            await Task.Delay(500); // Simula un accesso al database più lungo
            return await GetProductFromDatabaseAsync(id);
        },
        new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
            .SetSlidingExpiration(TimeSpan.FromMinutes(5)),
        new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(1))  // TTL breve per risultati negativi
    );

    if (product is null)
    {
        return Results.NotFound($"Prodotto con ID {id} non trovato.");
    }

    // Aggiungiamo un timestamp per mostrare quando è stato generato il prodotto
    var result = new
    {
        product.Id,
        product.Name,
        product.Price,
        product.LastUpdated,
        ServedAt = DateTime.UtcNow
    };

    Console.WriteLine($"Prodotto {id} restituito (usando Negative Caching)");
    return Results.Ok(result);
});

// Endpoint per la creazione o aggiornamento di un prodotto con invalidazione della cache
app.MapPost("/products", async (Product product, IDistributedCache cache) =>
{
    // Simula la creazione/aggiornamento nel database
    await Task.Delay(200); // Simula l'operazione di scrittura nel database

    // Aggiorna il timestamp
    product.LastUpdated = DateTime.UtcNow;

    // Invalida la cache per questo prodotto in tutti gli endpoint
    await cache.InvalidateCacheAsync("product:{0}", product.Id);
    await cache.InvalidateCacheAsync("product-v2:{0}", product.Id);
    await cache.InvalidateCacheAsync("product-v3:{0}", product.Id);
    await cache.InvalidateCacheAsync("product-v4:{0}", product.Id);
    await cache.InvalidateCacheAsync("product-v5:{0}", product.Id);
    await cache.InvalidateCacheAsync("product-lock:{0}", product.Id);
    await cache.InvalidateCacheAsync("product-staggered:{0}", product.Id);
    await cache.InvalidateCacheAsync("product-swr:{0}", product.Id);
    await cache.InvalidateCacheAsync("product-negative:{0}", product.Id);
    await cache.InvalidateCacheAsync("product-robust:{0}", product.Id);

    Console.WriteLine($"Prodotto {product.Id} creato/aggiornato e cache invalidata");

    return Results.Created($"/products/{product.Id}", product);
});

// Endpoint che combina lock distribuito e negative caching
app.MapGet("/products-robust/{id}", async (int id, IDistributedCache cache, IConnectionMultiplexer redis, LockService lockService) =>
{
    string cacheKey = $"product-robust:{id}";

    Console.WriteLine($"Richiesta per il prodotto {id} ricevuta (usando lock distribuito + negative caching)");

    var product = await lockService.GetOrCreateWithDistributedLockAndNegativeCachingAsync<Product>(
        cache,
        redis,
        cacheKey,
        async () =>
        {
            Console.WriteLine($"Recupero del prodotto {id} dal database (usando lock distribuito + negative caching)");
            await Task.Delay(500); // Simula un accesso al database più lungo
            return await GetProductFromDatabaseAsync(id);
        },
        5,//numero massimo di tentativi
        10, // Timeout del lock
        new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
            .SetSlidingExpiration(TimeSpan.FromMinutes(5)),
        new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(1)) // TTL breve per risultati negativi
    );

    if (product is null)
    {
        return Results.NotFound($"Prodotto con ID {id} non trovato.");
    }

    // Aggiungiamo un timestamp per mostrare quando è stato generato il prodotto
    var result = new
    {
        product.Id,
        product.Name,
        product.Price,
        product.LastUpdated,
        ServedAt = DateTime.UtcNow,
        Strategy = "Distributed Lock + Negative Caching"
    };

    Console.WriteLine($"Prodotto {id} restituito (usando lock distribuito + negative caching)");
    return Results.Ok(result);
});

// Simula il recupero del prodotto dal database
static async Task<Product?> GetProductFromDatabaseAsync(int id)
{
    // ... logica per recuperare il prodotto dal database ...
    await Task.Delay(100); // Simula un'operazione I/O
    return id switch
    {
        1 => new Product { Id = 1, Name = "Prodotto A", Price = 19.99M },
        2 => new Product { Id = 2, Name = "Prodotto B", Price = 29.99M },
        _ => null
    };
}

app.Run();


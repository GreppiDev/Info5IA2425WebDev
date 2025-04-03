using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using StackExchange.Redis;
using HybridCacheDemo.Models;
using HybridCacheDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
//aggiunge il servizio che permette ad OpenAPI di leggere i metadati delle API
builder.Services.AddEndpointsApiExplorer();
//configura il servizio OpenAPI
builder.Services.AddOpenApiDocument(config =>
    {
        config.Title = "Hybrid Cache Demo v1";
        config.DocumentName = "Hybrid Cache Demo API";
        config.Version = "v1";
    }
);

// Configura memory cache
builder.Services.AddMemoryCache();

// Configura Redis come Distributed Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetSection("Redis:ConnectionString").Value;
    options.InstanceName = builder.Configuration.GetSection("Redis:InstanceName").Value;
});

// Configura le impostazioni TTL predefinite per usarle in tutto il codice
var hybridSection = builder.Configuration.GetSection("HybridCache");
TimeSpan defaultDistributedCacheTTL = TimeSpan.FromMinutes(30);
TimeSpan defaultInMemoryCacheTTL = TimeSpan.FromMinutes(5);

if (TimeSpan.TryParse(hybridSection["DefaultDistributedCacheTTL"], out var distTtl))
    defaultDistributedCacheTTL = distTtl;

if (TimeSpan.TryParse(hybridSection["DefaultInMemoryCacheTTL"], out var memTtl))
    defaultInMemoryCacheTTL = memTtl;

// Mantiene il ConnectionMultiplexer per funzionalità Redis avanzate (locks, ecc.)
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    string? connectionString = builder.Configuration.GetSection("Redis:ConnectionString").Value;
    if (string.IsNullOrEmpty(connectionString))
    {
        connectionString = "localhost:6379"; // Valore di default
        Console.WriteLine("AVVISO: Redis:ConnectionString non trovata nella configurazione. Usando il valore di default: " + connectionString);
    }
    return ConnectionMultiplexer.Connect(connectionString);
});

// Registra una versione aggiornata del LockService compatibile con HybridCache
builder.Services.AddSingleton<CacheLockService>();

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
        config.DocumentTitle = "Hybrid Cache Demo v1";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });

    app.UseDeveloperExceptionPage();
}

//app.UseHttpsRedirection();

// Endpoint base che utilizza IDistributedCache e IMemoryCache insieme
app.MapGet("/products/{id}", async (int id, IDistributedCache distributedCache, IMemoryCache memoryCache) =>
{
    string cacheKey = $"product:{id}";

    // Prima controlla la memoria cache (più veloce)
    if (memoryCache.TryGetValue(cacheKey, out Product? product))
    {
        Console.WriteLine($"Prodotto {id} trovato nella cache in memoria");
        return Results.Ok(product);
    }

    // Controlla la cache distribuita
    byte[]? cachedData = await distributedCache.GetAsync(cacheKey);

    if (cachedData != null && cachedData.Length > 0)
    {
        // Deserializza il prodotto dalla cache distribuita
        try
        {
            product = JsonSerializer.Deserialize<Product>(cachedData);

            // Memorizza anche in memoria per accessi futuri più veloci
            var memoryCacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(defaultInMemoryCacheTTL);

            memoryCache.Set(cacheKey, product, memoryCacheOptions);

            Console.WriteLine($"Prodotto {id} recuperato dalla cache distribuita e memorizzato in memoria");
            return Results.Ok(product);
        }
        catch (JsonException)
        {
            // Se la deserializzazione fallisce, ignora la cache
        }
    }

    // Cache miss: recupera dal database
    product = await GetProductFromDatabaseAsync(id);

    if (product == null)
    {
        return Results.NotFound();
    }

    // Salva nella cache distribuita
    var distributedCacheOptions = new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(defaultDistributedCacheTTL);

    await distributedCache.SetAsync(
        cacheKey,
        JsonSerializer.SerializeToUtf8Bytes(product),
        distributedCacheOptions);

    // Salva anche in memoria
    var cacheEntryOptions = new MemoryCacheEntryOptions()
        .SetAbsoluteExpiration(defaultInMemoryCacheTTL);

    memoryCache.Set(cacheKey, product, cacheEntryOptions);

    Console.WriteLine($"Prodotto {id} recuperato dal database e salvato in entrambe le cache");
    return Results.Ok(product);
});

// Endpoint che dimostra il controllo preciso sulla scadenza
app.MapGet("/products-ttl/{id}", async (int id, IDistributedCache distributedCache, IMemoryCache memoryCache) =>
{
    string cacheKey = $"product-ttl:{id}";

    // Implementazione con TTL personalizzati
    if (memoryCache.TryGetValue(cacheKey, out Product? product))
    {
        Console.WriteLine($"Prodotto {id} trovato nella cache in memoria (con TTL personalizzati)");
        return Results.Ok(product);
    }

    // Controlla la cache distribuita
    byte[]? cachedData = await distributedCache.GetAsync(cacheKey);

    if (cachedData != null && cachedData.Length > 0)
    {
        // Deserializza il prodotto dalla cache distribuita
        try
        {
            product = JsonSerializer.Deserialize<Product>(cachedData);

            // Memorizza anche in memoria per accessi futuri più veloci con TTL personalizzato
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            memoryCache.Set(cacheKey, product, cacheOptions);

            Console.WriteLine($"Prodotto {id} recuperato dalla cache distribuita (con TTL personalizzati)");
            return Results.Ok(product);
        }
        catch (JsonException)
        {
            // Se la deserializzazione fallisce, ignora la cache
        }
    }

    // Cache miss: recupera dal database
    product = await GetProductFromDatabaseAsync(id);

    if (product == null)
    {
        return Results.NotFound();
    }

    // Salva nella cache distribuita con TTL personalizzato (più lungo)
    var distributedCacheOptions = new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromHours(1));

    await distributedCache.SetAsync(
        cacheKey,
        JsonSerializer.SerializeToUtf8Bytes(product),
        distributedCacheOptions);

    // Salva anche in memoria con TTL personalizzato (più breve)
    var memOptions = new MemoryCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
        .SetSlidingExpiration(TimeSpan.FromMinutes(2));

    memoryCache.Set(cacheKey, product, memOptions);

    Console.WriteLine($"Prodotto {id} recuperato dal database (con TTL personalizzati)");
    return Results.Ok(product);
});

// Endpoint che dimostra la gestione esplicita dei valori null (negative caching)
app.MapGet("/products-with-null/{id}", async (int id, IDistributedCache distributedCache, IMemoryCache memoryCache) =>
{
    string cacheKey = $"product-with-null:{id}";
    string nullMarkerKey = $"product-with-null:null-marker:{id}";

    // Controlla se esiste il marker per un valore null in memoria
    if (memoryCache.TryGetValue(nullMarkerKey, out bool _))
    {
        Console.WriteLine($"Prodotto {id} non trovato (marker null in cache memoria)");
        return Results.NotFound($"Prodotto con ID {id} non trovato.");
    }

    // Controlla se esiste il marker per un valore null in cache distribuita
    byte[]? nullMarkerData = await distributedCache.GetAsync(nullMarkerKey);
    if (nullMarkerData != null && nullMarkerData.Length > 0)
    {
        // Memorizza anche in memoria per accessi futuri più veloci
        memoryCache.Set(nullMarkerKey, true, TimeSpan.FromMinutes(1));

        Console.WriteLine($"Prodotto {id} non trovato (marker null in cache distribuita)");
        return Results.NotFound($"Prodotto con ID {id} non trovato.");
    }

    // Controlla se il prodotto esiste in memoria cache
    if (memoryCache.TryGetValue(cacheKey, out Product? product))
    {
        Console.WriteLine($"Prodotto {id} trovato in cache memoria");
        return Results.Ok(product);
    }

    // Controlla la cache distribuita
    byte[]? cachedData = await distributedCache.GetAsync(cacheKey);
    if (cachedData != null && cachedData.Length > 0)
    {
        try
        {
            product = JsonSerializer.Deserialize<Product>(cachedData);
            // Memorizza anche in memoria
            memoryCache.Set(cacheKey, product, TimeSpan.FromMinutes(1));
            Console.WriteLine($"Prodotto {id} recuperato dalla cache distribuita");
            return Results.Ok(product);
        }
        catch (JsonException)
        {
            // Se la deserializzazione fallisce, ignora la cache
        }
    }

    // Recupera il prodotto dal database
    product = await GetProductFromDatabaseAsync(id);

    // TTL più breve per i risultati null
    var options = new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));

    if (product != null)
    {
        // Memorizza il prodotto nelle cache
        await distributedCache.SetAsync(
            cacheKey,
            JsonSerializer.SerializeToUtf8Bytes(product),
            options);

        memoryCache.Set(cacheKey, product, TimeSpan.FromMinutes(1));
        Console.WriteLine($"Prodotto {id} recuperato dal DB e salvato in cache");
        return Results.Ok(product);
    }
    else
    {
        // Memorizziamo un marker per indicare che il prodotto è null
        await distributedCache.SetAsync(
            nullMarkerKey,
            JsonSerializer.SerializeToUtf8Bytes(true),
            options);

        memoryCache.Set(nullMarkerKey, true, TimeSpan.FromMinutes(1));
        Console.WriteLine($"Prodotto {id} non trovato (memorizzato marker null)");
        return Results.NotFound($"Prodotto con ID {id} non trovato.");
    }
});

// Endpoint con lock distribuito usando CacheLockService
app.MapGet("/products-lock/{id}", async (int id, IDistributedCache distributedCache, IMemoryCache memoryCache, CacheLockService lockService) =>
{
    string cacheKey = $"product-lock:{id}";

    Console.WriteLine($"Richiesta per il prodotto {id} ricevuta (usando lock distribuito)");

    var product = await lockService.GetOrSetWithLockAsync(
        distributedCache,
        memoryCache,
        cacheKey,
        async () =>
        {
            Console.WriteLine($"Recupero del prodotto {id} dal database (con lock distribuito)");
            await Task.Delay(500); // Simula un accesso al database più lungo
            return await GetProductFromDatabaseAsync(id);
        },
        defaultDistributedCacheTTL,
        defaultInMemoryCacheTTL);

    if (product is null)
    {
        return Results.NotFound();
    }

    var result = new
    {
        product.Id,
        product.Name,
        product.Price,
        product.LastUpdated,
        ServedAt = DateTime.UtcNow,
        Strategy = "Distributed Lock con cache ibrida"
    };

    Console.WriteLine($"Prodotto {id} restituito (con lock distribuito)");
    return Results.Ok(result);
});

// Simula il recupero del prodotto dal database
static async Task<Product?> GetProductFromDatabaseAsync(int id)
{
    await Task.Delay(100); // Simula un'operazione I/O
    return id switch
    {
        1 => new Product { Id = 1, Name = "Prodotto A", Price = 19.99M, LastUpdated = DateTime.UtcNow },
        2 => new Product { Id = 2, Name = "Prodotto B", Price = 29.99M, LastUpdated = DateTime.UtcNow },
        3 => new Product { Id = 3, Name = "Prodotto C", Price = 39.99M, LastUpdated = DateTime.UtcNow },
        4 => new Product { Id = 4, Name = "Prodotto D", Price = 49.99M, LastUpdated = DateTime.UtcNow },
        5 => new Product { Id = 5, Name = "Prodotto E", Price = 59.99M, LastUpdated = DateTime.UtcNow },
        _ => null
    };
}

app.Run();


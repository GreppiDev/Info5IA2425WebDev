using DistributedCacheDemo.Models;
using Microsoft.Extensions.Caching.Hybrid;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "Hybrid Cache Demo v1";
    config.DocumentName = "Hybrid Cache Demo API";
    config.Version = "v1";
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetSection("Redis:ConnectionString").Value;
    options.InstanceName = builder.Configuration.GetSection("Redis:InstanceName").Value;
});

//builder.Services.AddMemoryCache();
// Configurazione della cache in memoria locale
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 100 * 1024 * 1024; // 100 MB in memoria
});

//builder.Services.AddHybridCache();
builder.Services.AddHybridCache(options =>
    {
        options.MaximumPayloadBytes = 1024 * 1024;
        options.MaximumKeyLength = 1024;
        options.DefaultEntryOptions = new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromHours(1),
            LocalCacheExpiration = TimeSpan.FromMinutes(15),
        };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseOpenApi();
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

app.MapGet("/products-stateless/{id}", async (int id, HybridCache cache) =>
{
    // Simuliamo un'operazione di recupero del prodotto dal database
    // e lo memorizziamo nella cache ibrida.
    // La chiave della cache è "product:{id}".
    // La cache ibrida utilizza sia Redis che la cache in memoria locale.

    string cacheKey = $"product:{id}";
    Console.WriteLine($"Richiesta per il prodotto {id} ricevuta (usando stateless)");
    using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    var cancellationToken = cancellationTokenSource.Token;

    var product = await cache.GetOrCreateAsync(
        cacheKey,
        async (ct) =>
        {
            Console.WriteLine($"Recupero del prodotto {id} dal database");
            return await GetProductFromDatabaseAsync(id);

        },
        new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromHours(1),
            LocalCacheExpiration = TimeSpan.FromMinutes(15),
            Flags = HybridCacheEntryFlags.DisableDistributedCache
        },
        cancellationToken: cancellationToken);

    if (product is null)
    {
        return Results.NotFound();
    }
    return Results.Ok(product);

});

app.MapGet("/products-factory-with-state/{id}", async (int id, HybridCache cache) =>
{
    string cacheKey = $"product-factory:{id}";

    Console.WriteLine($"Richiesta per il prodotto {id} ricevuta (usando factory)");
    CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(5));
    var cancellationToken = cancellationTokenSource.Token;

    var product = await cache.GetOrCreateAsync(
        cacheKey,//key
        (id, cacheKey),//object state -->  può essere un oggetto complesso che viene passato alla factory
        async (state, ct) =>
        {
            Console.WriteLine($"Factory invocata per il prodotto {state.id} con cacheKey {state.cacheKey}");
            return await GetProductFromDatabaseAsync(state.id);
        },
        new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(30),
            LocalCacheExpiration = TimeSpan.FromMinutes(10)
        },
        cancellationToken: cancellationToken);

    if (product is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(product);
});

app.MapGet("/products-set/{id}", async (int id, HybridCache cache) =>
{
    string cacheKey = $"product-set:{id}";

    Console.WriteLine($"Recupero del prodotto {id} dal database (usando direttamente SetAsync)");
    var product = await GetProductFromDatabaseAsync(id);

    if (product is null)
    {
        return Results.NotFound();
    }

    await cache.SetAsync(
        cacheKey,
        product,
        new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(30),
            LocalCacheExpiration = TimeSpan.FromMinutes(5)
        });

    Console.WriteLine($"Prodotto {id} salvato direttamente nella cache ibrida con SetAsync");

    return Results.Ok(product);
});

app.MapDelete("/products/{id}", async (int id, HybridCache cache) =>
{
    Console.WriteLine($"Richiesta di invalidazione cache per prodotto {id}");

    await cache.RemoveAsync($"product:{id}");

    var keysToInvalidate = new[]
    {
        $"product:{id}",
        $"product-factory:{id}",
        $"product-hot:{id}"
    };

    await cache.RemoveAsync(keysToInvalidate);

    return Results.Ok(new
    {
        ProductId = id,
        Message = "Cache invalidata con successo"
    });
});

app.MapGet("/products-flags/{id}", async (int id, HybridCache cache) =>
{
    string cacheKey = $"product-flags:{id}";

    Console.WriteLine($"Richiesta per il prodotto {id} ricevuta (usando flags)");

    var product = await cache.GetOrCreateAsync(
        cacheKey,
        id,//qui lo stato è un int, non un oggetto complesso
        async (state, cancellationToken) =>
        {
            Console.WriteLine($"Recupero del prodotto {state} dal database (con flags)");
            return await GetProductFromDatabaseAsync(state);
        },
        new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromHours(1),
            LocalCacheExpiration = TimeSpan.FromMinutes(10),
            Flags = HybridCacheEntryFlags.DisableDistributedCache
        });

    if (product is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(product);
});

app.MapGet("/products-tagged/{category}", async (string category, HybridCache cache) =>
{
    var productsInCategory = await GetProductsByCategoryAsync(category);

    if (productsInCategory.Count == 0)
    {
        return Results.NotFound($"Nessun prodotto trovato nella categoria '{category}'");
    }

    foreach (var product in productsInCategory)
    {
        string cacheKey = $"product-in-category:{product.Id}";

        var tags = new[] { $"category:{category}", "products" };
        if (product.Price > 25)
        {
            tags = [.. tags, "premium"];
        }

        await cache.SetAsync(
            cacheKey,
            product,
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(1),
                LocalCacheExpiration = TimeSpan.FromMinutes(10)
            },
            tags);
    }

    return Results.Ok(new
    {
        Category = category,
        productsInCategory.Count,
        Products = productsInCategory,
        CachedWith = new[] { $"category:{category}", "products" }
    });
});

app.MapDelete("/products/category/{category}", async (string category, HybridCache cache) =>
{
    Console.WriteLine($"Invalidazione di tutti i prodotti della categoria '{category}'");

    await cache.RemoveByTagAsync($"category:{category}");

    return Results.Ok(new
    {
        Category = category,
        Message = $"Tutti i prodotti della categoria '{category}' sono stati rimossi dalla cache"
    });
});

app.MapDelete("/products/premium", async (HybridCache cache) =>
{
    Console.WriteLine("Invalidazione di tutti i prodotti premium");

    await cache.RemoveByTagAsync("premium");

    return Results.Ok(new
    {
        Message = "Tutti i prodotti premium sono stati rimossi dalla cache"
    });
});

app.MapDelete("/products/all", async (HybridCache cache) =>
{
    Console.WriteLine("Invalidazione di tutti i prodotti nella cache");

    await cache.RemoveByTagAsync("products");

    return Results.Ok(new
    {
        Message = "Tutti i prodotti sono stati rimossi dalla cache"
    });
});

static async Task<Product?> GetProductFromDatabaseAsync(int id)
{
    await Task.Delay(100);
    return id switch
    {
        1 => new Product { Id = 1, Name = "Prodotto A", Price = 19.99M, LastUpdated = DateTime.UtcNow },
        2 => new Product { Id = 2, Name = "Prodotto B", Price = 29.99M, LastUpdated = DateTime.UtcNow },
        _ => null
    };
}

static async Task<List<Product>> GetProductsByCategoryAsync(string category)
{
    await Task.Delay(150);

    return category.ToLower() switch
    {
        "elettronica" =>
        [
            new() { Id = 101, Name = "Smartphone", Price = 299.99M, LastUpdated = DateTime.UtcNow },
            new() { Id = 102, Name = "Laptop", Price = 799.99M, LastUpdated = DateTime.UtcNow },
            new() { Id = 103, Name = "Cuffie", Price = 49.99M, LastUpdated = DateTime.UtcNow }
        ],
        "libri" =>
        [
            new() { Id = 201, Name = "Clean Code", Price = 29.99M, LastUpdated = DateTime.UtcNow },
            new() { Id = 202, Name = "Design Patterns", Price = 34.99M, LastUpdated = DateTime.UtcNow }
        ],
        "cibo" =>
        [
            new() { Id = 301, Name = "Pasta", Price = 2.99M, LastUpdated = DateTime.UtcNow },
            new() { Id = 302, Name = "Salsa", Price = 3.99M, LastUpdated = DateTime.UtcNow },
            new() { Id = 303, Name = "Olio d'oliva premium", Price = 27.99M, LastUpdated = DateTime.UtcNow }
        ],
        _ => []
    };
}

app.Run();


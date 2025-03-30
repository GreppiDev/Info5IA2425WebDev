using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
//aggiunge il servizio che permette ad OpenAPI di leggere i metadati delle API
builder.Services.AddEndpointsApiExplorer();
//configura il servizio OpenAPI
builder.Services.AddOpenApiDocument(config =>
    {
        config.Title = "Memory Cache Demo v1";
        config.DocumentName = "Memory Cache Demo API";
        config.Version = "v1";
    }
);

// Registra il servizio IMemoryCache
builder.Services.AddMemoryCache();

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
        config.DocumentTitle = "Memory Cache Demo v1";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });

    app.UseDeveloperExceptionPage();
}

//app.UseHttpsRedirection();

app.MapGet("/products/{id}", async (int id, IMemoryCache cache) =>
{
    // Chiave univoca per identificare l'elemento nella cache
    string cacheKey = $"product:{id}";

    // Tenta di recuperare il prodotto dalla cache
    if (!cache.TryGetValue(cacheKey, out Product? product))
    {
        // Se non presente in cache, simula il recupero dal database
        product = await GetProductFromDatabaseAsync(id);

        if (product is not null)
        {
            // Imposta le opzioni di cache
            var cacheEntryOptions = new MemoryCacheEntryOptions()

                // Scadenza assoluta - rimuove l'elemento dopo 1 ora
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))

                // Scadenza scorrevole - rimuove l'elemento se non vi si accede per 10 minuti
                .SetSlidingExpiration(TimeSpan.FromMinutes(10))

                // Priorità - definisce la priorità di rimozione quando la memoria è sotto pressione
                .SetPriority(CacheItemPriority.Normal)

                // Imposta la chiamata di una callback function quando l'elemento viene tolto dalla cache
                .RegisterPostEvictionCallback((key, value, reason, state) =>
                    {
                        Console.WriteLine($"L'elemento con chiave {key} è stato rimosso per: {reason}");
                    });

            // Salva il prodotto nella cache
            cache.Set(cacheKey, product, cacheEntryOptions);
        }
    }

    if (product is null)
    {
        return Results.NotFound();
    }
    return Results.Ok(product);
});

// Endpoint che utilizza GetOrCreateAsync
app.MapGet("/products-v2/{id}", async (int id, IMemoryCache cache) =>
{
    // Chiave univoca per identificare l'elemento nella cache
    string cacheKey = $"product-v2:{id}";

    // Utilizza GetOrCreateAsync che semplifica il pattern di caching
    var product = await cache.GetOrCreateAsync(cacheKey, async entry =>
    {
        // Configura le opzioni di cache
        entry.SetAbsoluteExpiration(TimeSpan.FromHours(1));
        entry.SetSlidingExpiration(TimeSpan.FromMinutes(10));
        entry.SetPriority(CacheItemPriority.Normal);

        // Registra la callback di eviction
        entry.RegisterPostEvictionCallback((key, value, reason, state) =>
        {
            Console.WriteLine($"L'elemento con chiave {key} è stato rimosso per: {reason}");
        });

        // Recupero del prodotto dal database solo se non è presente in cache
        Console.WriteLine($"Recupero del prodotto {id} dal database (usando GetOrCreateAsync)");
        return await GetProductFromDatabaseAsync(id);
    });

    if (product is null)
    {
        return Results.NotFound();
    }
    return Results.Ok(product);
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

// Modello Product
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

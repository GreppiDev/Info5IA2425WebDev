//file Program.cs
using Microsoft.EntityFrameworkCore;
using MyApi.Model;
using MyApi.Data;

var builder = WebApplication.CreateBuilder(args);

// 👇 Costruzione della connection string usando variabili d'ambiente
// Le variabili d'ambiente hanno priorità; se non presenti, usa valori di default
var host = builder.Configuration["MARIADB_HOST"] ?? "mariadb-server1";
var port = builder.Configuration["MARIADB_PORT"] ?? "3306";
var database = builder.Configuration["MARIADB_DATABASE"] ?? "pizza_store";
var user = builder.Configuration["MARIADB_USER"] ?? "root";
var password = builder.Configuration["MARIADB_PASSWORD"] ?? "root";
var connectionString = $"Server={host};Port={port};Database={database};User Id={user};Password={password};";

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//AddOpenApi si può usare dalla versione 9 in poi di dotnet
//builder.Services.AddOpenApi();
//adding API explorer
builder.Services.AddEndpointsApiExplorer();
//adding OpenAPI configuration
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "PizzaStoreAPIv1";
    config.Title = "PizzaStore v1";
    config.Version = "v1";
});

//adding services to the container
if (builder.Environment.IsDevelopment())
{
    //il servizio AddDatabaseDeveloperPageExceptionFilter andrebbe usato solo in fase di testing e non in produzione.
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

// 👇 autodetect della versione del server di MariaDb
var serverVersion = ServerVersion.AutoDetect(connectionString);
// 👇 configurazione del provider per EF Core nel caso di MariaDb con il pacchetto Pomelo
builder.Services.AddDbContext<PizzaDb>(
        dbContextOptions => dbContextOptions
            .UseMySql(connectionString, serverVersion)
            // The following three options help with debugging, but should
            // be changed or removed for production.
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
    );

var app = builder.Build();


//app.UseHttpsRedirection();

//adding middleware for Swagger and OpenAPI
if (app.Environment.IsDevelopment())
{
    //adding middleware for OpenAPI
    //app.MapOpenApi();
    //adding middleware for Swagger
    app.UseOpenApi();
    app.UseSwaggerUi(config =>

    {
        config.DocumentTitle = "PizzaStore API v1";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}
//creations of API Endpoint Routes
// prima: app.MapGet("/", () => "Hello World!");

if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/swagger"));
}
else
{
    app.MapGet("/", () => "Hello World!");
}
//AsNoTracking(), disabilita il tracking 
//The change tracker will not track any of the entities that are returned from a LINQ query. If the entity instances are modified, this will not be detected by the change tracker and DbContext.SaveChanges() will not persist those changes to the database.

//Disabling change tracking is useful for read-only scenarios because it avoids the overhead of setting up change tracking for each entity instance. You should not disable change tracking if you want to manipulate entity instances and persist those changes to the database using DbContext.SaveChanges().
//							È utile in contesti di sola lettura 👇
app.MapGet("/pizzas", async (PizzaDb db) => await db.Pizzas.AsNoTracking().ToListAsync());


app.MapPost("/pizza", async (PizzaDb db, Pizza pizza) =>
{
    db.Pizzas.Add(pizza);
    await db.SaveChangesAsync();
    return Results.Created($"/pizza/{pizza.Id}", pizza);
});

app.MapGet("/pizza/{id}", async (PizzaDb db, int id) => await db.Pizzas.FindAsync(id));

app.MapPut("/pizza/{id}", async (PizzaDb db, Pizza updatePizza, int id) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null) return Results.NotFound();
    pizza.Name = updatePizza.Name;
    pizza.Description = updatePizza.Description;
    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.MapDelete("/pizza/{id}", async (PizzaDb db, int id) =>
{
    var pizza = await db.Pizzas.FindAsync(id);
    if (pizza is null)
    {
        return Results.NotFound();
    }
    db.Pizzas.Remove(pizza);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.Run();
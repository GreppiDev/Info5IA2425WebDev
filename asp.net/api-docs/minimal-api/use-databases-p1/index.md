# Utilizzo dei database in ASP.NET con EF Core

- [Utilizzo dei database in ASP.NET con EF Core](#utilizzo-dei-database-in-aspnet-con-ef-core)
	- [Progetto PizzaStore v1](#progetto-pizzastore-v1)
		- [Utilizzo del database InMemory](#utilizzo-del-database-inmemory)
		- [Utilizzo del database Sqlite](#utilizzo-del-database-sqlite)
		- [Uso del database MySQL/MariaDb - PizzaStore v2](#uso-del-database-mysqlmariadb---pizzastore-v2)

## Progetto PizzaStore v1

Si crei un progetto di Minimal ASP.NET Core in uno dei modi visti in precedenza, ad esempio, con il `C# Dev Kit`.

Si aggiungano i pacchetti (dalla shell posizionata sulla cartella del progetto):

```ps1
# Aggiunta di Pacchetti NuGet
 dotnet add package Microsoft.EntityFrameworkCore.InMemory
 dotnet add package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore
 dotnet add package Microsoft.AspNetCore.OpenApi
 dotnet add package NSwag.AspNetCore
```

Si noti che, nel caso in cui si utilizzi il template `web`, ossia il template per applicazioni ASP.NET Core vuoto, occorre esplicitamente installare anche il pacchetto `Microsoft.AspNetCore.OpenApi`; nel caso si utilizzi il template `webapi`, il pacchetto per il supporto ad `OpenApi` è già installato.

Si modifichi il codice prodotto dal template come segue:

```cs
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
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
	builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

var app = builder.Build();


app.UseHttpsRedirection();

//adding middleware for Swagger and OpenAPI
if (app.Environment.IsDevelopment())
{
	//adding middleware for OpenAPI
	app.MapOpenApi();
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
app.MapGet("/", () => "Hello World!");
app.Run();
```

Si modifichi il file `launchSettings.json` come segue, con le impostazioni per far partire il browser direttamente sulle pagine di Swagger:

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5028",
      "launchUrl": "swagger",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7293;http://localhost:5028",
      "launchUrl": "swagger",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### Utilizzo del database InMemory

Si crei il file `Pizza.cs` nella cartella `Model`, con il seguente contenuto:

```cs
//file Pizza.cs
namespace PizzaStoreV1.Model;
public class Pizza
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}
```

Si aggiunga la classe `PizzaDb` nella cartella `Data`:

```cs
using Microsoft.EntityFrameworkCore;

namespace PizzaStoreV1.Data;


// class PizzaDb : DbContext
// {
// 	public PizzaDb(DbContextOptions options) : base(options) { }
// 	public DbSet<Pizza> Pizzas => Set<Pizza>();
// }

//use Primary Constructor: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/primary-constructors
class PizzaDb(DbContextOptions options) : DbContext(options)
{
	public DbSet<Pizza> Pizzas => Set<Pizza>();
}
```

Si modifichi il file `Program.cs` come segue:

```cs

using Microsoft.EntityFrameworkCore;
using PizzaStoreV1.Model;
using PizzaStoreV1.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
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
	builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

builder.Services.AddDbContext<PizzaDb>(options => options.UseInMemoryDatabase("items")); // 👈 si aggiunga il DbContext per l'accesso al database 

var app = builder.Build();

//app.UseHttpsRedirection();

//adding middleware for Swagger and OpenAPI
if (app.Environment.IsDevelopment())
{
	//adding middleware for OpenAPI
	app.MapOpenApi();
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
app.MapGet("/", () => "Hello World!");

app.MapGet("/pizzas", async (PizzaDb db) => await db.Pizzas.ToListAsync());

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
```

Si testi il programma con Swagger per verificarne il corretto funzionamento.

### Utilizzo del database Sqlite

Si modifichi l’applicazione, usando `SQLite` come Data Provider di `EntityFramework`. Per l'utilizzo di `SQLite` in combinazione con `EntityFramework`, occorre installare i seguenti pacchetti:

```ps1
# Aggiunta di Pacchetti NuGet
 dotnet add package Microsoft.EntityFrameworkCore.Sqlite
 dotnet add package Microsoft.EntityFrameworkCore.Design
```

Nel file `Program.cs` si modifichi il codice con le istruzioni indicate di seguito:

```cs

using Microsoft.EntityFrameworkCore;
using PizzaStoreV1.Data;
using PizzaStoreV1.Model;

var builder = WebApplication.CreateBuilder(args);
// 👈 si aggiunga la connection string
var connectionString = builder.Configuration.GetConnectionString("Pizzas") ?? "Data Source=DefaultPizzas.db";

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
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
// 👈 si aggiunga il DbContext per l'accesso al database
//builder.Services.AddDbContext<PizzaDb>(options => options.UseInMemoryDatabase("items"));  
// 👈 si modifichi il DbContext per l'accesso al database 
builder.Services.AddDbContext<PizzaDb>(options => options.UseSqlite(connectionString)); 
var app = builder.Build();


//app.UseHttpsRedirection();

//adding middleware for Swagger and OpenAPI
if (app.Environment.IsDevelopment())
{
	//adding middleware for OpenAPI
	app.MapOpenApi();
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
app.MapGet("/", () => "Hello World!");

app.MapGet("/pizzas", async (PizzaDb db) => await db.Pizzas.ToListAsync());

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
```

Si modifichi il file `appsettings.json`, con i campi per la connection string:

```json
{
  "ConnectionStrings": {
    "Pizzas": "Data Source=Pizzas.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

Si modifichi il file `PizzaDb.cs` per aggiungere il codice che popola il database in fase di migrazione. Questa parte non è strettamente necessaria, ma in questo caso è stata aggiunta al solo scopo di avere il database già popolato alla partenza dell'applicazione.

```cs
using Microsoft.EntityFrameworkCore;
using PizzaStoreV1.Model;

namespace PizzaStoreV1.Data;


// class PizzaDb : DbContext
// {
// 	public PizzaDb(DbContextOptions options) : base(options) { }
// 	public DbSet<Pizza> Pizzas { get; set; }=null!;
// }

//use Primary Constructor: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/primary-constructors
class PizzaDb(DbContextOptions options) : DbContext(options)
{
	public DbSet<Pizza> Pizzas { get; set; }=null!;
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Pizza>().HasData(
				new Pizza { Id = 1, Name = "Montemagno", Description = "Pizza shaped like a great mountain" },
				new Pizza { Id = 2, Name = "The Galloway", Description = "Pizza shaped like a submarine, silent but deadly" },
				new Pizza { Id = 3, Name = "The Noring", Description = "Pizza shaped like a Viking helmet, where's the mead" }
			);
	}

}
```

:memo: Per operare con `EntityFrameworkCore` da command line, occorre aver installato il pacchetto `dotnet-ef`, descritto nella [documentazione Microsoft](https://learn.microsoft.com/en-us/ef/core/cli/dotnet):

```ps1
dotnet tool install --global dotnet-ef
# nel caso in cui occorra installare una versione specifica, ad esempio la 8.0.11
dotnet tool install --global dotnet-ef --version 8.0.11  
# nel caso in cui occorra effettuare un aggiornamento di dotnet-ef
dotnet tool update --global dotnet-ef 
# nel caso in cui occorra effettuare la rimozione di dotnet-ef
dotnet tool uninstall dotnet-ef --global
```

Dopo aver salvato tutti i file (in VS Code è presente la funzione `Auto Save`), si effettui una migrazione e un aggiornamento del database.

```ps1
# Shell posizionata sulla cartella del progetto
dotnet ef migrations add InitialMigrate
dotnet ef database update  
```

A questo punto è possibile far partire nuovamente il progetto, per verificare che funzioni tutto.

### Uso del database MySQL/MariaDb - PizzaStore v2

Si crei una nuova versione del progetto `PizzaStore`, denominato `PizzaStoreV2`, all'interno della stessa soluzione. Per l'aggiunta del progetto alla soluzione si può procedere con le funzionalità di `C# Dev Kit`, oppure con i comandi della `.NET CLI`, come visto in precedenza. In entrambi i casi il template da utilizzare è `webapi`.

Si installino (dalla shell posizionata sulla cartella del nuovo progetto) i pacchetti:

:memo: :warning: :fire: **Attenzione**: Nel momento in cui si scrivono queste note la versione del pacchetto `Pomelo.EntityFrameworkCore.MySql` che funziona con `net9.0` è una versione di pre-release e, di conseguenza, anche i pacchetti relativi a `EntityFrameworkCore` devono essere portati ad una versione specifica, affinché `EF Core` possa correttamente effettuare la migrazione e l'aggiornamento del database.

```ps1
# 👈 serve per avere informazioni diagnostiche per EntityFrameworkCore
# è usato quando si utilizzano i middleware come:
#   app.UseDeveloperExceptionPage();
#   app.UseDatabaseErrorPage();
 dotnet add package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore

# 👈 serve per il supporto a OpenApi, ma se si utilizza il template webapi, oppure il Dev Kit per creare il progetto è già installato
 dotnet add package Microsoft.AspNetCore.OpenApi 

# 👈 serve per il supporto a Swagger
 dotnet add package NSwag.AspNetCore 

# 👈 serve per effettuare il Design, ossia il progetto del database, ad esempio, mediante una migrazione
 dotnet add package Microsoft.EntityFrameworkCore.Design

# 👈 serve per poter utilizzare MySQL oppure MariaDb come database provider di EF Core
# https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql
# https://learn.microsoft.com/en-us/ef/core/providers/?tabs=dotnet-core-cli
dotnet add package Pomelo.EntityFrameworkCore.MySql
```

Versione di pre-release dei pacchetti NuGet da utilizzare per integrare il pacchetto `Pomelo.EntityFrameworkCore.MySql` in `net9.0`

```ps1
 dotnet add package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore --version 9.0.0-preview.1.24081.5
 dotnet add package Microsoft.AspNetCore.OpenApi 
 dotnet add package NSwag.AspNetCore 
 dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.0-preview.1.24081.2
 dotnet add package Pomelo.EntityFrameworkCore.MySql --version 9.0.0-preview.1
```

Si copi il contenuto di `Program.cs` e delle cartelle `Model` e `Data` nel progetto `PizzaStoreV2`, modificando opportunamente i namespace, con il nome del nuovo progetto. A questo punto, occorre modificare la parte di codice relativa al database provider e sostituire le istruzioni relative a `Sqlite`, con quelle relative a `MariaDb`. Di seguito si riportano i file del progetto con le modifiche apportate.

File `appsettings.json`

```json
{
  "ConnectionStrings": {
    "PizzasV2": "Server=localhost;port=3306;Database=pizza_store;User Id=root;Password=root;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

```cs
//file PizzaDb.cs
using Microsoft.EntityFrameworkCore;
using PizzaStoreV2.Model;

namespace PizzaStoreV2.Data;
// class PizzaDb : DbContext
// {
// 	public PizzaDb(DbContextOptions options) : base(options) { }
// 	public DbSet<Pizza> Pizzas => Set<Pizza>();
// }

//use Primary Constructor: https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/primary-constructors
class PizzaDb(DbContextOptions options) : DbContext(options)
{
	public DbSet<Pizza> Pizzas { get; set; }=null!;
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Pizza>().HasData(
				new Pizza { Id = 1, Name = "Montemagno", Description = "Pizza shaped like a great mountain" },
				new Pizza { Id = 2, Name = "The Galloway", Description = "Pizza shaped like a submarine, silent but deadly" },
				new Pizza { Id = 3, Name = "The Noring", Description = "Pizza shaped like a Viking helmet, where's the mead" }
			);
	}

}
```

```cs
// file Pizza.cs
namespace PizzaStoreV2.Model;
public class Pizza
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
}
```

```cs
//file Program.cs
using Microsoft.EntityFrameworkCore;
using PizzaStoreV2.Model;
using PizzaStoreV2.Data;

var builder = WebApplication.CreateBuilder(args);
// 👇 si aggiunga la connection string
//var connectionString = builder.Configuration.GetConnectionString("Pizzas") ?? "Data Source=DefaultPizzas.db";
var connectionString = builder.Configuration.GetConnectionString("PizzasV2");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//AddOpenApi si può usare dalla versione 9 in poi di dotnet
builder.Services.AddOpenApi();
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
// 👇 si aggiunga il DbContext per l'accesso al database
//builder.Services.AddDbContext<PizzaDb>(options => options.UseInMemoryDatabase("items"));  
// 👇 si modifichi il DbContext per l'accesso al database 
//builder.Services.AddDbContext<PizzaDb>(options => options.UseSqlite(connectionString));

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
	app.MapOpenApi();
	//adding middleware for Swagger
	app.UseOpenApi();app.UseOpenApi();
	app.UseSwaggerUi(config =>

	{
		config.DocumentTitle = "PizzaStore API v1";
		config.Path = "/swagger";
		config.DocumentPath = "/swagger/{documentName}/swagger.json";
		config.DocExpansion = "list";
	});
}
//creations of API Endpoint Routes
app.MapGet("/", () => "Hello World!");
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
```

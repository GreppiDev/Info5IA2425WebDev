using Microsoft.EntityFrameworkCore;
using ProtectedAPI.Data;
using ProtectedAPI.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
//aggiunge il servizio che permette ad OpenAPI di leggere i metadati delle API
builder.Services.AddEndpointsApiExplorer();
//configura il servizio OpenAPI di NSwag
builder.Services.AddOpenApiDocument(config =>
	{
		config.Title = "ProtectedAPI v1";
		config.DocumentName = "ProtectedAPI API";
		config.Version = "v1";
	}
);
if (builder.Environment.IsDevelopment())
{
	builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
//accediamo alla stringa di connessione
var connectionString = builder.Configuration.GetConnectionString("ProtectedAPIConnection");
var serverVersion = ServerVersion.AutoDetect(connectionString);
//Aggiungiamo il servizio database tramite EF Core con MariaDB
builder.Services.AddDbContext<AppDbContext>(
	opt => opt.UseMySql(connectionString, serverVersion)
	.LogTo(Console.WriteLine, LogLevel.Information)
	.EnableSensitiveDataLogging()
	.EnableDetailedErrors()
	);
var app = builder.Build();

// Configure the HTTP request pipeline.
//qui mettiamo i middleware
if (app.Environment.IsDevelopment())
{
	//permette di ottenere una rotta dove vedere la documentazione delle API secondo lo standard OpenAPI
	app.MapOpenApi();
	//permette a Swagger (NSwag) di generare un file JSON con le specifiche delle API
	app.UseOpenApi();
	//permette di configurare l'interfaccia SwaggerUI (l'interfaccia grafica web di Swagger (NSwag) che permette di interagire con le API)
	app.UseSwaggerUi(config =>
	{
		config.DocumentTitle = "ProtectedAPI v1";
		config.Path = "/swagger";
		config.DocumentPath = "/swagger/{documentName}/swagger.json";
		config.DocExpansion = "list";
	});

	app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();


app
.MapGroup("/api").MapTodoEndpoints().WithOpenApi().WithTags("Todos API");

app.Run();



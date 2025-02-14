using Microsoft.EntityFrameworkCore;
using TodoApi.Endpoints;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
//aggiunge il servizio che permette ad OpenAPI di leggere i metadati delle API
builder.Services.AddEndpointsApiExplorer();
//configura il servizio OpenAPI
builder.Services.AddOpenApiDocument(config =>
	{
		config.Title = "Todo API con form e file v1";
		config.DocumentName = "Todo API con form e file";
		config.Version = "v1";
	}
);
if (builder.Environment.IsDevelopment())
{
	builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

builder.Services.AddDbContext<TodoDb>(opt =>
	opt.UseInMemoryDatabase("TodoList"));

var app = builder.Build();

// Configure the HTTP request pipeline.
//qui mettiamo i middleware
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
	//permette di ottenere una rotta dove vedere la documentazione delle API secondo lo standard OpenAPI
	app.MapOpenApi();
	//permette a Swagger (NSwag) di generare un file JSON con le specifiche delle API
	app.UseOpenApi();
	//permette di configurare l'interfaccia SwaggerUI (l'interfaccia grafica web di Swagger (NSwag) che permette di interagire con le API)
	app.UseSwaggerUi(config =>
	{
		config.DocumentTitle = "Todo API con form e file v1";
		config.Path = "/swagger";
		config.DocumentPath = "/swagger/{documentName}/swagger.json";
		config.DocExpansion = "list";
	});
}

// Serve static files after Swagger

// Middleware per file statici

// 1. Configura il middleware per servire index.html dalla cartella root
app.UseDefaultFiles();

// 2. Middleware per file statici in wwwroot (CSS, JS, ecc.)
app.UseStaticFiles();


if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}
//-------- Start of API management ------------
app
.MapGroup("")
.MapTodosEndpoints()
.WithOpenApi()
.WithTags("Public API");
//-------- End of API management ------------

//avvia l'applicazione
app.Run();



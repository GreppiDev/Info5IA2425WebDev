using FilmAPI.Data;
using FilmAPI.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
//aggiunge il servizio che permette ad OpenAPI di leggere i metadati delle API
builder.Services.AddEndpointsApiExplorer();
//configura il servizio OpenAPI
builder.Services.AddOpenApiDocument(config =>
	
	{
		config.Title = "FilmAPI di Malafronte v1";
		config.DocumentName= "Film API di Malafronte";
		config.Version = "v1";	
	}
);

//accediamo alla stringa di connessione
var connectionString = builder.Configuration.GetConnectionString("FilmAPIConnection");
var serverVersion = ServerVersion.AutoDetect(connectionString);
//Aggiungiamo il servizio database tramite EF Core con MariaDB
builder.Services.AddDbContext<FilmDbContext>(
	opt => opt.UseMySql(connectionString, serverVersion)
	.LogTo(Console.WriteLine, LogLevel.Information)
	.EnableSensitiveDataLogging()
	.EnableDetailedErrors()
	);
if(builder.Environment.IsDevelopment())
{
	builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
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
		config.DocumentTitle = "Film API di Malafronte v1";
		config.Path= "/swagger";
		config.DocumentPath = "/swagger/{documentName}/swagger.json";
		config.DocExpansion = "list";
	});
}

app.UseHttpsRedirection();
app.UseStaticFiles();
//--------------------ENDPOINTS management--------------------
app.MapRegistaEndpoints();
app.MapFilmEndpoints();
app.MapProiezioniEndpoints();
app.MapCinemaEndpoints();
//--------------------ENDPOINTS management--------------------

//----------- Start of page management ------------
//basic page routing
app.MapGet("", () => Results.Redirect("/index.html"));

app.MapGet("/{page}", (HttpContext context, string? page = "index.html") =>
{
	var filePath = Path.Combine("wwwroot", page!);
	if (!File.Exists(filePath))
	{
		//return Results.NotFound();
		//Andrebbe fatta una redirect alla pagina di errore.
		//In questo esempio, per semplicità, si fa una redirect alla index.html (home page)
		return Results.Redirect("/index.html");
	}

	var isTextFile = page!.EndsWith(".html") || page.EndsWith(".css") || page.EndsWith(".js") || page.EndsWith(".txt");

	if (isTextFile)
	{
		var content = File.ReadAllText(filePath);
		var contentType = page.EndsWith(".html") ? "text/html" :
						  page.EndsWith(".css") ? "text/css" :
						  page.EndsWith(".js") ? "application/javascript" :
						  "text/plain";
		return Results.Content(content, contentType);
	}
	else
	{
		var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
		var contentType = "application/octet-stream";
		return Results.File(fileStream, contentType, enableRangeProcessing: true);
	}
});
//----------- End of page management ------------

app.Run();



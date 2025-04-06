using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
//aggiunge il servizio che permette ad OpenAPI di leggere i metadati delle API
builder.Services.AddEndpointsApiExplorer();
//configura il servizio OpenAPI
builder.Services.AddOpenApiDocument(config =>
    {
        config.Title = "Auth Cookie Demo Frontend v1";
        config.DocumentName = "Auth Cookie Demo Frontend API";
        config.Version = "v1";
    }
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
        config.DocumentTitle = "Auth Cookie Demo Frontend v1";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });

    app.UseDeveloperExceptionPage();
}

//app.UseHttpsRedirection();

// Serve static files after Swagger

// Middleware per file statici

// 1. Configura il middleware per servire index.html dalla cartella pages alla root
app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.WebRootPath, "pages")
    ),
    RequestPath = ""
});

// 2. Middleware per file statici in wwwroot (CSS, JS, ecc.)
app.UseStaticFiles();

// 3. Middleware di fallback per cercare file in wwwroot/pages
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.WebRootPath, "pages")
    )
});

// Add this before app.Run()
app.MapGet("/api/config", (IConfiguration config) =>
{
    return Results.Ok(new
    {
        backendUrl = config["BackendApi:BaseUrl"]
    });
});

app.Run();



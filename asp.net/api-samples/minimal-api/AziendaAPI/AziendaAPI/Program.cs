using AziendaAPI.Data;
using AziendaAPI.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
//adding API explorer
builder.Services.AddEndpointsApiExplorer();
//adding OpenAPI configuration
// builder.Services.AddOpenApiDocument(config =>
// {
//     config.DocumentName = "AziendaAPIv1";
//     config.Title = "AziendaAPI v1";
//     config.Version = "v1";
// });
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "AziendaAPIv1";
    config.Title = "AziendaAPI v1";
    config.Version = "v1";

    config.PostProcess = document =>
      {
          var schemasToRename = document.Components.Schemas
              .Where(kv => kv.Key.EndsWith("DTO", StringComparison.OrdinalIgnoreCase))
              .ToList(); // Creiamo una lista temporanea delle chiavi da rinominare

          foreach (var schema in schemasToRename)
          {
              var originalName = schema.Key;
              var newName = originalName[..^3];

              if (!document.Components.Schemas.ContainsKey(newName)) // Evitiamo duplicati
              {
                  document.Components.Schemas.Add(newName, schema.Value);
              }

              document.Components.Schemas.Remove(originalName);
          }
      };
});
//adding services to the container
if (builder.Environment.IsDevelopment())
{
    //il servizio AddDatabaseDeveloperPageExceptionFilter andrebbe usato solo in fase di testing e non in produzione.
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
var connectionString = builder.Configuration.GetConnectionString("AziendaAPIConnection");
var serverVersion = ServerVersion.AutoDetect(connectionString);
builder.Services.AddDbContext<AziendaDbContext>(
        opt => opt.UseMySql(connectionString, serverVersion)
            // The following three options help with debugging, but should
            // be changed or removed for production.
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
    );

var app = builder.Build();
//app.UseHttpsRedirection();

//adding middleware for Swagger
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>

    {
        config.DocumentTitle = "AziendaAPI v1";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";

    });
}

using (var serviceScope = app.Services.CreateScope())

{
    var context = serviceScope.ServiceProvider.GetRequiredService<AziendaDbContext>();
    context.Database.Migrate();
    //vedere IN ALTERNATIVA al comando context.Database.Migrate() anche
    //context.Database.EnsureCreated();

}

//app.UseHttpsRedirection();
//app.MapGet("/", () => "Hello World!");
//I metodi seguenti verranno implementati in classi di estensione (descritte più avanti)
app.MapAziendaEndpoints();
app.MapProdottoEndpoints();
app.MapSviluppatoreEndpoints();
app.MapSviluppaProdottoEndpoints();

app.Run();

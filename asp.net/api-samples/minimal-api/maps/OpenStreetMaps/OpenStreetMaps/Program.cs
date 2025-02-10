using OpenStreetMaps.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Register token services
builder.Services.AddKeyedScoped<ITokenService, CesiumIonTokenService>("cesium");
builder.Services.AddKeyedScoped<ITokenService, ArcGISLocationPlatformTokenService>("arcgis");
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();


// Enable serving static files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// Cesium Ion token endpoint
app.MapGet("/api/maps/cesium-ion-token", ([FromKeyedServices("cesium")] ITokenService tokenService) =>
{
    try
    {
        var token = tokenService.GetToken();
        return Results.Ok(new { token });
    }
    catch (Exception)
    {
        return Results.StatusCode(500);
    }
})
.WithName("GetCesiumIonToken");

// ArcGIS Platform token endpoint
app.MapGet("/api/maps/arcgis-token", ([FromKeyedServices("arcgis")] ITokenService tokenService) =>
{
    try
    {
        var token = tokenService.GetToken();
        return Results.Ok(new { token });
    }
    catch (Exception)
    {
        return Results.StatusCode(500);
    }
})
.WithName("GetArcGISPlatformToken");

app.Run();

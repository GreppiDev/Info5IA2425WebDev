using MapTiler.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Register MapKeyService
builder.Services.AddScoped<IMapKeyService, MapTilerKeyService>();

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

// MapTiler key endpoint
app.MapGet("/api/maps/map-tiler-key", (IMapKeyService mapKeyService) =>
{
    try
    {
        var key = mapKeyService.GetMapTilerKey();
        return Results.Ok(new { key });
    }
    catch (Exception)
    {
        return Results.StatusCode(500);
    }
})
.WithName("GetMapTilerKey");

app.Run();

using OpenStreetMaps.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Register MapKeyService
builder.Services.AddScoped<ITokenService, CesiumIonTokenService>();
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
app.MapGet("/api/maps/cesium-ion-token", (ITokenService tokenService) =>
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

app.Run();

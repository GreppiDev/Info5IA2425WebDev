using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddAntiforgery(); // Aggiunge il servizio Anti-Forgery
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAntiforgery(); // Aggiunge middleware Anti-Forgery

// Endpoint per ottenere l'Anti-Forgery Token (GET)
app.MapGet("/antiforgery/token", (IAntiforgery antiforgery, HttpContext context) =>
{
	var tokens = antiforgery.GetAndStoreTokens(context);
	return Results.Text(tokens.RequestToken ?? ""); // Restituisce solo il RequestToken come testo
});

// Endpoint per ottenere l'Anti-Forgery Token (GET) - Stateless
app.MapGet("/antiforgery/token2", (IAntiforgery antiforgery, HttpContext context) =>
{
	var tokens = antiforgery.GetTokens(context); // Utilizziamo GetTokens invece di GetAndStoreTokens
	return Results.Text(tokens.RequestToken ?? ""); // Restituisce solo il RequestToken come testo
});

// Endpoint protetto da Anti-Forgery (POST)
app.MapPost("/api/submitData", async ([FromForm] string messaggio, IAntiforgery antiforgery, HttpContext context) =>
{
	try
	{
		await antiforgery.ValidateRequestAsync(context);
		return Results.Ok(new { message = $"Messaggio ricevuto: {messaggio} (Anti-Forgery validato)" });
	}

	catch (AntiforgeryValidationException)
	{
		return Results.BadRequest("Invalid anti-forgery token");
	}
	catch (Exception ex)
	{
		return Results.BadRequest($"Error during form processing: {ex.Message}");
	}
})
.Accepts<Dictionary<string, string>>("application/x-www-form-urlencoded"); // Specifica che accetta form URL-encoded

app.Run();


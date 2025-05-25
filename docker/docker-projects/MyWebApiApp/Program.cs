var builder = WebApplication.CreateBuilder(args);


var app = builder.Build();

// Configura la pipeline di richieste HTTP.

// Abilita il serving di file di default (es. index.html) dalla wwwroot
app.UseDefaultFiles();
// Abilita il serving di file statici (es. CSS, JS, immagini) dalla wwwroot
app.UseStaticFiles();

// Endpoint API di esempio
app.MapGet("/hello", () =>
{
    return Results.Ok(new { Message = "Ciao dal backend ASP.NET Core Minimal API!", Timestamp = DateTime.UtcNow });
});

// Endpoint per il test della connessione al database (verrà implementato meglio più avanti)
app.MapGet("/dbtest", (IConfiguration config) =>
{
    var connectionString = config.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
    {
        return Results.Text("Stringa di connessione 'DefaultConnection' non configurata.");
    }
    // Qui andrà la logica di test della connessione effettiva
    return Results.Text($"Tentativo di test con la stringa di connessione (da implementare): {connectionString}");
});

app.Run();

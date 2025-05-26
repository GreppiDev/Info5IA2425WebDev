// MyWebApiApp/Program.cs
using Microsoft.EntityFrameworkCore;
using MyWebApiApp.Data;
using MyWebApiApp.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Leggi la stringa di connessione da appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Aggiungi AppDbContext ai servizi, configurandolo per usare MariaDB/MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

var app = builder.Build();

// Inizializza il database utilizzando le migrazioni
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.Initialize(context);
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/hello", () =>
{
    return Results.Ok(new { Message = "Ciao dal backend ASP.NET Core Minimal API!", Timestamp = DateTime.UtcNow });
});

app.MapGet("/dbtest", async (AppDbContext dbContext, IConfiguration config) =>
{
    var currentConnectionString = config.GetConnectionString("DefaultConnection");
    var sb = new StringBuilder();
    sb.AppendLine("Risultato Test Connessione Database MariaDB con Entity Framework Core:");
    sb.AppendLine($"Stringa di connessione usata: {currentConnectionString}");

    if (string.IsNullOrEmpty(currentConnectionString))
    {
        sb.AppendLine("ERRORE: Stringa di connessione 'DefaultConnection' non trovata o vuota.");
        return Results.Text(sb.ToString());
    }

    try
    {
        // Testa la connessione al database (il database è già stato inizializzato al startup)
        sb.AppendLine("Testando la connessione al database...");

        // Tenta di inserire un nuovo record
        var newEntry = new TestEntry { Message = $"Test EF Core - {DateTime.UtcNow}" };
        dbContext.TestEntries.Add(newEntry);
        await dbContext.SaveChangesAsync();
        sb.AppendLine($"SUCCESSO: Record inserito con ID: {newEntry.Id}");

        // Tenta di leggere il record appena inserito (o l'ultimo)
        var retrievedEntry = await dbContext.TestEntries
                                    .OrderByDescending(e => e.Timestamp)
                                    .FirstOrDefaultAsync();

        if (retrievedEntry != null)
        {
            sb.AppendLine($"SUCCESSO: Record letto: ID={retrievedEntry.Id}, Messaggio='{retrievedEntry.Message}', Timestamp='{retrievedEntry.Timestamp}'");
        }
        else
        {
            sb.AppendLine("ATTENZIONE: Nessun record trovato dopo l'inserimento.");
        }

        // Tenta una query LINQ per contare i record
        int count = await dbContext.TestEntries.CountAsync();
        sb.AppendLine($"SUCCESSO: Ci sono {count} voci nella tabella 'TestEntries'.");

    }
    catch (Exception ex) // Cattura eccezioni generiche (MySqlException è inclusa)
    {
        sb.AppendLine($"ERRORE durante l'interazione con il database: {ex.GetType().Name} - {ex.Message}");
        sb.AppendLine("Stack Trace Parziale:");
        sb.AppendLine(ex.StackTrace?.Substring(0, Math.Min(ex.StackTrace.Length, 500)) + "..."); // Mostra solo una parte per brevità
        sb.AppendLine("\nControllare:");
        sb.AppendLine("1. Che il container 'mariadb-container' sia in esecuzione e sulla stessa rete Docker.");
        sb.AppendLine("2. Che il nome del server ('mariadb-container') e le credenziali nella connection string siano corretti.");
        sb.AppendLine("3. Che il database specificato esista e l'utente abbia i permessi.");
        sb.AppendLine("4. Che il provider Pomelo.EntityFrameworkCore.MySql sia configurato correttamente.");
    }
    return Results.Text(sb.ToString());
});

app.Run();

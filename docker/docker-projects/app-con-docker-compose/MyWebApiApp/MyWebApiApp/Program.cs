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

// Endpoint per creare una nuova voce in TestEntries
app.MapPost("/entries", async (AppDbContext dbContext, IHostEnvironment env) =>
{
    var response = new StringBuilder();
    response.AppendLine($"Risultato Creazione Voce (Ambiente: {env.EnvironmentName}):");
    try
    {
        // Crea una nuova istanza dell'entità TestEntry
        var newEntry = new TestEntry
        {
            Message = $"Voce creata via API ({env.EnvironmentName}) alle {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}"
            // Timestamp è già impostato su DateTime.UtcNow di default nel modello TestEntry
        };

        // Aggiungi la nuova entità al DbContext
        dbContext.TestEntries.Add(newEntry);

        // Salva le modifiche nel database
        await dbContext.SaveChangesAsync();

        response.AppendLine($"SUCCESSO: Record 'TestEntry' inserito con ID: {newEntry.Id}.");
        response.AppendLine($"Messaggio: '{newEntry.Message}'");
        response.AppendLine($"Timestamp: {newEntry.Timestamp:yyyy-MM-dd HH:mm:ss UTC}");
    }
    catch (DbUpdateException ex) // Cattura eccezioni specifiche di EF Core durante il salvataggio
    {
        response.AppendLine($"ERRORE DbUpdateException: Non è stato possibile salvare la voce nel database.");
        response.AppendLine($"Messaggio: {ex.Message}");
        if (ex.InnerException != null)
        {
            response.AppendLine($"Inner Exception: {ex.InnerException.Message}");
        }
    }
    catch (Exception ex) // Cattura altre eccezioni generiche
    {
        response.AppendLine($"ERRORE Generico: {ex.GetType().Name} - {ex.Message}");
    }
    return Results.Text(response.ToString());
});

// Endpoint per leggere tutte le voci da TestEntries (o le più recenti)
app.MapGet("/entries", async (AppDbContext dbContext, IHostEnvironment env) =>
{
    var entriesOutput = new StringBuilder();
    entriesOutput.AppendLine($"Voci nel database 'TestEntries' (Ambiente: {env.EnvironmentName}):\n");
    try
    {
        // Recupera le ultime 20 voci, ordinate dalla più recente
        var entries = await dbContext.TestEntries
                                     .OrderByDescending(e => e.Timestamp) // Ordina per timestamp decrescente
                                     .Take(20)                            // Prendi al massimo le ultime 20
                                     .ToListAsync();                     // Esegui la query e materializza i risultati

        if (!entries.Any()) // .Any() è più efficiente di .Count() > 0 se devi solo controllare l'esistenza
        {
            entriesOutput.Append("Nessuna voce trovata nella tabella 'TestEntries'.\n");
            entriesOutput.Append("Suggerimento: Puoi creare una nuova voce facendo una richiesta POST a /entries.\n");
        }
        else
        {
            foreach (var entry in entries)
            {
                entriesOutput.Append($"ID: {entry.Id}, Messaggio: \"{entry.Message}\", CreatoIl: {entry.Timestamp:yyyy-MM-dd HH:mm:ss UTC}\n");
            }
        }
    }
    catch (Exception ex) // Cattura eccezioni generiche (es. se il database non è raggiungibile)
    {
        entriesOutput.Append($"ERRORE durante la lettura delle voci: {ex.GetType().Name} - {ex.Message}\n");
        entriesOutput.Append("Verificare che il database sia accessibile e che le migrazioni siano state applicate correttamente.\n");
    }
    return Results.Text(entriesOutput.ToString());
});

app.Run();

using Microsoft.EntityFrameworkCore;
using MyWebApiApp.Models;

namespace MyWebApiApp.Data;

public static class DbInitializer
{
    public static async Task Initialize(AppDbContext context)
    {
        Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] DbInitializer: Avvio processo di inizializzazione database...");

        try
        {
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] DbInitializer: Tentativo di applicare migrazioni in sospeso...");
            // Applica le migrazioni al database.
            // Questo creerà il database se non esiste (a seconda del provider e della configurazione)
            // e applicherà tutte le migrazioni non ancora applicate.
            await context.Database.MigrateAsync();
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] DbInitializer: Migrazioni applicate con successo o database già aggiornato.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] DbInitializer: ERRORE CRITICO durante l'applicazione delle migrazioni: {ex.Message}");
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] Stack Trace: {ex.StackTrace}");
            // In un'applicazione di produzione, potresti voler terminare l'applicazione qui
            // se il database non è in uno stato utilizzabile.
            // throw; // Rilanciare l'eccezione fermerebbe l'avvio dell'app.
            return; // Esce dal metodo se le migrazioni falliscono, per evitare ulteriori problemi.
        }

        // --- SEEDING DEI DATI DI ESEMPIO ---
        // È buona pratica rendere il seeding condizionale, ad esempio basato sull'ambiente
        // (es. solo in 'Development'). Questo può essere fatto passando IHostEnvironment
        // o leggendo IConfiguration. Per ora, lo facciamo sempre se la tabella è vuota.

        try
        {
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] DbInitializer: Verifica esistenza dati in 'TestEntries' per il seeding...");
            if (!await context.TestEntries.AnyAsync())
            {
                Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] DbInitializer: La tabella 'TestEntries' è vuota. Aggiunta di un record di esempio...");
                var sampleEntry = new TestEntry
                {
                    // Assicurati che la tua classe TestEntry abbia un costruttore
                    // o che le proprietà siano inizializzabili.
                    // Il Message = null!; nel modello è per il compilatore C#,
                    // ma qui devi assegnare un valore valido.
                    Message = $"Primo record di esempio creato da DbInitializer alle {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}",
                    Timestamp = DateTime.UtcNow // O lascia che il default del modello lo gestisca se preferisci
                };

                context.TestEntries.Add(sampleEntry);
                await context.SaveChangesAsync();
                Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] DbInitializer: Record di esempio aggiunto con ID: {sampleEntry.Id}.");
            }
            else
            {
                Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] DbInitializer: La tabella 'TestEntries' contiene già dati. Nessun dato di esempio aggiunto.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] DbInitializer: ERRORE durante il seeding dei dati di esempio: {ex.Message}");
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] Stack Trace: {ex.StackTrace}");
            // Anche qui, considera come gestire questo errore.
            // Potrebbe non essere critico come il fallimento delle migrazioni.
        }

        Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] DbInitializer: Processo di inizializzazione database completato.");
    }
}

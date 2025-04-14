using AziendaAPI.Data;
using AziendaAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace AziendaAPI.Endpoints;

public static class SviluppaProdottiEndpoints
{
    public static void MapSviluppaProdottoEndpoints(this WebApplication app)
    {
        // ORIGINALE LINQ:
        // app.MapPost("/sviluppa-prodotto/{sviluppatoreId}/{prodottoId}", async (AziendaDbContext db, int sviluppatoreId, int prodottoId) => { ... controlli vari, Add, SaveChangesAsync ... });

        // CON SQL RAW
        app.MapPost("/sviluppa-prodotto/{sviluppatoreId}/{prodottoId}",
             async (AziendaDbContext db, int sviluppatoreId, int prodottoId) =>
             {
                 try
                 {
                     // 1. Recupera AziendaId del prodotto (se esiste)
                     int? prodottoAziendaId = await db.Database.SqlQuery<int?>(
                         $"SELECT AziendaId AS Value FROM Prodotti WHERE Id = {prodottoId}")
                         .FirstOrDefaultAsync();

                     // 2. Recupera AziendaId dello sviluppatore (se esiste)
                     int? sviluppatoreAziendaId = await db.Database.SqlQuery<int?>(
                          $"SELECT AziendaId AS Value FROM Sviluppatori WHERE Id = {sviluppatoreId}")
                          .FirstOrDefaultAsync();

                     // 3. Controlla esistenza usando null check (più sicuro di '== default')
                     if (prodottoAziendaId == null || sviluppatoreAziendaId == null)
                     {
                         return Results.NotFound("Prodotto o Sviluppatore non trovato.");
                     }

                     // 4. Controlla appartenenza alla stessa azienda
                     // Ora accediamo a .Value perché sono Nullable<int>
                     if (prodottoAziendaId.Value != sviluppatoreAziendaId.Value)
                     {
                         return Results.BadRequest($"Sviluppatore e prodotto non appartengono alla stessa azienda.");
                     }

                     // 5. Controlla se l'associazione esiste già
                     var relationExists = await db.Database.SqlQuery<int>(
                         $@"SELECT 1 FROM SviluppaProdotti
                           WHERE SviluppatoreId = {sviluppatoreId} AND ProdottoId = {prodottoId}
                           LIMIT 1")
                         .AnyAsync();

                     if (relationExists)
                     {
                         return Results.NoContent(); // Associazione già presente
                     }

                     // 6. Crea l'associazione
                     await db.Database.ExecuteSqlAsync(
                         $@"INSERT INTO SviluppaProdotti (SviluppatoreId, ProdottoId)
                           VALUES ({sviluppatoreId}, {prodottoId})");

                     return Results.NoContent(); // Creato con successo
                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine($"ERRORE POST /sviluppa-prodotto/{sviluppatoreId}/{prodottoId}: {ex}"); // Log
                     return Results.Problem($"Errore server durante la creazione dell'associazione: {ex.Message}");
                 }
             });

        // ORIGINALE LINQ:
        // app.MapDelete("/sviluppa-prodotto/{sviluppatoreId}/{prodottoId}", async (AziendaDbContext db, int sviluppatoreId, int prodottoId) => { ... controlli, Remove, SaveChangesAsync ...});

        // CON SQL RAW
        app.MapDelete("/sviluppa-prodotto/{sviluppatoreId}/{prodottoId}",
             async (AziendaDbContext db, int sviluppatoreId, int prodottoId) =>
             {
                 try
                 {
                     int rowsAffected = await db.Database.ExecuteSqlAsync(
                         $@"DELETE FROM SviluppaProdotti
                           WHERE SviluppatoreId = {sviluppatoreId} AND ProdottoId = {prodottoId}");

                     return rowsAffected > 0 ? Results.NoContent() : Results.NotFound();
                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine($"ERRORE DELETE /sviluppa-prodotto/{sviluppatoreId}/{prodottoId}: {ex}"); // Log
                     return Results.Problem($"Errore server durante l'eliminazione dell'associazione: {ex.Message}");
                 }
             });
    }
}
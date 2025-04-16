using AziendaAPI.Data;
using AziendaAPI.Model;
using AziendaAPI.ModelDTO;
using Microsoft.EntityFrameworkCore;

namespace AziendaAPI.Endpoints;

public static class ProdottoEndpoints
{
    public static void MapProdottoEndpoints(this WebApplication app)
    {
        // ORIGINALE LINQ con Include:
        // app.MapGet("/aziende/{id}/prodotti", async (AziendaDbContext db, int id) =>
        // {
        //     Azienda? azienda = await db.Aziende.Where(a => a.Id == id).Include(a => a.Prodotti).FirstOrDefaultAsync();
        //     if (azienda != null) { ... } else { return Results.NotFound(); }
        // });

        // CON SQL RAW (recuperando direttamente le entità Prodotto)
        app.MapGet("/aziende/{id}/prodotti", async (AziendaDbContext db, int id) =>
        {
            // Opzionale ma consigliato: Verificare prima se l'azienda esiste per un 404 più preciso
            var aziendaExists = await db.Database
                                        .SqlQuery<int>($"SELECT 1 FROM Aziende WHERE Id = {id} LIMIT 1").AnyAsync(); // Controlla se esiste almeno una riga

            if (!aziendaExists)
            {
                return Results.NotFound($"Azienda con id {id} non trovata.");
            }

            // Recupera i prodotti usando FromSql (restituisce entità Prodotto)
            var prodotti = await db.Prodotti
                                   .FromSql($"SELECT * FROM Prodotti WHERE AziendaId = {id}")
                                   .AsNoTracking() // Ottimo per query di sola lettura
                                   .ToListAsync();

            // Mappa le entità Prodotto recuperate in ProdottoDTO
            return Results.Ok(prodotti.Select(p => new ProdottoDTO(p)).ToList());
        });

        // ORIGINALE LINQ:
        // app.MapPost("/aziende/{id}/prodotti", async (AziendaDbContext db, int id, ProdottoDTO prodottoDTO) =>
        // {
        //     Azienda? azienda = await db.Aziende.FindAsync(id);
        //     if (azienda != null) { ... AddAsync, SaveChangesAsync ... } else { return Results.NotFound(); }
        // });

        // CON SQL RAW (INSERT + Get ID)
        app.MapPost("/aziende/{id}/prodotti", async (AziendaDbContext db, int id, ProdottoDTO prodottoDTO) =>
        {
            // 1. Verifica esistenza Azienda
            var aziendaExists = await db.Database
                                        .SqlQuery<int>($"SELECT 1 FROM Aziende WHERE Id = {id} LIMIT 1").AnyAsync();
            if (!aziendaExists)
            {
                return Results.NotFound($"Azienda con id {id} non trovata.");
            }

            // Validazione DTO base
            if (prodottoDTO == null || string.IsNullOrWhiteSpace(prodottoDTO.Nome))
                return Results.BadRequest("Dati prodotto non validi.");

            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                // 2. Esegui INSERT
                int rowsAffected = await db.Database.ExecuteSqlAsync(
                    $@"INSERT INTO Prodotti (Nome, Descrizione, AziendaId)
                       VALUES ({prodottoDTO.Nome}, {prodottoDTO.Descrizione}, {id})"); // Usa l'ID dalla route

                int generatedId = 0;
                if (rowsAffected > 0)
                {
                    // 3. Recupera ID generato
                    generatedId = await db.Database.SqlQuery<int>(
                        $"SELECT LAST_INSERT_ID() AS Value")
                        .SingleAsync();

                    await transaction.CommitAsync();

                    // Aggiorna DTO con ID e restituisci
                    prodottoDTO.Id = generatedId;
                    prodottoDTO.AziendaId = id; // Imposta anche AziendaId nel DTO restituito
                    return Results.Created($"/aziende/{id}/prodotti/{generatedId}", prodottoDTO);
                }
                else
                {
                    await transaction.RollbackAsync();
                    return Results.Problem("Creazione prodotto fallita (nessuna riga inserita).");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"ERRORE POST /aziende/{id}/prodotti: {ex}"); // Log
                return Results.Problem($"Errore server durante la creazione del prodotto: {ex.Message}");
            }
        });

        // ORIGINALE LINQ:
        // app.MapGet("/prodotti", async (AziendaDbContext db) => Results.Ok(await db.Prodotti.Select(x => new ProdottoDTO(x)).ToListAsync()));

        // CON SQL RAW (recuperando direttamente i DTO)
        app.MapGet("/prodotti", async (AziendaDbContext db) =>
        {
            // Seleziona direttamente le colonne necessarie per ProdottoDTO
            var prodottiDto = await db.Database.SqlQuery<ProdottoDTO>(
                $"SELECT Id, AziendaId, Nome, Descrizione FROM Prodotti")
                .ToListAsync();
            return Results.Ok(prodottiDto);
        });

        // ORIGINALE LINQ:
        // app.MapGet("/prodotti/{id}", async (AziendaDbContext db, int id) => { ... FindAsync ... });

        // CON SQL RAW (recuperando direttamente il DTO)
        app.MapGet("/prodotti/{id}", async (AziendaDbContext db, int id) =>
        {
            // Seleziona le colonne per ProdottoDTO filtrando per Id
            var prodottoDto = await db.Database.SqlQuery<ProdottoDTO>(
                $"SELECT Id, AziendaId, Nome, Descrizione FROM Prodotti WHERE Id = {id}")
                .FirstOrDefaultAsync(); // Usa FirstOrDefaultAsync per gestire il caso Not Found

            return prodottoDto is not null ? Results.Ok(prodottoDto) : Results.NotFound();
        });

        // ORIGINALE LINQ:
        // app.MapPut("/prodotti/{id}", async (AziendaDbContext db, ProdottoDTO updateProdotto, int id) => { ... FindAsync, update, SaveChangesAsync ...});

        // CON SQL RAW (UPDATE)
        app.MapPut("/prodotti/{id}", async (AziendaDbContext db, ProdottoDTO updateProdotto, int id) =>
        {
            // Validazione DTO
            if (updateProdotto == null || string.IsNullOrWhiteSpace(updateProdotto.Nome))
                return Results.BadRequest("Dati prodotto non validi.");

            // Esegui direttamente l'UPDATE
            int rowsAffected = await db.Database.ExecuteSqlAsync(
                $@"UPDATE Prodotti
                   SET Nome = {updateProdotto.Nome},
                       Descrizione = {updateProdotto.Descrizione}
                   WHERE Id = {id}");

            // Se rowsAffected è 0, l'ID non esisteva
            return rowsAffected > 0 ? Results.NoContent() : Results.NotFound();
        });

        // ORIGINALE LINQ:
        // app.MapDelete("/prodotti/{id}", async (AziendaDbContext db, int id) => { ... FindAsync, RemoveRange SviluppaProdotti, Remove Prodotto, SaveChangesAsync ... });

        // CON SQL RAW (DELETE con gestione dipendenza)
        app.MapDelete("/prodotti/{id}", async (AziendaDbContext db, int id) =>
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                // 1. Verifica esistenza Prodotto (Opzionale, il DELETE fallirebbe comunque, ma utile per 404)
                // Possiamo anche solo controllare rowsAffected del secondo DELETE
                bool prodottoExists = await db.Database.SqlQuery<int>($"SELECT 1 FROM Prodotti WHERE Id = {id} LIMIT 1").AnyAsync();
                if (!prodottoExists) return Results.NotFound();


                // 2. Elimina le righe dipendenti in SviluppaProdotti (necessario a causa di Restrict)
                // Non dà errore se non ci sono righe da eliminare
                await db.Database.ExecuteSqlAsync(
                    $"DELETE FROM SviluppaProdotti WHERE ProdottoId = {id}");

                // 3. Elimina il prodotto
                int rowsAffectedProdotto = await db.Database.ExecuteSqlAsync(
                    $"DELETE FROM Prodotti WHERE Id = {id}");

                if (rowsAffectedProdotto > 0)
                {
                    await transaction.CommitAsync();
                    // return Results.Ok(); // Ok è accettabile
                    return Results.NoContent(); // NoContent è più standard per DELETE
                }
                else
                {
                    // Se rowsAffectedProdotto è 0, significa che il prodotto non esisteva
                    await transaction.RollbackAsync();
                    return Results.NotFound();
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"ERRORE DELETE /prodotti/{id}: {ex}"); // Log
                return Results.Problem($"Errore server durante l'eliminazione del prodotto: {ex.Message}");
            }
        });
    }
}
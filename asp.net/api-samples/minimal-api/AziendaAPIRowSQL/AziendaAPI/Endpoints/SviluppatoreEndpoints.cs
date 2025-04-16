using AziendaAPI.Data;
using AziendaAPI.Model;
using AziendaAPI.ModelDTO;
using Microsoft.AspNetCore.Mvc; // Necessario per [FromQuery]
using Microsoft.EntityFrameworkCore;

namespace AziendaAPI.Endpoints;

public static class SviluppatoreEndpoints
{
    public static void MapSviluppatoreEndpoints(this WebApplication app)
    {
        // ORIGINALE LINQ:
        // app.MapGet("/sviluppatori", async (AziendaDbContext db) => Results.Ok(await db.Sviluppatori.Select(x => new SviluppatoreDTO(x)).ToListAsync()));

        // CON SQL RAW (recuperando direttamente DTO)
        app.MapGet("/sviluppatori", async (AziendaDbContext db) =>
        {
            // Seleziona direttamente le colonne per SviluppatoreDTO
            var sviluppatoriDto = await db.Database.SqlQuery<SviluppatoreDTO>(
                $"SELECT Id, AziendaId, Nome, Cognome FROM Sviluppatori")
                .ToListAsync();
            return Results.Ok(sviluppatoriDto);
        });

        // ORIGINALE LINQ:
        // app.MapPost("/aziende/{id}/sviluppatori", async (AziendaDbContext db, int id, SviluppatoreDTO sviluppatoreDTO) => { ... FindAsync Azienda, Add Sviluppatore, SaveChangesAsync ... });

        // CON SQL RAW (INSERT + Get ID)
        app.MapPost("/aziende/{id}/sviluppatori", async (AziendaDbContext db, int id, SviluppatoreDTO sviluppatoreDTO) =>
        {
            // 1. Verifica esistenza Azienda
             var aziendaExists = await db.Database
                                        .SqlQuery<int>($"SELECT 1 FROM Aziende WHERE Id = {id} LIMIT 1").AnyAsync();
            if (!aziendaExists)
            {
                return Results.NotFound($"Azienda con id {id} non trovata.");
            }

            // Validazione DTO base
            if (sviluppatoreDTO == null || string.IsNullOrWhiteSpace(sviluppatoreDTO.Nome) || string.IsNullOrWhiteSpace(sviluppatoreDTO.Cognome))
                return Results.BadRequest("Dati sviluppatore non validi.");

            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                // 2. Esegui INSERT
                int rowsAffected = await db.Database.ExecuteSqlAsync(
                    $@"INSERT INTO Sviluppatori (Nome, Cognome, AziendaId)
                       VALUES ({sviluppatoreDTO.Nome}, {sviluppatoreDTO.Cognome}, {id})"); // Usa id dalla route

                int generatedId = 0;
                if (rowsAffected > 0)
                {
                    // 3. Recupera ID generato
                    generatedId = await db.Database.SqlQuery<int>(
                        $"SELECT LAST_INSERT_ID() AS Value")
                        .SingleAsync();

                    await transaction.CommitAsync();

                    // Aggiorna DTO e restituisci
                    sviluppatoreDTO.Id = generatedId;
                    sviluppatoreDTO.AziendaId = id;
                    return Results.Created($"/aziende/{id}/sviluppatori/{generatedId}", sviluppatoreDTO);
                }
                else
                {
                     await transaction.RollbackAsync();
                    return Results.Problem("Creazione sviluppatore fallita (nessuna riga inserita).");
                }
            }
            catch (Exception ex)
            {
                 await transaction.RollbackAsync();
                 Console.WriteLine($"ERRORE POST /aziende/{id}/sviluppatori: {ex}"); // Log
                 return Results.Problem($"Errore server durante la creazione dello sviluppatore: {ex.Message}");
            }
        });


        // ORIGINALE LINQ (una delle versioni):
        // var listaSviluppatori = await db.Prodotti.Where(p => p.Id == id).SelectMany(p => p.Sviluppatori).ToListAsync();
        // return listaSviluppatori is not null? Results.Ok(listaSviluppatori.Select(s => new SviluppatoreDTO(s))) : Results.NotFound();

        // CON SQL RAW (JOIN per ottenere SviluppatoreDTO)
        app.MapGet("/prodotti/{id}/sviluppatori", async (AziendaDbContext db, int id) =>
        {
             // 1. Controlla se il prodotto esiste (per un NotFound più pulito)
             var prodottoExists = await db.Database
                                        .SqlQuery<int>($"SELECT 1 FROM Prodotti WHERE Id = {id} LIMIT 1").AnyAsync();
             if (!prodottoExists)
             {
                 return Results.NotFound($"Prodotto con id {id} non trovato.");
             }

             // 2. Esegui la query JOIN per ottenere gli sviluppatori associati
             var sviluppatoriDto = await db.Database.SqlQuery<SviluppatoreDTO>(
                 $@"SELECT s.Id, s.AziendaId, s.Nome, s.Cognome
                    FROM Sviluppatori s
                    INNER JOIN SviluppaProdotti sp ON s.Id = sp.SviluppatoreId
                    WHERE sp.ProdottoId = {id}")
                 .ToListAsync();

             // La query restituisce lista vuota se non ci sono sviluppatori, che è corretto.
             return Results.Ok(sviluppatoriDto);
        });


        // ORIGINALE LINQ (con logica condizionale):
        // app.MapGet("/aziende/{id}/sviluppatori", async (AziendaDbContext db, int id, [FromQuery(Name = "prodottoId")] int? prodottoId) => { ... logica if/else ... });

        // CON SQL RAW (con logica condizionale)
        app.MapGet("/aziende/{id}/sviluppatori",
            async (AziendaDbContext db, int id, [FromQuery(Name = "prodottoId")] int? prodottoId) =>
            {
                // Verifica esistenza azienda principale
                var aziendaExists = await db.Database
                                            .SqlQuery<int>($"SELECT 1 FROM Aziende WHERE Id = {id} LIMIT 1").AnyAsync();
                if (!aziendaExists)
                {
                    return Results.NotFound($"Azienda con id {id} non trovata.");
                }

                // CASO 1: Solo sviluppatori dell'azienda (prodottoId non fornito)
                if (prodottoId == null)
                {
                    var sviluppatoriAziendaDto = await db.Database.SqlQuery<SviluppatoreDTO>(
                        $"SELECT Id, AziendaId, Nome, Cognome FROM Sviluppatori WHERE AziendaId = {id}")
                        .ToListAsync();
                    return Results.Ok(sviluppatoriAziendaDto);
                }
                // CASO 2: Sviluppatori di un prodotto specifico di quell'azienda
                else
                {
					// Verifica che il prodotto esista e appartenga all'azienda specificata
					// ** CORREZIONE: Aggiungere "AS Value" all'alias della colonna **
					//EF Core (nella configurazione specifica versione/provider Pomelo) sta "avvolgendo" la query
					// SQL SELECT AziendaId FROM Prodotti WHERE Id = @p0 in una query esterna. 
					//Questa query esterna si aspetta che la colonna restituita dalla sottoquery si chiami Value
					int? prodottoAziendaId = await db.Database.SqlQuery<int?>(
							// Seleziona AziendaId e chiamalo 'Value'
							$"SELECT AziendaId AS Value FROM Prodotti WHERE Id = {prodottoId}")
							.FirstOrDefaultAsync(); // Restituisce null se non trovato, altrimenti l'AziendaId

					if (prodottoAziendaId == null)
					{
						return Results.NotFound($"Prodotto con id {prodottoId} non trovato.");
					}
					
					// Ora sappiamo che prodottoAziendaId ha un valore. Confrontiamolo con l'ID dell'azienda dalla route.
					// Accediamo a .Value perché prodottoAziendaId è un int? (Nullable<int>)
					if (prodottoAziendaId.Value != id)
					{
						return Results.BadRequest($"Il prodotto con id={prodottoId} non appartiene all'azienda con id={id}");
					}

					// Prodotto valido e appartenente all'azienda, esegui la JOIN
					var sviluppatoriProdottoDto = await db.Database.SqlQuery<SviluppatoreDTO>(
						$@"SELECT s.Id, s.AziendaId, s.Nome, s.Cognome
                       FROM Sviluppatori s
                       INNER JOIN SviluppaProdotti sp ON s.Id = sp.SviluppatoreId
                       WHERE sp.ProdottoId = {prodottoId} AND s.AziendaId = {id}")
						.ToListAsync();
					return Results.Ok(sviluppatoriProdottoDto);
				}
			});


        // ORIGINALE LINQ:
        // app.MapGet("/sviluppatori/{id}", async (AziendaDbContext db, int id) => { ... FindAsync ... });

        // CON SQL RAW (recuperando DTO)
        app.MapGet("/sviluppatori/{id}", async (AziendaDbContext db, int id) =>
        {
            var sviluppatoreDto = await db.Database.SqlQuery<SviluppatoreDTO>(
                $"SELECT Id, AziendaId, Nome, Cognome FROM Sviluppatori WHERE Id = {id}")
                .FirstOrDefaultAsync();

            return sviluppatoreDto is not null ? Results.Ok(sviluppatoreDto) : Results.NotFound();
        });

        // ORIGINALE LINQ:
        // app.MapPut("/sviluppatori/{id}", async (AziendaDbContext db, SviluppatoreDTO updateSviluppatore, int id) => { ... FindAsync, update, SaveChangesAsync ... });

        // CON SQL RAW (UPDATE)
        app.MapPut("/sviluppatori/{id}", async (AziendaDbContext db, SviluppatoreDTO updateSviluppatore, int id) =>
        {
             // Validazione DTO
            if (updateSviluppatore == null || string.IsNullOrWhiteSpace(updateSviluppatore.Nome) || string.IsNullOrWhiteSpace(updateSviluppatore.Cognome))
                return Results.BadRequest("Dati sviluppatore non validi.");

            int rowsAffected = await db.Database.ExecuteSqlAsync(
                $@"UPDATE Sviluppatori
                   SET Nome = {updateSviluppatore.Nome},
                       Cognome = {updateSviluppatore.Cognome}
                   WHERE Id = {id}");

            return rowsAffected > 0 ? Results.NoContent() : Results.NotFound();
        });

        // ORIGINALE LINQ:
        // app.MapDelete("/sviluppatori/{id}", async (AziendaDbContext db, int id) => { ... FindAsync, RemoveRange SviluppaProdotti, Remove Sviluppatore, SaveChangesAsync ...});

        // CON SQL RAW (DELETE con gestione dipendenza)
        app.MapDelete("/sviluppatori/{id}", async (AziendaDbContext db, int id) =>
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                 // 1. Elimina le righe dipendenti in SviluppaProdotti
                await db.Database.ExecuteSqlAsync(
                    $"DELETE FROM SviluppaProdotti WHERE SviluppatoreId = {id}");

                // 2. Elimina lo sviluppatore
                int rowsAffectedSviluppatore = await db.Database.ExecuteSqlAsync(
                    $"DELETE FROM Sviluppatori WHERE Id = {id}");

                if (rowsAffectedSviluppatore > 0)
                {
                    await transaction.CommitAsync();
                    return Results.NoContent();
                }
                else
                {
                    // Sviluppatore non trovato
                    await transaction.RollbackAsync();
                    return Results.NotFound();
                }
            }
             catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"ERRORE DELETE /sviluppatori/{id}: {ex}"); // Log
                return Results.Problem($"Errore server durante l'eliminazione dello sviluppatore: {ex.Message}");
            }
        });
    }
}
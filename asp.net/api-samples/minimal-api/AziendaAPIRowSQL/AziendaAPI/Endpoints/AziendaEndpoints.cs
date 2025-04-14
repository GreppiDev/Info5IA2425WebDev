using AziendaAPI.Data;
using AziendaAPI.Model;
using AziendaAPI.ModelDTO;
using Microsoft.EntityFrameworkCore;

namespace AziendaAPI.Endpoints;

//gestione aziende
public static class AziendaEndpoints
{
    public static void MapAziendaEndpoints(this WebApplication app)
    {
        // VERSIONE CON LINQ
        // app.MapGet("/aziende",
        //     async (AziendaDbContext db)
        //     => Results.Ok(await db.Aziende.Select(x => new AziendaDTO(x)).ToListAsync()));

        // VERSIONE CON SQL INTERPOLATO
        app.MapGet("/api/aziende", async (AziendaDbContext db) =>
        {
            // Seleziona direttamente le colonne necessarie per AziendaDTO
            var aziendeDto = await db.Database.SqlQuery<AziendaDTO>(
                $"SELECT Id, Nome, Indirizzo FROM Aziende")
                .ToListAsync();
            return Results.Ok(aziendeDto);
        });

        // VERSIONE CON LINQ
        // app.MapPost("/aziende",
        //     async (AziendaDbContext db, AziendaDTO aziendaDTO) =>
        // {
        //     var azienda = new Azienda { Nome = aziendaDTO.Nome, Indirizzo = aziendaDTO.Indirizzo };
        //     await db.Aziende.AddAsync(azienda);
        //     await db.SaveChangesAsync();
        //     return Results.Created($"/aziende/{azienda.Id}", new AziendaDTO(azienda));
        // });

        // VERSIONE CON SQL INTERPOLATO
        app.MapPost("/api/aziende", async (AziendaDbContext db, AziendaDTO aziendaDTO) =>
        {
            // Validazione...
            if (aziendaDTO == null) return Results.BadRequest("Dati azienda mancanti.");

            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                // 1. Esegui l'INSERT
                int rowsAffected = await db.Database.ExecuteSqlAsync(
                    $@"INSERT INTO Aziende (Nome,Indirizzo)
                    VALUES ({aziendaDTO.Nome},{aziendaDTO.Indirizzo})");

                int generatedId = 0;

                if (rowsAffected > 0)
                {
                    // 2. Recupera l'ID con l'alias corretto
                    generatedId = await db.Database.SqlQuery<int>(
                        $"SELECT LAST_INSERT_ID() AS Value") // <-- AS Value serve perché EF Core nel caso di query che restituiscono uno scalare effettua wrapping della query in una query esterna su cui invoca il campo Value
                        .SingleAsync();

                    aziendaDTO.Id = generatedId;

                    await transaction.CommitAsync();

                    return Results.Created($"/api/aziende/{generatedId}", aziendaDTO);
                }
                else
                {
                    await transaction.RollbackAsync();
                    return Results.Problem("Creazione fallita (nessuna riga inserita).");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Logga l'errore ex (importante!)
                Console.WriteLine($"ERRORE: {ex.ToString()}"); // Log di base per debug
                return Results.Problem($"Errore durante la creazione: {ex.Message}");
            }
        });

        // VERSIONE CON LINQ
        // app.MapGet("/aziende/{id}", async (AziendaDbContext db, int id) =>
        // await db.Aziende.FindAsync(id)
        //  is Azienda azienda
        //             ? Results.Ok(new AziendaDTO(azienda))
        //             : Results.NotFound()
        // );

        // VERSIONE CON SQL INTERPOLATO
        app.MapGet("/api/aziende/{id:int}", async (int id, AziendaDbContext db) =>
        {
            var aziendaDto = await db.Database.SqlQuery<AziendaDTO>(
                    $"SELECT Id, Nome, Indirizzo FROM Aziende WHERE Id = {id}")
                    .FirstOrDefaultAsync(); // O SingleOrDefaultAsync
            return aziendaDto is not null ? Results.Ok(aziendaDto) : Results.NotFound();
        });

        // VERSIONE CON LINQ
        // app.MapPut("/aziende/{id}", async (AziendaDbContext db, AziendaDTO updateAzienda, int id) =>
        // {
        //     var azienda = await db.Aziende.FindAsync(id);
        //     if (azienda is null) return Results.NotFound();
        //     azienda.Nome = updateAzienda.Nome;
        //     azienda.Indirizzo = updateAzienda.Indirizzo;
        //     await db.SaveChangesAsync();
        //     return Results.NoContent();
        // });

        // VERSIONE CON SQL INTERPOLATO
        app.MapPut("/api/aziende/{id:int}", async (AziendaDbContext db, AziendaDTO updateAzienda, int id) =>
        {
            // Validazione DTO...
            if (updateAzienda == null || string.IsNullOrWhiteSpace(updateAzienda.Nome))
                return Results.BadRequest("Dati azienda non validi.");

            int rowsAffected = await db.Database.ExecuteSqlAsync(
                $@"UPDATE Aziende
                SET Nome = {updateAzienda.Nome}, Indirizzo = {updateAzienda.Indirizzo}
                WHERE Id = {id}");

            // Se rowsAffected è 0, considera l'entità come non trovata.
            return rowsAffected > 0 ? Results.NoContent() : Results.NotFound();
        });

        // VERSIONE CON LINQ
        // app.MapDelete("/azienda/{id}", async (AziendaDbContext db, int id) =>
        // {
        //     var azienda = await db.Aziende.FindAsync(id);
        //     if (azienda is null)
        //     {
        //         return Results.NotFound();
        //     }
        //     db.Aziende.Remove(azienda);
        //     await db.SaveChangesAsync();
        //     return Results.Ok();
        // });

        // VERSIONE CON SQL INTERPOLATO
        app.MapDelete("/api/aziende/{id:int}", async (int id, AziendaDbContext context) =>
        {
            int rowsAffected = await context.Database.ExecuteSqlAsync( // Usa ExecuteSqlAsync (interpolato)
                $"DELETE FROM Aziende WHERE Id = {id}");
            return rowsAffected > 0 ? Results.NoContent() : Results.NotFound();
        });
    }

}

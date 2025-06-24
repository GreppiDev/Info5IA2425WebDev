using Microsoft.EntityFrameworkCore;
using EducationalGames.Data;
using EducationalGames.ModelsDTO;
using System.Security.Claims;
using EducationalGames.Models;

namespace EducationalGames.Endpoints;

public static class ClassificheEndpoints
{
    public static RouteGroupBuilder MapClassificheEndpoints(this RouteGroupBuilder group, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("EducationalGames.Endpoints.Classifiche");

        // GET /api/classifiche/classe/{idClasse}/gioco/{idGioco}
        // Restituisce la classifica per un gioco specifico in una classe
        group.MapGet("/classifiche/classe/{idClasse:int}/gioco/{idGioco:int}", async (
            int idClasse,
            int idGioco,
            AppDbContext db,
            HttpContext ctx) =>
        {
            logger.LogInformation("Recupero classifica per Gioco {GiocoId} in Classe {ClasseId}", idGioco, idClasse);

            var userIdString = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId)) return Results.Unauthorized();

            // Verifica se l'utente (studente o docente) ha accesso a questa classe
            var haAccessoClasse = await db.Iscrizioni.AnyAsync(i => i.StudenteId == userId && i.ClasseId == idClasse) ||
                                  await db.ClassiVirtuali.AnyAsync(c => c.Id == idClasse && c.DocenteId == userId) ||
                                  ctx.User.IsInRole(nameof(RuoloUtente.Admin));

            if (!haAccessoClasse)
            {
                logger.LogWarning("Accesso negato a classifica classe {ClasseId} per utente {UserId}", idClasse, userId);
                return Results.Forbid();
            }

            try
            {
                var classifica = await db.ProgressiStudenti
                    .AsNoTracking()
                    .Where(p => p.ClasseId == idClasse && p.GiocoId == idGioco)
                    .Include(p => p.Studente) // Include i dati dello studente
                    .OrderByDescending(p => p.MoneteRaccolte) // Ordina per monete
                    .ThenBy(p => p.Studente.Cognome) // Ordinamento secondario
                    .ThenBy(p => p.Studente.Nome)
                    .Select(p => new ClassificaEntryDto(
                        p.StudenteId,
                        $"{p.Studente.Nome} {p.Studente.Cognome}", // Nome completo
                        p.MoneteRaccolte
                    ))
                    .ToListAsync();

                return Results.Ok(classifica);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore recupero classifica per Gioco {GiocoId} in Classe {ClasseId}", idGioco, idClasse);
                return Results.Problem("Errore nel recupero della classifica.", statusCode: 500);
            }
        })
        .WithName("GetClassificaGioco")
        .Produces<List<ClassificaEntryDto>>()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        // Richiede Docente della classe o Studente iscritto (o Admin)
        .RequireAuthorization(); // La logica di accesso è verificata manualmente sopra


        // GET /api/classifiche/classe/{idClasse}
        // Restituisce la classifica generale per una classe (somma monete di tutti i giochi)
        group.MapGet("/classifiche/classe/{idClasse:int}", async (
            int idClasse,
            AppDbContext db,
            HttpContext ctx) =>
        {
            logger.LogInformation("Recupero classifica generale per Classe {ClasseId}", idClasse);

            var userIdString = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId)) return Results.Unauthorized();

            // Verifica accesso alla classe (come sopra)
            var haAccessoClasse = await db.Iscrizioni.AnyAsync(i => i.StudenteId == userId && i.ClasseId == idClasse) ||
                                  await db.ClassiVirtuali.AnyAsync(c => c.Id == idClasse && c.DocenteId == userId) ||
                                  ctx.User.IsInRole(nameof(RuoloUtente.Admin));
            if (!haAccessoClasse)
            {
                logger.LogWarning("Accesso negato a classifica classe {ClasseId} per utente {UserId}", idClasse, userId);
                return Results.Forbid();
            }

            try
            {
                // Step 1: Aggregate progress by student ID in the database
                var progressiAggregati = await db.ProgressiStudenti
                    .AsNoTracking()
                    .Where(p => p.ClasseId == idClasse)
                    .GroupBy(p => p.StudenteId) // Group only by StudenteId
                    .Select(g => new
                    {
                        StudenteId = g.Key,
                        // Sum requires casting uint to a larger type (like long) for SQL translation, then cast back
                        TotalMonete = (uint)g.Sum(p => (long)p.MoneteRaccolte)
                    })
                    .ToListAsync(); // Execute aggregation query

                // Step 2: Get the list of student IDs from the aggregated results
                var studenteIds = progressiAggregati.Select(p => p.StudenteId).ToList();

                // Step 3: Fetch student details for the relevant IDs
                var studenti = await db.Utenti
                    .AsNoTracking()
                    .Where(u => studenteIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.Nome, u.Cognome }) // Select only needed fields
                    .ToDictionaryAsync(u => u.Id); // Create a dictionary for efficient lookup

                // Step 4: Combine aggregated progress with student details in memory
                var classificaGenerale = progressiAggregati
                    .Select(p =>
                    {
                        // Lookup student details
                        var studente = studenti.TryGetValue(p.StudenteId, out var s) ? s : null;
                        return new ClassificaEntryDto(
                            p.StudenteId,
                            studente != null ? $"{studente.Nome} {studente.Cognome}" : "Studente Sconosciuto", // Handle potential missing student
                            p.TotalMonete
                        );
                    })
                    .OrderByDescending(entry => entry.Monete) // Sort the final list in memory
                    .ThenBy(entry => entry.NomeStudente)
                    .ToList();

                return Results.Ok(classificaGenerale);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore recupero classifica generale per Classe {ClasseId}", idClasse);
                return Results.Problem("Errore nel recupero della classifica generale.", statusCode: 500);
            }
        })
        .WithName("GetClassificaGeneraleClasse")
        .Produces<List<ClassificaEntryDto>>()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .RequireAuthorization(); // La logica di accesso è verificata manually sopra

        return group;
    }
}

using Microsoft.EntityFrameworkCore;
using EducationalGames.Data;
using EducationalGames.ModelsDTO;
using EducationalGames.Models;
using System.Security.Claims;

namespace EducationalGames.Endpoints;

public static class ProgressoEndpoints
{
    public static RouteGroupBuilder MapProgressoEndpoints(this RouteGroupBuilder group, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("EducationalGames.Endpoints.Progresso");

        // POST /api/progressi - Salva o aggiorna il progresso di uno studente per un gioco in una classe
        group.MapPost("/progressi", async (
            AppDbContext db,
            HttpContext ctx,
            AggiornaProgressoDto progressoDto) =>
        {
            logger.LogInformation("Tentativo aggiornamento progresso da utente {UserId} per Gioco {GiocoId} in Classe {ClasseId}",
                ctx.User.FindFirstValue(ClaimTypes.NameIdentifier), progressoDto.GiocoId, progressoDto.ClasseId);

            // 1. Recupera ID studente e verifica ruolo
            var studenteIdString = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(studenteIdString, out var studenteId)) return Results.Unauthorized();
            if (!ctx.User.IsInRole(nameof(RuoloUtente.Studente))) return Results.Forbid();

            // 2. Validazione DTO automatica

            // 3. Verifica iscrizione studente alla classe
            var iscrizioneValida = await db.Iscrizioni
                                           .AnyAsync(i => i.StudenteId == studenteId && i.ClasseId == progressoDto.ClasseId);
            if (!iscrizioneValida)
            {
                logger.LogWarning("Aggiornamento progresso fallito: Studente {StudenteId} non iscritto a Classe {ClasseId}", studenteId, progressoDto.ClasseId);
                return Results.Problem(statusCode: StatusCodes.Status403Forbidden, title: "Non autorizzato", detail: "Non sei iscritto a questa classe.");
            }

            // 4. Verifica associazione gioco alla classe e ottieni MaxMonete
            var giocoInfo = await db.ClassiGiochi
                                    .Where(cg => cg.ClasseId == progressoDto.ClasseId && cg.GiocoId == progressoDto.GiocoId)
                                    .Select(cg => new { cg.Gioco.MaxMonete }) // Seleziona solo MaxMonete
                                    .FirstOrDefaultAsync();

            if (giocoInfo == null)
            {
                logger.LogWarning("Aggiornamento progresso fallito: Gioco {GiocoId} non associato a Classe {ClasseId}", progressoDto.GiocoId, progressoDto.ClasseId);
                return Results.Problem(statusCode: StatusCodes.Status403Forbidden, title: "Gioco non valido", detail: "Questo gioco non è disponibile in questa classe.");
            }

            // 5. Verifica che le monete raccolte non superino il massimo
            if (progressoDto.MoneteRaccolte > giocoInfo.MaxMonete)
            {
                logger.LogWarning("Aggiornamento progresso fallito: Monete {MoneteRaccolte} superano MaxMonete {MaxMonete} per Gioco {GiocoId} in Classe {ClasseId}",
                                  progressoDto.MoneteRaccolte, giocoInfo.MaxMonete, progressoDto.GiocoId, progressoDto.ClasseId);
                // Restituisci un BadRequest o un Problem
                return Results.ValidationProblem(new Dictionary<string, string[]> {
                        { nameof(AggiornaProgressoDto.MoneteRaccolte), new[] { $"Il punteggio non può superare il massimo di {giocoInfo.MaxMonete} monete per questo gioco." } }
                });
            }

            // 6. Esegui UPSERT (Update or Insert) del progresso
            try
            {
                var progressoEsistente = await db.ProgressiStudenti
                                                 .FirstOrDefaultAsync(p => p.StudenteId == studenteId &&
                                                                           p.GiocoId == progressoDto.GiocoId &&
                                                                           p.ClasseId == progressoDto.ClasseId);

                if (progressoEsistente != null)
                {
                    // Aggiorna solo se il nuovo punteggio è MIGLIORE (o uguale, a seconda della logica desiderata)
                    if (progressoDto.MoneteRaccolte >= progressoEsistente.MoneteRaccolte)
                    {
                        logger.LogInformation("Aggiornamento progresso esistente per Studente {StudenteId}, Gioco {GiocoId}, Classe {ClasseId}. Vecchio: {OldScore}, Nuovo: {NewScore}",
                                              studenteId, progressoDto.GiocoId, progressoDto.ClasseId, progressoEsistente.MoneteRaccolte, progressoDto.MoneteRaccolte);
                        progressoEsistente.MoneteRaccolte = progressoDto.MoneteRaccolte;
                        progressoEsistente.UltimoAggiornamento = DateTime.UtcNow; // EF potrebbe gestirlo con ValueGeneratedOnUpdate
                                                                                  // db.ProgressiStudenti.Update(progressoEsistente); // Non necessario se tracciato
                    }
                    else
                    {
                        logger.LogInformation("Nuovo punteggio {NewScore} inferiore a quello esistente {OldScore}. Progresso non aggiornato.",
                                                 progressoDto.MoneteRaccolte, progressoEsistente.MoneteRaccolte);
                        // Restituisci Ok o NoContent senza salvare
                        return Results.Ok(new { Message = "Punteggio precedente mantenuto." });
                    }
                }
                else
                {
                    // Inserisci nuovo record progresso
                    logger.LogInformation("Inserimento nuovo progresso per Studente {StudenteId}, Gioco {GiocoId}, Classe {ClasseId} con {MoneteRaccolte} monete.",
                                          studenteId, progressoDto.GiocoId, progressoDto.ClasseId, progressoDto.MoneteRaccolte);
                    var nuovoProgresso = new ProgressoStudente
                    {
                        StudenteId = studenteId,
                        GiocoId = progressoDto.GiocoId,
                        ClasseId = progressoDto.ClasseId,
                        MoneteRaccolte = progressoDto.MoneteRaccolte,
                        UltimoAggiornamento = DateTime.UtcNow
                    };
                    db.ProgressiStudenti.Add(nuovoProgresso);
                }

                await db.SaveChangesAsync();
                return Results.Ok(new { Message = "Progresso salvato con successo." }); // O Results.NoContent()
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore durante il salvataggio del progresso per Studente {StudenteId}, Gioco {GiocoId}, Classe {ClasseId}.",
                                studenteId, progressoDto.GiocoId, progressoDto.ClasseId);
                return Results.Problem("Errore durante il salvataggio del progresso.", statusCode: 500);
            }
        })
        .WithName("AggiornaProgresso")
        .Produces<object>(StatusCodes.Status200OK) // Messaggio successo
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest) // DTO non valido o monete > max
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden) // Non iscritto o gioco non valido
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .RequireAuthorization(policy => policy.RequireRole(nameof(RuoloUtente.Studente))); // Solo Studente

        return group;
    }
}
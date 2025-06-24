using Microsoft.EntityFrameworkCore;
using EducationalGames.Data;
using EducationalGames.ModelsDTO;
using EducationalGames.Models;
using System.Security.Claims;

namespace EducationalGames.Endpoints;

public static class DashboardEndpoints
{
    public static RouteGroupBuilder MapDashboardEndpoints(this RouteGroupBuilder group, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("EducationalGames.Endpoints.Dashboard");

        // GET /api/dashboard/docente - Recupera dati per la dashboard del docente
        group.MapGet("/dashboard/docente", async (AppDbContext db, HttpContext ctx) =>
        {
            logger.LogInformation("Recupero dati dashboard per docente loggato.");

            var docenteIdString = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(docenteIdString, out var docenteId))
            {
                logger.LogWarning("Accesso dashboard docente fallito: ID utente non trovato nei claims.");
                return Results.Unauthorized();
            }

            // Verifica ruolo (solo Docente può accedere a questa specifica dashboard)
            if (!ctx.User.IsInRole(nameof(RuoloUtente.Docente)))
            {
                logger.LogWarning("Accesso dashboard docente negato per utente {UserId} (non docente).", docenteId);
                return Results.Forbid();
            }

            try
            {
                // Query per le classi del docente (riutilizzabile)
                var classiQuery = db.ClassiVirtuali
                                    .Where(c => c.DocenteId == docenteId);

                // 1. Statistiche Generali
                int totaleClassi = await classiQuery.CountAsync();

                // Conta studenti distinti iscritti alle classi del docente
                int totaleStudentiDistinti = await db.Iscrizioni
                                                    .Where(i => classiQuery.Any(c => c.Id == i.ClasseId))
                                                    .Select(i => i.StudenteId)
                                                    .Distinct()
                                                    .CountAsync();

                // 2. Ultime Classi Create/Modificate (Esempio: le 5 più recenti per ID)
                var ultimeClassi = await classiQuery
                                        .OrderByDescending(c => c.Id) // Ordina per ID decrescente
                                        .Take(5)
                                        .Include(c => c.Materia)
                                        .Include(c => c.Iscrizioni) // Per contare gli iscritti
                                        .Include(c => c.Giochi) // Per contare i giochi
                                        .Select(c => new ClasseRiepilogoDto(
                                            c.Id,
                                            c.Nome,
                                            c.Materia.Nome,
                                            c.Iscrizioni.Count,
                                            c.Giochi.Count
                                        ))
                                        .ToListAsync();

                // 3. Studenti più Attivi (Esempio: i 5 con più monete totali nelle classi del docente)
                var studentiPiuAttivi = await db.ProgressiStudenti
                                            .Where(p => classiQuery.Any(c => c.Id == p.ClasseId)) // Filtra progressi delle classi del docente
                                            .Include(p => p.Studente)
                                            .GroupBy(p => new { p.StudenteId, p.Studente.Nome, p.Studente.Cognome })
                                            .Select(g => new
                                            {
                                                StudenteId = g.Key.StudenteId,
                                                NomeCompleto = $"{g.Key.Nome} {g.Key.Cognome}",
                                                MoneteTotali = (uint)g.Sum(p => p.MoneteRaccolte), // Somma monete
                                                UltimaAttivita = g.Max(p => (DateTime?)p.UltimoAggiornamento) // Data/ora ultimo progresso
                                            })
                                            .OrderByDescending(s => s.MoneteTotali) // Ordina per monete
                                            .ThenByDescending(s => s.UltimaAttivita) // Poi per attività recente
                                            .Take(5) // Prendi i primi 5
                                            .Select(s => new StudenteAttivitaDto(
                                                s.StudenteId,
                                                s.NomeCompleto,
                                                s.MoneteTotali,
                                                s.UltimaAttivita
                                            ))
                                            .ToListAsync();


                // Costruisci il DTO finale
                var dashboardDto = new DashboardDocenteDto(
                    totaleClassi,
                    totaleStudentiDistinti,
                    ultimeClassi,
                    studentiPiuAttivi
                );

                return Results.Ok(dashboardDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore recupero dati dashboard per docente {DocenteId}", docenteId);
                return Results.Problem("Errore nel recupero dei dati della dashboard.", statusCode: 500);
            }
        })
        .WithName("GetDashboardDocente")
        .Produces<DashboardDocenteDto>()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .RequireAuthorization(policy => policy.RequireRole(nameof(RuoloUtente.Docente))); // Solo Docente
        
        // Dashboard Studente
        group.MapGet("/dashboard/studente", async (AppDbContext db, HttpContext ctx) =>
        {
            var loggerStudente = loggerFactory.CreateLogger("EducationalGames.Endpoints.Dashboard.Studente");
            loggerStudente.LogInformation("Recupero dati dashboard per studente loggato.");

            var studenteIdString = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(studenteIdString, out var studenteId)) return Results.Unauthorized();
            if (!ctx.User.IsInRole(nameof(RuoloUtente.Studente))) return Results.Forbid();

            try
            {
                // Query per le iscrizioni dello studente
                var iscrizioniQuery = db.Iscrizioni
                                        .Where(i => i.StudenteId == studenteId);

                // 1. Statistiche Personali
                int classiIscritteCount = await iscrizioniQuery.CountAsync();

                // Somma monete totali
                uint moneteTotali = (uint)await db.ProgressiStudenti
                                            .Where(p => p.StudenteId == studenteId)
                                            .SumAsync(p => (long)p.MoneteRaccolte); // Somma come long per evitare overflow, poi cast a uint

                // Conta giochi completati (ipotizzando completato = raggiunto MaxMonete)
                // Questa query potrebbe essere pesante, valuta alternative se necessario
                // La query per calcolare i giochi completati potrebbe diventare pesante se ci sono molti progressi; 
                // in un'applicazione reale, si potrebbero usare campi denormalizzati o viste materializzate per ottimizzare.
                int giochiCompletatiCount = await db.ProgressiStudenti
                    .Where(p => p.StudenteId == studenteId)
                    .Include(p => p.Gioco) // Necessario per accedere a MaxMonete
                    .CountAsync(p => p.MoneteRaccolte >= p.Gioco.MaxMonete && p.Gioco.MaxMonete > 0); // Conta solo se MaxMonete > 0

                var statistiche = new StatistichePersonaliDto(moneteTotali, giochiCompletatiCount, classiIscritteCount);

                // 2. Classi Recenti (Esempio: le 5 più recenti a cui si è iscritto)
                var classiRecenti = await iscrizioniQuery
                    .OrderByDescending(i => i.DataIscrizione)
                    .Take(5)
                    .Include(i => i.Classe).ThenInclude(c => c.Materia)
                    .Include(i => i.Classe).ThenInclude(c => c.Docente)
                    .Include(i => i.Classe).ThenInclude(c => c.Giochi) // Giochi associati alla classe
                    .Include(i => i.Studente).ThenInclude(s => s.Progressi) // Progressi dello studente
                    .Select(i => new ClasseIscrittaRiepilogoDto(
                        i.ClasseId,
                        i.Classe.Nome,
                        $"{i.Classe.Docente.Nome} {i.Classe.Docente.Cognome}",
                        i.Classe.Giochi.Count, // Numero giochi totali nella classe
                                               // Conta i giochi completati DALLO STUDENTE IN QUESTA CLASSE
                        i.Classe.Giochi.Count(g =>
                            i.Studente.Progressi.Any(p => p.GiocoId == g.Id && p.ClasseId == i.ClasseId && p.MoneteRaccolte >= g.MaxMonete && g.MaxMonete > 0))
                    ))
                    .ToListAsync();

                // Costruisci DTO finale
                var dashboardDto = new DashboardStudenteDto(statistiche, classiRecenti);

                return Results.Ok(dashboardDto);
            }
            catch (Exception ex)
            {
                loggerStudente.LogError(ex, "Errore recupero dati dashboard per studente {StudenteId}", studenteId);
                return Results.Problem("Errore nel recupero dei dati della dashboard.", statusCode: 500);
            }

        })
        .WithName("GetDashboardStudente")
        .Produces<DashboardStudenteDto>()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .RequireAuthorization(policy => policy.RequireRole(nameof(RuoloUtente.Studente))); // Solo Studente

        // Aggiungere qui /api/admin/dashboard/stats per l'admin (Fase 9)

        return group;
    }
}
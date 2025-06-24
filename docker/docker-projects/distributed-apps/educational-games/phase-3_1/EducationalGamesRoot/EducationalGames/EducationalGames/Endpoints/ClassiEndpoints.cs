
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; 
using EducationalGames.Data;
using EducationalGames.ModelsDTO;
using EducationalGames.Models;

namespace EducationalGames.Endpoints;

public static class ClassiEndpoints
{
    // Helper per generare codice iscrizione univoco e sicuro
    private static string GeneraCodiceIscrizione(AppDbContext db)
    {
        const string chars = "ABCDEFGHIJKLMNPQRSTUVWXYZ123456789"; // Caratteri permessi
        const int length = 8; // Lunghezza codice
        var random = Random.Shared; // Usa Random.Shared per thread-safety
        string codice;
        bool esiste;
        int tentativi = 0;
        const int maxTentativi = 10; // Limite per evitare loop infiniti (improbabile)

        do
        {
            codice = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            // Verifica (raro) che il codice non esista già
            esiste = db.ClassiVirtuali.Any(c => c.CodiceIscrizione == codice);
            tentativi++;
        } while (esiste && tentativi < maxTentativi);

        if (esiste)
        {
            // Se dopo N tentativi troviamo ancora collisioni, lancia eccezione o genera codice più lungo
            throw new InvalidOperationException("Impossibile generare un codice di iscrizione univoco.");
        }
        return codice;
    }


    public static RouteGroupBuilder MapClassiEndpoints(this RouteGroupBuilder group, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("EducationalGames.Endpoints.Classi");

        // Endpoint per ottenere l'elenco delle materie disponibili
        group.MapGet("/materie", async (AppDbContext db) =>
        {
            logger.LogInformation("Recupero lista materie.");
            try
            {
                var materie = await db.Materie
                                      .OrderBy(m => m.Nome)
                                      .Select(m => new MateriaDto(m.Id, m.Nome))
                                      .ToListAsync();
                return Results.Ok(materie);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore nel recupero delle materie.");
                return Results.Problem("Errore nel recupero delle materie.", statusCode: 500);
            }
        })
        .WithName("GetMaterie")
        .Produces<List<MateriaDto>>()
        .ProducesProblem(500)
        // Richiede Docente o Admin (Admin potrebbe voler vedere le materie)
        .RequireAuthorization(policy => policy.RequireRole(nameof(RuoloUtente.Docente), nameof(RuoloUtente.Admin)));


        // Endpoint per creare una nuova classe virtuale
        group.MapPost("/classi", async (
            AppDbContext db,
            HttpContext ctx,
            CreaClasseDto creaClasseDto) => // Riceve il DTO dal corpo della richiesta
        {
            logger.LogInformation("Tentativo creazione nuova classe da utente {UserId}", ctx.User.FindFirstValue(ClaimTypes.NameIdentifier));

            // 1. Recupera l'ID del docente loggato dai claims
            var docenteIdString = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(docenteIdString, out var docenteId))
            {
                logger.LogWarning("Creazione classe fallita: ID docente non trovato nei claims.");
                return Results.Unauthorized();
            }

            // 2. Verifica che l'utente sia effettivamente un docente (già fatto da RequireAuthorization, ma doppia sicurezza)
            if (!ctx.User.IsInRole(nameof(RuoloUtente.Docente)))
            {
                logger.LogWarning("Utente {UserId} non autorizzato (non Docente) ha tentato di creare una classe.", docenteId);
                return Results.Forbid();
            }

            // 3. Validazione modello automatica (grazie a Minimal API e attributi DTO)

            // 4. Verifica che la materia esista
            var materia = await db.Materie.FindAsync(creaClasseDto.MateriaId);
            if (materia == null)
            {
                logger.LogWarning("Creazione classe fallita: Materia ID {MateriaId} non valida.", creaClasseDto.MateriaId);
                return Results.BadRequest(new { Errors = new { MateriaId = new[] { "Materia selezionata non valida." } } });
            }

            // 5. Verifica unicità nome classe PER DOCENTE
            var nomeClasseEsiste = await db.ClassiVirtuali.AnyAsync(c => c.DocenteId == docenteId && c.Nome == creaClasseDto.NomeClasse);
            if (nomeClasseEsiste)
            {
                logger.LogWarning("Creazione classe fallita: Docente {DocenteId} ha già una classe chiamata '{NomeClasse}'.", docenteId, creaClasseDto.NomeClasse);
                return Results.Conflict(new { Message = $"Hai già una classe chiamata '{creaClasseDto.NomeClasse}'." });
            }

            // 6. Genera codice iscrizione univoco
            string codiceIscrizione;
            try
            {
                codiceIscrizione = GeneraCodiceIscrizione(db);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "Errore generazione codice iscrizione.");
                return Results.Problem("Impossibile generare codice univoco, riprovare.", statusCode: 500);
            }


            // 7. Crea nuova classe
            var nuovaClasse = new ClasseVirtuale
            {
                Nome = creaClasseDto.NomeClasse,
                CodiceIscrizione = codiceIscrizione,
                DocenteId = docenteId,
                MateriaId = creaClasseDto.MateriaId
                // Le collection (Iscrizioni, ClassiGiochi, etc.) sono inizializzate vuote dal costruttore o da EF Core
            };

            // 8. Salva nel DB
            db.ClassiVirtuali.Add(nuovaClasse);
            try
            {
                await db.SaveChangesAsync();
                logger.LogInformation("Nuova classe '{NomeClasse}' (ID: {ClasseId}) creata da Docente {DocenteId}.", nuovaClasse.Nome, nuovaClasse.Id, docenteId);

                // 9. Restituisci la classe creata (come DTO)
                var classeDto = new ClasseDto(
                    nuovaClasse.Id,
                    nuovaClasse.Nome,
                    materia.Nome, // Usiamo il nome della materia recuperata
                    nuovaClasse.CodiceIscrizione,
                    0 // Numero iscritti iniziale è 0
                );
                // Usiamo CreatedAtRoute se abbiamo un endpoint GET per dettaglio classe
                // Altrimenti usiamo Created
                return Results.Created($"/api/classi/{nuovaClasse.Id}", classeDto); // URL fittizio per ora
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore durante il salvataggio della nuova classe '{NomeClasse}'.", nuovaClasse.Nome);
                return Results.Problem("Errore durante la creazione della classe.", statusCode: 500);
            }

        })
        .WithName("CreaClasse")
        .Produces<ClasseDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem() // Per errori DTO
        .Produces(StatusCodes.Status409Conflict) // Per nome classe duplicato
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .RequireAuthorization(policy => policy.RequireRole(nameof(RuoloUtente.Docente))); // Solo Docente


        // Endpoint per ottenere le classi create dal docente loggato
        group.MapGet("/classi/mie", async (AppDbContext db, HttpContext ctx) =>
        {
            logger.LogInformation("Recupero classi per docente loggato.");

            var docenteIdString = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(docenteIdString, out var docenteId))
            {
                logger.LogWarning("Recupero 'mie classi' fallito: ID docente non trovato nei claims.");
                return Results.Unauthorized();
            }

            try
            {
                var classi = await db.ClassiVirtuali
                                    .Where(c => c.DocenteId == docenteId)
                                    .Include(c => c.Materia) // Carica la materia associata
                                    .Include(c => c.Iscrizioni) // Carica le iscrizioni per contarle
                                    .OrderBy(c => c.Nome)
                                    .Select(c => new ClasseDto(
                                        c.Id,
                                        c.Nome,
                                        c.Materia.Nome, // Nome materia
                                        c.CodiceIscrizione,
                                        c.Iscrizioni.Count // Conta gli studenti iscritti
                                    ))
                                    .ToListAsync();

                return Results.Ok(classi);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore nel recupero delle classi per docente {DocenteId}", docenteId);
                return Results.Problem("Errore nel recupero delle classi.", statusCode: 500);
            }
        })
        .WithName("GetMieClassi")
        .Produces<List<ClasseDto>>()
        .ProducesProblem(500)
        .RequireAuthorization(policy => policy.RequireRole(nameof(RuoloUtente.Docente))); // Solo Docente

        // GET /api/classi/{idClasse} - Dettaglio Classe (con giochi)
        group.MapGet("/classi/{idClasse:int}", async (int idClasse, AppDbContext db, HttpContext ctx) =>
        {
            var loggerDetail = loggerFactory.CreateLogger("EducationalGames.Endpoints.Classi.Detail");
            loggerDetail.LogInformation("Recupero dettagli classe ID: {ClasseId}", idClasse);

            var userIdString = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId)) return Results.Unauthorized();

            // Include tutte le relazioni necessarie per il DTO
            var classe = await db.ClassiVirtuali
                                 .AsNoTracking() // Usiamo AsNoTracking per query di sola lettura
                                 .Include(c => c.Materia)
                                 .Include(c => c.Docente)
                                 .Include(c => c.Giochi) // Include GIOCHI associati tramite skip navigation
                                    .ThenInclude(g => g.Argomenti) // Include ARGOMENTI dei giochi
                                 .FirstOrDefaultAsync(c => c.Id == idClasse);

            if (classe == null) return Results.NotFound(new { Message = $"Classe con ID {idClasse} non trovata." });

            // Verifica ownership o ruolo Admin
            if (classe.DocenteId != userId && !ctx.User.IsInRole(nameof(RuoloUtente.Admin)))
            {
                loggerDetail.LogWarning("Accesso negato dettagli classe {ClasseId} per utente {UserId}", idClasse, userId);
                return Results.Forbid();
            }

            var classeDetailDto = new ClasseDetailDto(
                classe.Id,
                classe.Nome,
                classe.Materia.Nome,
                $"{classe.Docente.Nome} {classe.Docente.Cognome}",
                classe.CodiceIscrizione,
                classe.Giochi.Select(g => new GiocoDto( // Proietta i giochi associati
                    g.Id, g.Titolo, g.DescrizioneBreve, g.MaxMonete, g.Immagine1,
                    g.Argomenti.Select(a => new ArgomentoDto(a.Id, a.Nome)).ToList()
                )).OrderBy(g => g.Titolo).ToList() // Ordina giochi per titolo
            );

            return Results.Ok(classeDetailDto);
        })
        .WithName("GetClasseDetail") // Nome univoco per questo endpoint
        .Produces<ClasseDetailDto>()
        .ProducesProblem(StatusCodes.Status404NotFound) // Usa Problem per standardizzare
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .RequireAuthorization(policy => policy.RequireRole(nameof(RuoloUtente.Docente), nameof(RuoloUtente.Admin)));


        // POST /api/classi/{idClasse}/giochi - Associa un gioco alla classe
        group.MapPost("/classi/{idClasse:int}/giochi", async (
            int idClasse,
            AssociaGiocoDto assocDto, // Riceve { "giocoId": ID }
            AppDbContext db,
            HttpContext ctx) =>
        {
            var loggerAssocia = loggerFactory.CreateLogger("EducationalGames.Endpoints.Classi.AssociaGioco");
            var userIdString = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId)) return Results.Unauthorized();

            // Trova la classe includendo la collection dei giochi per verifica e modifica
            var classe = await db.ClassiVirtuali
                                 .Include(c => c.Giochi) // Importante includere per modificare la collection
                                 .FirstOrDefaultAsync(c => c.Id == idClasse);

            if (classe == null) return Results.NotFound(new { Message = $"Classe {idClasse} non trovata." });
            // Verifica ownership o ruolo Admin
            if (classe.DocenteId != userId && !ctx.User.IsInRole(nameof(RuoloUtente.Admin))) return Results.Forbid();

            var gioco = await db.Videogiochi.FindAsync(assocDto.GiocoId);
            if (gioco == null) return Results.ValidationProblem(new Dictionary<string, string[]> { { nameof(AssociaGiocoDto.GiocoId), new[] { $"Gioco ID {assocDto.GiocoId} non trovato." } } });

            if (classe.Giochi.Any(g => g.Id == assocDto.GiocoId))
            {
                loggerAssocia.LogInformation("Gioco {GiocoId} già associato a classe {ClasseId}.", assocDto.GiocoId, idClasse);
                return Results.NoContent(); // Idempotente
            }

            classe.Giochi.Add(gioco);
            try
            {
                await db.SaveChangesAsync();
                loggerAssocia.LogInformation("Gioco {GiocoId} associato a classe {ClasseId}.", assocDto.GiocoId, idClasse);
                return Results.NoContent(); // Successo
            }
            catch (Exception ex) { loggerAssocia.LogError(ex, "Errore associazione gioco."); return Results.Problem("Errore associazione gioco.", statusCode: 500); }
        })
        .WithName("AssociaGiocoClasse")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesValidationProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .RequireAuthorization(policy => policy.RequireRole(nameof(RuoloUtente.Docente), nameof(RuoloUtente.Admin)));

        // DELETE /api/classi/{idClasse}/giochi/{idGioco} - Rimuove associazione gioco-classe
        group.MapDelete("/classi/{idClasse:int}/giochi/{idGioco:int}", async (
            int idClasse,
            int idGioco,
            AppDbContext db,
            HttpContext ctx) =>
        {
            var loggerDissocia = loggerFactory.CreateLogger("EducationalGames.Endpoints.Classi.DissociaGioco");
            var userIdString = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId)) return Results.Unauthorized();

            var classe = await db.ClassiVirtuali.Include(c => c.Giochi).FirstOrDefaultAsync(c => c.Id == idClasse);
            if (classe == null) return Results.NotFound(new { Message = $"Classe {idClasse} non trovata." });
            if (classe.DocenteId != userId && !ctx.User.IsInRole(nameof(RuoloUtente.Admin))) return Results.Forbid();

            var giocoDaRimuovere = classe.Giochi.FirstOrDefault(g => g.Id == idGioco);
            if (giocoDaRimuovere == null)
            {
                loggerDissocia.LogInformation("Gioco {GiocoId} non associato a classe {ClasseId}.", idGioco, idClasse);
                return Results.NoContent(); // Idempotente
            }

            classe.Giochi.Remove(giocoDaRimuovere);
            try
            {
                await db.SaveChangesAsync();
                loggerDissocia.LogInformation("Gioco {GiocoId} dissociato da classe {ClasseId}.", idGioco, idClasse);
                return Results.NoContent();
            }
            catch (Exception ex) { loggerDissocia.LogError(ex, "Errore dissociazione gioco."); return Results.Problem("Errore dissociazione gioco.", statusCode: 500); }
        })
        .WithName("DissociaGiocoClasse")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .RequireAuthorization(policy => policy.RequireRole(nameof(RuoloUtente.Docente), nameof(RuoloUtente.Admin)));

        return group;
    }
}
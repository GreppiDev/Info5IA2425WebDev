using Microsoft.EntityFrameworkCore; 
using EducationalGames.Data;
using EducationalGames.ModelsDTO; 
using Microsoft.AspNetCore.Mvc; 
using EducationalGames.Models;
using System.Security.Claims;

namespace EducationalGames.Endpoints;

public static class GameEndpoints
{
    public static RouteGroupBuilder MapGameEndpoints(this RouteGroupBuilder group, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("EducationalGames.Endpoints.Games");

        // Endpoint per ottenere tutti gli argomenti
        group.MapGet("/argomenti", async (AppDbContext db) =>
        {
            logger.LogInformation("Recupero lista argomenti.");
            try
            {
                var argomenti = await db.Argomenti
                                        .OrderBy(a => a.Nome)
                                        .Select(a => new ArgomentoDto(a.Id, a.Nome)) // Proietta in DTO
                                        .ToListAsync();
                return Results.Ok(argomenti);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore nel recupero degli argomenti.");
                return Results.Problem("Errore nel recupero degli argomenti.", statusCode: 500);
            }
        })
        .WithName("GetArgomenti")
        .Produces<List<ArgomentoDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        // Richiede Docente o Admin per vedere gli argomenti
        .RequireAuthorization(policy => policy.RequireRole(nameof(RuoloUtente.Docente), nameof(RuoloUtente.Admin)));

        // Endpoint per ottenere i giochi, con filtro opzionale per argomento
        group.MapGet("/giochi", async (AppDbContext db, [FromQuery] int? argomentoId) =>
        {
            logger.LogInformation("Recupero lista giochi. Filtro argomentoId: {ArgomentoId}", argomentoId ?? -1);

            try
            {
                // Inizia la query includendo la skip navigation property Argomenti
                // Dichiariamo esplicitamente il tipo come IQueryable<Videogioco>
                IQueryable<Videogioco> query = db.Videogiochi
                                                 .Include(v => v.Argomenti); // Include direttamente gli argomenti

                // Applica filtro se argomentoId è fornito
                if (argomentoId.HasValue && argomentoId > 0)
                {
                    // Filtra usando la skip navigation property
                    query = query.Where(v => v.Argomenti.Any(a => a.Id == argomentoId.Value));
                }

                // Seleziona e proietta in DTO
                var giochi = await query
                                    .OrderBy(v => v.Titolo)
                                    .Select(v => new GiocoDto(
                                        v.Id,
                                        v.Titolo,
                                        v.DescrizioneBreve,
                                        v.MaxMonete, // Manteniamo uint
                                        v.Immagine1,
                                        // Proietta gli argomenti direttamente dalla navigation property inclusa
                                        v.Argomenti.Select(a => new ArgomentoDto(a.Id, a.Nome)).ToList()
                                    ))
                                    .ToListAsync();

                return Results.Ok(giochi);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore nel recupero dei giochi con filtro argomentoId: {ArgomentoId}", argomentoId ?? -1);
                return Results.Problem("Errore nel recupero dei giochi.", statusCode: 500);
            }
        })
        .WithName("GetGiochi")
        .Produces<List<GiocoDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        // Richiede Docente o Admin per vedere il catalogo giochi
        .RequireAuthorization(policy => policy.RequireRole(nameof(RuoloUtente.Docente), nameof(RuoloUtente.Admin)));

        // ENDPOINT PER OTTENERE DATI GIOCO (STUDENTE)
        group.MapGet("/giochi/{idGioco:int}/play", async (
            int idGioco,
            [FromQuery] int? classeId, // Richiede ID classe per contesto
            AppDbContext db,
            HttpContext ctx) =>
        {
            var loggerPlay = loggerFactory.CreateLogger("EducationalGames.Endpoints.Games.Play");
            loggerPlay.LogInformation("Richiesta dati gioco {GiocoId} per classe {ClasseId} da utente {UserId}",
                idGioco, classeId ?? -1, ctx.User.FindFirstValue(ClaimTypes.NameIdentifier));

            var studenteIdString = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(studenteIdString, out var studenteId)) return Results.Unauthorized();
            if (!ctx.User.IsInRole(nameof(RuoloUtente.Studente))) return Results.Forbid();
            if (!classeId.HasValue) return Results.BadRequest(new { Message = "ID della classe mancante." });

            // Verifica se lo studente è iscritto alla classe E se il gioco è associato a quella classe
            var accessoValido = await db.Iscrizioni
                .AnyAsync(i => i.StudenteId == studenteId && i.ClasseId == classeId.Value &&
                               i.Classe.Giochi.Any(g => g.Id == idGioco));

            if (!accessoValido)
            {
                loggerPlay.LogWarning("Accesso negato a gioco {GiocoId} in classe {ClasseId} per studente {StudenteId}", idGioco, classeId.Value, studenteId);
                return Results.Forbid();
            }

            // Recupera i dati del gioco necessari per il player
            var gioco = await db.Videogiochi
                                .AsNoTracking()
                                .Where(v => v.Id == idGioco)
                                .Select(v => new GiocoPlayDto( // Proietta nel DTO specifico
                                    v.Id,
                                    v.Titolo,
                                    v.MaxMonete,
                                    v.DefinizioneGioco, // Invia la definizione JSON
                                    null // In questo esempio assumiamo giochi interni, non URL esterni
                                ))
                                .FirstOrDefaultAsync();

            if (gioco == null) return Results.NotFound(new { Message = "Gioco non trovato." });

            // Se DefinizioneGioco è null o vuoto, potrebbe essere un errore di configurazione
            if (string.IsNullOrWhiteSpace(gioco.DefinizioneGioco))
            {
                loggerPlay.LogError("Definizione gioco mancante per Gioco ID {GiocoId}", idGioco);
                return Results.Problem("Contenuto del gioco non disponibile.", statusCode: 500);
            }

            return Results.Ok(gioco);
        })
        .WithName("GetGiocoPlayData")
        .Produces<GiocoPlayDto>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .RequireAuthorization(policy => policy.RequireRole(nameof(RuoloUtente.Studente))); // Solo Studente
        
        // --- Endpoint CRUD per Giochi (Solo Admin) ---

        // GET /api/admin/giochi/{id} - Dettaglio Gioco per Admin
        group.MapGet("/admin/giochi/{id:int}", async (int id, AppDbContext db) =>
        {
            var loggerAdmin = loggerFactory.CreateLogger("EducationalGames.Endpoints.Admin.Games");
            loggerAdmin.LogInformation("Recupero dettaglio gioco ID: {GiocoId}", id);
            var gioco = await db.Videogiochi
                                .Include(v => v.Argomenti)
                                .FirstOrDefaultAsync(v => v.Id == id);

            if (gioco == null) return Results.NotFound();

            // Usiamo un DTO dettagliato (da creare se non esiste)
            var giocoDetail = new GiocoDetailDto( // Assumendo esista GiocoDetailDto in ModelsDTO
                gioco.Id,
                gioco.Titolo,
                gioco.DescrizioneBreve,
                gioco.DescrizioneEstesa,
                gioco.MaxMonete,
                gioco.Immagine1,
                gioco.Immagine2,
                gioco.Immagine3,
                gioco.DefinizioneGioco,
                gioco.Argomenti.Select(a => new ArgomentoDto(a.Id, a.Nome)).ToList()
            );
            return Results.Ok(giocoDetail);
        })
        .WithName("GetGiocoDetailAdmin")
        .Produces<GiocoDetailDto>()
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization(policy => policy.RequireRole(nameof(RuoloUtente.Admin)));

        // POST /api/admin/giochi - Crea Nuovo Gioco
        group.MapPost("/admin/giochi", async (GiocoCreateUpdateDto nuovoGiocoDto, AppDbContext db) => // Usa DTO specifico per creazione/update
        {
            var loggerAdmin = loggerFactory.CreateLogger("EducationalGames.Endpoints.Admin.Games");
            loggerAdmin.LogInformation("Tentativo creazione nuovo gioco con titolo: {Titolo}", nuovoGiocoDto.Titolo);

            // Validazione modello automatica

            // Verifica unicità titolo
            if (await db.Videogiochi.AnyAsync(v => v.Titolo == nuovoGiocoDto.Titolo))
            {
                return Results.Conflict(new { Message = "Un gioco con questo titolo esiste già." });
            }

            var nuovoGioco = new Videogioco
            {
                Titolo = nuovoGiocoDto.Titolo,
                DescrizioneBreve = nuovoGiocoDto.DescrizioneBreve,
                DescrizioneEstesa = nuovoGiocoDto.DescrizioneEstesa,
                MaxMonete = nuovoGiocoDto.MaxMonete,
                Immagine1 = nuovoGiocoDto.Immagine1,
                Immagine2 = nuovoGiocoDto.Immagine2,
                Immagine3 = nuovoGiocoDto.Immagine3,
                DefinizioneGioco = nuovoGiocoDto.DefinizioneGioco
            };

            // Gestione Argomenti (se il DTO include una lista di ID Argomento)
            if (nuovoGiocoDto.ArgomentiId != null && nuovoGiocoDto.ArgomentiId.Any())
            {
                var argomentiEsistenti = await db.Argomenti
                                                 .Where(a => nuovoGiocoDto.ArgomentiId.Contains(a.Id))
                                                 .ToListAsync();
                nuovoGioco.Argomenti = argomentiEsistenti; // Associa gli argomenti trovati
            }

            db.Videogiochi.Add(nuovoGioco);
            await db.SaveChangesAsync();
            loggerAdmin.LogInformation("Nuovo gioco creato con ID: {GiocoId}", nuovoGioco.Id);

            // Restituisci il gioco creato (o un DTO) con status 201 Created
            var giocoDto = new GiocoDto(nuovoGioco.Id, nuovoGioco.Titolo, nuovoGioco.DescrizioneBreve, nuovoGioco.MaxMonete, nuovoGioco.Immagine1,
                                       nuovoGioco.Argomenti.Select(a => new ArgomentoDto(a.Id, a.Nome)).ToList());
            return Results.CreatedAtRoute("GetGiocoDetailAdmin", new { id = nuovoGioco.Id }, giocoDto);

        })
        .WithName("CreateGiocoAdmin")
        .Produces<GiocoDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status409Conflict)
        .ProducesValidationProblem() // Per errori DTO
        .RequireAuthorization(policy => policy.RequireRole(nameof(RuoloUtente.Admin)));

        // PUT /api/admin/giochi/{id} - Modifica Gioco Esistente
        group.MapPut("/admin/giochi/{id:int}", async (int id, GiocoCreateUpdateDto giocoModificatoDto, AppDbContext db) =>
        {
            var loggerAdmin = loggerFactory.CreateLogger("EducationalGames.Endpoints.Admin.Games");
            loggerAdmin.LogInformation("Tentativo modifica gioco ID: {GiocoId}", id);

            var giocoEsistente = await db.Videogiochi
                                       .Include(v => v.Argomenti) // Carica argomenti esistenti
                                       .FirstOrDefaultAsync(v => v.Id == id);

            if (giocoEsistente == null) return Results.NotFound();

            // Verifica unicità titolo (se cambiato)
            if (giocoEsistente.Titolo != giocoModificatoDto.Titolo &&
                 await db.Videogiochi.AnyAsync(v => v.Titolo == giocoModificatoDto.Titolo && v.Id != id))
            {
                return Results.Conflict(new { Message = "Un altro gioco con questo titolo esiste già." });
            }

            // Aggiorna proprietà
            giocoEsistente.Titolo = giocoModificatoDto.Titolo;
            giocoEsistente.DescrizioneBreve = giocoModificatoDto.DescrizioneBreve;
            giocoEsistente.DescrizioneEstesa = giocoModificatoDto.DescrizioneEstesa;
            giocoEsistente.MaxMonete = giocoModificatoDto.MaxMonete;
            giocoEsistente.Immagine1 = giocoModificatoDto.Immagine1;
            giocoEsistente.Immagine2 = giocoModificatoDto.Immagine2;
            giocoEsistente.Immagine3 = giocoModificatoDto.Immagine3;
            giocoEsistente.DefinizioneGioco = giocoModificatoDto.DefinizioneGioco;

            // Aggiorna Argomenti (rimuovi i vecchi non presenti, aggiungi i nuovi)
            var idArgomentiSelezionati = giocoModificatoDto.ArgomentiId ?? new List<int>();
            var argomentiDaAggiungere = await db.Argomenti
                                                .Where(a => idArgomentiSelezionati.Contains(a.Id) && !giocoEsistente.Argomenti.Any(ga => ga.Id == a.Id))
                                                .ToListAsync();
            var argomentiDaRimuovere = giocoEsistente.Argomenti
                                                     .Where(a => !idArgomentiSelezionati.Contains(a.Id))
                                                     .ToList();

            foreach (var arg in argomentiDaRimuovere) giocoEsistente.Argomenti.Remove(arg);
            foreach (var arg in argomentiDaAggiungere) giocoEsistente.Argomenti.Add(arg);

            await db.SaveChangesAsync();
            loggerAdmin.LogInformation("Gioco ID: {GiocoId} modificato.", id);
            return Results.NoContent(); // Successo standard per PUT
        })
        .WithName("UpdateGiocoAdmin")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict)
        .ProducesValidationProblem()
        .RequireAuthorization(policy => policy.RequireRole(nameof(RuoloUtente.Admin)));


        // DELETE /api/admin/giochi/{id} - Cancella Gioco
        group.MapDelete("/admin/giochi/{id:int}", async (int id, AppDbContext db) =>
        {
            var loggerAdmin = loggerFactory.CreateLogger("EducationalGames.Endpoints.Admin.Games");
            loggerAdmin.LogWarning("Tentativo cancellazione gioco ID: {GiocoId}", id); // Warning per azione distruttiva

            var gioco = await db.Videogiochi.FindAsync(id);
            if (gioco == null) return Results.NotFound();

            db.Videogiochi.Remove(gioco);
            await db.SaveChangesAsync(); // Le relazioni M:N verranno cancellate in cascade se configurato nel DbContext
            loggerAdmin.LogInformation("Gioco ID: {GiocoId} cancellato.", id);
            return Results.NoContent();
        })
        .WithName("DeleteGiocoAdmin")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .RequireAuthorization(policy => policy.RequireRole(nameof(RuoloUtente.Admin)));

        return group;
    }
}
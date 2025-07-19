using Microsoft.EntityFrameworkCore;
using EducationalGames.Data;
using EducationalGames.ModelsDTO;
using EducationalGames.Models;
using System.Security.Claims;


namespace EducationalGames.Endpoints;


    public static class IscrizioniEndpoints
    {
        public static RouteGroupBuilder MapIscrizioniEndpoints(this RouteGroupBuilder group, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("EducationalGames.Endpoints.Iscrizioni");

            // POST /api/iscrizioni - Iscrive lo studente loggato a una classe
            group.MapPost("/iscrizioni", async (
                AppDbContext db,
                HttpContext ctx,
                IscrivitiDto iscrivitiDto) =>
            {
                logger.LogInformation("Tentativo iscrizione a classe da utente {UserId}", ctx.User.FindFirstValue(ClaimTypes.NameIdentifier));

                // 1. Recupera ID studente e verifica ruolo
                var studenteIdString = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(studenteIdString, out var studenteId)) return Results.Unauthorized();
                if (!ctx.User.IsInRole(nameof(RuoloUtente.Studente)))
                {
                    logger.LogWarning("Utente {UserId} non autorizzato (non Studente) ha tentato l'iscrizione.", studenteId);
                    return Results.Forbid();
                }

                // 2. Validazione DTO automatica

                // 3. Trova la classe tramite codice iscrizione
                var classe = await db.ClassiVirtuali
                                     .FirstOrDefaultAsync(c => c.CodiceIscrizione == iscrivitiDto.CodiceIscrizione);

                if (classe == null)
                {
                    logger.LogWarning("Iscrizione fallita: Codice '{Codice}' non valido per utente {UserId}.", iscrivitiDto.CodiceIscrizione, studenteId);
                    // Restituisci un errore di validazione specifico per il campo
                    return Results.ValidationProblem(new Dictionary<string, string[]> {
                        { nameof(IscrivitiDto.CodiceIscrizione), new[] { "Codice iscrizione non valido o classe non trovata." } }
                    });
                }

                // 4. Verifica se lo studente è già iscritto a questa classe
                var giaIscritto = await db.Iscrizioni
                                          .AnyAsync(i => i.StudenteId == studenteId && i.ClasseId == classe.Id);
                if (giaIscritto)
                {
                    logger.LogInformation("Utente {UserId} già iscritto a classe {ClasseId}.", studenteId, classe.Id);
                    // Restituisce un Conflict con messaggio chiaro
                    return Results.Conflict(new { Message = $"Sei già iscritto alla classe '{classe.Nome}'." });
                }

                // 5. Crea la nuova iscrizione
                var nuovaIscrizione = new Iscrizione
                {
                    StudenteId = studenteId,
                    ClasseId = classe.Id,
                    DataIscrizione = DateTime.UtcNow // Imposta data corrente (anche se DB ha default)
                };

                // 6. Salva nel DB
                db.Iscrizioni.Add(nuovaIscrizione);
                try
                {
                    await db.SaveChangesAsync();
                    logger.LogInformation("Utente {UserId} iscritto con successo a classe {ClasseId} ('{NomeClasse}').", studenteId, classe.Id, classe.Nome);
                    // Restituisci 201 Created o 200 OK con messaggio
                    return Results.Ok(new { Message = $"Iscrizione alla classe '{classe.Nome}' avvenuta con successo!" });
                    // return Results.StatusCode(StatusCodes.Status201Created);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Errore durante salvataggio iscrizione per utente {UserId} a classe {ClasseId}.", studenteId, classe.Id);
                    return Results.Problem("Errore durante l'iscrizione alla classe.", statusCode: 500);
                }
            })
            .WithName("IscrivitiClasse")
            .Produces<object>(StatusCodes.Status200OK) // Messaggio successo
            //.Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest) // Codice non valido/mancante
            .ProducesProblem(StatusCodes.Status409Conflict) // Già iscritto
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .RequireAuthorization(policy => policy.RequireRole(nameof(RuoloUtente.Studente))); // Solo Studente


        // GET /api/iscrizioni/mie - Ottiene le classi a cui lo studente è iscritto
        group.MapGet("/iscrizioni/mie", async (AppDbContext db, HttpContext ctx) =>
       {
           logger.LogInformation("Recupero classi per studente loggato.");

           var studenteIdString = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
           if (!int.TryParse(studenteIdString, out var studenteId)) return Results.Unauthorized();
           // La verifica del ruolo è già gestita da RequireAuthorization, ma la lasciamo per doppia sicurezza
           if (!ctx.User.IsInRole(nameof(RuoloUtente.Studente))) return Results.Forbid();

           try
           {
               // 1. Recupera le iscrizioni con tutti i dati correlati necessari
               var iscrizioni = await db.Iscrizioni
                   .AsNoTracking()
                   .Where(i => i.StudenteId == studenteId)
                   .Include(i => i.Classe)
                       .ThenInclude(c => c.Materia)
                   .Include(i => i.Classe)
                       .ThenInclude(c => c.Docente)
                   .Include(i => i.Classe)
                       .ThenInclude(c => c.Giochi) // Include la relazione ClasseGioco
                            .ThenInclude(ga => ga.Argomenti) // Include l'Argomento tramite GiocoArgomento
                   .OrderBy(i => i.Classe.Nome) // Ordina le iscrizioni per nome classe
                   .ToListAsync(); // Esegui la query e porta i dati in memoria

               // 2. Proietta i risultati nei DTO (operazione eseguita in memoria)
               var classiIscritteDto = iscrizioni.Select(i => new ClasseIscrittaDto( // Proietta nel DTO
                            i.ClasseId,
                            i.Classe.Nome,
                            i.Classe.Materia.Nome,
                            $"{i.Classe.Docente.Nome} {i.Classe.Docente.Cognome}",
                            [.. i.Classe.Giochi.Select(g => new GiocoDto( // Proietta i giochi in GiocoDto
                                g.Id,
                                g.Titolo,
                                g.DescrizioneBreve,
                                g.MaxMonete,
                                g.Immagine1,
                                [.. g.Argomenti.Select(a => new ArgomentoDto(a.Id, a.Nome))]
                            )).OrderBy(g => g.Titolo)] // Ordina giochi
                        ));


               return Results.Ok(classiIscritteDto);
           }
           catch (Exception ex)
           {
               logger.LogError(ex, "Errore recupero classi iscritte per studente {StudenteId}", studenteId);
               return Results.Problem("Errore nel recupero delle tue classi.", statusCode: 500);
           }
       })
       .WithName("GetMieIscrizioni")
       .Produces<List<ClasseIscrittaDto>>()
       .ProducesProblem(StatusCodes.Status401Unauthorized)
       .ProducesProblem(StatusCodes.Status403Forbidden)
       .ProducesProblem(StatusCodes.Status500InternalServerError)
       .RequireAuthorization(policy => policy.RequireRole(nameof(RuoloUtente.Studente))); // Solo Studente


        return group;
        }
    }


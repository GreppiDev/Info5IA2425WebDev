using Microsoft.EntityFrameworkCore;
using EducationalGames.Data;
using EducationalGames.ModelsDTO;
using EducationalGames.Models;

namespace EducationalGames.Endpoints;

public static class AdminEndpoints
{
    public static RouteGroupBuilder MapAdminEndpoints(this RouteGroupBuilder group, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger("EducationalGames.Endpoints.Admin");

        // --- Endpoint per Statistiche Dashboard Admin ---
        group.MapGet("/dashboard/stats", async (AppDbContext db) =>
        {
            logger.LogInformation("Recupero statistiche dashboard admin.");
            try
            {
                // Esegui le query Count() SEQUENZIALMENTE
                var totalUtenti = await db.Utenti.CountAsync();
                var totalDocenti = await db.Utenti.CountAsync(u => u.Ruolo == RuoloUtente.Docente);
                var totalStudenti = await db.Utenti.CountAsync(u => u.Ruolo == RuoloUtente.Studente);
                var totalClassi = await db.ClassiVirtuali.CountAsync();
                var totalGiochi = await db.Videogiochi.CountAsync();
                var totalArgomenti = await db.Argomenti.CountAsync();
                var totalMaterie = await db.Materie.CountAsync();

                // Costruisci il DTO con i risultati
                var stats = new DashboardAdminDto(
                    totalUtenti,
                    totalDocenti,
                    totalStudenti,
                    totalClassi,
                    totalGiochi,
                    totalArgomenti,
                    totalMaterie
                );

                return Results.Ok(stats);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore recupero statistiche dashboard admin.");
                return Results.Problem("Errore nel recupero delle statistiche.", statusCode: 500);
            }
        })
        .WithName("GetAdminDashboardStats")
        .Produces<DashboardAdminDto>()
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .RequireAuthorization(policy => policy.RequireRole(nameof(RuoloUtente.Admin))); // SOLO Admin

        // --- Endpoint CRUD Opzionali (da implementare qui se si sceglie di farlo) ---

        // Esempio: GET /api/admin/utenti
        group.MapGet("/utenti", async (AppDbContext db) =>
        {
            // TODO: Implementare logica per recuperare utenti con paginazione/filtri
            // Esempio semplice:
            var utenti = await db.Utenti
                                .OrderBy(u => u.Cognome).ThenBy(u => u.Nome)
                                .Select(u => new { u.Id, u.Nome, u.Cognome, u.Email, u.Ruolo, u.EmailVerificata }) // Seleziona solo dati non sensibili
                                .ToListAsync();
            return Results.Ok(utenti);
        }).RequireAuthorization(policy => policy.RequireRole(nameof(RuoloUtente.Admin)));

        // Aggiungere qui GET /utenti/{id}, PUT /utenti/{id}, DELETE /utenti/{id}
        // Aggiungere qui CRUD per Giochi, Argomenti, Materie se necessario

        // Ricorda che POST /api/account/admin/create-user è già in AccountEndpoints.cs

        return group;
    }
}
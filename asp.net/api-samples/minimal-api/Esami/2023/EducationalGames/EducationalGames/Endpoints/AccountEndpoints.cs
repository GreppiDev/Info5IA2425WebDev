using System;
using System.Security.Claims;
using EducationalGames.Utils;
using EducationalGames.Data;
using EducationalGames.ModelsDTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using EducationalGames.Models;
using Microsoft.EntityFrameworkCore;

namespace EducationalGames.Endpoints;

public static class AccountEndpoints
{
    public static RouteGroupBuilder MapAccountEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/login", async (AppDbContext db, HttpContext ctx, LoginModel loginModel, [FromQuery] string? returnUrl) =>
        {
            var hasher = new PasswordHasher<Utente>();
            // Cerca l'utente nel database tramite email (ignorando maiuscole/minuscole)
            // --- CORREZIONE ERRORE EF CORE ---
            // Modificato il confronto per essere traducibile in SQL da EF Core MySQL.
            // Si convertono entrambe le email in minuscolo per un confronto case-insensitive.
            var user = await db.Utenti.FirstOrDefaultAsync(u => u.Email.ToLower() == loginModel.Email.ToLower());
            
            // Se l'utente non esiste, restituisce Unauthorized
            if (user is null)
            {
                // Si potrebbe voler restituire un messaggio generico per motivi di sicurezza
                return Results.Problem("Credenziali non valide.", statusCode: StatusCodes.Status401Unauthorized);
                //oppure 
                //return Results.Unauthorized();
            }

            // Verifica l'hash della password fornita con quello memorizzato nel database
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, loginModel.Password);

            // Se la password non corrisponde, restituisce Unauthorized
            if (result == PasswordVerificationResult.Failed)
            {
                return Results.Problem("Credenziali non valide.", statusCode: StatusCodes.Status401Unauthorized);
                //oppure
                //return Results.Unauthorized();
            }

            // --- Gestione dei Ruoli in Cascata ---
            // Ottiene i ruoli in cascata basati sul ruolo dell'utente dal database
            var cascadedRoles = RoleUtils.GetCascadedRoles(user.Ruolo); // Usa il metodo che restituisce string[]

            // --- Creazione dei Claims ---
            var claims = new List<Claim>
            {
                // Claim per l'identificativo dell'utente (spesso l'email o un ID univoco)
                new(ClaimTypes.NameIdentifier, user.Id.ToString()), // Aggiunto ID utente come NameIdentifier
                new(ClaimTypes.Name, user.Email), // Claim per il nome utente (email in questo caso)
                new(ClaimTypes.GivenName, user.Nome), // Claim per il nome
                new(ClaimTypes.Surname, user.Cognome), // Claim per il cognome
                // Aggiungere un claim per il ruolo "principale" dell'utente (opzionale se si usano i ruoli in cascata come in questo caso)
                // new Claim(ClaimTypes.Role, user.Ruolo.ToString())
            };

            // Aggiunge un claim di tipo Role per *ciascun* ruolo in cascata
            foreach (var role in cascadedRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // --- Creazione dell'Identità e del Principal ---
            // Crea l'identità dell'utente basata sui claims e sullo schema di autenticazione (Cookie)
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            // Crea il principal che rappresenta l'utente autenticato
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // --- Autenticazione ---
            // Controlla se l'utente ha richiesto "Ricordami"
            AuthenticationProperties? authProperties = null;
            if (loginModel.RememberMe)
            {
                authProperties = new AuthenticationProperties
                {
                    // Rende il cookie persistente (sopravvive alla chiusura del browser)
                    IsPersistent = true,
                    // Imposta una scadenza assoluta per il cookie (es. 14 giorni)
                    // SlidingExpiration (impostato nel Program.cs) rinnoverà questa scadenza
                    // se l'utente è attivo prima che scada.
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14),
                    // AllowRefresh è generalmente implicito quando si usa SlidingExpiration nel middleware,
                    // ma può essere impostato esplicitamente se necessario per chiarezza o casi specifici.
                    AllowRefresh = true
                };
            }

            // Effettua il login dell'utente creando il cookie di autenticazione.
            // Passa le proprietà di autenticazione se l'utente ha scelto "Ricordami",
            // altrimenti viene creato un cookie di sessione (non persistente).
            await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties); // Passiamo authProperties qui


            // --- Redirect ---
            // Reindirizza l'utente alla returnUrl se fornita, altrimenti alla home page
            // Assicurarsi che returnUrl sia un URL locale per prevenire attacchi di open redirect
            if (!string.IsNullOrEmpty(returnUrl) && Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
            {
                return Results.Redirect(returnUrl);
            }
            else
            {
                return Results.Redirect("/"); //redirect alla home oppure a un'altra pagina di default
            }

        }).AllowAnonymous(); // Permette accesso anonimo al login

        // --- Endpoint di Registrazione Pubblica (Docente/Studente) ---
        group.MapPost("/register", async (AppDbContext db, RegisterModel model) =>
        {
            // 1. Validazione del Modello (automatica con Minimal API + attributi)
            //    Se il modello non è valido, Minimal API restituisce automaticamente Bad Request.

            // 2. Controllo esplicito sul ruolo
            if (model.Ruolo == RuoloUtente.Admin)
            {
                return Results.BadRequest("La registrazione come Admin non è permessa.");
            }

            // 3. Verifica se l'email esiste già
            var existingUser = await db.Utenti.AnyAsync(u => u.Email == model.Email);
            if (existingUser)
            {
                // Usiamo Problem per dare più dettagli standardizzati sull'errore
                return Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Email già registrata.",
                    detail: "L'indirizzo email fornito è già associato a un account."
                );
            }

            // 4. Hashing della password
            var hasher = new PasswordHasher<Utente>();
            var newUser = new Utente
            {
                Nome = model.Nome,
                Cognome = model.Cognome,
                Email = model.Email,
                Ruolo = model.Ruolo // Ruolo fornito (Docente o Studente)
                                    // PasswordHash verrà impostato sotto
            };
            newUser.PasswordHash = hasher.HashPassword(newUser, model.Password);

            // 5. Salvataggio nel database
            db.Utenti.Add(newUser);
            await db.SaveChangesAsync();

            // 6. Restituisce Ok o Created
            // Si potrebbe restituire l'utente creato (senza password hash) o solo Ok
            return Results.Ok(new { Message = "Registrazione completata con successo." });

        }).AllowAnonymous(); // Permette accesso anonimo alla registrazione

        // --- Endpoint per Creazione Utenti da parte dell'Admin ---
        group.MapPost("/admin/create-user", async (AppDbContext db, AdminCreateUserModel model) =>
        {
            // 1. Validazione del Modello (automatica)

            // 2. Verifica se l'email esiste già
            var existingUser = await db.Utenti.AnyAsync(u => u.Email == model.Email);
            if (existingUser)
            {
                return Results.Problem(
                       statusCode: StatusCodes.Status409Conflict,
                       title: "Email già registrata.",
                       detail: "L'indirizzo email fornito è già associato a un account."
                   );
            }

            // 3. Hashing della password
            var hasher = new PasswordHasher<Utente>();
            var newUser = new Utente
            {
                Nome = model.Nome,
                Cognome = model.Cognome,
                Email = model.Email,
                Ruolo = model.Ruolo // L'admin può specificare qualsiasi ruolo
            };
            newUser.PasswordHash = hasher.HashPassword(newUser, model.Password);

            // 4. Salvataggio nel database
            db.Utenti.Add(newUser);
            await db.SaveChangesAsync();

            // 5. Restituisce Ok o Created
            return Results.Ok(new { Message = $"Utente {model.Email} creato con successo con ruolo {model.Ruolo}." });

        }).RequireAuthorization("AdminOnly"); // SOLO gli Admin possono chiamare questo endpoint

        // --- Altri Endpoint (Profile, Admin-Area, Power-Area, My-Roles, Logout...) ---
        // ... Inserisci qui gli altri endpoint che hai già definito ...
        group.MapGet("/profile", (HttpContext ctx) =>
        {
            // Il check IsAuthenticated è ridondante grazie a RequireAuthorization
            return Results.Ok($"Benvenuto, {ctx.User.FindFirstValue(ClaimTypes.GivenName)} {ctx.User.FindFirstValue(ClaimTypes.Surname)} ({ctx.User.FindFirstValue(ClaimTypes.Name)})");
        }).RequireAuthorization(); // Richiede solo autenticazione generica

        group.MapGet("/admin-area", (HttpContext ctx) =>
        {
            return Results.Ok($"Benvenuto nell'area amministrativa, {ctx.User.FindFirstValue(ClaimTypes.GivenName)}");
        }).RequireAuthorization("AdminOnly"); // Usa la policy definita in Program.cs

        group.MapGet("/power-area", (HttpContext ctx) =>
        {
            return Results.Ok($"Benvenuto nell'area power, {ctx.User.FindFirstValue(ClaimTypes.GivenName)}");
        }).RequireAuthorization("AdminOrDocente"); // Usa la policy definita in Program.cs

        group.MapGet("/my-roles", (HttpContext ctx) =>
        {
            var roles = ctx.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return Results.Ok(new
            {
                Username = ctx.User.FindFirstValue(ClaimTypes.Name),
                Roles = roles,
                IsAdmin = ctx.User.IsInRole("Admin"),
                IsDocente = ctx.User.IsInRole("Docente"),
                IsStudente = ctx.User.IsInRole("Studente")
            });
        }).RequireAuthorization(); // Richiede autenticazione

        // --- Endpoint /logout ---
        group.MapPost("/logout", async (HttpContext ctx, [FromQuery] string? returnUrl) =>
        {
            // Esegui il logout rimuovendo il cookie
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Se la richiesta proviene da un endpoint API (es. chiamata da JavaScript SPA),
            // restituisce uno status code senza redirect.
            if (ctx.Request.Path.StartsWithSegments("/api"))
            {
                // HTTP 204 No Content è appropriato per un'azione POST/DELETE riuscita senza corpo di risposta
                return Results.NoContent();
                // In alternativa: return Results.Ok(new { message = "Logout successful" });
            }

            // Se la richiesta NON proviene da un'API (es. submit di un form HTML):
            // Prova a reindirizzare a returnUrl SE è fornito ED è un URL locale/sicuro.
            if (!string.IsNullOrEmpty(returnUrl))
            {
                // Results.LocalRedirect verifica internamente che l'URL sia locale
                // per prevenire vulnerabilità Open Redirect usando la validazione standard.
                // Questo è l'overload corretto del metodo.
                return Results.LocalRedirect(returnUrl);
            }

            // Se non c'è un returnUrl valido, reindirizza alla pagina principale (o altra pagina di default)
            return Results.Redirect("/");

        }).RequireAuthorization(); // È necessario essere loggati per poter fare logout

        return group;
    }
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EducationalGames.Data; 
using EducationalGames.Models; 
using EducationalGames.Utils; 

namespace EducationalGames.Auth;

public static class GoogleAuthEvents
{
    public static async Task HandleTicketReceived(TicketReceivedContext context)
    {
        // Ottieni servizi necessari
        var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        var hasher = context.HttpContext.RequestServices.GetRequiredService<PasswordHasher<Utente>>();
        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("EducationalGames.GoogleOnTicketReceived");

        logger.LogInformation(">>> [EVENT OnTicketReceived] Started processing Google callback.");

        // Controlla Principal
        if (context.Principal == null)
        {
            logger.LogError(">>> [EVENT OnTicketReceived] Principal is null.");
            // --- Gestione Errore Utente ---
            context.Response.Redirect("/login-failed.html?reason=internal_error_principal");
            context.HandleResponse();
            // --- Fine Gestione ---
            return;
        }

        // Estrai Claims
        var claims = context.Principal.Claims;
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var googleUserId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var givenName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? "Utente";
        var surname = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
        if (string.IsNullOrWhiteSpace(surname)) { surname = "-"; }
        logger.LogInformation(">>> [EVENT OnTicketReceived] External claims extracted: Email={Email}, GoogleUserId={GoogleUserId}", email, googleUserId);

        // Valida Claims
        if (string.IsNullOrEmpty(googleUserId) || string.IsNullOrEmpty(email))
        {
            logger.LogWarning(">>> [EVENT OnTicketReceived] Google authentication succeeded but missing required claims (UserId or Email).");
            // --- Gestione Errore Utente ---
            context.Response.Redirect("/login-failed.html?reason=google_missing_claims");
            context.HandleResponse();
            // --- Fine Gestione ---
            return;
        }

        // Cerca o Crea Utente Locale
        Utente? user = null;
        try
        {
            logger.LogInformation(">>> [EVENT OnTicketReceived] Searching for existing user with email {Email}", email);
            user = await dbContext.Utenti.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }
        catch (Exception ex_search)
        {
            logger.LogError(ex_search, ">>> [EVENT OnTicketReceived] Database error while searching for user with email {Email}", email);
            // --- Gestione Errore Utente ---
            context.Response.Redirect("/login-failed.html?reason=db_search_error");
            context.HandleResponse();
            // --- Fine Gestione ---
            return;
        }

        if (user is null)
        {
            // Crea nuovo utente
            logger.LogInformation(">>> [EVENT OnTicketReceived] No local user found for Google email {Email}. Creating new user.", email);
            var newUser = new Utente
            {
                Nome = givenName,
                Cognome = surname,
                Email = email,
                Ruolo = RuoloUtente.Studente, // Ruolo di default
                EmailVerificata = true, // Imposta a true per Google
                PasswordHash = hasher.HashPassword(null!, Guid.NewGuid().ToString()) // Hash fittizio
            };
            dbContext.Utenti.Add(newUser);
            try
            {
                await dbContext.SaveChangesAsync();
                user = newUser;
                logger.LogInformation(">>> [EVENT OnTicketReceived] New user {Email} saved successfully with ID {UserId}", email, user.Id);
            }
            catch (Exception ex_save) // Cattura generica per semplicità qui
            {
                logger.LogError(ex_save, ">>> [EVENT OnTicketReceived] Error while saving new user for Google login {Email}.", email);
                // --- Gestione Errore Utente ---
                context.Response.Redirect("/login-failed.html?reason=user_creation_error");
                context.HandleResponse();
                // --- Fine Gestione ---
                return;
            }
        }
        else { logger.LogInformation(">>> [EVENT OnTicketReceived] Found existing local user {Email} with ID {UserId}", email, user.Id); }

        // Assicurati che user non sia null
        if (user is null)
        {
            logger.LogError(">>> [EVENT OnTicketReceived] User is null after find/create logic.");
            context.Response.Redirect("/login-failed.html?reason=internal_error_user");
            context.HandleResponse();
            return;
        }


        // Costruisci Principal Locale
        logger.LogInformation(">>> [EVENT OnTicketReceived] Preparing local principal for user {Email}", user.Email);
        var cascadedRoles = RoleUtils.GetCascadedRoles(user.Ruolo);
        var cookieClaims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Name,           user.Email),
                    new(ClaimTypes.GivenName,      user.Nome),
                    new(ClaimTypes.Surname,        user.Cognome)
                };
        foreach (var role in cascadedRoles) { cookieClaims.Add(new Claim(ClaimTypes.Role, role)); }
        var identity = new ClaimsIdentity(cookieClaims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
        var localPrincipal = new ClaimsPrincipal(identity);

        // Recupera returnUrl
        var properties = context.Properties;
        string? callbackReturnUrl = null;
        properties?.Items.TryGetValue(".redirect", out callbackReturnUrl);
        if (string.IsNullOrEmpty(callbackReturnUrl)) { properties?.Items.TryGetValue("returnUrl", out callbackReturnUrl); }

        // AZIONI ESPLICITE (SignIn e Redirect)
        try
        {
            // 1. Esegui SignInAsync locale ESPLICITAMENTE
            logger.LogInformation(">>> [EVENT OnTicketReceived] Executing explicit local SignInAsync for user {Email}", user.Email);
            await context.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, localPrincipal, properties);
            logger.LogInformation(">>> [EVENT OnTicketReceived] Explicit local SignInAsync completed.");

            // 2. Determina redirect finale
            string finalRedirectUri;
            var isSpecificLocalReturnUrl = !string.IsNullOrEmpty(callbackReturnUrl) && callbackReturnUrl.StartsWith('/') && callbackReturnUrl != "/";
            if (isSpecificLocalReturnUrl) { finalRedirectUri = callbackReturnUrl!; } else { finalRedirectUri = "/"; }
            logger.LogWarning(">>> [EVENT OnTicketReceived] Final redirect check: Target='{FinalRedirectUri}'", finalRedirectUri);

            // 3. Esegui Redirect ESPLICITAMENTE
            logger.LogInformation(">>> [EVENT OnTicketReceived] Issuing explicit redirect to {Url}", finalRedirectUri);
            context.Response.Redirect(finalRedirectUri);

            // 4. Segnala che la risposta è stata gestita
            context.HandleResponse();
            logger.LogInformation(">>> [EVENT OnTicketReceived] Finished processing and redirect issued.");
        }
        catch (Exception ex_signin)
        {
            logger.LogError(ex_signin, ">>> [EVENT OnTicketReceived] Error during explicit SignInAsync or Redirect for user {Email}", user.Email);
            // --- Gestione Errore Utente ---
            context.Response.Redirect("/login-failed.html?reason=signin_error");
            context.HandleResponse();
            // --- Fine Gestione ---
        }
    }
    // --- Gestore per fallimenti generici ---
    public static Task HandleRemoteFailure(RemoteFailureContext context)
    {
        // Ottieni logger
        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("EducationalGames.GoogleOnRemoteFailure");

        // Logga l'errore originale per diagnostica server-side
        // context.Failure contiene l'eccezione che ha causato il fallimento
        logger.LogError(context.Failure, ">>> [EVENT OnRemoteFailure] External login failed. Path={Path}, Error={Error}", context.Request.Path, context.Failure?.Message);

        // Prepara un messaggio/codice per la pagina di errore
        var reason = context.Failure?.Message ?? "unknown_external_error";
        // Tronca messaggi lunghi e fa l'escape per l'URL
        var reasonCode = Uri.EscapeDataString(reason.Length > 100 ? reason.Substring(0, 100) : reason);

        // Reindirizza alla pagina di errore generica passando il motivo
        context.Response.Redirect($"/login-failed.html?reason={reasonCode}");
        // Segnala che abbiamo gestito la risposta, il middleware non deve fare altro
        context.HandleResponse();
        return Task.CompletedTask;
    }

    // --- Gestore per accesso negato dall'utente ---
    public static Task HandleAccessDenied(AccessDeniedContext context)
    {
        // Ottieni logger
        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("EducationalGames.GoogleOnAccessDenied");
        logger.LogWarning(">>> [EVENT OnAccessDenied] User denied access during external login.");

        // Reindirizza alla pagina di errore con un motivo specifico
        context.Response.Redirect("/login-failed.html?reason=access_denied");
        // Segnala che abbiamo gestito la risposta
        context.HandleResponse();
        return Task.CompletedTask;
    }

}

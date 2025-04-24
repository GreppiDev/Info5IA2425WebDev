using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EducationalGames.Data; 
using EducationalGames.Models; 
using EducationalGames.Utils; 


namespace EducationalGames.Auth; 

public static class MicrosoftAuthEvents
{
    // Gestore per il successo dell'autenticazione esterna Microsoft
    public static async Task HandleTicketReceived(TicketReceivedContext context)
    {
        var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        var hasher = context.HttpContext.RequestServices.GetRequiredService<PasswordHasher<Utente>>();
        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("EducationalGames.MicrosoftOnTicketReceived");

        logger.LogInformation(">>> [EVENT MS OnTicketReceived] Started processing Microsoft callback.");

        // Controlla Principal
        if (context.Principal == null)
        {
            logger.LogError(">>> [EVENT MS OnTicketReceived] Principal is null.");
            context.Fail("Principal is null.");
            return;
        }
        // Estrai Claims (NOTA: I tipi di claim potrebbero differire leggermente da Google)
        // --- Estrazione Claims (Logica per Microsoft) ---
        var claims = context.Principal.Claims;
        logger.LogDebug(">>> [EVENT MS OnTicketReceived] Received Claims from Microsoft:");
        foreach (var claim in claims) { logger.LogDebug("   - Type: {ClaimType}, Value: {ClaimValue}", claim.Type, claim.Value); }

        // Prova diversi tipi comuni per l'email
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                 ?? claims.FirstOrDefault(c => c.Type == ClaimTypes.Upn)?.Value // User Principal Name
                 ?? claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;

        // Prova diversi tipi comuni per l'ID univoco
        var microsoftUserId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                           ?? claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value; // Object ID (OID)

        // Nome e Cognome (standard dovrebbero funzionare, ma aggiungiamo log)
        var givenName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? "Utente";
        var surname = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
        if (string.IsNullOrWhiteSpace(surname)) { surname = "-"; }

        logger.LogInformation(">>> [EVENT MS OnTicketReceived] Extracted Claims: Email={Email}, UserId={UserId}, GivenName={GivenName}, Surname={Surname}", email, microsoftUserId, givenName, surname);
        // --- Fine Estrazione Claims ---

        // Valida Claims essenziali
        if (string.IsNullOrEmpty(microsoftUserId) || string.IsNullOrEmpty(email))
        {
            logger.LogWarning(">>> [EVENT MS OnTicketReceived] Microsoft authentication succeeded but missing required claims (UserId or Email). Redirecting to failure page.");
            context.Response.Redirect("/login-failed.html?reason=microsoft_missing_claims");
            context.HandleResponse();
            return;
        }

        // Cerca o Crea Utente Locale (Logica IDENTICA a Google)
        Utente? user = null;
        try { user = await dbContext.Utenti.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower()); }
        catch (Exception ex_search) { logger.LogError(ex_search, "DB search error"); context.Fail(ex_search); return; }

        if (user is null)
        {
            logger.LogInformation(">>> [EVENT MS OnTicketReceived] No local user found for email {Email}. Creating new user.", email);
            var newUser = new Utente
            {
                Nome = givenName,
                Cognome = surname,
                Email = email,
                Ruolo = RuoloUtente.Studente, // Default
                EmailVerificata = true, // Imposta a true per Microsoft
                PasswordHash = hasher.HashPassword(null!, Guid.NewGuid().ToString())
            };
            dbContext.Utenti.Add(newUser);
            try { await dbContext.SaveChangesAsync(); user = newUser; logger.LogInformation("New user saved ID {UserId}", user.Id); }
            catch (Exception ex_save) { logger.LogError(ex_save, "Error saving new user"); context.Fail(ex_save); return; }
        }
        else { logger.LogInformation(">>> [EVENT MS OnTicketReceived] Found existing local user {Email} with ID {UserId}", email, user.Id); }

        if (user is null) { context.Fail("User could not be found or created."); return; }

        // --- Costruisci Principal Locale (IMPORTANTE USARE I TIPI STANDARD QUI) ---
        logger.LogInformation(">>> [EVENT MS OnTicketReceived] Preparing local principal for user {Email}", user.Email);
        var cascadedRoles = RoleUtils.GetCascadedRoles(user.Ruolo);
        var cookieClaims = new List<Claim>
        {
            // Usiamo SEMPRE i ClaimTypes standard per il nostro cookie locale
            new(ClaimTypes.NameIdentifier, user.Id.ToString()), // ID del NOSTRO DB
            new(ClaimTypes.Name,           user.Email),         // Email come Name
            new(ClaimTypes.GivenName,      user.Nome),
            new(ClaimTypes.Surname,        user.Cognome)
        };
        foreach (var role in cascadedRoles)
        {
            cookieClaims.Add(new Claim(ClaimTypes.Role, role));
        }
        // Specifichiamo lo schema di autenticazione (Cookies) e i tipi per Name e Role
        var identity = new ClaimsIdentity(cookieClaims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
        var localPrincipal = new ClaimsPrincipal(identity);
        // --- Fine Costruzione Principal Locale ---


        // Recupera returnUrl
        var properties = context.Properties;
        string? callbackReturnUrl = null;
        properties?.Items.TryGetValue(".redirect", out callbackReturnUrl);
        if (string.IsNullOrEmpty(callbackReturnUrl)) { properties?.Items.TryGetValue("returnUrl", out callbackReturnUrl); }

        // AZIONI ESPLICITE (SignIn e Redirect  - Logica IDENTICA a Google)
        try
        {
            logger.LogInformation(">>> [EVENT MS OnTicketReceived] Executing explicit local SignInAsync for user {Email}", user.Email);
            await context.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, localPrincipal, properties);
            logger.LogInformation(">>> [EVENT MS OnTicketReceived] Explicit local SignInAsync completed.");

            string finalRedirectUri;
            var isSpecificLocalReturnUrl = !string.IsNullOrEmpty(callbackReturnUrl) && callbackReturnUrl.StartsWith('/') && callbackReturnUrl != "/";
            if (isSpecificLocalReturnUrl) { finalRedirectUri = callbackReturnUrl!; } else { finalRedirectUri = "/profile.html"; }
            logger.LogWarning(">>> [EVENT MS OnTicketReceived] Final redirect check: Target='{FinalRedirectUri}'", finalRedirectUri);

            logger.LogInformation(">>> [EVENT MS OnTicketReceived] Issuing explicit redirect to {Url}", finalRedirectUri);
            context.Response.Redirect(finalRedirectUri);
            context.HandleResponse();
            logger.LogInformation(">>> [EVENT MS OnTicketReceived] Finished processing and redirect issued.");
        }
        catch (Exception ex_signin)
        {
            logger.LogError(ex_signin, ">>> [EVENT MS OnTicketReceived] Error during explicit SignInAsync or Redirect for user {Email}", user.Email);
            context.Response.Redirect("/login-failed.html?reason=signin_error");
            context.HandleResponse();
        }
    }

    // Gestore per fallimenti generici (Logica IDENTICA a Google, cambia solo categoria logger)
    public static Task HandleRemoteFailure(RemoteFailureContext context)
    {
        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("EducationalGames.MicrosoftOnRemoteFailure"); // Categoria diversa
        logger.LogError(context.Failure, ">>> [EVENT MS OnRemoteFailure] External login failed. Path={Path}, Error={Error}", context.Request.Path, context.Failure?.Message);
        var reason = context.Failure?.Message ?? "unknown_microsoft_error"; // Motivo diverso
        var reasonCode = Uri.EscapeDataString(reason.Length > 100 ? reason[..100] : reason);
        context.Response.Redirect($"/login-failed.html?reason={reasonCode}");
        context.HandleResponse();
        return Task.CompletedTask;
    }

    // Gestore per accesso negato dall'utente (Logica IDENTICA a Google, cambia solo categoria logger)
    public static Task HandleAccessDenied(AccessDeniedContext context)
    {
        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("EducationalGames.MicrosoftOnAccessDenied"); // Categoria diversa
        logger.LogWarning(">>> [EVENT MS OnAccessDenied] User denied access during external login.");
        context.Response.Redirect("/login-failed.html?reason=access_denied_microsoft"); // Motivo diverso
        context.HandleResponse();
        return Task.CompletedTask;
    }
}

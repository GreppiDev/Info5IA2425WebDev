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
using EducationalGames.Services;

namespace EducationalGames.Endpoints;

public static class AccountEndpoints
{
    public static RouteGroupBuilder MapAccountEndpoints(this RouteGroupBuilder group, ILoggerFactory loggerFactory)
    {
        group.MapPost("/login", async (AppDbContext db, HttpContext ctx, LoginModel loginModel, [FromQuery] string? returnUrl) =>
        {
            // Crea logger specifico per questo endpoint
            var logger = loggerFactory.CreateLogger("EducationalGames.Endpoints.Login");
            var hasher = new PasswordHasher<Utente>();
            // Cerca l'utente nel database tramite email (ignorando maiuscole/minuscole)
            // --- CORREZIONE ERRORE EF CORE ---
            // Modificato il confronto per essere traducibile in SQL da EF Core MySQL.
            // Si convertono entrambe le email in minuscolo per un confronto case-insensitive.
            var user = await db.Utenti.FirstOrDefaultAsync(u => u.Email.ToLower() == loginModel.Email.ToLower());

            // Se l'utente non esiste, restituisce Unauthorized
            if (user is null)
            {
                logger.LogWarning("Login fallito: utente non trovato per email {Email}", loginModel.Email);
                return Results.Problem("Credenziali non valide.", statusCode: StatusCodes.Status401Unauthorized);
            }

            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, loginModel.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                logger.LogWarning("Login fallito: password errata per utente {Email}", loginModel.Email);
                return Results.Problem("Credenziali non valide.", statusCode: StatusCodes.Status401Unauthorized);
            }

            if (!user.EmailVerificata)
            {
                logger.LogWarning("Login fallito: email non verificata per utente {Email}", loginModel.Email);
                // Restituisce un errore specifico per email non verificata
                return Results.Problem(
                    statusCode: StatusCodes.Status403Forbidden, // 403 Forbidden è appropriato qui
                    title: "Email non verificata.",
                    detail: "Il tuo indirizzo email non è stato ancora verificato. Controlla la tua casella di posta per il link di verifica.",
                    // Potresti aggiungere un campo extra qui per indicare al frontend di mostrare il pulsante "Re-invia Email"
                    extensions: new Dictionary<string, object?> { { "resendAvailable", true } }
                );
            }

            logger.LogInformation("Login riuscito per utente {Email}", loginModel.Email);

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
            // Reindirizza l'utente alla returnUrl se fornita altrimenti a una pagina di successo predefinita.
            // Assicurarsi che returnUrl sia un URL locale per prevenire attacchi di open redirect
            if (!string.IsNullOrEmpty(returnUrl) && Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
            {
                return Results.Redirect(returnUrl);
            }
            else
            {
                return Results.Redirect("/"); // Reindirizza a una pagina di successo predefinita
            }

        }).AllowAnonymous(); // Permette accesso anonimo al login

        // --- Endpoint di Registrazione Pubblica (Docente/Studente) ---
        group.MapPost("/register", async (AppDbContext db, RegisterModel model, IEmailService emailService, IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            LinkGenerator linkGenerator) =>
        {
            // Crea logger specifico
            var logger = loggerFactory.CreateLogger("EducationalGames.Endpoints.Register");

            // 1. Validazione del Modello (automatica con Minimal API + attributi)
            //    Se il modello non è valido, Minimal API restituisce automaticamente Bad Request.

            // 2. Controllo esplicito sul ruolo
            if (model.Ruolo == RuoloUtente.Admin)
            {
                return Results.BadRequest("La registrazione come Admin non è permessa.");
            }

            // 3. Verifica se l'email esiste già
            var existingUser = await db.Utenti.AnyAsync(u => u.Email.ToLower() == model.Email.ToLower());
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
                Ruolo = model.Ruolo,
                //PasswordHash = hasher.HashPassword(null!, model.Password), // Usa null! per il TUser fittizio

            };

            // Passa l'oggetto `newUser` completo al metodo HashPassword
            // per garantire che il contesto di hashing sia corretto e consistente.
            newUser.PasswordHash = hasher.HashPassword(newUser, model.Password);

            // Legge il flag di bypass dalla configurazione.
            // La variabile d'ambiente Testing__BypassEmailVerification=true mappa a Testing:BypassEmailVerification.
            var bypassVerification = configuration.GetValue<bool>("Testing:BypassEmailVerification");

            if (bypassVerification)
            {
                newUser.EmailVerificata = true; // Bypassa la verifica per l'ambiente di test
                logger.LogWarning("Bypass della verifica email abilitato. L'utente '{Email}' è stato creato come confermato.", newUser.Email);
                newUser.TokenVerificaEmail = null; // Non serve token in questo caso
                newUser.ScadenzaTokenVerificaEmail = null; // Non serve scadenza in questo caso
            }
            else // Se non si bypassa la verifica, imposta i campi di verifica email
            {
                newUser.EmailVerificata = false; // Imposta a false alla registrazione
                newUser.TokenVerificaEmail = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) // Genera token univoco URL-safe
                                        .Replace("+", "-").Replace("/", "_").TrimEnd('='); // Rende URL-safe
                newUser.ScadenzaTokenVerificaEmail = DateTime.UtcNow.AddHours(24); // Scadenza 24 ore
            }
            ;

            db.Utenti.Add(newUser);
            try
            {
                await db.SaveChangesAsync();
                logger.LogInformation("Nuovo utente {Email} creato con successo, in attesa di verifica.", newUser.Email);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore durante il salvataggio del nuovo utente {Email}", newUser.Email);
                return Results.Problem("Errore durante la registrazione.", statusCode: StatusCodes.Status500InternalServerError);
            }


            // --- INVIO EMAIL DI VERIFICA ---
            // Invia l'email di conferma solo se il bypass non è attivo
            if (!bypassVerification)
            {
                try
                {
                    // Ottieni l'URL base dell'applicazione (es. https://localhost:7269)
                    if (httpContextAccessor.HttpContext?.Request is null) throw new InvalidOperationException("Impossibile accedere a HttpContext Request.");

                    // Genera il link di verifica usando LinkGenerator (più robusto)
                    // Assicurati che l'endpoint /verify-email sia mappato correttamente
                    var verificationLink = linkGenerator.GetUriByName(
                        httpContextAccessor.HttpContext!,
                        "VerifyEmailEndpoint", // Nome univoco da assegnare all'endpoint di verifica
                        new { token = newUser.TokenVerificaEmail } // Parametri per il link
                    );


                    if (string.IsNullOrEmpty(verificationLink))
                    {
                        logger.LogError("Impossibile generare il link di verifica per l'utente {Email}", newUser.Email);
                        // Restituisci un errore generico o un messaggio specifico
                        return Results.Problem("Errore nella generazione del link di verifica.", statusCode: StatusCodes.Status500InternalServerError);
                    }

                    logger.LogInformation("Invio email di verifica a {Email} con link: {Link}", newUser.Email, verificationLink);
                    await emailService.SendEmailConfirmationAsync(newUser.Email, verificationLink);
                }
                catch (Exception emailEx)
                {
                    logger.LogError(emailEx, "Errore durante l'invio dell'email di verifica a {Email}, ma utente creato.", newUser.Email);
                    // Non bloccare la risposta all'utente se l'invio fallisce, ma logga l'errore.
                    // L'utente potrebbe dover richiedere un nuovo invio in seguito.
                }
            }
            // --- FINE INVIO EMAIL ---

            // Restituisci un messaggio che informa l'utente di controllare l'email
            return Results.Ok(new { Message = "Registrazione quasi completata! Controlla la tua email per il link di verifica." });

        }).AllowAnonymous();


        group.MapGet("/verify-email", async (
            AppDbContext db,
            [FromQuery] string token) =>
        {
            // Crea logger specifico
            var logger = loggerFactory.CreateLogger("EducationalGames.Endpoints.VerifyEmail");
            if (string.IsNullOrEmpty(token))
            {
                return Results.BadRequest("Token di verifica mancante.");
            }

            // Cerca l'utente con il token fornito
            var user = await db.Utenti.FirstOrDefaultAsync(u => u.TokenVerificaEmail == token);

            // Controlli di validità
            if (user is null)
            {
                logger.LogWarning("Verifica email fallita: token non trovato o già usato.");
                // Reindirizza a una pagina che spiega che il link non è valido o è scaduto
                return Results.Redirect("/login-failed.html?reason=invalid_token");
            }
            if (user.EmailVerificata)
            {
                logger.LogInformation("Verifica email: email {Email} già verificata.", user.Email);
                // Già verificato, reindirizza al login o a una pagina informativa
                return Results.Redirect("/login-page.html?message=email_already_verified");
            }
            if (user.ScadenzaTokenVerificaEmail.HasValue && user.ScadenzaTokenVerificaEmail.Value < DateTime.UtcNow)
            {
                logger.LogWarning("Verifica email fallita: token scaduto per utente {Email}", user.Email);
                // Reindirizza a una pagina che spiega che il link è scaduto e magari permette di richiederne uno nuovo
                return Results.Redirect("/login-failed.html?reason=expired_token");
            }

            // Verifica riuscita: aggiorna l'utente
            user.EmailVerificata = true;
            user.TokenVerificaEmail = null; // Invalida il token
            user.ScadenzaTokenVerificaEmail = null;

            try
            {
                await db.SaveChangesAsync();
                logger.LogInformation("Email {Email} verificata con successo.", user.Email);
                // Reindirizza alla pagina di login con un messaggio di successo
                return Results.Redirect("/login-page.html?message=email_verified_success");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore durante il salvataggio della verifica email per {Email}", user.Email);
                // Reindirizza a una pagina di errore generica
                return Results.Redirect("/login-failed.html?reason=verification_save_error");
            }

        })
        .WithName("VerifyEmailEndpoint") // Assegna un nome all'endpoint per LinkGenerator
        .AllowAnonymous(); // Deve essere accessibile senza login

        group.MapPost("/forgot-password", async (
            AppDbContext db,
            ForgotPasswordModel model,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            LinkGenerator linkGenerator) =>
        {
            var logger = loggerFactory.CreateLogger("EducationalGames.Endpoints.ForgotPassword");
            var user = await db.Utenti.FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

            // Restituisci sempre OK anche se l'utente non esiste, per non rivelare email registrate
            if (user is not null && user.EmailVerificata)
            {
                // Genera token di reset e scadenza
                user.TokenResetPassword = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("+", "-").Replace("/", "_").TrimEnd('=');
                user.ScadenzaTokenResetPassword = DateTime.UtcNow.AddHours(1); // Scadenza più breve per reset password (es. 1 ora)

                try
                {
                    await db.SaveChangesAsync();
                    logger.LogInformation("Token reset password generato per {Email}", user.Email);

                    // Invia email di reset
                    var request = httpContextAccessor.HttpContext?.Request;
                    if (request == null) throw new InvalidOperationException("...");

                    // Genera link alla pagina/componente frontend che gestirà l'inserimento nuova password
                    // Passa il token come parametro
                    // NOTA: Il link NON deve puntare a un endpoint API POST, ma a una pagina HTML
                    var resetPageLink = $"{request.Scheme}://{request.Host}/reset-password-page.html?token={Uri.EscapeDataString(user.TokenResetPassword)}";
                    // Oppure, se usi routing frontend: var resetPageLink = $"/reset-password?token={...}";

                    logger.LogInformation("Invio email reset password a {Email}", user.Email);
                    await emailService.SendPasswordResetAsync(user.Email, resetPageLink);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Errore durante generazione token o invio email reset per {Email}", user.Email);
                    // Non rivelare l'errore all'utente
                }
            }
            else
            {
                logger.LogWarning("Richiesta reset password per email non trovata: {Email}", model.Email);
            }

            // Messaggio generico per l'utente
            return Results.Ok(new { Message = "Se l'indirizzo email è registrato, riceverai un link per reimpostare la password." });

        }).AllowAnonymous();


        group.MapPost("/reset-password", async (
            AppDbContext db,
            ResetPasswordModel model) =>
        {
            var logger = loggerFactory.CreateLogger("EducationalGames.Endpoints.ResetPassword");

            // Validazione modello automatica (controlla anche ConfirmPassword)

            // Cerca utente tramite token
            var user = await db.Utenti.FirstOrDefaultAsync(u => u.TokenResetPassword == model.Token);

            // Controlli token
            if (user is null)
            {
                logger.LogWarning("Tentativo reset password con token non valido o già usato.");
                return Results.Problem(statusCode: 400, title: "Link non valido", detail: "Il link per il reset della password non è valido o è scaduto.");
            }
            if (user.TokenResetPassword is not null && !user.TokenResetPassword.Equals(model.Token)) // Controllo aggiuntivo (dovrebbe essere già coperto dalla query)
            {
                logger.LogWarning("Token reset password non corrispondente per utente {Email}", user.Email);
                return Results.Problem(statusCode: 400, title: "Link non valido");
            }
            if (user.ScadenzaTokenResetPassword.HasValue && user.ScadenzaTokenResetPassword.Value < DateTime.UtcNow)
            {
                logger.LogWarning("Tentativo reset password con token scaduto per utente {Email}", user.Email);
                // Invalida il token scaduto per sicurezza
                user.TokenResetPassword = null;
                user.ScadenzaTokenResetPassword = null;
                await db.SaveChangesAsync();
                return Results.Problem(statusCode: 400, title: "Link scaduto", detail: "Il link per il reset della password è scaduto. Richiedine uno nuovo.");
            }

            // Resetta la password
            var hasher = new PasswordHasher<Utente>();
            user.PasswordHash = hasher.HashPassword(user, model.NewPassword);
            user.TokenResetPassword = null; // Invalida il token dopo l'uso
            user.ScadenzaTokenResetPassword = null;
            // Opzionale: forza la verifica email se non era già verificata? Dipende dalla logica desiderata.
            // user.EmailVerificata = true;

            try
            {
                await db.SaveChangesAsync();
                logger.LogInformation("Password resettata con successo per utente {Email}", user.Email);
                // Non fare login automatico qui per sicurezza
                return Results.Ok(new { Message = "Password reimpostata con successo. Ora puoi effettuare il login con la nuova password." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore durante il salvataggio della nuova password per {Email}", user.Email);
                return Results.Problem("Errore durante il reset della password.", statusCode: 500);
            }

        }).AllowAnonymous(); // Permette accesso anonimo perché l'utente non è loggato


        group.MapPost("/resend-verification", async (
            AppDbContext db,
            ResendVerificationModel model,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            LinkGenerator linkGenerator) =>
        {
            var logger = loggerFactory.CreateLogger("EducationalGames.Endpoints.ResendVerification");
            var user = await db.Utenti.FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

            // Restituisci sempre OK per non rivelare email registrate
            if (user is not null)
            {
                // Controlla se l'email è GIA' verificata
                if (user.EmailVerificata)
                {
                    logger.LogInformation("Richiesta reinvio verifica per email già verificata: {Email}", model.Email);
                    // Non fare nulla, ma restituisci OK
                    return Results.Ok(new { Message = "Se l'indirizzo email è registrato e non ancora verificato, riceverai un nuovo link." });
                }

                // Genera NUOVO token e scadenza
                user.TokenVerificaEmail = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("+", "-").Replace("/", "_").TrimEnd('=');
                user.ScadenzaTokenVerificaEmail = DateTime.UtcNow.AddHours(24);

                try
                {
                    await db.SaveChangesAsync();
                    logger.LogInformation("Nuovo token di verifica generato per {Email}", user.Email);

                    // Invia nuova email di verifica
                    var request = httpContextAccessor.HttpContext?.Request;
                    if (request == null) throw new InvalidOperationException("...");
                    var verificationLink = linkGenerator.GetUriByName(httpContextAccessor.HttpContext!, "VerifyEmailEndpoint", new { token = user.TokenVerificaEmail });
                    if (string.IsNullOrEmpty(verificationLink)) { logger.LogError("Impossibile generare link reinvio..."); /* Non rivelare errore */ }
                    else
                    {
                        logger.LogInformation("Reinvio email di verifica a {Email}", user.Email);
                        await emailService.SendEmailConfirmationAsync(user.Email, verificationLink);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Errore durante generazione token o invio email reinvio per {Email}", user.Email);
                    // Non rivelare l'errore all'utente
                }
            }
            else
            {
                logger.LogWarning("Richiesta reinvio verifica per email non trovata: {Email}", model.Email);
            }

            // Messaggio generico per l'utente
            return Results.Ok(new { Message = "Se l'indirizzo email è registrato e non ancora verificato, riceverai un nuovo link di verifica." });

        }).AllowAnonymous();

        // --- Aggiornamento Profilo Utente ---
        group.MapPut("/profile", async (
            AppDbContext db,
            HttpContext ctx,
            UpdateProfileModel model) => // Riceve il DTO con i dati aggiornati
        {
            var logger = loggerFactory.CreateLogger("EducationalGames.Endpoints.UpdateProfile");

            var userIdString = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                logger.LogWarning("Aggiornamento profilo fallito: ID utente non trovato nei claims.");
                return Results.Unauthorized();
            }

            var user = await db.Utenti.FindAsync(userId);
            if (user is null)
            {
                logger.LogWarning("Aggiornamento profilo fallito: utente {UserId} non trovato.", userId);
                return Results.NotFound("Utente non trovato.");
            }

            // Validazione modello automatica

            // Sicurezza: Impedisci auto-promozione ad Admin
            if (model.Ruolo == RuoloUtente.Admin && user.Ruolo != RuoloUtente.Admin)
            {
                logger.LogWarning("Tentativo non autorizzato di impostare ruolo Admin per utente {UserId}.", userId);
                return Results.BadRequest("Modifica del ruolo non permessa.");
            }

            // Aggiorna proprietà
            bool roleChanged = user.Ruolo != model.Ruolo; // Verifica se il ruolo è cambiato
            user.Nome = model.Nome;
            user.Cognome = model.Cognome;
            user.Ruolo = model.Ruolo; // Aggiorna ruolo (già validato come Studente o Docente dal DTO)

            try
            {
                await db.SaveChangesAsync();
                logger.LogInformation("Profilo per utente {UserId} ({Email}) aggiornato.", userId, user.Email);

                // Aggiorna Cookie di Autenticazione
                var cascadedRoles = RoleUtils.GetCascadedRoles(user.Ruolo);
                var newClaims = new List<Claim>
                 {
                     new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                     new(ClaimTypes.Name,           user.Email),
                     new(ClaimTypes.GivenName,      user.Nome),
                     new(ClaimTypes.Surname,        user.Cognome)
                 };
                foreach (var role in cascadedRoles) { newClaims.Add(new Claim(ClaimTypes.Role, role)); }
                var newIdentity = new ClaimsIdentity(newClaims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                var newPrincipal = new ClaimsPrincipal(newIdentity);
                var authResult = await ctx.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = authResult?.Properties;
                await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, newPrincipal, authProperties);
                logger.LogInformation("Cookie di autenticazione aggiornato per utente {UserId}.", userId);
                return Results.NoContent(); // Successo
            }

            catch (DbUpdateConcurrencyException dbEx)
            {
                logger.LogError(dbEx, "Errore di concorrenza aggiornando profilo utente {UserId}. Tentativo di sovrascrittura rilevato.", userId);
                // Informa il client che c'è stato un conflitto
                return Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Errore di Concorrenza",
                    detail: "I dati del profilo sono stati modificati da un'altra operazione dopo che li hai caricati. Ricarica la pagina e riprova le modifiche.",
                    // È possibile aggiungere dettagli sull'entità in conflitto se utile per il debug:
                    extensions: new Dictionary<string, object?> { { "conflictingEntity", dbEx.Entries.Select(e => e.Metadata.Name).FirstOrDefault() } }
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore durante il salvataggio del profilo per utente {UserId}", userId);
                return Results.Problem("Errore durante l'aggiornamento del profilo.", statusCode: 500);
            }

        }).RequireAuthorization(); // Richiede login

        // --- Endpoint per Creazione Utenti da parte dell'Admin ---
        group.MapPost("/admin/create-user", async (AppDbContext db, AdminCreateUserModel model) =>
        {
            // Crea logger specifico
            var logger = loggerFactory.CreateLogger("EducationalGames.Endpoints.CreateUserByAdmin");
            // 1. Validazione del Modello (automatica)

            // 2. Verifica se l'email esiste già
            var existingUser = await db.Utenti.AnyAsync(u => u.Email.ToLower() == model.Email.ToLower());
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
                Ruolo = model.Ruolo,
                PasswordHash = hasher.HashPassword(null!, model.Password),
                // --- IMPOSTA EMAIL COME VERIFICATA PER UTENTI CREATI DA ADMIN ---
                EmailVerificata = true,
                TokenVerificaEmail = null, // Non serve token
                ScadenzaTokenVerificaEmail = null
                // --- FINE ---
            };

            db.Utenti.Add(newUser);
            await db.SaveChangesAsync();
            logger.LogInformation("Utente {Email} creato da admin con ruolo {Role}", newUser.Email, newUser.Ruolo);
            return Results.Ok(new { Message = $"Utente {model.Email} creato con successo." });

        }).RequireAuthorization("AdminOnly");

        // --- Altri Endpoint (Profile, Admin-Area, Power-Area, My-Roles, Logout...) ---
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
                Username = ctx.User.FindFirstValue(ClaimTypes.Name), // Email
                GivenName = ctx.User.FindFirstValue(ClaimTypes.GivenName), // Nome
                Surname = ctx.User.FindFirstValue(ClaimTypes.Surname), // Cognome
                NameIdentifier = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier), // ID
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

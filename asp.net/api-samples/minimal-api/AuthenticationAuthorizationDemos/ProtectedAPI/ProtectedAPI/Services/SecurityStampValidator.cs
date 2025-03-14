using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using ProtectedAPI.Model;

namespace ProtectedAPI.Services;

/// <summary>
/// Middleware per la validazione del security stamp nei token JWT
/// </summary>
public class SecurityStampValidator
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityStampValidator> _logger;

    public SecurityStampValidator(RequestDelegate next, ILogger<SecurityStampValidator> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        // Esegui la validazione solo se l'utente è autenticato
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Verifica se la richiesta usa autenticazione Bearer token
            var isJwtAuth = context.Request.Headers.Authorization
                .FirstOrDefault()?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ?? false;

            if (isJwtAuth)
            {
                // Gestisci validazione per token JWT
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId != null)
                {
                    var securityStampClaim = context.User.FindFirstValue("AspNet.Identity.SecurityStamp");
                    if (securityStampClaim != null)
                    {
                        var user = await userManager.FindByIdAsync(userId);
                        if (user != null)
                        {
                            var currentSecurityStamp = await userManager.GetSecurityStampAsync(user);
                            if (currentSecurityStamp != securityStampClaim)
                            {
                                _logger.LogWarning("Security stamp non valido per l'utente JWT {UserId}", userId);
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                await context.Response.WriteAsJsonAsync(new { message = "Token invalidato. Effettua nuovamente l'accesso." });
                                return;
                            }
                        }
                    }
                }
            }
            else
            {
                // Gestisci validazione per cookie auth
                var user = await userManager.GetUserAsync(context.User);
                if (user != null)
                {
                    var validatedPrincipal = await signInManager.ValidateSecurityStampAsync(context.User);
                    if (validatedPrincipal == null)
                    {
                        _logger.LogWarning("Cookie di autenticazione non più valido per l'utente {UserId}", user.Id);
                        await signInManager.SignOutAsync();

                        // Reindirizza alla pagina di login invece di restituire un errore 401
                        if (context.Request.Headers.Accept.ToString().Contains("text/html"))
                        {
                            // Per richieste HTML, reindirizza al login
                            context.Response.Redirect("/login");
                        }
                        else
                        {
                            // Per richieste API, restituisci 401 con messaggio appropriato
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await context.Response.WriteAsJsonAsync(new { message = "Sessione scaduta. Effettua nuovamente l'accesso." });
                        }
                        return;
                    }

                    // Aggiorna il sign-in
                    await signInManager.RefreshSignInAsync(user);
                }
            }
        }

        // Continua con il pipeline
        await _next(context);
    }
}

/// <summary>
/// Estensione per registrare il middleware
/// </summary>
public static class SecurityStampValidatorExtensions
{
    /// <summary>
    /// Aggiunge il middleware per validare il security stamp
    /// </summary>
    public static IApplicationBuilder UseSecurityStampValidator(this IApplicationBuilder app, params string[] paths)
    {
        return app.UseWhen(context =>
            ShouldValidateStamp(context.Request.Path, paths),
            builder => builder.UseMiddleware<SecurityStampValidator>());
    }

    private static bool ShouldValidateStamp(PathString path, string[] paths)
    {
        foreach (var protectedPath in paths)
        {
            if (path.StartsWithSegments(protectedPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}

/// <summary>
/// Filtro per la validazione del security stamp nei token JWT
/// </summary>
public class SecurityStampValidatorFilter : IEndpointFilter
{
    private readonly ILogger<SecurityStampValidatorFilter> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public SecurityStampValidatorFilter(
        ILogger<SecurityStampValidatorFilter> logger,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        // Esegui la validazione solo se l'utente è autenticato
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            // Verifica se la richiesta usa autenticazione Bearer token
            var isJwtAuth = httpContext.Request.Headers.Authorization
                .FirstOrDefault()?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ?? false;

            if (isJwtAuth)
            {
                // Gestisci validazione per token JWT
                var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var securityStampClaim = httpContext.User.FindFirstValue("AspNet.Identity.SecurityStamp");

                if (userId != null && securityStampClaim != null)
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        var currentStamp = await _userManager.GetSecurityStampAsync(user);
                        if (currentStamp != securityStampClaim)
                        {
                            _logger.LogWarning("Security stamp non valido per l'utente JWT {UserId}", userId);
                            return TypedResults.Unauthorized();
                        }
                        _logger.LogInformation("Security stamp validato con successo per l'utente JWT {UserId}", userId);
                    }
                }
            }
            else
            {
                // Gestisci validazione per cookie auth nel filtro
                var user = await _userManager.GetUserAsync(httpContext.User);
                if (user != null)
                {
                    var validatedPrincipal = await _signInManager.ValidateSecurityStampAsync(httpContext.User);
                    if (validatedPrincipal == null)
                    {
                        _logger.LogWarning("Cookie di autenticazione non più valido per l'utente {UserId}", user.Id);
                        await _signInManager.SignOutAsync();
                        return TypedResults.Unauthorized();
                    }

                    // Aggiorna il sign-in
                    await _signInManager.RefreshSignInAsync(user);
                }
            }
        }

        // Continua con l'esecuzione dell'endpoint
        return await next(context);
    }
}

/// <summary>
/// Estensioni per applicare il filtro di validazione del security stamp
/// </summary>
public static class SecurityStampValidationExtensions
{
    /// <summary>
    /// Applica il filtro di validazione del security stamp all'endpoint
    /// </summary>
    public static RouteHandlerBuilder ValidateSecurityStamp(this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter<SecurityStampValidatorFilter>();
    }
}
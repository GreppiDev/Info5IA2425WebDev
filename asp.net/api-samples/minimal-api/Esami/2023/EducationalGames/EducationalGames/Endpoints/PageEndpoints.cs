using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Primitives;

namespace EducationalGames.Endpoints;

public static class PageEndpoints
{
    public static RouteGroupBuilder MapPageEndpoints(this RouteGroupBuilder group)
    {
        //Un esempio di pagina protetta da autenticazione
        //Questa pagina è servita da un file statico in wwwroot
        //Non è necessario un controller MVC per servire file statici, basta usare MapGet e specificare il percorso fisico del file 
        //e il Content-Type corretto (text/html per i file HTML)
        //Il middleware di autenticazione gestirà la richiesta e reindirizzerà l'utente alla pagina di login se non autenticato
        // oppure restituirà 401/403 per le richieste API non autorizzate 
        //Se l'utente è autenticato, il file verrà servito normalmente, e l'utente vedrà la pagina HTML come se fosse una normale richiesta
        // di un file statico
        
        //Non per forza bisogna usare questo approccio per impedire l'accesso a file riservati a utenti non autenticati
        //Si può anche non effettuare questo controllo e lasciare che la pagina sia accessibile a tutti
        //ma poi fare assicurare che ja JavaScript non sia possibile accedere a funzioni o dati riservati
        group.MapGet("/profile.html", (HttpContext context, IWebHostEnvironment env) =>
        {
            // RequireAuthorization ensures this endpoint is only accessible by authenticated users.
            // The authentication middleware (configured earlier) will handle redirecting
            // unauthenticated browser requests to the login page or returning 401/403 for API requests.

            // Construct the physical path to the file within wwwroot
            var filePath = Path.Combine(env.WebRootPath, "profile.html");

            // Check if the file exists
            if (!System.IO.File.Exists(filePath))
            {
                // Return a 404 Not Found if the HTML file doesn't exist in wwwroot
                return Results.NotFound("The requested page was not found.");
            }

            // Serve the physical file. Results.File handles setting the correct Content-Type.
            return Results.File(filePath, "text/html");

        }).RequireAuthorization(); // Apply authorization policy

        // Endpoint di sfida a Google
        group.MapGet("/login-google", async (HttpContext httpContext, [FromQuery] string? returnUrl) =>
        {
            // Logica per validare returnUrl e costruire target...
            var actionContext = new ActionContext(httpContext, httpContext.GetRouteData(), new ActionDescriptor());
            var urlHelper = new UrlHelper(actionContext);
            var target = "/";
            if (!string.IsNullOrEmpty(returnUrl) && urlHelper.IsLocalUrl(returnUrl)) { target = returnUrl; }

            // Proprietà passate alla sfida. 
            var props = new AuthenticationProperties
            {
                Items = { ["returnUrl"] = target } // Passiamo solo la destinazione finale
            };

            // Esegui la sfida esplicita
            await httpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, props);

        }).AllowAnonymous();


        // Endpoint di sfida a Microsoft
        group.MapGet("/login-microsoft", async (HttpContext httpContext, [FromQuery] string? returnUrl) =>
        {
            var target = "/";
            // ... validazione returnUrl (identica a Google) ...
            var actionContext = new ActionContext(httpContext, httpContext.GetRouteData(), new ActionDescriptor());
            var urlHelper = new UrlHelper(actionContext);
            if (!string.IsNullOrEmpty(returnUrl) && urlHelper.IsLocalUrl(returnUrl)) { target = returnUrl; }

            var props = new AuthenticationProperties { Items = { [".redirect"] = target } };
            // Sfida lo schema MicrosoftAccount
            await httpContext.ChallengeAsync(MicrosoftAccountDefaults.AuthenticationScheme, props);
        }).AllowAnonymous();



        // Endpoint per gestire i redirect a pagine HTML specifiche
        // Questi endpoint vengono chiamati dal middleware dei cookie quando rileva
        // una richiesta non API che richiede login o non ha i permessi.
        group.MapGet("/login-required", (HttpContext context) =>
        {
            // Legge il parametro ReturnUrl aggiunto automaticamente dal middleware
            context.Request.Query.TryGetValue("ReturnUrl", out StringValues returnUrlSv);
            var returnUrl = returnUrlSv.FirstOrDefault();

            // Costruisce l'URL per la pagina di login HTML
            var redirectUrl = "/login-page.html";
            if (!string.IsNullOrEmpty(returnUrl))
            {
                // Aggiunge il ReturnUrl alla pagina di login, così può reindirizzare dopo il login
                redirectUrl += $"?ReturnUrl={Uri.EscapeDataString(returnUrl)}";
            }
            // Esegue il redirect alla pagina di login HTML
            return Results.Redirect(redirectUrl);

        }).AllowAnonymous();

        group.MapGet("/access-denied", (HttpContext context) =>
        {
            // Legge il parametro ReturnUrl aggiunto automaticamente dal middleware
            context.Request.Query.TryGetValue("ReturnUrl", out StringValues returnUrlSv);
            var returnUrl = returnUrlSv.FirstOrDefault();

            // Costruisce l'URL per la pagina di accesso negato HTML
            var redirectUrl = "/access-denied.html";
            if (!string.IsNullOrEmpty(returnUrl))
            {
                // Aggiunge il ReturnUrl alla pagina di accesso negato (utile per logging o messaggi)
                redirectUrl += $"?ReturnUrl={Uri.EscapeDataString(returnUrl)}";
            }
            // Esegue il redirect alla pagina di accesso negato HTML
            return Results.Redirect(redirectUrl);

        }).AllowAnonymous();

        // Endpoint di fallback per errori generici
        group.MapGet("/error", () => Results.Problem("Si è verificato un errore interno.")).AllowAnonymous();


        return group;
    }
}

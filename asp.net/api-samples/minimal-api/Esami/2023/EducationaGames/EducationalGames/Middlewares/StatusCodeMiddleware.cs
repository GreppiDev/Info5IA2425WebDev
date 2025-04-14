using EducationalGames.Utils;


namespace EducationalGames.Middlewares
{
    public class StatusCodeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<StatusCodeMiddleware> _logger;

        public StatusCodeMiddleware(RequestDelegate next, ILogger<StatusCodeMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            var response = context.Response;

            // Verifica che la risposta non sia già iniziata e che ci sia un errore client/server
            if (response.HasStarted || response.StatusCode < 400 || response.StatusCode >= 600)
            {
                return;
            }

            if (response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                _logger.LogWarning("Handling 401 Unauthorized for {Path}", context.Request.Path);
                // Per richieste HTML, redirect alla pagina di login
                if (HttpUtils.IsHtmlRequest(context.Request))
                {
                    var returnUrl = context.Request.Path + context.Request.QueryString;
                    response.Redirect($"/login-page.html?returnUrl={Uri.EscapeDataString(returnUrl)}");
                    return; // Importante terminare l'esecuzione qui
                }

                // Per API, restituisci risposta JSON
                response.ContentType = "application/json";
                await response.WriteAsJsonAsync(new
                {
                    status = 401,
                    message = "Non sei autenticato. Effettua il login per accedere a questa risorsa.",
                    timestamp = DateTime.UtcNow,
                    path = context.Request.Path
                });
            }
            else if (response.StatusCode == StatusCodes.Status403Forbidden)
            {
                _logger.LogWarning("Handling 403 Forbidden for {Path}", context.Request.Path);
                // Per richieste HTML, redirect alla pagina di accesso negato
                if (HttpUtils.IsHtmlRequest(context.Request))
                {
                    var returnUrl = context.Request.Path + context.Request.QueryString;
                    response.Redirect($"/access-denied.html?returnUrl={Uri.EscapeDataString(returnUrl)}");
                    return; // Importante terminare l'esecuzione qui
                }

                // Per API, restituisci risposta JSON
                response.ContentType = "application/json";
                await response.WriteAsJsonAsync(new
                {
                    status = 403,
                    message = "Non hai i permessi necessari per accedere a questa risorsa.",
                    timestamp = DateTime.UtcNow,
                    path = context.Request.Path
                });
            }
            // Puoi aggiungere altri gestori per diversi codici di stato qui se necessario
        }
    }
}

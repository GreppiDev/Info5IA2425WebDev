using System.Text.Json; // Per JsonSerializerOptions

namespace EducationalGames.Middlewares
{
    public class StatusCodeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<StatusCodeMiddleware> _logger;

        // Opzioni per la serializzazione JSON (camelCase)
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };


        public StatusCodeMiddleware(RequestDelegate next, ILogger<StatusCodeMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context); // Esegui il resto della pipeline

            var response = context.Response;

            // Controlla solo se la richiesta è per un'API e la risposta non è già iniziata
            // e c'è un errore client/server rilevante (401, 403, 404).
            if (response.HasStarted || !context.Request.Path.StartsWithSegments("/api") || response.StatusCode < 400 || response.StatusCode >= 600)
            {
                return; // Non fare nulla se non è un errore API gestibile
            }

            // Se è un errore API (401, 403, 404), formatta la risposta come JSON standard
            // senza fare redirect. I redirect per non-API sono gestiti altrove.

            if (response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                _logger.LogWarning("API Request Unauthorized (401) for {Path}", context.Request.Path);
                response.ContentType = "application/json";
                await response.WriteAsJsonAsync(new ApiErrorResponse
                {
                    Status = 401,
                    Title = "Unauthorized",
                    Detail = "Autenticazione richiesta per accedere a questa risorsa API.",
                    Path = context.Request.Path,
                    Timestamp = DateTime.UtcNow
                }, _jsonOptions);
            }
            else if (response.StatusCode == StatusCodes.Status403Forbidden)
            {
                _logger.LogWarning("API Request Forbidden (403) for {Path}", context.Request.Path);
                response.ContentType = "application/json";
                await response.WriteAsJsonAsync(new ApiErrorResponse
                {
                    Status = 403,
                    Title = "Forbidden",
                    Detail = "Non hai i permessi necessari per accedere a questa risorsa API.",
                    Path = context.Request.Path,
                    Timestamp = DateTime.UtcNow
                }, _jsonOptions);
            }
            else if (response.StatusCode == StatusCodes.Status404NotFound)
            {
                _logger.LogWarning("API Resource Not Found (404) for {Path}", context.Request.Path);
                response.ContentType = "application/json";
                await response.WriteAsJsonAsync(new ApiErrorResponse
                {
                    Status = 404,
                    Title = "Not Found",
                    Detail = "La risorsa API richiesta non è stata trovata.",
                    Path = context.Request.Path,
                    Timestamp = DateTime.UtcNow
                }, _jsonOptions);
            }
            // Puoi aggiungere altri 'else if' per gestire altri codici di stato per le API
        }

        // Classe helper per standardizzare le risposte di errore API
        private sealed class ApiErrorResponse
        {
            public int Status { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Detail { get; set; } = string.Empty;
            public string Path { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
        }
    }
}

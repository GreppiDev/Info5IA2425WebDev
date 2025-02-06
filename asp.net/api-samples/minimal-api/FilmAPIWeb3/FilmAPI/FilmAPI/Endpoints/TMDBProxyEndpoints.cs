using FilmAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FilmAPI.Endpoints;

public static class TMDBProxyEndpoints
{
    private static async Task<IResult> HandleTMDBRequest(
        HttpContext context,
        ITMDBHttpClientService tmdbService,
        IRequestValidationService validationService,
        string endpoint)
    {
        try
        {
            // Verifica se la richiesta proviene da una origine consentita
            if (!validationService.IsValidRequest(context))
            {
                return Results.Unauthorized();
            }

            if (string.IsNullOrEmpty(endpoint))
            {
                return Results.BadRequest("The endpoint path is required");
            }

            var query = context.Request.QueryString.Value ?? string.Empty;
            var data = await tmdbService.SendRequest(endpoint, query);
            return Results.Content(data, "application/json");
        }
        catch (HttpRequestException ex)
        {
            return Results.Problem(
                title: "TMDB API Error",
                detail: ex.Message,
                statusCode: (int?)ex.StatusCode ?? 500
            );
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    }

    public static RouteGroupBuilder MapTMDBProxyEndpoints(this RouteGroupBuilder group)
    {
        // Endpoint per Swagger UI a solo scopo di documentazione
        //non è destinato all'uso diretto da parte dell'applicazione client
        group.MapGet("/tmdb/proxy", async (
            HttpContext context,
            ITMDBHttpClientService tmdbService,
            IRequestValidationService validationService,
            [FromQuery(Name = "path")] string endpoint) =>
        {
            return await HandleTMDBRequest(context, tmdbService, validationService, endpoint);
        })
        .RequireRateLimiting("TMDBPolicy")
        .WithName("GetTMDBDataSwagger")
        .WithOpenApi(operation => {
            operation.Summary = "Proxy endpoint for TMDB API (Swagger UI)";
            operation.Description = "Forwards requests to The Movie Database API. Use the path parameter to specify the TMDB endpoint (e.g. movie/98, movie/popular)";
            return operation;
        });

        // Endpoint per le chiamate effettive a TMDB
        //non è inserito in Swagger UI perché non è destinato all'uso diretto dagli utenti
        //e perché in swagger non è possibile specificare un parametro path che accetti qualsiasi valore
        group.MapGet("/tmdb/{*endpoint}", async (
            HttpContext context,
            ITMDBHttpClientService tmdbService,
            IRequestValidationService validationService,
            string endpoint) =>
        {
            return await HandleTMDBRequest(context, tmdbService, validationService, endpoint);
        })
        .RequireRateLimiting("TMDBPolicy")
        .ExcludeFromDescription(); // This hides it from Swagger UI

        return group;
    }
}

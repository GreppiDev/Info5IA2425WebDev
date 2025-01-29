using System.Net.Http.Headers;
using FilmAPI.Utils;
using Microsoft.AspNetCore.Mvc;
namespace FilmAPI.Endpoints;

public static class TMDBProxyEndpoints
{
	private static async Task<IResult> HandleTMDBRequest(
		HttpContext context,
		IConfiguration config,
		string endpoint)
	{
		try
		{
			if (string.IsNullOrEmpty(endpoint))
			{
				return Results.BadRequest("The endpoint path is required");
			}

			var bearerToken = config["TMDB:BearerToken"];
			var baseUrl = config["TMDB:BaseUrl"];

			//using var httpClient = new HttpClient();
			//per far funzionare il proxy a scuola
			using var httpClient = HttpProxyHelper.CreateHttpClient(setProxy: true);
			// Aggiungi il Bearer Token all'header
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

			// Costruisci l'URL per TMDB
			var query = context.Request.QueryString.Value;
			var url = $"{baseUrl}/{endpoint.TrimStart('/')}";
			if (!string.IsNullOrEmpty(query))
			{
				url += query;
			}

			// Inoltra la richiesta a TMDB
			var response = await httpClient.GetAsync(url);
			response.EnsureSuccessStatusCode();
			var data = await response.Content.ReadAsStringAsync();

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
	}

	public static RouteGroupBuilder MapTMDBProxyEndpoints(this RouteGroupBuilder group)
	{
		// Endpoint per Swagger UI a solo scopo di documentazione
		//non è destinato all'uso diretto da parte dell'applicazione client
		group.MapGet("/tmdb/proxy", async (
			HttpContext context,
			IConfiguration config,
			[FromQuery(Name = "path")] string endpoint) =>
		{
			return await HandleTMDBRequest(context, config, endpoint);
		})
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
			IConfiguration config,
			string endpoint) =>
		{
			return await HandleTMDBRequest(context, config, endpoint);
		}).ExcludeFromDescription(); // This hides it from Swagger UI

		return group;
	}
}

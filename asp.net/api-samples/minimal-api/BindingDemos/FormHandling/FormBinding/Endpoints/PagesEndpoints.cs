
namespace TodoApi.Endpoints;

public static class PagesEndpoints
{
	public static RouteGroupBuilder MapPagesEndpoints(this RouteGroupBuilder group)
	{
		group.MapGet("", () => Results.Redirect("/index.html"));

		group.MapGet("/{page}", (HttpContext context, string? page = "index.html") =>
		{
			var filePath = Path.Combine("wwwroot", page!);
			if (!File.Exists(filePath))
			{
				//return Results.NotFound();
				//Andrebbe fatta una redirect alla pagina di errore.
				//In questo esempio, per semplicità, si fa una redirect alla index.html (home page)
				return Results.Redirect("/index.html");
			}

			var isTextFile = page!.EndsWith(".html") || page.EndsWith(".css") || page.EndsWith(".js") || page.EndsWith(".txt");

			if (isTextFile)
			{
				var content = File.ReadAllText(filePath);
				var contentType = page.EndsWith(".html") ? "text/html" :
								  page.EndsWith(".css") ? "text/css" :
								  page.EndsWith(".js") ? "application/javascript" :
								  "text/plain";
				return Results.Content(content, contentType);
			}
			else
			{
				var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
				var contentType = "application/octet-stream";
				return Results.File(fileStream, contentType, enableRangeProcessing: true);
			}
		});
		return group;
	}
}
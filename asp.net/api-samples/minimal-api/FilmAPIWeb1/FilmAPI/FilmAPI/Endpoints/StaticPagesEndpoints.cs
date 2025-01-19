namespace FilmAPI.Endpoints;

public static class StaticPagesEndpoints
{
	public static RouteGroupBuilder MapStaticPagesEndpoints(this RouteGroupBuilder group)
	{
		// Gestisce qualsiasi percorso, con priorità più bassa delle API
		group.MapGet("{*path}", HandleStaticFile)
			 .AddEndpointFilter(async (context, next) =>
			 {
				 // Se il percorso inizia con /swagger, salta questo handler
				 var path = context.HttpContext.Request.Path.Value ?? "";
				 if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
				 {
					 return await next(context);
				 }
				 // Se il percorso inizia con /api, restituisce Empty perché la rotta /api 
				 //dovrebbe essere già gestita con priorità più elevata.
				 if (path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
				 {
					 return Results.Empty;
				 }
				 return await next(context);
			 });

		return group;
	}

	private static IResult HandleStaticFile(string? path)
	{
		// Se non viene specificato un percorso, reindirizza a index.html
		if (string.IsNullOrEmpty(path))
		{
			return Results.Redirect("/index.html");
		}

		// Normalizza il percorso
		path = path.TrimStart('/');

		// Lista dei possibili percorsi da controllare
		var possiblePaths = new[]
		{
			Path.Combine("wwwroot", path),                    // Percorso diretto in wwwroot
			Path.Combine("wwwroot", "pages", path),           // Percorso in wwwroot/pages
			Path.Combine("wwwroot", path, "index.html")       // index.html nelle sottocartelle
		};

		// Cerca il primo percorso valido
		string? validPath = possiblePaths.FirstOrDefault(File.Exists);

		if (validPath == null)
		{
			// Qui puoi implementare la logica per la pagina di errore 404
			return Results.Redirect("/index.html");
			// Alternativa: return Results.NotFound();
		}

		return ServeFile(validPath);
	}

	private static readonly string[] TextFileExtensions = [".html", ".css", ".js", ".txt", ".json", ".xml"];

	private static IResult ServeFile(string filePath)
	{
		var extension = Path.GetExtension(filePath).ToLowerInvariant();

		// Mappa delle estensioni dei file ai content type
		var contentTypes = new Dictionary<string, string>
		{
			[".html"] = "text/html",
			[".css"] = "text/css",
			[".js"] = "application/javascript",
			[".txt"] = "text/plain",
			[".json"] = "application/json",
			[".xml"] = "application/xml",
			[".pdf"] = "application/pdf",
			[".png"] = "image/png",
			[".jpg"] = "image/jpeg",
			[".jpeg"] = "image/jpeg",
			[".gif"] = "image/gif",
			[".svg"] = "image/svg+xml",
			[".mp4"] = "video/mp4",
			[".webm"] = "video/webm",
			[".ogg"] = "video/ogg",
			[".mp3"] = "audio/mpeg",
			[".wav"] = "audio/wav",
			[".ico"] = "image/x-icon",
			[".zip"] = "application/zip",
			[".rar"] = "application/x-rar-compressed",
			[".tar"] = "application/x-tar",
			[".gz"] = "application/gzip",
			[".doc"] = "application/msword",
			[".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
			[".xls"] = "application/vnd.ms-excel",
			[".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
			[".ppt"] = "application/vnd.ms-powerpoint",
			[".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation"
			
		};

		// Verifica se è un file di testo
		var isTextFile = TextFileExtensions.Contains(extension);

		if (isTextFile)
		{
			var content = File.ReadAllText(filePath);
			var contentType = contentTypes.GetValueOrDefault(extension, "text/plain");
			return Results.Content(content, contentType);
		}
		else
		{
			var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
			var contentType = contentTypes.GetValueOrDefault(extension, "application/octet-stream");
			return Results.File(fileStream, contentType, enableRangeProcessing: true);
		}
	}
}
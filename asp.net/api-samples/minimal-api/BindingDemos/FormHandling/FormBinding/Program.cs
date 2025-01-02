
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDbContext<TodoDb>(opt =>
	opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddAntiforgery();
var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

//-------- Start of API management ------------
app.MapGet("/todos", async (TodoDb db) =>
{
	var todos = await db.Todos.Select(x => new TodoDto(x)).ToListAsync();
	return Results.Ok(todos);
});

app.MapGet("/todos/{id}", async (int Id, TodoDb Db) =>
	await Db.Todos.FindAsync(Id)
		is Todo todo
			? Results.Ok(new TodoDto(todo))
			: Results.NotFound());
			
app.MapDelete("/todos/{id}", async (int id, TodoDb db) =>
{
	var todo = await db.Todos.FindAsync(id);
	if (todo is null)
	{
		return Results.NotFound();
	}
	if (!string.IsNullOrEmpty(todo.Attachment))
	{
		var filePath = Path.Combine("wwwroot", "user-content", todo.Attachment);
		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}
	}
	db.Todos.Remove(todo);
	await db.SaveChangesAsync();
	return Results.Ok();
});

app.MapPut("/todos/{id}", async (int id, [FromForm] string name,
	[FromForm] Visibility visibility, IFormFile? attachment, TodoDb db) =>
{
	var todo = await db.Todos.FindAsync(id);
	if (todo is null)
	{
		return Results.NotFound();
	}

	todo.Name = name;
	todo.Visibility = visibility;

	if (attachment is not null)
	{
		if (!string.IsNullOrEmpty(todo.Attachment))
		{
			var oldFilePath = Path.Combine("wwwroot", "user-content", todo.Attachment);
			if (File.Exists(oldFilePath))
			{
				File.Delete(oldFilePath);
			}
		}

		var extension = Path.GetExtension(attachment.FileName);
		var attachmentName = Path.GetRandomFileName() + extension;
		var uploadPath = Path.Combine("wwwroot", "user-content", attachmentName);
		using var stream = File.Create(uploadPath);
		await attachment.CopyToAsync(stream);
		todo.Attachment = attachmentName;
	}

	await db.SaveChangesAsync();

	return Results.Ok();
}).DisableAntiforgery();
			
app.MapGet("/todos/{id}/attachment", async (int id, TodoDb db) =>
{
	var todo = await db.Todos.FindAsync(id);
	if (todo is null || string.IsNullOrEmpty(todo.Attachment))
	{
		return Results.NotFound();
	}

	var filePath = Path.Combine("wwwroot", "user-content", todo.Attachment);
	if (!File.Exists(filePath))
	{
		return Results.NotFound();
	}

	var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
	return Results.File(fileStream, "application/octet-stream", todo.Attachment);
});

// Avoid reading the incoming file stream directly into memory all at once.
// For example, don't copy all of the file's bytes into a System.IO.MemoryStream
// or read the entire stream into a byte array all at once.
// Reading the incoming file stream directly into memory can result in 
// performance and security problems. Rather, consider adopting either of the following approaches:

// * On the server of a server app, copy the stream directly
//   to a file on disk without reading it into memory.
// * Upload files from the client directly to an external service.

app.MapPost("/ap/todos", async ([AsParameters] NewTodoRequest request, TodoDb db) =>
{
	var todo = new Todo
	{
		Name = request.Name,
		Visibility = request.Visibility
	};

	if (request.Attachment is not null)
	{
		var extension = Path.GetExtension(request.Attachment.FileName);
		var attachmentName = Path.GetRandomFileName() + extension;
		var uploadPath = Path.Combine("wwwroot", "user-content", attachmentName);
		using var stream = File.Create(uploadPath);
		await request.Attachment.CopyToAsync(stream);
		todo.Attachment = attachmentName;
	}

	db.Todos.Add(todo);
	await db.SaveChangesAsync();

	return Results.Ok();
}).DisableAntiforgery();

//note that AddAntiforgery is enabled by default, so we need to disable it
//By default, minimal APIs that accept form data require antiforgery token validation.
//https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?preserve-view=true#antiforgery-with-minimal-apis

app.MapPost("/todos", async ([FromForm] string name,
	[FromForm] Visibility visibility, IFormFile? attachment, TodoDb db) =>
{
	var todo = new Todo
	{
		Name = name,
		Visibility = visibility
	};

	if (attachment is not null)
	{
		var extension = Path.GetExtension(attachment.FileName);
		var attachmentName = Path.GetRandomFileName() + extension;
		var uploadPath = Path.Combine("wwwroot", "user-content", attachmentName);
		using var stream = File.Create(uploadPath);
		await attachment.CopyToAsync(stream);
		todo.Attachment = attachmentName;
	}

	db.Todos.Add(todo);
	await db.SaveChangesAsync();

	return Results.Ok();
}).DisableAntiforgery();
//-------- End of API management ------------


//----------- Start of page management ------------
//basic page routing
app.MapGet("", () => Results.Redirect("/index.html"));

app.MapGet("/{page}", (HttpContext context, string? page = "index.html") =>
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
//----------- End of page management ------------


app.UseAntiforgery();
app.Run();



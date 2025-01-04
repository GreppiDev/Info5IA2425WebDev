
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Endpoints;

public static class TodosEndpoints
{
	public static RouteGroupBuilder MapTodosEndpoints(this RouteGroupBuilder group)
	{
		group.MapGet("/todos", async (TodoDb db) =>
		{
			var todos = await db.Todos.Select(x => new TodoDto(x)).ToListAsync();
			return Results.Ok(todos);
		});

		group.MapGet("/todos/{id}", async (int Id, TodoDb Db) =>
			await Db.Todos.FindAsync(Id)
				is Todo todo
					? Results.Ok(new TodoDto(todo))
					: Results.NotFound());

		group.MapDelete("/todos/{id}", async (int id, TodoDb db) =>
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

		group.MapPut("/todos/{id}", async (int id, [FromForm] string name,
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

		group.MapGet("/todos/{id}/attachment", async (int id, TodoDb db) =>
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

		group.MapPost("/ap/todos", async ([AsParameters] NewTodoRequest request, TodoDb db) =>
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

		group.MapPost("/todos", async ([FromForm] string name,
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


		return group;
		
	}
}

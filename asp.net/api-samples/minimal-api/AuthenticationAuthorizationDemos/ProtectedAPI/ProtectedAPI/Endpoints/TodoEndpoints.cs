using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProtectedAPI.Data;
using ProtectedAPI.Model;
using ProtectedAPI.ModelDTO;
using ProtectedAPI.Services;

namespace ProtectedAPI.Endpoints;

public static class TodoEndpoints
{
	public static RouteGroupBuilder MapTodoEndpoints(this RouteGroupBuilder group)
	{
		// GET - public access con gestione errori
		group.MapGet("/todos", async (ILogger<Program> logger, AppDbContext db) =>
		{
			try
			{
				logger.LogInformation("Starting GET /todos endpoint execution");

				// Ottimizzazioni di query
				var todos = await db.Todos
					.AsNoTracking()
					.Select(t => new TodoDTO(t))
					.ToListAsync();

				logger.LogInformation("Retrieved {Count} todos", todos.Count);
				return Results.Ok(todos);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error retrieving todos");
				return Results.Problem("An error occurred while retrieving todos");
			}
		})
		.AllowAnonymous()
		.WithName("GetAllTodos");

		group.MapGet("/todos/{id}", async (AppDbContext db, int id) =>
		{
			var todo = await db.Todos.FindAsync(id);
			return todo is null ? Results.NotFound() : Results.Ok(new TodoDTO(todo));
		})
		.AllowAnonymous();

		// POST - requires Member or Admin role with security stamp validation
		group.MapPost("/todos", async (HttpContext context, AppDbContext db, TodoDTO todoDto) =>
		{
			// Ottieni l'ID dell'utente autenticato
			var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

			var todo = new Todo
			{
				Title = todoDto.Title,
				Description = todoDto.Description,
				IsComplete = todoDto.IsCompleted,
				OwnerId = userId, // Imposta l'OwnerId con l'ID dell'utente autenticato
				CreatedAt = DateTime.UtcNow
			};

			db.Todos.Add(todo);
			await db.SaveChangesAsync();
			return Results.Created($"/todos/{todo.Id}", new TodoDTO(todo));
		})
		.RequireAuthorization("RequireMemberOrAdmin")
		.ValidateSecurityStamp();

		// PUT - requires authentication
		group.MapPut("/todos/{id}", async (HttpContext context, AppDbContext db, int id, TodoDTO todoDto) =>
		{
			if (id != todoDto.Id)
			{
				return Results.BadRequest("Id mismatch");
			}

			var todo = await db.Todos.FindAsync(id);
			if (todo == null)
			{
				return Results.NotFound();
			}

			// Verifica che l'utente sia il proprietario o un admin
			var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
			var isAdmin = context.User.IsInRole("Admin");

			if (!isAdmin && todo.OwnerId != userId)
			{
				return Results.Forbid();
			}

			todo.Title = todoDto.Title;
			todo.Description = todoDto.Description;
			todo.IsComplete = todoDto.IsCompleted;

			await db.SaveChangesAsync();
			return Results.NoContent();
		})
		.RequireAuthorization();

		// DELETE - requires Admin role or ownership
		group.MapDelete("/todos/{id}", async (HttpContext context, AppDbContext db, int id) =>
		{
			var todo = await db.Todos.FindAsync(id);
			if (todo == null)
			{
				return Results.NotFound();
			}

			// Consenti l'eliminazione agli admin o al proprietario
			var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
			var isAdmin = context.User.IsInRole("Admin");

			if (!isAdmin && todo.OwnerId != userId)
			{
				return Results.Forbid();
			}

			db.Todos.Remove(todo);
			await db.SaveChangesAsync();
			return Results.NoContent();
		})
		.RequireAuthorization(); // Cambiato da RequireAdminRole

		return group;
	}
}

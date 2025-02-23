using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProtectedAPI.Data;
using ProtectedAPI.Model;
using ProtectedAPI.ModelDTO;

namespace ProtectedAPI.Endpoints;

public static class TodoEndpoints
{
	public static RouteGroupBuilder MapTodoEndpoints(this RouteGroupBuilder group)
	{
		// GET - public access
		group.MapGet("/todos", async (AppDbContext db) =>
		{
			var todos = await db.Todos.ToListAsync();
			return todos.Select(t => new TodoDTO(t));
		})
		.AllowAnonymous();

		group.MapGet("/todos/{id}", async (AppDbContext db, int id) =>
		{
			var todo = await db.Todos.FindAsync(id);
			return todo is null ? Results.NotFound() : Results.Ok(new TodoDTO(todo));
		})
		.AllowAnonymous();

		// POST - requires Member role
		group.MapPost("/todos", async (AppDbContext db, TodoDTO todoDto) =>
		{
			var todo = new Todo
			{
				Title = todoDto.Title,
				Description = todoDto.Description,
				IsComplete = todoDto.IsCompleted
			};
			db.Todos.Add(todo);
			await db.SaveChangesAsync();
			return Results.Created($"/todos/{todo.Id}", new TodoDTO(todo));
		})
		.RequireAuthorization("RequireMemberRole");

		// PUT - requires authentication
		group.MapPut("/todos/{id}", async (AppDbContext db, int id, TodoDTO todoDto) =>
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

			todo.Title = todoDto.Title;
			todo.Description = todoDto.Description;
			todo.IsComplete = todoDto.IsCompleted;

			await db.SaveChangesAsync();
			return Results.NoContent();
		})
		.RequireAuthorization();

		// DELETE - requires Admin role
		group.MapDelete("/todos/{id}", async (AppDbContext db, int id) =>
		{
			var todo = await db.Todos.FindAsync(id);
			if (todo == null)
			{
				return Results.NotFound();
			}
			db.Todos.Remove(todo);
			await db.SaveChangesAsync();
			return Results.NoContent();
		})
		.RequireAuthorization("RequireAdminRole");

		return group;
	}
}

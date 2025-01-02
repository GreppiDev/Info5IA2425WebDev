using System;
using FilmAPI.Data;
using FilmAPI.Model;
using FilmAPI.ModelDTO;
using Microsoft.EntityFrameworkCore;

namespace FilmAPI.Endpoints;

public static class CinemaEndpoints
{
	public static void MapCinemaEndpoints(this WebApplication app)
	{
		//gestione cinema
		// GET / cinemas
		// - restituisce la lista dei cinema usando cinemaDTO;
		app.MapGet("/cinemas", async (FilmDbContext db) =>
		{
			var cinemas = await db.Cinemas.Select(c => new CinemaDTO(c)).ToListAsync();
			return Results.Ok(cinemas);
		});
		
		// GET / cinemas / {id}
		// - restituisce il cinema con l'id specificato usando cinemaDTO;
		app.MapGet("/cinemas/{id}", async (FilmDbContext db, int id) =>
		{
			var cinema = await db.Cinemas.FindAsync(id);
			if (cinema is null)
			{
				return Results.NotFound();
			}
			return Results.Ok(new CinemaDTO(cinema));
		});
		
		// POST / cinemas
		// - per creare un nuovo cinema;
		app.MapPost("/cinemas", async (FilmDbContext db, CinemaDTO cinemaDto) =>
		{
			var cinema = new Cinema
			{
				Nome = cinemaDto.Nome,
				Città = cinemaDto.Città,
				Indirizzo = cinemaDto.Indirizzo
			};
			db.Cinemas.Add(cinema);
			await db.SaveChangesAsync();
			return Results.Created($"/cinemas/{cinema.Id}", cinema);
		});
		
		// PUT / cinemas / {id}
		// - per modificare un cinema esistente;
		app.MapPut("/cinemas/{id}", async (FilmDbContext db, int id, CinemaDTO cinemaDto) =>
		{
			var cinema = await db.Cinemas.FindAsync(id);
			if (cinema is null)
			{
				return Results.NotFound();
			}
			cinema.Nome = cinemaDto.Nome;
			cinema.Città = cinemaDto.Città;
			cinema.Indirizzo = cinemaDto.Indirizzo;
			await db.SaveChangesAsync();
			return Results.NoContent();
		});
		
		// DELETE / cinemas / {id}
		// - per eliminare un cinema esistente;
		app.MapDelete("/cinemas/{id}", async (FilmDbContext db, int id) =>
		{
			var cinema = await db.Cinemas.FindAsync(id);
			if (cinema is null)
			{
				return Results.NotFound();
			}
			db.Cinemas.Remove(cinema);
			await db.SaveChangesAsync();
			return Results.NoContent();
		});


	}
}

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


	}
}

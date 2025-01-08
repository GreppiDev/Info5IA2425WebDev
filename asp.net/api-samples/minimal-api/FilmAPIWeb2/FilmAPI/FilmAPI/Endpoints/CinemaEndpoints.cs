using FilmAPI.Data;
using FilmAPI.Model;
using FilmAPI.ModelDTO;
using Microsoft.EntityFrameworkCore;

namespace FilmAPI.Endpoints;

public static class CinemaEndpoints
{
	public static RouteGroupBuilder MapCinemaEndpoints(this RouteGroupBuilder group)
	{
		//gestione cinema
		// GET / cinemas
		// - restituisce la lista dei cinema usando cinemaDTO;
		group.MapGet("/cinemas", async (FilmDbContext db) =>
		{
			var cinemas = await db.Cinemas.Select(c => new CinemaDTO(c)).ToListAsync();
			return Results.Ok(cinemas);
		});

		// GET / cinemas / {id}
		// - restituisce il cinema con l'id specificato usando cinemaDTO;
		group.MapGet("/cinemas/{id}", async (FilmDbContext db, int id) =>
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
		group.MapPost("/cinemas", async (FilmDbContext db, CinemaDTO cinemaDto) =>
		{
			var cinema = new Cinema
			{
				Nome = cinemaDto.Nome,
				Città = cinemaDto.Citta,
				Indirizzo = cinemaDto.Indirizzo
			};
			db.Cinemas.Add(cinema);
			await db.SaveChangesAsync();
			return Results.Created($"/cinemas/{cinema.Id}", cinema);
		});

		// PUT / cinemas / {id}
		// - per modificare un cinema esistente;
		group.MapPut("/cinemas/{id}", async (FilmDbContext db, int id, CinemaDTO cinemaDto) =>
		{
			var cinema = await db.Cinemas.FindAsync(id);
			if (cinema is null)
			{
				return Results.NotFound();
			}
			cinema.Nome = cinemaDto.Nome;
			cinema.Città = cinemaDto.Citta;
			cinema.Indirizzo = cinemaDto.Indirizzo;
			await db.SaveChangesAsync();
			return Results.NoContent();
		});

		// DELETE / cinemas / {id}
		// - per eliminare un cinema esistente;
		group.MapDelete("/cinemas/{id}", async (FilmDbContext db, int id) =>
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

		return group;

	}
}

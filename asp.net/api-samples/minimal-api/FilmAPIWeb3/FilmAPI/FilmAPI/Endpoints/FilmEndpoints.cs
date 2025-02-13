using System;
using FilmAPI.Data;
using FilmAPI.Model;
using FilmAPI.ModelDTO;
using Microsoft.EntityFrameworkCore;

namespace FilmAPI.Endpoints;

public static class FilmEndpoints
{
	public static RouteGroupBuilder MapFilmEndpoints(this RouteGroupBuilder group)
	{
		//GET /films/tmdb/{tmdbId}
		//restituisce il film con il TmdbId specificato
		group.MapGet("/films/tmdb/{tmdbId}", async (FilmDbContext db, int tmdbId) =>
		{
			var film = await db.Films.FirstOrDefaultAsync(f => f.TmdbId == tmdbId);
			if (film is null)
			{
				return Results.NotFound();
			}
			return Results.Ok(new FilmDTO(film));
		});
		
		//GET /films
		//restituisce tutti i film
		group.MapGet("/films", async (FilmDbContext db)=> Results.Ok(await db.Films.Select(f =>new FilmDTO(f)).AsNoTracking().ToListAsync()));

		//GET /films/{id}
		//restituisce il film con l'id specificato
		group.MapGet("/films/{id}", async (FilmDbContext db, int id)=>
		{
			Film? film = await db.Films.FindAsync(id);
			if(film is null)
			{
				return Results.NotFound();
			}
			return Results.Ok(new FilmDTO(film));
		});

		//PUT /films/{id} 
		//modifica il film con l'id specificato
		group.MapPut("/films/{id}", async (FilmDbContext db, int id, FilmDTO filmDTO)=>
		{
			//verifico che il film con l'id specificato esista
			Film? film = await db.Films.FindAsync(id);
			if(film is null)
			{
				return Results.NotFound();
			}
			//modifico il film
			film.Titolo = filmDTO.Titolo;
			film.RegistaId = filmDTO.RegistaId;
			film.Durata = filmDTO.Durata;
			film.DataProduzione = filmDTO.DataProduzione;
			if(filmDTO.TmdbId.HasValue)
			{
				// Check if film with same TmdbId already exists
				var existingFilm = await db.Films
					.FirstOrDefaultAsync(f => f.TmdbId == filmDTO.TmdbId);
				if (existingFilm != null)
				{
					return Results.Conflict($"Film with TmdbId {filmDTO.TmdbId} already exists in the database");
				}
			}
			film.TmdbId = filmDTO.TmdbId;
			//salvo il film modificato
			await db.SaveChangesAsync();
			//restituisco la risposta
			return Results.NoContent();
		});
		
		//POST /films
		//crea un nuovo film
		group.MapPost("/films", async (FilmDbContext db, FilmDTO filmDTO)=>
		{
			// Check if film with same TmdbId already exists
			if (filmDTO.TmdbId.HasValue)
			{
				var existingFilm = await db.Films
					.FirstOrDefaultAsync(f => f.TmdbId == filmDTO.TmdbId);
				if (existingFilm != null)
				{
					return Results.Conflict($"Film with TmdbId {filmDTO.TmdbId} already exists in the database");
				}
			}
		
			//creo un nuovo film
			Film film = new()
			{
				Titolo = filmDTO.Titolo,
				RegistaId = filmDTO.RegistaId,
				Durata = filmDTO.Durata,
				DataProduzione = filmDTO.DataProduzione,
				TmdbId = filmDTO.TmdbId
			};
			//aggiungo il film al database
			db.Films.Add(film);
			await db.SaveChangesAsync();
			//restituisco la risposta
			return Results.Created($"/films/{film.Id}", new FilmDTO(film));
		});

		//DELETE /films/{id}
		//elimina il film con l'id specificato
		group.MapDelete("/films/{id}", async (FilmDbContext db, int id) => 
		{
			//verifico che il film con l'id specificato esista
			Film? film = await db.Films.FindAsync(id);
			if( film is null)
			{
				return Results.NotFound();
			}
			//elimino il film
			db.Remove(film);
			await db.SaveChangesAsync();
			//restituisco la risposta
			return Results.NoContent();
		});
		
		return group;
	}

}

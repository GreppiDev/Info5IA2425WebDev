using System;
using FilmAPI.Data;
using FilmAPI.Model;
using FilmAPI.ModelDTO;
using Microsoft.EntityFrameworkCore;

namespace FilmAPI.Endpoints;

public static class FilmEndpoints
{
	public static void MapFilmEndpoints(this WebApplication app)
	{
		//GET /films
		//restituisce tutti i film
		app.MapGet("/films", async (FilmDbContext db)=> Results.Ok(await db.Films.ToListAsync()));

		//GET /films/{id}
		//restituisce il film con l'id specificato
		app.MapGet("/films/{id}", async (FilmDbContext db, int id)=>
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
		app.MapPut("/films/{id}", async (FilmDbContext db, int id, FilmDTO filmDTO)=>
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
			//salvo il film modificato
			db.Add(film);
			await db.SaveChangesAsync();
			//restituisco la risposta
			return Results.NoContent();
		});

		//DELETE /films/{id}
		//elimina il film con l'id specificato
		app.MapDelete("/films/{id}", async (FilmDbContext db, int id) => 
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
	}

}

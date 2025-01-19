using System;
using FilmAPI.Data;
using FilmAPI.Model;
using FilmAPI.ModelDTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilmAPI.Endpoints;

public static class RegistaEndpoints
{
	public static RouteGroupBuilder MapRegistaEndpoints(this RouteGroupBuilder group)
	{

		//GET /registi/{id}/films
		//restituisce tutti i film del regista con l'id specificato
		group.MapGet("/registi/{id}/films", async (FilmDbContext db, int id) =>
		{
			//verifico che il regista con l'id specificato esista
			Regista? regista = await db.Registi.FindAsync(id);
			if (regista is null)
			{
				return Results.NotFound();
			}
			//il regista esiste e recupero i suoi films
			var filmsDelRegista = await db.Films.Where(f => f.RegistaId == regista.Id).Select(f => new FilmDTO(f)).AsNoTracking().ToListAsync();
			return Results.Ok(filmsDelRegista);
		});

		//POST /registi/{id}/films
		//aggiunge un film al regista con l'id specificato
		group.MapPost("/registi/{id}/films", async (FilmDbContext db, int id, FilmDTO filmDTO) =>
		{
			//verifico che il regista con l'id specificato esista
			Regista? regista = await db.Registi.FindAsync(id);
			if (regista is null)
			{
				return Results.NotFound($"Il regista con l'id = {id} non esiste");
			}
			//creo un oggetto film
			Film film = new()
			{
				Titolo = filmDTO.Titolo,
				DataProduzione = filmDTO.DataProduzione,
				Durata = filmDTO.Durata,
				RegistaId = filmDTO.RegistaId
			};
			//salvo il film
			db.Films.Add(film);
			await db.SaveChangesAsync();
			//restituisco la risposta al client
			//creo un nuovo DTO
			FilmDTO returnedFilmDTO = new(film);
			return Results.Created($"/registi/{returnedFilmDTO.Id}/films", returnedFilmDTO);

		});
		//GET /registi
		//restituisce tutti i registi
		_ = group.MapGet("/registi", async (FilmDbContext db, [FromQuery(Name = "cognome")] string? cognome) =>
		{
			if (cognome is not null)
			{
				return Results.Ok(await db.Registi.Where(r => r.Cognome.Contains(cognome)).Select(r => new RegistaDTO(r)).AsNoTracking().ToListAsync());
			}
			return Results.Ok(await db.Registi.Select(r => new RegistaDTO(r)).AsNoTracking().ToListAsync());
		});



		//GET /registi/{id}
		//restituisce il regista con l'id specificato
		group.MapGet("/registi/{id}", async (FilmDbContext db, int id) =>
		{
			Regista? regista = await db.Registi.FindAsync(id);
			if (regista is null)
			{
				return Results.NotFound();
			}
			return Results.Ok(new RegistaDTO(regista));
		});
		//POST /registi
		//crea un nuovo regista
		group.MapPost("/registi", async (FilmDbContext db, RegistaDTO registaDTO) =>
		{
			//non faccio la validazione dell'input
			//creo il regista a partire da RegistaDTO
			Regista regista = new()
			{
				Nome = registaDTO.Nome,
				Cognome = registaDTO.Cognome,
				Nazionalità = registaDTO.Nazionalità
			};
			//aggiungo il regista al DB
			db.Registi.Add(regista);
			//salvo le modifiche
			await db.SaveChangesAsync();
			return Results.Created($"/registi/{regista.Id}", new RegistaDTO(regista));
		});
		//PUT /registi/{id}
		//modifica il regista con l'id specificato
		group.MapPut("/registi/{id}", async (FilmDbContext db, int id, RegistaDTO registaDTO) =>
		{
			//verifico che il regista esista
			Regista? regista = await db.Registi.FindAsync(id);
			if (regista is null)
			{
				return Results.NotFound();
			}
			//aggiorno i campi del regista
			regista.Nome = registaDTO.Nome;
			regista.Cognome = registaDTO.Cognome;
			regista.Nazionalità = registaDTO.Nazionalità;
			//salvo le modifiche
			await db.SaveChangesAsync();
			return Results.NoContent();
		});

		//DELETE /registi/{id}
		//elimina il regista con l'id specificato
		group.MapDelete("/registi/{id}", async (FilmDbContext db, int id) =>
		{
			//verifico che il regista esista
			Regista? regista = await db.Registi.FindAsync(id);
			if (regista is null)
			{
				return Results.NotFound();
			}
			//rimuovo il regista
			db.Registi.Remove(regista);
			//effettuo le modifiche nel database
			await db.SaveChangesAsync();
			//restituisco il codice di 
			return Results.NoContent();
		});

		return group;
	}

}

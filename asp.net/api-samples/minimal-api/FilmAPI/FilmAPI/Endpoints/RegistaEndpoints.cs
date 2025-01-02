using System;
using FilmAPI.Data;
using FilmAPI.Model;
using FilmAPI.ModelDTO;
using Microsoft.EntityFrameworkCore;

namespace FilmAPI.Endpoints;

public static class RegistaEndpoints
{
	public static void MapRegistaEndpoints(this WebApplication app)
	{

		//GET /registi/{id}/films
		//restituisce tutti i film del regista con l'id specificato
		app.MapGet("/registi/{id}/films", async (FilmDbContext db, int id) =>
		{
			//verifico che il regista con l'id specificato esista
			Regista? regista = await db.Registi.FindAsync(id);
			if (regista is null)
			{
				return Results.NotFound();
			}
			//il regista esiste e recupero i suoi films
			var filmsDelRegista = await db.Films.Where(f => f.RegistaId == regista.Id).Select(f => new RegistaDTO(regista)).ToListAsync();
			return Results.Ok(filmsDelRegista);
		});

		//POST /registi/{id}/films
		//aggiunge un film al regista con l'id specificato
		app.MapPost("/registi/{id}/films", async (FilmDbContext db, int id, FilmDTO filmDTO) =>
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
			db.Add(film);
			await db.SaveChangesAsync();
			//restituisco la risposta al client
			//creo un nuovo DTO
			FilmDTO returnedFilmDTO = new FilmDTO(film);
			return Results.Created($"/registi/{returnedFilmDTO.Id}/films", returnedFilmDTO);

		});
		//GET /registi
		//restituisce tutti i registi
		app.MapGet("/registi", async (FilmDbContext db) => Results.Ok(await db.Registi.ToListAsync()));

		//GET /registi/{id}
		//restituisce il regista con l'id specificato
		app.MapGet("/registi/{id}", async (FilmDbContext db, int id)=> 
		{
			Regista? regista = await db.Registi.FindAsync(id);
			if(regista is null)
			{
				return Results.NotFound();
			}
			return Results.Ok(new RegistaDTO(regista));
		});
		//POST /registi
		//crea un nuovo regista
		app.MapPost("/registi", (FilmDbContext db, RegistaDTO registaDTO)=> 
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
			db.SaveChangesAsync();
			return Results.Created($"/registi/{regista.Id}", new RegistaDTO(regista));
		});
		//PUT /registi/{id}
		//modifica il regista con l'id specificato
		app.MapPut("/registi/{id}", async (FilmDbContext db, int id, RegistaDTO registaDTO) => 
		{
			//verifico che il regista esista
			Regista? regista = await db.Registi.FindAsync(id);
			if(regista is null)
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
		app.MapDelete("/registi/{id}", async (FilmDbContext db, int id) => 
		{
			//verifico che il regista esista
			Regista? regista = await db.Registi.FindAsync(id);
			if(regista is null)
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
	}

}

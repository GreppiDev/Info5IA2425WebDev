using System;
using FilmAPI.Data;
using FilmAPI.Model;
using FilmAPI.ModelDTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilmAPI.Endpoints;

public static class ProiezioniEndpoints
{
	public static RouteGroupBuilder MapProiezioniEndpoints(this RouteGroupBuilder group)
	{
		//POST /proiezioni/
		// crea una nuova proiezione;
		group.MapPost("/proiezioni/", async (FilmDbContext db, ProiezioneDTO proiezioneDTO) =>
		{
			//codice che controlla l'integrità referenziale
			Film? film = await db.Films.FindAsync(proiezioneDTO.FilmId);
			if(film is null)
			{
				return Results.NotFound();
			}
			Cinema? cinema = await db.Cinemas.FindAsync(proiezioneDTO.CinemaId);
			if (cinema is null)
			{
				return Results.NotFound();
			}
			//creo un oggetto di tipo Proiezione
			Proiezione proiezione = new ()
			{
				FilmId = proiezioneDTO.FilmId,
				CinemaId = proiezioneDTO.CinemaId,
				Data = proiezioneDTO.Data,
				Ora= proiezioneDTO.Ora
			};
			db.Proiezioni.Add(proiezione);
			await db.SaveChangesAsync();
			return Results.Created();
		});
		
		//GET /proiezioni/
		// restituisce tutte le proiezioni
		group.MapGet("/proiezioni/", async (FilmDbContext db) => Results.Ok(await db.Proiezioni.Select(p => new ProiezioneDTO(p)).AsNoTracking().ToListAsync()));
		
		//GET /proiezioni/{filmId}/{cinemaId}
		// restituisce la proiezione con il filmId e il cinemaId specificati
		// se vengono specificate le date iniziale e finale, restituisce le proiezioni comprese tra quelle date
		// se viene specificata solo la data iniziale, restituisce le proiezioni successive a quella data
		// se viene specificata solo la data finale, restituisce le proiezioni precedenti a quella data
		// se non esiste nessuna proiezione che soddisfi i criteri, restituisce una lista vuota
		group.MapGet("/proiezioni/{filmId}/{cinemaId}", async (FilmDbContext db, int filmId, int cinemaId, [FromQuery(Name = "dataIniziale")] DateOnly? dataIniziale, [FromQuery(Name = "dataFinale")] DateOnly? dataFinale) =>
		{
			IQueryable<Proiezione> query = db.Proiezioni.Where(p => p.FilmId == filmId && p.CinemaId == cinemaId);

			if (dataIniziale.HasValue)
			{
				query = query.Where(p => p.Data >= dataIniziale.Value);
			}

			if (dataFinale.HasValue)
			{
				query = query.Where(p => p.Data <= dataFinale.Value);
			}
			List<ProiezioneDTO> proiezioniDTO = await  query.Select(p => new ProiezioneDTO(p)).AsNoTracking().ToListAsync();
			return Results.Ok(proiezioniDTO);
		});

		////GET /proiezioni/{id}
		// restituisce la proiezione con l'id specificato
		// se non esiste nessuna proiezione con l'id specificato, restituisce NotFound
		group.MapGet("/proiezioni/{id}", async (FilmDbContext db, int id) =>
		{
			Proiezione? proiezione = await db.Proiezioni.FindAsync(id);
			if (proiezione is null)
			{
				return Results.NotFound();
			}
			return Results.Ok(new ProiezioneDTO(proiezione));
		});
		
		return group;
	}
	
}

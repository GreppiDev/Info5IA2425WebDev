using System;
using FilmAPI.Data;
using FilmAPI.Model;
using FilmAPI.ModelDTO;

namespace FilmAPI.Endpoints;

public static class ProiezioniEndpoints
{
	public static void MapProiezioniEndpoints(this WebApplication app)
	{
		//POST /proiezioni/
		// crea una nuova proiezione;
		app.MapPost("/proiezioni/", async (FilmDbContext db, ProiezioneDTO proiezioneDTO) =>
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
	}
}

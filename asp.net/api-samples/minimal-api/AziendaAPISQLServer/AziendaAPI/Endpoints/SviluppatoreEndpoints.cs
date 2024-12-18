using System;
using AziendaAPI.Data;
using AziendaAPI.Model;
using AziendaAPI.ModelDTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AziendaAPI.Endpoints;

public static class SviluppatoreEndpoints
{
	public static void MapSviluppatoreEndpoints(this WebApplication app)
	{
		app.MapGet("/sviluppatori", async (AziendaDbContext db) => Results.Ok(await db.Sviluppatori.Select(x => new SviluppatoreDTO(x)).ToListAsync()));

		app.MapPost("/aziende/{id}/sviluppatori", async (AziendaDbContext db, int id, SviluppatoreDTO sviluppatoreDTO) =>
		{
			Azienda? azienda = await db.Aziende.FindAsync(id);
			if (azienda != null)
			{
				Sviluppatore sviluppatore = new() { Nome = sviluppatoreDTO.Nome, Cognome = sviluppatoreDTO.Cognome, AziendaId = azienda.Id };
				await db.Sviluppatori.AddAsync(sviluppatore);
				await db.SaveChangesAsync();
				return Results.Created($"/aziende/{id}/sviluppatori/{sviluppatore.Id}", new SviluppatoreDTO(sviluppatore));
			}
			else
			{
				return Results.NotFound();
			}
		});

		app.MapGet("/prodotti/{id}/sviluppatori", async (AziendaDbContext db, int id) =>
		{
			// Prodotto? prodotto = await db.Prodotti.
			// 	Where(p => p.Id == id).
			// 	Include(p => p.SviluppaProdotti).
			// 	ThenInclude(s => s.Sviluppatore).
			// 	FirstOrDefaultAsync();
			// if (prodotto != null)
			// {
			// 	return Results.Ok(prodotto.SviluppaProdotti.Select(x => new SviluppatoreDTO(x.Sviluppatore)).ToList());
			// }
			// else
			// {
			// 	return Results.NotFound();
			// }
			
			//versione alternativa
			// Prodotto? prodotto = await db.Prodotti.FindAsync(id);
			// if (prodotto != null)
			// {
			// 	List<Sviluppatore> listaSviluppatori = await db.Sviluppatori
			// .Where(s => s.SviluppaProdotti
			// .Any(sp => sp.ProdottoId == id))
			// .ToListAsync();
			// 	return Results.Ok(listaSviluppatori.Select(s => new SviluppatoreDTO(s)));
			// }
			// else
			// {
			// 	return Results.NotFound();
			// }
			
			//versione alternativa
			var listaSviluppatori = await db.Prodotti
			.Where(p => p.Id == id)
			.SelectMany(p => p.Sviluppatori)
			.ToListAsync();
			return listaSviluppatori is not null? Results.Ok(listaSviluppatori.Select(s => new SviluppatoreDTO(s)))
				: Results.NotFound();
		});

		app.MapGet("/aziende/{id}/sviluppatori",
			async (AziendaDbContext db, int id, [FromQuery(Name = "prodottoId")] int? prodottoId) =>
			{
				//questo è il caso in cui non è stato specificato il prodottoId nella query string
				//si deve restituire l'elenco degli sviluppatori dell'azienda di cui è stato fornito l'aziendaId
				if (prodottoId == null)
				{
					Azienda? azienda = await db.Aziende.
					Where(a => a.Id == id).
					Include(a => a.Sviluppatori).
					FirstOrDefaultAsync();

					if (azienda != null)
					{
						return Results.Ok(azienda.Sviluppatori.Select(x => new SviluppatoreDTO(x)).ToList());
					}
					else
					{
						return Results.NotFound();
					}
				}
				else //questo è il caso in cui è stato specificato anche il prodottoId
				{
					Prodotto? prodotto = await db.Prodotti.FindAsync(prodottoId);
					if (prodotto == null)
					{
						return Results.NotFound();
					}
					else
					{
						//si deve controllare che il prodotto appartenga all'azienda specificata mediante AziendaId
						if (prodotto.AziendaId != id)
						{
							return Results.BadRequest($"Il prodotto con prodottoId={prodottoId} non appartiene alla'azienda con id={id}");
						}
						else
						{
							//si effettua la join tra Sviluppatori e SviluppaProdotti per ottenere gli sviluppatori che 
							//hanno partecipato allo sviluppo di un determinato prodotto
							List<Sviluppatore> listaSviluppatori =
								await db.Sviluppatori.Where(s => s.AziendaId == id).
								Join(db.SviluppaProdotti.Where(sp => sp.ProdottoId == prodottoId),
									s => s.Id,
									sp => sp.SviluppatoreId,
									(s, sp) => s).ToListAsync();
							return Results.Ok(listaSviluppatori.Select(s => new SviluppatoreDTO(s)));
							
							//oppure in maniera alternativa, usando le navigation property
							// List<Sviluppatore> listaSviluppatori = await db.Sviluppatori.
							// Where(s => s.AziendaId == id && s.SviluppaProdotti.Any(sp => sp.ProdottoId == prodottoId)).
							// ToListAsync();
							// return Results.Ok(listaSviluppatori.Select(s => new SviluppatoreDTO(s)));

							//oppure in maniera alternativa, usando le skip navigation property
							// var listaSviluppatori = await db.Prodotti.
							// Where(p => p.Id == prodottoId)
							// .SelectMany(p => p.Sviluppatori)
							// //.Where(s => s.AziendaId == id)
							// .ToListAsync();
							// return Results.Ok(listaSviluppatori.Select(s => new SviluppatoreDTO(s)));
						}

					}

				}

			});

		app.MapGet("/sviluppatori/{id}", async (AziendaDbContext db, int id) =>
		{
			Sviluppatore? sviluppatore = await db.Sviluppatori.FindAsync(id);
			if (sviluppatore != null)
			{
				return Results.Ok(new SviluppatoreDTO(sviluppatore));
			}
			else
			{
				return Results.NotFound();
			}

		});

		app.MapPut("/sviluppatori/{id}", async (AziendaDbContext db, SviluppatoreDTO updateSviluppatore, int id) =>
		{
			Sviluppatore? sviluppatore = await db.Sviluppatori.FindAsync(id);
			if (sviluppatore is null) return Results.NotFound();
			sviluppatore.Nome = updateSviluppatore.Nome;
			sviluppatore.Cognome = updateSviluppatore.Cognome;
			await db.SaveChangesAsync();
			return Results.NoContent();
		});

		app.MapDelete("/sviluppatori/{id}", async (AziendaDbContext db, int id) =>
		{
			Sviluppatore? sviluppatore = await db.Sviluppatori.FindAsync(id);
			if (sviluppatore is null)
			{
				return Results.NotFound();
			}
			else
			{
				//si elimina prima le righe in SviluppaProdotti
				//questa azione è necessaria perché è stato configurato l'opzione .OnDelete(DeleteBehavior.Restrict) sulla tabella SviluppaProdotto
				//nel collegamento sulla chiave esterna verso Sviluppatore
				//Se non avessimo impostato questa opzione sarebbe bastato eliminare lo sviluppatore e, a cascata, sarebbero state eliminate anche tutte le 
				//righe delle tabelle collegate tramite foreign key a quello sviluppatore.
				var righeDaEliminareInSviluppaProdotti = db.SviluppaProdotti.Where(sp => sp.SviluppatoreId == id);
				db.SviluppaProdotti.RemoveRange(righeDaEliminareInSviluppaProdotti);
				//poi elimino il prodotto
				db.Sviluppatori.Remove(sviluppatore);
				//salvo le modifiche nel database
				await db.SaveChangesAsync();
				return Results.Ok();
			}
		});
	}

}

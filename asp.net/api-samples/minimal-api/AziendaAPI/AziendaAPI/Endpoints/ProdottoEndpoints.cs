using AziendaAPI.Data;
using AziendaAPI.Model;
using AziendaAPI.ModelDTO;
using Microsoft.EntityFrameworkCore;

namespace AziendaAPI.Endpoints;

public static class ProdottoEndpoints
{
    public static void MapProdottoEndpoints(this WebApplication app)
    {

        app.MapGet("/aziende/{id}/prodotti", async (AziendaDbContext db, int id) =>
        {
            Azienda? azienda = await db.Aziende.Where(a => a.Id == id).Include(a => a.Prodotti).FirstOrDefaultAsync();
            if (azienda != null)
            {
                return Results.Ok(azienda.Prodotti.Select(p => new ProdottoDTO(p)).ToList());
            }
            else
            {
                return Results.NotFound();
            }
        });

        app.MapPost("/aziende/{id}/prodotti", async (AziendaDbContext db, int id, ProdottoDTO prodottoDTO) =>
        {
            Azienda? azienda = await db.Aziende.FindAsync(id);
            if (azienda != null)
            {
                Prodotto prodotto = new() { Nome = prodottoDTO.Nome, Descrizione = prodottoDTO.Descrizione, AziendaId = azienda.Id };
                await db.Prodotti.AddAsync(prodotto);
                await db.SaveChangesAsync();
                return Results.Created($"/aziende/{id}/prodotti/{prodotto.Id}", new ProdottoDTO(prodotto));
            }
            else
            {
                return Results.NotFound();
            }
        });

        app.MapGet("/prodotti", async (AziendaDbContext db) => Results.Ok(await db.Prodotti.Select(x => new ProdottoDTO(x)).ToListAsync()));

        app.MapGet("/prodotti/{id}", async (AziendaDbContext db, int id) =>
        {
            Prodotto? prodotto = await db.Prodotti.Where(p => p.Id == id).FirstOrDefaultAsync();
            if (prodotto != null)
            {
                return Results.Ok(new ProdottoDTO(prodotto));
            }
            else
            {
                return Results.NotFound();
            }

        });

        app.MapPut("/prodotti/{id}", async (AziendaDbContext db, ProdottoDTO updateProdotto, int id) =>
        {
            Prodotto? prodotto = await db.Prodotti.FindAsync(id);
            if (prodotto is null) return Results.NotFound();
            prodotto.Nome = updateProdotto.Nome;
            prodotto.Descrizione = updateProdotto.Descrizione;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        app.MapDelete("/prodotti/{id}", async (AziendaDbContext db, int id) =>
        {
            Prodotto? prodotto = await db.Prodotti.FindAsync(id);
            if (prodotto is null)
            {
                return Results.NotFound();
            }
            else
            {
                //elimina prima le righe in SviluppaProdotti
                //questa azione è necessaria perché è stato configurato l'opzione .OnDelete(DeleteBehavior.Restrict) sulla tabella SviluppaProdotto
                //nel collegamento sulla chiave esterna verso Prodotto
                //Se non avessimo impostato questa opzione sarebbe bastato eliminare il prodotto e, a cascata, sarebbero state eliminate anche tutte le 
                //righe delle tabelle collegate tramite foreign key a quel prodotto.
                var righeDaEliminareInSviluppaProdotti = db.SviluppaProdotti.Where(sp => sp.ProdottoId == id);
                db.SviluppaProdotti.RemoveRange(righeDaEliminareInSviluppaProdotti);
                //poi elimina il prodotto
                db.Prodotti.Remove(prodotto);
                //salva le modifiche nel database
                await db.SaveChangesAsync();
                return Results.Ok();
            }
        });
    }
}


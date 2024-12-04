using AziendaAPI.Data;
using AziendaAPI.Model;
using AziendaAPI.ModelDTO;
using Microsoft.EntityFrameworkCore;

namespace AziendaAPI.Endpoints;

//gestione aziende
public static class AziendaEndpoints
{
    public static void MapAziendaEndpoints(this WebApplication app)
    {
        app.MapGet("/aziende",
            async (AziendaDbContext db)
            => Results.Ok(await db.Aziende.Select(x => new AziendaDTO(x)).ToListAsync()));

        app.MapPost("/aziende",
            async (AziendaDbContext db, AziendaDTO aziendaDTO) =>
        {
            var azienda = new Azienda { Nome = aziendaDTO.Nome, Indirizzo = aziendaDTO.Indirizzo };
            await db.Aziende.AddAsync(azienda);
            await db.SaveChangesAsync();
            return Results.Created($"/aziende/{azienda.Id}", new AziendaDTO(azienda));
        });

        app.MapGet("/aziende/{id}", async (AziendaDbContext db, int id) =>
        await db.Aziende.FindAsync(id)
         is Azienda azienda
                    ? Results.Ok(new AziendaDTO(azienda))
                    : Results.NotFound()
        );

        app.MapPut("/aziende/{id}", async (AziendaDbContext db, AziendaDTO updateAzienda, int id) =>
        {
            var azienda = await db.Aziende.FindAsync(id);
            if (azienda is null) return Results.NotFound();
            azienda.Nome = updateAzienda.Nome;
            azienda.Indirizzo = updateAzienda.Indirizzo;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
        app.MapDelete("/azienda/{id}", async (AziendaDbContext db, int id) =>
        {
            var azienda = await db.Aziende.FindAsync(id);
            if (azienda is null)
            {
                return Results.NotFound();
            }
            db.Aziende.Remove(azienda);
            await db.SaveChangesAsync();
            return Results.Ok();
        });
    }

}

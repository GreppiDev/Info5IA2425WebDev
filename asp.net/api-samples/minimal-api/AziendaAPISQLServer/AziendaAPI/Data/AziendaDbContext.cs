using System;
using AziendaAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace AziendaAPI.Data;

public class AziendaDbContext(DbContextOptions<AziendaDbContext> options) : DbContext(options)
{
	public DbSet<Azienda> Aziende { get; set; } = null!;
	public DbSet<Prodotto> Prodotti { get; set; } = null!;
	public DbSet<Sviluppatore> Sviluppatori { get; set; } = null!;
	public DbSet<SviluppaProdotto> SviluppaProdotti { get; set; } = null!;
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{

		//se non si utilizzano le convenzioni di denominazione di EF Core
		//è necessario definire le chiavi esterne. In questo esempio la 
		//definizione delle chiavi esterne è fatta mediante due lambda 
		//expressions all'interno del metodo UsingEntity
		modelBuilder.Entity<Sviluppatore>()
			.HasMany(s => s.Prodotti)
			.WithMany(p => p.Sviluppatori)
			.UsingEntity<SviluppaProdotto>(
				//prima lambda expression per definire la chiave esterna su Prodotti
				left => left
					.HasOne(sp => sp.Prodotto)
					.WithMany(p => p.SviluppaProdotti)
					.OnDelete(DeleteBehavior.Restrict)
					.HasForeignKey(sp => sp.ProdottoId),
				//seconda lambda expression per definire la chiave esterna su Sviluppatori
				right => right
					.HasOne(sp => sp.Sviluppatore)
					.WithMany(s => s.SviluppaProdotti)
					.OnDelete(DeleteBehavior.Restrict)
					.HasForeignKey(sp => sp.SviluppatoreId),
				//terza lambda expression per definire la chiave primaria
				join => join.HasKey(sp => new { sp.SviluppatoreId, sp.ProdottoId })
			);

		modelBuilder.Entity<Azienda>().HasData(
			new() { Id = 1, Nome = "Microsoft", Indirizzo = "One Microsoft Way, Redmond, WA 98052, Stati Uniti" },
			new() { Id = 2, Nome = "Google", Indirizzo = "1600 Amphitheatre Pkwy, Mountain View, CA 94043, Stati Uniti" },
			new() { Id = 3, Nome = "Apple", Indirizzo = "1 Apple Park Way Cupertino, California, 95014-0642 United States" }
			);
		modelBuilder.Entity<Prodotto>().HasData(
			new() { Id = 1, Nome = "SuperNote", Descrizione = "Applicazione per la gestione delle Note", AziendaId = 1 },
			new() { Id = 2, Nome = "My Cinema", Descrizione = "Applicazione per la visione di film in streaming", AziendaId = 1 },
			new() { Id = 3, Nome = "SuperCad", Descrizione = "Applicazione per il cad 3d", AziendaId = 2 }
			);
		modelBuilder.Entity<Sviluppatore>().HasData(
			new() { Id = 1, Nome = "Mario", Cognome = "Rossi", AziendaId = 1 },
			new() { Id = 2, Nome = "Giulio", Cognome = "Verdi", AziendaId = 1 },
			new() { Id = 3, Nome = "Leonardo", Cognome = "Bianchi", AziendaId = 2 }
			);
		modelBuilder.Entity<SviluppaProdotto>().HasData(
			new() { SviluppatoreId = 1, ProdottoId = 1 },
			new() { SviluppatoreId = 2, ProdottoId = 1 },
			new() { SviluppatoreId = 3, ProdottoId = 3 }
			);

	}
}
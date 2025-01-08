using System;
using FilmAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace FilmAPI.Data;

public class FilmDbContext:DbContext
{
	//questo costruttore serve per passare le opzioni di configurazione del DBContext da Program.cs
	public FilmDbContext(DbContextOptions<FilmDbContext> opt):base(opt)
	{
		
	}
	
	//definizione delle properties di tipo DbSet<T> --> corrispondono alle tabelle del DB
	public DbSet<Film> Films { get; set; }=null!;
	public DbSet<Regista> Registi { get; set; }=null!;

	//nella prima parte dell'esercizio c'è solo la uno a molti tra Regista e Film e sono state rispettate le convenzioni di EF Core
	//quindi non c'è bisogno di fare override del metodo OnModelCreating

	//aggiungiamo le tabelle di Cinema e Proiezioni
	public DbSet<Cinema> Cinemas { get; set; } = null!;
	public DbSet<Proiezione> Proiezioni { get; set; } = null!;

	//override del metodo OnModelCreating per definire la molti a molti tra Film e Cinema
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		//serve per indicare la molti a moti
		modelBuilder
		.Entity<Film>()
		.HasMany(f => f.Cinemas)
		.WithMany(c => c.Films)
		.UsingEntity<Proiezione>();
		//serve per indicare che la chiave primaria è composta dai campi
		//CinemaId e FilmId
		//modelBuilder.Entity<Proiezione>().HasKey(k => new { k.CinemaId, k.FilmId });
		
		//nella terza migrazione si opta per un id univoco per la tabella Proiezioni
		//in questo modo possono esserci più proiezioni dello stesso film nello stesso cinema in date diverse e con orari diversi
	}
}

using Microsoft.EntityFrameworkCore;
using EducationalGames.Models; 

namespace EducationalGames.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // DbSet per ogni entità
    public DbSet<Utente> Utenti { get; set; } = null!;
    public DbSet<Materia> Materie { get; set; } = null!;
    public DbSet<Argomento> Argomenti { get; set; } = null!;
    public DbSet<Videogioco> Videogiochi { get; set; } = null!;
    public DbSet<ClasseVirtuale> ClassiVirtuali { get; set; } = null!;
    public DbSet<Iscrizione> Iscrizioni { get; set; } = null!;
    public DbSet<ClasseGioco> ClassiGiochi { get; set; } = null!;
    public DbSet<GiocoArgomento> GiochiArgomenti { get; set; } = null!;
    public DbSet<ProgressoStudente> ProgressiStudenti { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurazione Utente
        modelBuilder.Entity<Utente>()
            .HasIndex(u => u.Email)
            .IsUnique();
        modelBuilder.Entity<Utente>()
            .Property(u => u.Ruolo)
            .HasConversion<string>();

        // Configurazione Materia
        modelBuilder.Entity<Materia>()
            .HasIndex(m => m.Nome)
            .IsUnique();

        // Configurazione Argomento
        modelBuilder.Entity<Argomento>()
            .HasIndex(a => a.Nome)
            .IsUnique();

        // Configurazione Videogioco
        modelBuilder.Entity<Videogioco>()
            .HasIndex(v => v.Titolo)
            .IsUnique();
        modelBuilder.Entity<Videogioco>().Property(v => v.DefinizioneGioco).HasColumnType("json");

        // Configurazione ClasseVirtuale
        modelBuilder.Entity<ClasseVirtuale>()
            .HasIndex(cv => cv.CodiceIscrizione)
            .IsUnique();
        modelBuilder.Entity<ClasseVirtuale>()
            .HasIndex(cv => new { cv.DocenteId, cv.Nome })
            .IsUnique();
        // Le FK DocenteId e MateriaId sono definite tramite [ForeignKey] nel modello
        // Ma definiamo qui il comportamento OnDelete
        modelBuilder.Entity<ClasseVirtuale>()
            .HasOne(cv => cv.Docente)
            .WithMany(u => u.ClassiCreate)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ClasseVirtuale>()
            .HasOne(cv => cv.Materia)
            .WithMany(m => m.ClassiVirtuali)
            .OnDelete(DeleteBehavior.Restrict);


        // --- Configurazione Iscrizione (M:N Utente-ClasseVirtuale con Skip Navigation) ---
        modelBuilder.Entity<Utente>()
            .HasMany(u => u.ClassiIscritte) // Skip Navigation da Utente a Classe
            .WithMany(cv => cv.StudentiIscritti) // Skip Navigation da Classe a Utente
            .UsingEntity<Iscrizione>(j => // Specifica l'entità di join Iscrizione
            {
                j.ToTable("ISCRIZIONI"); // Nome tabella
                j.HasKey(i => new { i.StudenteId, i.ClasseId }); // Chiave primaria composita

                // Configura la relazione Iscrizione -> Utente (Studente)
                // e collega alla navigation property Iscrizioni su Utente
                j.HasOne(i => i.Studente)
                 .WithMany(u => u.Iscrizioni) // Collega alla collection di Iscrizione in Utente
                 .OnDelete(DeleteBehavior.Cascade);

                // Configura la relazione Iscrizione -> ClasseVirtuale
                // e collega alla navigation property Iscrizioni su ClasseVirtuale
                j.HasOne(i => i.Classe)
                 .WithMany(cv => cv.Iscrizioni) // Collega alla collection di Iscrizione in ClasseVirtuale
                 .OnDelete(DeleteBehavior.Cascade);

                // Configura proprietà specifiche dell'entità di join Iscrizione
                j.Property(i => i.DataIscrizione)
                 .ValueGeneratedOnAdd()
                 .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

        // --- Configurazione ClasseGioco (M:N ClasseVirtuale-Videogioco con Skip Navigation) ---
        modelBuilder.Entity<ClasseVirtuale>()
            .HasMany(cv => cv.Giochi)
            .WithMany(g => g.ClassiVirtuali)
            .UsingEntity<ClasseGioco>(j =>
            {
                j.ToTable("CLASSI_GIOCHI");
                j.HasKey(cg => new { cg.ClasseId, cg.GiocoId });
                // Le FK sono definite tramite [ForeignKey] in ClasseGioco.cs
                // Definiamo qui il comportamento OnDelete per le relazioni *dalla* join table
                j.HasOne(cg => cg.Classe)
                 .WithMany(cv => cv.ClassiGiochi)
                 .OnDelete(DeleteBehavior.Cascade);
                j.HasOne(cg => cg.Gioco)
                 .WithMany(v => v.ClassiGiochi)
                 .OnDelete(DeleteBehavior.Cascade);
            });


        // --- Configurazione GiocoArgomento (M:N Videogioco-Argomento con Skip Navigation) ---
        modelBuilder.Entity<Videogioco>()
            .HasMany(v => v.Argomenti)
            .WithMany(a => a.Videogiochi)
            .UsingEntity<GiocoArgomento>(j =>
            {
                j.ToTable("GIOCHI_ARGOMENTI");
                j.HasKey(ga => new { ga.GiocoId, ga.ArgomentoId });
                // Le FK sono definite tramite [ForeignKey] in GiocoArgomento.cs
                // Definiamo qui il comportamento OnDelete per le relazioni *dalla* join table
                j.HasOne(ga => ga.Gioco)
                 .WithMany(v => v.GiochiArgomenti)
                 .OnDelete(DeleteBehavior.Cascade);
                j.HasOne(ga => ga.Argomento)
                 .WithMany(a => a.GiochiArgomenti)
                 .OnDelete(DeleteBehavior.Cascade);
            });

        // --- Configurazione ProgressoStudente ---
        modelBuilder.Entity<ProgressoStudente>()
            .HasKey(ps => new { ps.StudenteId, ps.GiocoId, ps.ClasseId });
        // Le FK sono definite tramite [ForeignKey] nel modello
        // Ma definiamo qui il comportamento OnDelete
        modelBuilder.Entity<ProgressoStudente>()
            .HasOne(ps => ps.Studente)
            .WithMany(u => u.Progressi)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ProgressoStudente>()
            .HasOne(ps => ps.Gioco)
            .WithMany(v => v.Progressi)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ProgressoStudente>()
            .HasOne(ps => ps.Classe)
            .WithMany(cv => cv.Progressi)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ProgressoStudente>()
           .Property(ps => ps.UltimoAggiornamento)
           .ValueGeneratedOnAddOrUpdate()
           .HasDefaultValueSql("CURRENT_TIMESTAMP")
           .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
    }
}

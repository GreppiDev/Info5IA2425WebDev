# Indicazioni per lo sviluppo del progetto

Dobbiamo realizzare un prototipo funzionante per la [traccia di esame di maturità di informatica del 2023](https://www.istruzione.it/esame_di_stato/202223/Istituti%20tecnici/Ordinaria/A038_ORD23.pdf) (con particolare riferimento al punto 6 della prima parte della traccia)

L'architettura di riferimento è una **Applicazione Unificata (Minimal API serve sia API che Pagine)**. Questa applicazione sarà strutturata secondo una Multi Page Application (MPA), come indicato nel documento [progetto-educational-games](progetto-educational-games.md).

Procediamo con i seguenti step:

## Setup del progetto

- partiamo da una struttura di progetto derivata dall'esempio `CookieBasedAuthentication`

## Configurazione del database (Modello, DbContext, Migrazione)

- installiamo i pacchetti Nuget aggiuntivi necessari per il progetto
  - `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore`
  - `Microsoft.EntityFrameworkCore.Design`
  - `Pomelo.EntityFrameworkCore.MySql`
- configuriamo la stringa di connessione per il database MariaDb (che è in funzione in locale su un container Docker). Per il momento, in fase di sviluppo, usiamo l'account root, ma successivamente verrà creato un account specifico per l'applicazione:
  in `appsettings.json`:

    ```json
    "ConnectionStrings": {
    "EducationalGamesConnection": "Server=localhost;Port=3306;Database=educational_games;User Id=root;Password=root;"
  }
    ```

- Creiamo la cartella Data con all'interno la classe `AppDbContext` che verrà successivamente popolata con i campi necessari per lo sviluppo del progetto.

    ```cs
    using Microsoft.EntityFrameworkCore;

    namespace EducationalGames.Data;

    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {

    }
    ```

- Aggiungiamo il servizio di connessione al database alla pipeline dell'applicazione:

    ```cs
    //adding services to the container
    if (builder.Environment.IsDevelopment())
    {
        //il servizio AddDatabaseDeveloperPageExceptionFilter andrebbe usato solo in fase di testing e non in produzione.
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
    }
    var connectionString = builder.Configuration.GetConnectionString("EducationalGamesConnection");
    var serverVersion = ServerVersion.AutoDetect(connectionString);
    builder.Services.AddDbContext<AppDbContext>(
            opt => opt.UseMySql(connectionString, serverVersion)
                // The following three options help with debugging, but should
                // be changed or removed for production.
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
        );
    ```

- Nella cartella Models aggiungiamo le classi necessarie a mappare le tabelle del database educational_games definito in [init-db.sql](../EducationaGames/Scripts/init-db.sql). Ad esempio, scriviamo alcune classi del model come segue:

    ```cs
    //Utente.cs
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace EducationalGames.Models
    {
        public enum RuoloUtente
        {
            Docente,
            Studente,
            Admin
        }

        [Table("UTENTI")]
        public class Utente
        {
            [Key]
            [Column("ID_Utente")]
            public int Id { get; set; }

            [Required]
            [StringLength(50)]
            public string Nome { get; set; } = null!;

            [Required]
            [StringLength(50)]
            public string Cognome { get; set; } = null!;

            [Required]
            [StringLength(100)]
            [EmailAddress]
            public string Email { get; set; } = null!;

            [Required]
            [StringLength(255)]
            [Column("PasswordHash")]
            public string PasswordHash { get; set; } = null!; // Hash of the password

            [Required]
            [Column("Ruolo")]
            public RuoloUtente Ruolo { get; set; }

            // Navigation properties
            public virtual ICollection<ClasseVirtuale> ClassiCreate { get; set; } = [];
            public virtual ICollection<Iscrizione> Iscrizioni { get; set; } = [];
            public virtual ICollection<ProgressoStudente> Progressi { get; set; } = [];

            // --- SKIP NAVIGATION PROPERTY (per Studenti) ---
            public virtual ICollection<ClasseVirtuale> ClassiIscritte { get; set; } = [];
        }
    }

    ```

    ```cs
    //GiocoArgomento.cs
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace EducationalGames.Models
    {
        [Table("GIOCHI_ARGOMENTI")]
        public class GiocoArgomento
        {
            [Key] // Part of composite key
            [Column("ID_Gioco")]
            public int GiocoId { get; set; }

            [Key] // Part of composite key
            [Column("ID_Argomento")]
            public int ArgomentoId { get; set; }

            // Navigation properties
            [ForeignKey("GiocoId")]
            public virtual Videogioco Gioco { get; set; } = null!;
            [ForeignKey("ArgomentoId")]
            public virtual Argomento Argomento { get; set; } = null!;
        }
    }
    ```

    ```cs
    //ProgressoStudente.cs
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace EducationalGames.Models
    {
        [Table("PROGRESSI_STUDENTI")]
        public class ProgressoStudente
        {
            [Key] // Part of composite key
            [Column("ID_Studente")]
            public int StudenteId { get; set; }

            [Key] // Part of composite key
            [Column("ID_Gioco")]
            public int GiocoId { get; set; }

            [Key] // Part of composite key
            [Column("ID_Classe")]
            public int ClasseId { get; set; }

            [Required]
            [Column("MoneteRaccolte")]
            public uint MoneteRaccolte { get; set; } = 0; // Use uint for UNSIGNED

            [Column("UltimoAggiornamento")]
            public DateTime UltimoAggiornamento { get; set; } // Default/Update handled by DB

            // Navigation properties
            [ForeignKey("StudenteId")]
            public virtual Utente Studente { get; set; } = null!;
            [ForeignKey("GiocoId")]
            public virtual Videogioco Gioco { get; set; } = null!;
            [ForeignKey("ClasseId")]
            public virtual ClasseVirtuale Classe { get; set; } = null!;
        }
    }
    ```

    Si osservi che nella scrittura delle chiavi esterne con EF Core sono state utilizzate le convenzioni di EF Core:

    1. **Convenzione C# / Entity Framework Core:** In C# e .NET, la convenzione standard è usare **PascalCase** per i nomi delle proprietà (es. `GiocoId`, `ArgomentoId`). Per le chiavi esterne (Foreign Keys), la convenzione specifica di EF Core è spesso quella di usare il nome della *proprietà di navigazione* seguito da `Id` (es., se si ha una proprietà `public virtual Videogioco Gioco { get; set; }`, la chiave esterna corrispondente per convenzione si chiama `GiocoId`).
    2. **Convenzione SQL (nello script SQL):** Nello script [init-db.sql](../EducationaGames/Scripts/init-db.sql), è stata usata una convenzione diversa, **SNAKE_CASE** maiuscolo con un prefisso `ID_` (es. `ID_Gioco`, `ID_Argomento`).

        **Perché EF Core non usa direttamente i nomi SQL?**

        EF Core fa da ponte tra il tuo codice C# (object-oriented) e il database relazionale (SQL). Preferisce seguire le convenzioni del linguaggio C# per rendere il codice più naturale e leggibile per gli sviluppatori .NET.

        **Come avviene il collegamento (Mapping)?**

        Anche se i nomi sono diversi, EF Core è in grado di capire che la proprietà C# `GiocoId` corrisponde alla colonna SQL `ID_Gioco` (e similmente per le altre). Questo avviene principalmente tramite:

        - **Convenzioni:** EF Core ha delle regole predefinite. Se si ha una proprietà di navigazione `Gioco` e una proprietà `GiocoId`, EF Core presume che `GiocoId` sia la chiave esterna per `Gioco`.
        - **Configurazione Esplicita (Fluent API o Data Annotations):** Se le convenzioni non bastano o i nomi sono molto diversi, si può dire esplicitamente a EF Core come mappare una proprietà a una colonna specifica usando:
            - **Fluent API (in `OnModelCreating`):** `modelBuilder.Entity<GiocoArgomento>().Property(ga => ga.GiocoId).HasColumnName("ID_Gioco");`
            - **Data Annotations (sulla proprietà nel modello):** `[Column("ID_Gioco")] public int GiocoId { get; set; }`

        Nel codice `AppDbContext` C# è stata usata la configurazione Fluent API (`HasForeignKey`) per definire le *relazioni* basandoci sulle proprietà con nome convenzionale (`GiocoId`, `ArgomentoId`). Non si sono aggiunte esplicitamente `HasColumnName` perché spesso EF Core (specialmente con provider come Pomelo per MySQL/MariaDB) è abbastanza intelligente da mappare `GiocoId` a `ID_Gioco` (ignorando maiuscole/minuscole e il trattino basso) o la migrazione stessa si occupa di creare la colonna con il nome atteso dal modello C# se il database viene creato da zero tramite migrazioni.

        In sintesi: si usano le convenzioni C# nel codice per coerenza e leggibilità, e si lascia che EF Core gestisca la mappatura verso le convenzioni (potenzialmente diverse) del database, eventualmente aiutandolo con configurazioni esplicite se necessario.
  
- Aggiorniamo la classe `AppDbContext` per fare in modo di avere la corretta implementazione dei DbSet e di configurazioni aggiuntive necessarie per mappare correttamente il database descritto nello script [init-db.sql](../EducationaGames/Scripts/init-db.sql)
  
  ```cs
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
  ```

- assicuriamoci di avere dotnet-ef installato. Nel caso non lo sia si procederà ad installarlo con il comando:
  
  ```ps
    dotnet tool install --global dotnet-ef
  ```

- Effettuiamo la prima migrazione
  
  ```ps
    dotnet ef migrations add InitialCreate --project EducationalGames
  ```

- Applichiamo la migrazione
  
  ```ps
    dotnet ef database update --project EducationalGames
  ```

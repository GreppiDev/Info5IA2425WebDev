# Indicazioni per lo sviluppo del progetto

- [Indicazioni per lo sviluppo del progetto](#indicazioni-per-lo-sviluppo-del-progetto)
  - [Setup del progetto](#setup-del-progetto)
  - [Configurazione del database (Modello, DbContext, Migrazione)](#configurazione-del-database-modello-dbcontext-migrazione)
  - [Autenticazione e autorizzazione degli utenti e seed del database all'avvio (funzionalità non richieste dalla traccia, ma fondamentali per il prototipo)](#autenticazione-e-autorizzazione-degli-utenti-e-seed-del-database-allavvio-funzionalità-non-richieste-dalla-traccia-ma-fondamentali-per-il-prototipo)
    - [Implementazione del seeding del database in produzione](#implementazione-del-seeding-del-database-in-produzione)
    - [Autenticazione basata su Google](#autenticazione-basata-su-google)

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

## Autenticazione e autorizzazione degli utenti e seed del database all'avvio (funzionalità non richieste dalla traccia, ma fondamentali per il prototipo)

Per la gestione degli accessi nel prototipo verrà utilizzato il seguente approccio:

- Definizione dei ruoli degli utenti: `Admin`, `Docente`, `Studente`. Per rendere più semplice la gestione dei permessi verranno attribuiti `ruoli in cascata`, ossia l'utente che ha i permessi associati al ruolo gerarchicamente più alto, riceve anche tutti i permessi dei ruoli gerarchicamente inferiori.
- Accesso autenticato alla piattaforma mediante un meccanismo di accesso basato sulla verifica di `username` (corrispondente alla e-mail dell'utente) e `password`. Le password verranno memorizzate in formato criptato nella tabella degli utenti, ricorrendo agli algoritmi di hashing implementati dalla libreria Identity di Microsoft (nello specifico `PBKDF2 with HMAC-SHA1, 128-bit salt, 256-bit subkey, 1000 iterations`). Durante la procedura di login, dopo aver verificato la corrispondenza tra username (e-mail) e password, verrà creato un cookie con il `ClaimsPrincipal` che rappresenta l'utente autenticato.
- Successivamente verrà implementato anche un meccanismo di accesso autenticato, basato sullo standard `OAuth 2.0`, per l'autenticazione degli utenti mediante Google.
- Gli utenti con ruolo `Docente`, oppure `Studente` possono registrarsi liberamente alla piattaforma, mentre gli utenti `Admin` devono necessariamente essere creati da un altro utente con il ruolo di `Admin`.
- La creazione e il popolamento del database in fase di sviluppo viene effettuato direttamente da codice, pertanto alla partenza dell'applicazione, se il database non esiste viene creato e vengono applicate le migrazioni; inoltre viene creato anche un account amministrativo predefinito le cui credenziali vengono prese dagli `User Secrets`.
- Definizione di policies di sicurezza per l'accesso agli endpoint delle API
- Gestione dei codici di errore del client 401 (tentativo di accesso a risorse che richiedono autenticazione) /403 (tentativo di accesso di utente autenticato, ma senza i permessi richiesti) /404 (tentativo di accesso a risorsa non trovata) per richieste API e per richieste non API.
  - Richiesta API(`/api/...`) con Errore (401/403/404):
    - Il `StatusCodeMiddleware` intercetta l'errore e restituisce una risposta JSON standardizzata.
  - Richiesta NON API (`/pagina-protetta`) che richiede Login (401):
    - `CookieAuthenticationEvents` reindirizza a `/login-required`, che poi reindirizza a `/login-page.html?ReturnUrl=....`
  - Richiesta NON API (`/area-admin` oppure `/area-docente` per utente non autorizzato) senza Permessi (403):
    - `CookieAuthenticationEvents` reindirizza a `/access-denied`, che poi reindirizza a `/access-denied.html?ReturnUrl=....`
  - Richiesta NON API (`/pagina-inesistente.html`) non trovata (404):
    - `UseStatusCodePagesWithRedirects` reindirizza a `/not-found.html`.

Il codice di `Program.cs` nel prototipo diventa:

```cs
using Microsoft.AspNetCore.Authentication.Cookies;
using EducationalGames.Models;
using Microsoft.EntityFrameworkCore;
using EducationalGames.Data;
using EducationalGames.Middlewares;
using EducationalGames.Endpoints;
using Microsoft.AspNetCore.Identity; // Per PasswordHasher
using Microsoft.Extensions.Primitives; // Per StringValues

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "Educational Games v1";
    config.DocumentName = "Educational Games API";
    config.Version = "v1";
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}


// Configura le opzioni di System.Text.Json per gestire gli enum come stringhe
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    // Aggiunge il convertitore che permette di leggere/scrivere enum come stringhe
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());

    // Opzionale: Rende i nomi delle proprietà JSON case-insensitive durante la deserializzazione
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

// Se si stesse usando AddControllers() invece di Minimal API, la configurazione sarebbe simile:
// builder.Services.AddControllers().AddJsonOptions(options => {
//     options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
// });

// --- Configurazione DbContext ---
var connectionString = builder.Configuration.GetConnectionString("EducationalGamesConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'EducationalGamesConnection' not found.");
}
var serverVersion = ServerVersion.AutoDetect(connectionString);
builder.Services.AddDbContext<AppDbContext>(
    opt => opt.UseMySql(connectionString, serverVersion)
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()) // Log sensibili solo in DEV
        .EnableDetailedErrors(builder.Environment.IsDevelopment())      // Errori dettagliati solo in DEV
);

// --- Configurazione Autenticazione Cookie ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = ".AspNetCore.Authentication.EducationalGames"; // Nome specifico per l'app
        options.Cookie.HttpOnly = true;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Esempio: timeout di 60 minuti (resettato da SlidingExpiration)

        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.None
            : CookieSecurePolicy.Always; // Forza HTTPS in produzione

        options.Cookie.SameSite = builder.Environment.IsDevelopment()
            ? SameSiteMode.Lax
            : SameSiteMode.Strict; // Più sicuro in produzione

        options.LoginPath = "/login-required"; // Percorso intermedio gestito sotto
        options.AccessDeniedPath = "/access-denied"; // Percorso intermedio gestito sotto

        // Gestione personalizzata redirect per API vs HTML
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api")) // Se è una richiesta API
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.Headers.Remove("Location");
                }
                else // Altrimenti, redirect standard (che punterà a /login-required)
                {
                    context.Response.Redirect(context.RedirectUri);
                }
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                if (context.Request.Path.StartsWithSegments("/api")) // Se è una richiesta API
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.Headers.Remove("Location");
                }
                else // Altrimenti, redirect standard (che punterà a /access-denied)
                {
                    context.Response.Redirect(context.RedirectUri);
                }
                return Task.CompletedTask;
            }
        };
    });

// --- Configurazione Autorizzazione con Policy ---
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    .AddPolicy("DocenteOnly", policy => policy.RequireRole("Docente"))
    .AddPolicy("AdminOrDocente", policy => policy.RequireRole("Admin", "Docente"))
    .AddPolicy("RegisteredUsers", policy => policy.RequireAuthenticatedUser()); // Richiede solo utente autenticato

// Aggiungi PasswordHasher come servizio
builder.Services.AddScoped<PasswordHasher<Utente>>();


var app = builder.Build();

// --- APPLICA MIGRAZIONI E SEEDING ADMIN ALL'AVVIO ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var passwordHasher = services.GetRequiredService<PasswordHasher<Utente>>();

        // Applica migrazioni (solo in sviluppo)
        if (app.Environment.IsDevelopment())
        {
            logger.LogInformation("Development environment detected. Applying database migrations...");
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");
        }

        // Seed Admin User
        logger.LogInformation("Checking for existing Admin user...");
        if (!await dbContext.Utenti.AnyAsync(u => u.Ruolo == RuoloUtente.Admin))
        {
            logger.LogWarning("No Admin user found. Attempting to seed default Admin...");
            var adminEmail = configuration["DefaultAdminCredentials:Email"];
            var adminPassword = configuration["DefaultAdminCredentials:Password"];
            var adminNome = configuration["DefaultAdminCredentials:Nome"] ?? "Admin";
            var adminCognome = configuration["DefaultAdminCredentials:Cognome"] ?? "Default";

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                logger.LogError("Default Admin Email or Password not found in configuration. Cannot seed Admin user.");
            }
            else if (adminPassword.Length < 8)
            {
                logger.LogError("Default Admin Password must be at least 8 characters long. Cannot seed Admin user.");
            }
            else
            {
                var adminUser = new Utente
                {
                    Nome = adminNome,
                    Cognome = adminCognome,
                    Email = adminEmail,
                    Ruolo = RuoloUtente.Admin
                };
                adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, adminPassword);
                dbContext.Utenti.Add(adminUser);
                await dbContext.SaveChangesAsync();
                logger.LogInformation("Default Admin user '{Email}' created successfully.", adminEmail);
            }
        }
        else
        {
            logger.LogInformation("Admin user already exists. Skipping seeding.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database migration or seeding.");
    }
}
// --- FINE MIGRAZIONI E SEEDING ---


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "Educational Games v1";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Middleware per file statici
app.UseDefaultFiles();
app.UseStaticFiles();

// Middleware Autenticazione/Autorizzazione
app.UseAuthentication();
app.UseAuthorization();

// Middleware per gestire gli errori di stato delle API
// Riepilogo della Logica Risultante:
// Richiesta API(/api/...) con Errore(401/403/404): 
//  Il StatusCodeMiddleware intercetta l'errore e restituisce una risposta JSON standardizzata.
// Richiesta NON API (/pagina-protetta) che richiede Login (401): 
//  CookieAuthenticationEvents reindirizza a /login-required, che poi reindirizza a /login-page.html?ReturnUrl=....
// Richiesta NON API (/area-admin) senza Permessi (403): 
//  CookieAuthenticationEvents reindirizza a /access-denied, che poi reindirizza a /access-denied.html?ReturnUrl=....
// Richiesta NON API (/pagina-inesistente.html) non trovata (404):
//   UseStatusCodePagesWithRedirects reindirizza a /not-found.html.

app.UseMiddleware<StatusCodeMiddleware>();

// Reindirizza a /not-found.html per errori 404 che non sono stati gestiti
// e non sono richieste API (perché il middleware StatusCodeMiddleware
// intercetterebbe gli errori API prima che questo venga eseguito completamente)

// NOTA: Questo catturerà anche richieste a file statici non esistenti.
app.UseStatusCodePagesWithRedirects("/not-found.html");

// Map API endpoints
app.MapGroup("/api/account")
   .WithTags("Account")
   .MapAccountEndpoints();

// Endpoint per gestire i redirect a pagine HTML specifiche
// Questi endpoint vengono chiamati dal middleware dei cookie quando rileva
// una richiesta non API che richiede login o non ha i permessi.
app.MapGet("/login-required", (HttpContext context) =>
{
    // Legge il parametro ReturnUrl aggiunto automaticamente dal middleware
    context.Request.Query.TryGetValue("ReturnUrl", out StringValues returnUrlSv);
    var returnUrl = returnUrlSv.FirstOrDefault();

    // Costruisce l'URL per la pagina di login HTML
    var redirectUrl = "/login-page.html";
    if (!string.IsNullOrEmpty(returnUrl))
    {
        // Aggiunge il ReturnUrl alla pagina di login, così può reindirizzare dopo il login
        redirectUrl += $"?ReturnUrl={Uri.EscapeDataString(returnUrl)}";
    }
    // Esegue il redirect alla pagina di login HTML
    return Results.Redirect(redirectUrl);

}).AllowAnonymous();

app.MapGet("/access-denied", (HttpContext context) =>
{
    // Legge il parametro ReturnUrl aggiunto automaticamente dal middleware
    context.Request.Query.TryGetValue("ReturnUrl", out StringValues returnUrlSv);
    var returnUrl = returnUrlSv.FirstOrDefault();

    // Costruisce l'URL per la pagina di accesso negato HTML
    var redirectUrl = "/access-denied.html";
    if (!string.IsNullOrEmpty(returnUrl))
    {
        // Aggiunge il ReturnUrl alla pagina di accesso negato (utile per logging o messaggi)
        redirectUrl += $"?ReturnUrl={Uri.EscapeDataString(returnUrl)}";
    }
    // Esegue il redirect alla pagina di accesso negato HTML
    return Results.Redirect(redirectUrl);

}).AllowAnonymous();

// Endpoint di fallback per errori generici
app.MapGet("/error", () => Results.Problem("Si è verificato un errore interno.")).AllowAnonymous();


app.Run();
```

Il codice del middleware `StatusCodeMiddleware` per il controllo dei codici di risposta nel caso di richiesta API (`/api/...`) è:

```cs
using System.Text.Json; // Per JsonSerializerOptions

namespace EducationalGames.Middlewares
{
    public class StatusCodeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<StatusCodeMiddleware> _logger;

        // Opzioni per la serializzazione JSON (camelCase)
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };


        public StatusCodeMiddleware(RequestDelegate next, ILogger<StatusCodeMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context); // Esegui il resto della pipeline

            var response = context.Response;

            // Controlla solo se la richiesta è per un'API e la risposta non è già iniziata
            // e c'è un errore client/server rilevante (401, 403, 404).
            if (response.HasStarted || !context.Request.Path.StartsWithSegments("/api") || response.StatusCode < 400 || response.StatusCode >= 600)
            {
                return; // Non fare nulla se non è un errore API gestibile
            }

            // Se è un errore API (401, 403, 404), formatta la risposta come JSON standard
            // senza fare redirect. I redirect per non-API sono gestiti altrove.

            if (response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                _logger.LogWarning("API Request Unauthorized (401) for {Path}", context.Request.Path);
                response.ContentType = "application/json";
                await response.WriteAsJsonAsync(new ApiErrorResponse
                {
                    Status = 401,
                    Title = "Unauthorized",
                    Detail = "Autenticazione richiesta per accedere a questa risorsa API.",
                    Path = context.Request.Path,
                    Timestamp = DateTime.UtcNow
                }, _jsonOptions);
            }
            else if (response.StatusCode == StatusCodes.Status403Forbidden)
            {
                _logger.LogWarning("API Request Forbidden (403) for {Path}", context.Request.Path);
                response.ContentType = "application/json";
                await response.WriteAsJsonAsync(new ApiErrorResponse
                {
                    Status = 403,
                    Title = "Forbidden",
                    Detail = "Non hai i permessi necessari per accedere a questa risorsa API.",
                    Path = context.Request.Path,
                    Timestamp = DateTime.UtcNow
                }, _jsonOptions);
            }
            else if (response.StatusCode == StatusCodes.Status404NotFound)
            {
                _logger.LogWarning("API Resource Not Found (404) for {Path}", context.Request.Path);
                response.ContentType = "application/json";
                await response.WriteAsJsonAsync(new ApiErrorResponse
                {
                    Status = 404,
                    Title = "Not Found",
                    Detail = "La risorsa API richiesta non è stata trovata.",
                    Path = context.Request.Path,
                    Timestamp = DateTime.UtcNow
                }, _jsonOptions);
            }
            // Puoi aggiungere altri 'else if' per gestire altri codici di stato per le API
        }

        // Classe helper per standardizzare le risposte di errore API
        private sealed class ApiErrorResponse
        {
            public int Status { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Detail { get; set; } = string.Empty;
            public string Path { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
        }
    }
}
```

### Implementazione del seeding del database in produzione

Il seeding automatico all'avvio, come implementato nel `Program.cs` (all'interno di `if (app.Environment.IsDevelopment())`), è comodo per lo sviluppo ma **non è la pratica consigliata per la produzione** per diversi motivi:

1. **Controllo:** In produzione, vuoi avere un controllo preciso su quando e come vengono apportate modifiche al database, inclusa l'aggiunta di utenti iniziali. Un avvio automatico potrebbe fallire o avere effetti indesiderati.
2. **Sicurezza delle Credenziali:** Leggere la password dell'admin da `appsettings.Production.json` è rischioso. Anche se usi Azure Key Vault o variabili d'ambiente, eseguire questa logica ad ogni avvio potrebbe esporre le credenziali più del necessario.
3. **Idempotenza:** La logica attuale controlla se l'admin esiste già (`!await dbContext.Utenti.AnyAsync(...)`), il che è buono, ma in scenari complessi, eseguire seeding ad ogni avvio può essere problematico.

**Approcci Comuni per il Seeding in Produzione:**

Due metodi robusti e sicuri per creare l'utente admin iniziale in un ambiente di produzione, tenendo conto dell'hashing della password sono i seguenti:

**Metodo 1: Script SQL Manuale (con Password Pre-Hashed):**

1. **Generare l'Hash:** Bisogna generare l'hash della password che si vuole usare per l'admin *usando lo stesso algoritmo di hashing* che ASP.NET Core Identity utilizza (`PasswordHasher`). Si può farlo:
    - Creando una piccola utility console temporanea che usa `PasswordHasher<Utente>` per generare l'hash di una password data.
    - Temporaneamente aggiungendo un endpoint di debug nella tua applicazione (da rimuovere prima del deploy!) che prende una password e restituisce il suo hash.
    - Eseguendo la logica di hashing in un ambiente di sviluppo e copiando l'hash risultante.
2. **Creare lo Script SQL:** Scrivere uno script SQL `INSERT` che inserisca l'utente admin nella tabella `UTENTI`. Includere tutti i campi necessari (`Nome`, `Cognome`, `Email`, `Ruolo`) e **usare l'hash generato al punto 1** per il campo `PasswordHash`.SQL

    ```sql
    -- Esempio di script SQL (verifica i nomi esatti delle colonne/tabella)
    -- Assicurati che l'email non esista già prima di eseguire!
    INSERT INTO UTENTI (Nome, Cognome, Email, PasswordHash, Ruolo)
    VALUES (
        'Admin',
        'Produzione',
        'admin.prod@tuodominio.com',
        'AQAAAAIAAYagAAAAEBLongGeneratedHashStringFromStep1...', -- Incollare l'hash qui
        'Admin' -- Assumendo che gli enumerativi siano salvati come stringa
    );

    ```

3. **Eseguire lo Script:** Si esegua questo script SQL manualmente sul database di produzione *una sola volta* dopo aver applicato le migrazioni del database e prima di avviare l'applicazione per la prima volta, o come parte del tuo processo di deployment iniziale.

- **Pro:** Controllo completo, nessuna credenziale in chiaro nella configurazione dell'app, pratica standard per DBA.
- **Contro:** Richiede un passaggio manuale o script di deployment separato, devi generare l'hash esternamente.

**Metodo 2: Comando CLI Personalizzato nell'Applicazione:**

1. **Modificare `Program.cs`:** Adattare `Program.cs` per accettare argomenti da riga di comando specifici per il seeding.
2. **Logica di Seeding:** Creare una funzione (es. `SeedAdminUserAsync`) che contenga la logica attuale di seeding (controllo esistenza, lettura credenziali *passate come argomenti*, hashing, salvataggio).
3. **Esecuzione Condizionale:** All'inizio di `Program.cs`, controllare se sono stati passati gli argomenti per il seeding (es. `dotnet run --seed-admin --admin-email="..." --admin-password="..."`). Se sì, eseguire la funzione `SeedAdminUserAsync` e poi terminare l'applicazione (`Environment.Exit(0)`). Altrimenti, procedere con la normale configurazione e avvio del web host (`var app = builder.Build(); ... app.Run();`).
4. **Deployment:** Nello script di deployment per l'ambiente di produzione, dopo aver installato l'applicazione e applicato le migrazioni, eseguire il comando una volta:

    ```sh
    # Esempio (le credenziali vanno passate in modo sicuro, es. da variabili della pipeline)
    dotnet YourApp.dll --seed-admin --admin-email="admin.prod@tuodominio.com" --admin-password="PASSWORD_SICURA_DA_VARIABILE"
    ```

- **Pro:** Riutilizza la logica C# dell'applicazione (DbContext, PasswordHasher), può essere automatizzato negli script di deployment, le credenziali possono essere gestite in modo sicuro tramite variabili d'ambiente o segreti della pipeline di deployment.
- **Contro:** Richiede modifiche a `Program.cs` per gestire gli argomenti, leggermente più complesso dello script SQL.

**In sintesi:** Per la produzione, evitare il seeding automatico all'avvio dell'applicazione web. Scegliere tra uno script SQL manuale (più semplice per un'operazione una tantum) o un comando CLI integrato nell'app (più flessibile e riutilizza la logica C#) per creare l'utente admin iniziale in modo controllato e sicuro.

### Autenticazione basata su Google


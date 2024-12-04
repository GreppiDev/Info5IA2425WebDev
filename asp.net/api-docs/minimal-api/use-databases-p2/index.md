# Un esempio completo di Minimal API con MariaDB: `Azienda API`

- [Un esempio completo di Minimal API con MariaDB: `Azienda API`](#un-esempio-completo-di-minimal-api-con-mariadb-azienda-api)
  - [Traccia del progetto](#traccia-del-progetto)
    - [Gestione delle aziende](#gestione-delle-aziende)
    - [Gestione dei prodotti](#gestione-dei-prodotti)
    - [Gestione degli sviluppatori](#gestione-degli-sviluppatori)
    - [Gestione dei progetti](#gestione-dei-progetti)
  - [Sviluppo del progetto](#sviluppo-del-progetto)
    - [Creazione del Model](#creazione-del-model)
    - [Gestione di una associazione "molti a molti"](#gestione-di-una-associazione-molti-a-molti)
      - [Primo modo per gestire la *molti a molti*](#primo-modo-per-gestire-la-molti-a-molti)
      - [Secondo modo per gestire la *molti a molti*](#secondo-modo-per-gestire-la-molti-a-molti)
    - [Collegamento dell'applicazione con il DBMS](#collegamento-dellapplicazione-con-il-dbms)
    - [Migration](#migration)
    - [Migrazione con `Code First Approach` (solo per sviluppo e testing)](#migrazione-con-code-first-approach-solo-per-sviluppo-e-testing)
    - [Configurazione di Swagger con il View Model](#configurazione-di-swagger-con-il-view-model)
    - [Gestione di tanti Endpoints](#gestione-di-tanti-endpoints)
      - [Il file `Program.cs` finale](#il-file-programcs-finale)
        - [Gestione degli Endpoint (Handlers)](#gestione-degli-endpoint-handlers)

## Traccia del progetto

Implementare un servizio di API per interfacciarsi ad un database di MariaDb (da creare mediante EF Core) con il seguente schema:

```ps1
aziende(Id, Nome, Indirizzo)
prodotti(Id, AziendaId*, Nome, Descrizione)
sviluppa_prodotti(ProdottoId*, SviluppatoreId*)
sviluppatori(Id,AziendaId*, Nome, Cognome)
```

Come si vede tra `sviluppatori` e `prodotti` c’è una tabella di collegamento che rappresenta un'associazione molti a molti, poiché uno sviluppatore può sviluppare più prodotti e uno stesso prodotto può essere sviluppato da più sviluppatori. Sia la tabella degli sviluppatori che quella dei prodotti hanno una chiave esterna sulla tabella delle aziende. Uno sviluppatore lavora presso una sola azienda, ma un’azienda può avere più sviluppatori. Allo stesso modo, un prodotto appartiene ad una sola azienda, ma un’azienda può possedere più prodotti.

:warning: Sul database c'è un vincolo non esprimibile mediante la notazione del modello relazionale: *uno sviluppatore può sviluppare solo prodotti che appartengono alla stessa azienda a cui afferisce lo sviluppatore*. Quest’ultimo vincolo viene garantito dall'applicazione software: quando si inserisce una nuova riga nella tabella sviluppa_prodotti si deve assicurare che sia lo sviluppatore che il prodotto abbiano lo stesso valore della chiave esterna sulla tabella aziende.

Il servizio deve prevedere i seguenti endpoints:

### Gestione delle aziende

- **GET /aziende**

  - restituisce la lista delle aziende

- **POST /aziende**

  - per creare una nuova azienda;

- **GET /aziende/{Id}**

  - restituisce i dati di una specifica azienda

- **PUT /aziende/{Id}**

  - modifica una specifica azienda;

- **DELETE /aziende/{Id}**

  - elimina l'azienda specificata;

### Gestione dei prodotti

- **GET /aziende/{Id}/prodotti**

  - restituisce la lista dei prodotti dell'azienda specificata

- **POST /aziende/{Id}/prodotti**

  - per creare un prodotto appartenente all'azienda specificata

- **GET /prodotti**

  - restituisce tutti i prodotti;

- **GET /prodotti/{Id}**

  - per ottenere il prodotto specificato;

- **PUT /prodotti/{Id}**

  - per modificare il prodotto specificato;

- **DELETE /prodotti/{Id}**

  - per eliminare il prodotto specificato

### Gestione degli sviluppatori

- **GET /sviluppatori**

  - per ottenere la lista di tutti gli sviluppatori;

- **POST /aziende/{Id}/sviluppatori**

  - per creare uno sviluppatore

- **GET /prodotti/{Id}/sviluppatori**

  - per ottenere la lista degli sviluppatori di un determinato prodotto

- **GET /aziende/{Id}/sviluppatori**

  - per ottenere la lista degli sviluppatori di una determinata azienda

- **GET /aziende/{Id}/sviluppatori?prodottoId={prodottoId}**

  - per ottenere la lista degli sviluppatori di una determinata azienda che hanno lavorato a un determinato prodotto. Se non è presente la query string con la chiave prodottoId viene restituita la lista di tutti gli sviluppatori di una determinata azienda.

- **GET /sviluppatori/{Id}**

  - per ottenere i dati di uno sviluppatore

- **PUT /sviluppatori/{Id}**

  - per modificare uno sviluppatore

- **DELETE /sviluppatori/{Id}**

  - per eliminare uno sviluppatore. Elimina anche tutte le associazioni tra lo sviluppatore e i prodotti a cui ha lavorato (progetti)

### Gestione dei progetti

- **POST /sviluppa-prodotto/{sviluppatoreId}/{prodottoId}**

  - per associare uno sviluppatore ad un prodotto. Il prodotto e lo sviluppatore devono esistere. Lo sviluppatore deve appartenere alla stessa azienda a cui appartiene il prodotto.

- **DELETE /sviluppa-prodotto/{sviluppatoreId}/{prodottoId}**

  - per eliminare l'associazione tra uno sviluppatore e un prodotto. Sviluppatore e prodotto devono esistere

## Sviluppo del progetto

Per prima cosa si crei un progetto di `Minimal API ASP.NET`. Si installino i pacchetti richiesti per l'integrazione di EF Core con MariaDb e per il supporto a OpenApi. Nell'elenco riportato di seguito sono indicate alcune versioni pre-release, in attesa che venga rilasciata la versione stabile di `Pomelo.EntityFrameworkCore.MySql` per `net9.0`:

```ps1
 dotnet add package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore --version 9.0.0-preview.1.24081.5
 dotnet add package Microsoft.AspNetCore.OpenApi 
 dotnet add package NSwag.AspNetCore 
 dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.0-preview.1.24081.2
 dotnet add package Pomelo.EntityFrameworkCore.MySql --version 9.0.0-preview.1
```

:memo: **Nota sull'uso di MySQL/MariaDb**: quando si utilizza la libreria Pomelo, il `default charset` (la codifica usata per memorizzare su file i caratteri) è `utf8mb4`[^1][^2][^3], ossia la codifica UTF8 standard a lunghezza variabile fino a 4 byte per carattere. Questa codifica non è supportata dalle vecchie versioni di MySQL, come ad esempio la versione 5.7. In questi casi, per evitare un errore a runtime quando si effettua la migrazione occorre procedere con una configurazione manuale del charset attraverso il metodo `OnConfiguring`, come descritto nella pagina della documentazione della libreria `Pomelo`:

```cs
// The database, tables and columns will explicitly be set to "latin1" by default.

modelBuilder.HasCharSet("latin1");

modelBuilder.Entity<LegacyAsciiStuff>(entity => {

// The "LegacyAsciiStuff" table and all its columns will explicitly set to "ascii" by default.

 entity.HasCharSet("ascii");

// The "LegacyUnicodeTranslation" column uses the deprecated "utf8mb3" character set.
 entity.Property(e => e.LegacyUnicodeTranslation)
  .HasCharSet("utf8mb3");

});
```

**Con MariaDb non c'è bisogno di alcuna configurazione manuale del charset di default, a meno che non sia proprio richiesto dall'applicazione**.

### Creazione del Model

Si crei una cartella `Model` nella quale sverranno scritte le classi del modello, necessarie per la creazione del database. Per ogni tabella si scriva una classe:

```cs
//file Azienda.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace AziendaAPI.Model;

public class Azienda
{
    public int Id { get; set; }

    [Column(TypeName = "nvarchar(100)")]
    public string Nome { get; set; } = null!;

    [Column(TypeName = "nvarchar(100)")]
    public string? Indirizzo { get; set; }
    public List<Prodotto> Prodotti { get; set; } =null!;
    public List<Sviluppatore> Sviluppatori { get; set; } = null!;
}
```

```cs
//file Prodotto.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace AziendaAPI.Model;

public class Prodotto
{
	public int Id { get; set; }
	public int AziendaId { get; set; }
	public Azienda Azienda { get; set; } = null!;

	[Column(TypeName = "nvarchar(100)")]
	public string Nome { get; set; } = null!;

	[Column(TypeName = "nvarchar(200)")]
	public string? Descrizione { get; set; }
	//collection property
	public List<SviluppaProdotto> SviluppaProdotti { get; set; } = null!;
	//skip navigation property
	public List<Sviluppatore> Sviluppatori { get; set; } = null!;
}
```

```cs
//file Sviluppatore.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace AziendaAPI.Model;

public class Sviluppatore
{
	public int Id { get; set; }
	public int AziendaId { get; set; }
	public Azienda Azienda { get; set; } = null!;

	[Column(TypeName = "nvarchar(40)")]
	public string Nome { get; set; } = null!;

	[Column(TypeName = "nvarchar(40)")]
	public string Cognome { get; set; } = null!;
	//collection property
	public List<SviluppaProdotto> SviluppaProdotti { get; set; } = null!;
	//skip navigation property
	public List<Prodotto> Prodotti { get; set; } = null!;
}
```

```cs
//file SviluppaProdotto.cs
namespace AziendaAPI.Model;

public class SviluppaProdotto
{
    public int ProdottoId { get; set; }
    public Prodotto Prodotto { get; set; } = null!;
    public int SviluppatoreId { get; set; }
    public Sviluppatore Sviluppatore { get; set; } = null!;
}
```

Si crei la cartella `Data` con al suo interno il file `AziendaDbContext.cs`

```cs
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
    
		//se non si utilizzano le convenzioni di denominazione di EF Core è necessario definire le chiavi esterne. 
		//In questo esempio la definizione delle chiavi esterne è fatta mediante due lambda 
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
```

### Gestione di una associazione "molti a molti"

I modi per gestire una associazione *"molti a molti"* con EF Core sono molteplici e dipendono anche dal tipo di proprietà di navigazione che sono state inserite nel modello dei dati. Di seguito si riporta una casistica che parte dal presupposto di mappare sempre la tabella di collegamento su un'entità cosiddetta *denominata*, ossia su una classe del modello dei dati.

Supponendo di prendere come esempio l'associazione `SviluppaProdotti` tra `Sviluppatore` e `Prodotto` e assumendo di aver creato su entrambe le entità collegate, sia le collection properties verso `SviluppaProdotto`, che le skip navigation properties verso l'entità collegata (ad esempio una collection di `Prodotto` nella classe `Sviluppatore` e una collection di `Sviluppatore` nella classe `Prodotto`), valgono le seguenti considerazioni:

#### Primo modo per gestire la *molti a molti*

Questo modo di configurare l'associazione "molti a molti" può essere applicato quando si utilizzano le convenzioni di default di EF Core per denominare le chiavi primarie e le chiavi esterne delle entità ([*Conventions for relationship discovery*](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/conventions)).

```cs
//codice da inserire nel metodo OnModelCreating

// si definisce la chiave primaria composta da SviluppatoreId e ProdottoId
// e si definiscono le chiavi esterne su Sviluppatore e Prodotto
//per la gestione della molti a molti "SviluppaProdotto"
modelBuilder.Entity<SviluppaProdotto>()
  .HasKey(sp => new { sp.SviluppatoreId, sp.ProdottoId });//definizione di chiave primaria

//avendo definito le skip navigation property tra Sviluppatore e Prodotto
//occorre istruire EF Core su come gestire la relazione molti a molti
//in questo caso si dice la tabella di collegamento è SviluppaProdotto
modelBuilder.Entity<Sviluppatore>()
.HasMany(s => s.Prodotti)
.WithMany(p => p.Sviluppatori)
.UsingEntity<SviluppaProdotto>();
*/
```

Nel caso che si debba configurare qualche opzione differente dalle impostazioni di default di EF Core, occorre scrivere altro codice nel metodo `OnModelCreating`. Ad esempio, nel caso in cui si debba stabilire l'opzione `ON DELETE RESTRICT` sulle chiavi esterne, occorre aggiungere anche il codice seguente:

```cs
//codice da inserire nel metodo OnModelCreating

//definizione esplicita della chiave esterna su Sviluppatore
modelBuilder.Entity<SviluppaProdotto>()
  .HasOne(sp => sp.Sviluppatore)
  .WithMany(s => s.SviluppaProdotti)
  //https://learn.microsoft.com/en-us/ef/core/saving/cascade-delete
  .OnDelete(DeleteBehavior.Restrict)
  .HasForeignKey(s => s.SviluppatoreId);//definizione di chiave esterna

//definizione esplicita di chiave esterna su Prodotto
		modelBuilder.Entity<SviluppaProdotto>()
			.HasOne(sp => sp.Prodotto)
			.WithMany(c => c.SviluppaProdotti)
			.OnDelete(DeleteBehavior.Restrict)
			.HasForeignKey(bc => bc.ProdottoId);//definizione di chiave esterna
```

:memo: :warning: :fire: **Osservazione importante**: Il vincolo `.OnDelete(DeleteBehavior.Restrict)` non è sempre necessario perché per impostazione predefinita EF Core applica la regola del **"cascade delete"** in tutti i casi in cui la chiave esterna non può assumere il valore nullo, perché la relazione "uno a molti" è obbligatoria; EF Core applica la regola del **"set null"** in tutti i casi in cui la chiave esterna può assumere il valore nullo perché la relazione "uno a molti" è opzionale.

Nel caso del modello di `AziendaApi` tutte le chiavi esterne sono state scritte come obbligatorie e poiché c'è un loop tra le tabelle (si osservi il diagramma E/R per rendersene conto) quando si effettua la creazione del database con SQL Server si otterrebbe un messaggio d'errore che indica un messaggio del tipo "*cycles or multiple cascade paths.* *Specify ON DELETE NO ACTION or ON UPDATE NO ACTION or modify other FOREIGN KEY constraints*". Per evitare questo errore che si verifica con SQL Server e che non si verifica con MySQL/MariaDB, è stata aggiunta la clausola che impone il `DeleteBehavior.Restrict`.

Cosa vuol dire `DeleteBehavior.Restrict` in concreto? Nella [documentazione Microsoft](https://learn.microsoft.com/en-us/ef/core/saving/cascade-delete) viene riportato che:

*For entities being tracked by the context, the values of foreign key properties in dependent entities are set to null when the related principal is deleted.*

*This helps keep the graph of entities in a consistent state while they are being tracked, such that a fully consistent graph can then be written to the database. If a property cannot be set to null because it is not a nullable type, then an exception will be thrown when SaveChanges()is called.*

*Entity Framework Core (EF Core) represents relationships using foreign keys. An entity with a foreign key is the child or dependent entity in the relationship. This entity's foreign key value must match the primary key value (or an alternate key value) of the related principal/parent entity.*

*If the principal/parent entity is deleted, then the foreign key values of the dependents/children will no longer match the primary or alternate key of **any** principal/parent. This is an invalid state and will cause a referential constraint violation in most databases.*

*There are two options to avoid this referential constraint violation:*

1. *Set the FK values to null*
2. *Also delete the dependent/child entities*

*The [first option](https://learn.microsoft.com/en-us/ef/core/saving/cascade-delete#cascading-nulls) is only valid for optional relationships where the foreign key property (and the database column to which it is mapped) must be nullable.*

*The [second option](https://learn.microsoft.com/en-us/ef/core/saving/cascade-delete#when-cascading-behaviors-happen) is valid for any kind of relationship and is known as "cascade delete".*

Con il vincolo imposto sulla "molti a molti" `SviluppaProdotto` bisogna prestare attenzione quando si vuole eliminare un elemento che è collegato a qualche riga della tabella `SviluppaProdotto`. Infatti, ad esempio, se si volesse eliminare uno sviluppatore di cui sono presenti dei prodotti nel database, si dovrebbe prima eliminare tutte le righe nella tabella `SviluppaProdotto` che puntano allo sviluppatore che si vuole eliminare e poi si potrebbe procedere all'eliminazione dello sviluppatore. Allo stesso modo, se si volesse eliminare un prodotto per cui ci sono righe collegate in `SviluppaProdotto` bisognerebbe prima eliminare tali righe in `SviluppaProdotto` e poi si potrebbe procedere all'eliminazione del prodotto.

#### Secondo modo per gestire la *molti a molti*

**Nel caso in cui non siano state utilizzate le convenzioni di default per denominare le chiavi primarie e le chiavi esterne, occorre istruire EF Core, tramite il codice da inserire nel metodo `OnModelCreating`**. Ad esempio, nel caso del progetto `AziendaAPI`, un modo alternativo di definire l'associazione "molti a molti" tra `Sviluppatore` e `Prodotto` è il seguente:

```cs
//se invece non si utilizzano le convenzioni di denominazione di EF Core
			//è necessario definire le chiavi esterne
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
```

### Collegamento dell'applicazione con il DBMS

Prima di poter effettuare la migrazione occorre:

1. configurare il servizio relativo al DBMS MySQL/MariaDb (intervenendo sul file `appsettings.json` e nel codice di `Program.cs`)

2. assicurarsi che il server di MySQL/MariaDB sia raggiungibile dall'applicazione

Per la configurazione del servizio DBMS si può procedere con la descrizione dei servizi e della pipeline dell'app nel file `Program.cs`, in maniera del tutto analoga a quanto visto negli esempi precedenti. Per prima cosa bisogna gestire la connessione con il database MySQL/MariaDb, introducendo nel file **appsettings.json** la stringa di connessione al database:

```json
{
  "ConnectionStrings": {
    "AziendaAPIConnection": "Server=localhost;Port=3306;Database=azienda_api;User Id=root;Password=root;"
  },

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

In questo caso l'accesso è effettuato con le credenziali di `root`, ma è buona norma creare un account specifico per l'app con permessi limitati per questioni di sicurezza.

Il database che sarà creato si chiama **azienda_api** e sarà gestito dal DBMS MySQL/MariaDb in ascolto sulla porta 3306 in localhost.

:memo: :warning: Si noti che nel caso di un'applicazione reale potrebbe succedere che il database non sia installato in `localhost`, ma sia su un altro host nel cloud. In questo caso occorre inserire nella stringa di connessione al posto di `localhost`, l'indirizzo `IP`, oppure il `DNS fully qualified name` della macchina server che ospita il DBMS e usare come porta, quella sulla quale è in ascolto il DBMS.

Per permettere all'applicazione ASP.NET Core di interagire con il database di MySQL/MariaDb occorre associare un servizio all'app che consenta di interfacciarsi con il DBMS. In questo esempio viene utilizzato il provider **Pomelo.EntityFrameworkCore.MySql** che permette di usare EF Core sia con MySQL che con MariaDb.

Nel file `Program.cs` si scriva il codice seguente (dopo la creazione dell'oggetto `builder` e prima della creazione dell'oggetto `app`):

```cs
var connectionString = builder.Configuration.GetConnectionString("AziendaAPIConnection");
var serverVersion = ServerVersion.AutoDetect(connectionString);
builder.Services.AddDbContext<AziendaDbContext>(
        opt => opt.UseMySql(connectionString, serverVersion)
            // The following three options help with debugging, but should
            // be changed or removed for production.
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
    );
```

Questo codice consente di iniettare nella pipeline dell'`app` il servizio che permetterà di interfacciarci con MariaDb.

### Migration

Si effettui la migrazione del database con i comandi della `.NET CLI`. Con la shell posizionata sulla cartella del progetto:

```ps1
dotnet ef migrations add InitialMigrate
```

Per creare il database fisico si può procedere in diversi modi:

1. eseguendo il comando della `:NET CLI` per l'aggiornamento del database in base alla migrazione fatta.

   ```ps1
	dotnet ef database update
	```

2. effettuando la migrazione direttamente da codice, come sarà mostrato nei prossimi paragrafi.

Nel paragrafo seguente viene mostrato come realizzare un View Model per i dati esposti dalle REST API

### View Model per i dati (Data Transfer Object DTO)

Prima di procedere con la scrittura del codice dell'applicazione web si proceda alla scrittura di una View per il modello dei dati che le API esporranno alle applicazioni client. Infatti, il modello dei dati utilizzato dall'applicazione contiene una serie di componenti che sono funzionali al modo di gestire i dati mediante l'ORM (Object Relational Mapper) `EF Core`, ma che non devono necessariamente essere esposte alle applicazioni client. Ad esempio, le componenti di tipo reference, usate per le chiavi esterne, oppure le collections introdotte per creare le cosiddette navigation properties non dovrebbero comparire negli oggetti restituiti dalle Web API. Tutti questi dettagli devono essere filtrati alle applicazioni client. Per fare questo verrà utilizzato il concetto di [`Data Transfer Object`](https://en.wikipedia.org/wiki/Data_transfer_object), già illustrato negli esempi precedenti.

Si può fare a meno dei DTO? Praticamente sì, ma non è una buona idea. Basta leggere alcuni articoli online per rendersene conto, come, ad esempio:

- [**REST API - DTOs or not?**](https://stackoverflow.com/a/36175349).

	*This pattern was created with a very well defined purpose: **transfer data to *remote interfaces***, just like **web services**. This pattern fits very well in a REST API and DTOs will give you more **flexibility** in the long run.*

	*The models that represent the **domain** of your application and the models that represent the **data handled by your API** are (or at least should be) **different concerns** and should be **decoupled** from each other. You don't want to break your API clients when you add, remove or rename a field from the application domain model.*

	*While your service layer operates over the domain/persistence models, your API controllers should operate over a different set of models. As your domain/persistence models evolve to support new business requirements, for example, you may want to create new versions of the API models to support these changes. You also may want to deprecate the old versions of your API as new versions are released. And it's perfectly possible to achieve when the things are decoupled.*

	*Just to mention a few benefits of exposing DTOs instead of persistence models:*

	- *Decouple* persistence models from API models.
	- DTOs can be *tailored* to your needs and they are great when exposing only a set of attributes of your persistence entities. You won't need annotations such as [@XmlTransient](http://docs.oracle.com/javaee/7/api/javax/xml/bind/annotation/XmlTransient.html) and [@JsonIgnore](https://fasterxml.github.io/jackson-annotations/javadoc/2.7/com/fasterxml/jackson/annotation/JsonIgnore.html) to avoid the serialization of some attributes.
	- By using DTOs, you will avoid a *hell of annotations* in your persistence entities, that is, your persistence entities won't be bloated with non persistence related annotations.
	- You will have *full control* over the attributes you are receiving when creating or updating a resource.
	- If you are using [Swagger](https://github.com/swagger-api/swagger-core), you can use [@ApiModel](https://github.com/swagger-api/swagger-core/wiki/Annotations-1.5.X#apimodel) and [@ApiModelProperty](https://github.com/swagger-api/swagger-core/wiki/Annotations-1.5.X#apimodelproperty) annotations to document your API models without messing your persistence entities.
	- You can have different DTOs for each version of your API.
	- You'll have more flexibility when mapping relationships.
	- You can have different DTOs for different media types.
	- Your DTOs can have a list of links for [HATEOAS](https://en.wikipedia.org/wiki/HATEOAS). That's the kind of thing that shouldn't be added to persistence objects. When using [Spring HATEOAS](https://spring.io/projects/spring-hateoas), you can make your DTO classes extend [RepresentationModel](https://docs.spring.io/spring-hateoas/docs/current/api/org/springframework/hateoas/RepresentationModel.html) (formerly known as [ResourceSupport](https://docs.spring.io/spring-hateoas/docs/0.25.0.RELEASE/api/org/springframework/hateoas/ResourceSupport.html)) or wrap them with [EntityModel](https://docs.spring.io/spring-hateoas/docs/current/api/org/springframework/hateoas/EntityModel.html) (formerly known as [Resource](https://docs.spring.io/spring-hateoas/docs/0.25.0.RELEASE/api/org/springframework/hateoas/Resource.html)).

- [**Why you need to use DTO's in your REST API**](https://medium.com/@enocklubowa/why-you-need-to-use-dtos-in-your-rest-api-d9d6d7be5450)

Si crei la cartella `ModelDTO` con dentro le versioni DTO dei dati del modello:

```cs
//file AziendaDTO.cs
using AziendaAPI.Model;

namespace AziendaAPI.ModelDTO;

public class AziendaDTO
{
    public int AziendaId { get; set; }
    public string Nome { get; set; } = null!;
    public string? Indirizzo { get; set; }
    public AziendaDTO() { }
    public AziendaDTO(Azienda azienda) =>
    (AziendaId, Nome, Indirizzo) = (azienda.Id, azienda.Nome, azienda.Indirizzo);
}
```

```cs
//file ProdottoDTO.cs
using AziendaAPI.Model;

namespace AziendaAPI.ModelDTO;

public class ProdottoDTO
{
    public int ProdottoId { get; set; }
    public int AziendaId { get; set; }
    public string Nome { get; set; } = null!;
    public string? Descrizione { get; set; }
    public ProdottoDTO() { }
    public ProdottoDTO(Prodotto prodotto) =>
    (ProdottoId, AziendaId, Nome, Descrizione) = (prodotto.Id, prodotto.AziendaId, prodotto.Nome, prodotto.Descrizione);

}
```

```cs
//file SviluppatoreDTO.cs
using AziendaAPI.Model;

namespace AziendaAPI.ModelDTO;

public class SviluppatoreDTO
{
    public int SviluppatoreId { get; set; }
    public int AziendaId { get; set; }
    public string Nome { get; set; } = null!;
    public string Cognome { get; set; } = null!;
    public SviluppatoreDTO() { }
    public SviluppatoreDTO(Sviluppatore sviluppatore) =>
    (SviluppatoreId, AziendaId, Nome, Cognome) = (sviluppatore.Id, sviluppatore.AziendaId, sviluppatore.Nome, sviluppatore.Cognome);
}
```

Si noti che non è stata creata una versione `DTO` della classe `SviluppaProdotto` perché le API non restituiscono valori di questa classe, che serve solo per descrivere i collegamenti tra `Sviluppatore` e `Prodotto`.

### Migrazione con `Code First Approach` (solo per sviluppo e testing)

Per poter creare il database direttamente dal codice occorre che:

1. il DBMS sia in ascolto all'indirizzo e porta specificati nella stringa di connessione

2. iniettare un servizio nell'app che effettui la migrazione se richiesto

```cs
//questa parte va messa dopo aver creato l'app a partire dal builder
//https://stackoverflow.com/questions/42355481/auto-create-database-in-entity-framework-core
//https://stackoverflow.com/a/55232983

//crea il database se non esiste a partire dalla migrazione (che deve esistere)
//se non è stata fatta la migrazione il database non verrà creato
using (var serviceScope = app.Services.CreateScope())

{
     var context = serviceScope.ServiceProvider.GetRequiredService<AziendaDbContext>();
     context.Database.Migrate();

    //vedere IN ALTERNATIVA al comando context.Database.Migrate() anche
    //context.Database.EnsureCreated();

}
```

L'istruzione `context.Database.Migrate()` effettua in automatico la migrazione direttamente quando si manda in esecuzione l'app: **se il database non esiste viene creato ed eventualmente anche popolato con i dati che sono stati inseriti nei file di migration. Se il database esiste già non verrà eseguita alcuna azione.**

### Configurazione di Swagger con il View Model

Per evitare che `Swagger` riporti come schema dei dati restituiti dagli endpoints delle API i modelli DTO è possibile istruire il servizio in modo tale da togliere il suffisso DTO ai nomi delle classi esposte. In questo modo le applicazioni client vedranno la descrizione dei tipi senza il suffisso DTO:

```cs
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
//adding API explorer
builder.Services.AddEndpointsApiExplorer();
//adding OpenAPI configuration
// builder.Services.AddOpenApiDocument(config =>
// {
//     config.DocumentName = "AziendaAPIv1";
//     config.Title = "AziendaAPI v1";
//     config.Version = "v1";
// });
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "AziendaAPIv1";
    config.Title = "AziendaAPI v1";
    config.Version = "v1";

    config.PostProcess = document =>
      {
          var schemasToRename = document.Components.Schemas
              .Where(kv => kv.Key.EndsWith("DTO", StringComparison.OrdinalIgnoreCase))
              .ToList(); // Crea una lista temporanea delle chiavi da rinominare

          foreach (var schema in schemasToRename)
          {
              var originalName = schema.Key;
              var newName = originalName[..^3];

              if (!document.Components.Schemas.ContainsKey(newName)) // Si evitano i duplicati
              {
                  document.Components.Schemas.Add(newName, schema.Value);
              }

              document.Components.Schemas.Remove(originalName);
          }
      };
});
```

### Gestione di tanti Endpoints

In questo esempio ci sono molte rotte e per ognuna di queste occorre definire del codice che descriva l’azione da compiere. In questo caso il file `Program.cs` diventerebbe molto grande con moltissime righe di codice. Per evitare di dover gestire tutto il codice all’interno del file `Program.cs` useremo una tecnica basata sulla scrittura di metodi di estensione. Questo non è il metodo migliore in assoluto, ma è sicuramente uno dei più semplice. Ad esempio, esistono altri meccanismi più complessi basati sulla reflection, che permettono di ridurre ulteriormente il codice inserito nel file `Program.cs`, ma richiedono alcune competenze avanzate di programmazione relative alla reflection delle classi.

#### Il file `Program.cs` finale

```cs
//file Program.cs

using AziendaAPI.Data;
using AziendaAPI.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
//adding API explorer
builder.Services.AddEndpointsApiExplorer();
//adding OpenAPI configuration
// builder.Services.AddOpenApiDocument(config =>
// {
//     config.DocumentName = "AziendaAPIv1";
//     config.Title = "AziendaAPI v1";
//     config.Version = "v1";
// });
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "AziendaAPIv1";
    config.Title = "AziendaAPI v1";
    config.Version = "v1";

    config.PostProcess = document =>
      {
          var schemasToRename = document.Components.Schemas
              .Where(kv => kv.Key.EndsWith("DTO", StringComparison.OrdinalIgnoreCase))
              .ToList(); // Creiamo una lista temporanea delle chiavi da rinominare

          foreach (var schema in schemasToRename)
          {
              var originalName = schema.Key;
              var newName = originalName[..^3];

              if (!document.Components.Schemas.ContainsKey(newName)) // Evitiamo duplicati
              {
                  document.Components.Schemas.Add(newName, schema.Value);
              }

              document.Components.Schemas.Remove(originalName);
          }
      };
});
//adding services to the container
if (builder.Environment.IsDevelopment())
{
    //il servizio AddDatabaseDeveloperPageExceptionFilter andrebbe usato solo in fase di testing e non in produzione.
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
var connectionString = builder.Configuration.GetConnectionString("AziendaAPIConnection");
var serverVersion = ServerVersion.AutoDetect(connectionString);
builder.Services.AddDbContext<AziendaDbContext>(
        opt => opt.UseMySql(connectionString, serverVersion)
            // The following three options help with debugging, but should
            // be changed or removed for production.
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
    );

var app = builder.Build();
//app.UseHttpsRedirection();

//adding middleware for Swagger
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>

    {
        config.DocumentTitle = "AziendaAPI v1";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";

    });
}

using (var serviceScope = app.Services.CreateScope())

{
    var context = serviceScope.ServiceProvider.GetRequiredService<AziendaDbContext>();
    context.Database.Migrate();
    //vedere IN ALTERNATIVA al comando context.Database.Migrate() anche
    //context.Database.EnsureCreated();

}

//app.UseHttpsRedirection();
//app.MapGet("/", () => "Hello World!");
//I metodi seguenti verranno implementati in classi di estensione (descritte più avanti)
app.MapAziendaEndpoints();
app.MapProdottoEndpoints();
app.MapSviluppatoreEndpoints();
app.MapSviluppaProdottoEndpoints();

app.Run();
```

##### Gestione degli Endpoint (Handlers)

Creiamo la cartella `Endpoints` con al suo interno i file seguenti:

```cs
//file AziendaEndpoints.cs

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
```

```cs
//file ProdottoEndpoints.cs

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
```

```cs
//file SviluppatoreEndpoints.cs

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
            Prodotto? prodotto = await db.Prodotti.
                Where(p => p.Id == id).
                Include(p => p.SviluppaProdotti).
                ThenInclude(s => s.Sviluppatore).
                FirstOrDefaultAsync();
            if (prodotto != null)
            {
                return Results.Ok(prodotto.SviluppaProdotti.Select(x => new SviluppatoreDTO(x.Sviluppatore)).ToList());
            }
            else
            {
                return Results.NotFound();
            }
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
```

```cs
//file SviluppaProdottiEndpoints.cs

using System;
using AziendaAPI.Data;
using AziendaAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace AziendaAPI.Endpoints;

public static class SviluppaProdottiEndpoints
{

    public static void MapSviluppaProdottoEndpoints(this WebApplication app)
    {
        app.MapPost("/sviluppa-prodotto/{sviluppatoreId}/{prodottoId}",
            async (AziendaDbContext db, int sviluppatoreId, int prodottoId) =>
            {
                Prodotto? prodotto = await db.Prodotti.FindAsync(prodottoId);
                Sviluppatore? sviluppatore = await db.Sviluppatori.FindAsync(sviluppatoreId);

                //controlla che l'associazione non sia già stata creata
                SviluppaProdotto? rigaInTabella = await db.SviluppaProdotti.
                    Where(sp => sp.SviluppatoreId == sviluppatoreId && sp.ProdottoId == prodottoId).
                    FirstOrDefaultAsync();
                //la riga esiste già - nessuna azione da effettuare
                if (rigaInTabella != null)
                {

                    return Results.NoContent();
                }
                //la riga non esiste e ci sono il prodotto e lo sviluppatore nelle rispettive tabelle
                if (prodotto != null && sviluppatore != null)
                {
                    //controlla che sviluppatore e prodotto appartengano alla stessa azienda
                    bool prodottoSviluppatoreStessaAzienda = prodotto.AziendaId == sviluppatore.AziendaId;
                    //si deve creare la riga in tabella
                    if (prodottoSviluppatoreStessaAzienda)
                    {
                        var rigaDaCreare = new SviluppaProdotto() { ProdottoId = prodottoId, SviluppatoreId = sviluppatoreId };
                        db.SviluppaProdotti.Add(rigaDaCreare);
                        await db.SaveChangesAsync();
                        return Results.NoContent();
                    }
                    else //sviluppatore e prodotto NON appartengano alla stessa azienda
                    {
                        return Results.BadRequest($"Sviluppatore e prodotto non appartengono alla stessa azienda");
                    }
                }
                else //almeno uno dei due id non è stato trovato
                {
                    return Results.NotFound();
                }
            });

        app.MapDelete("/sviluppa-prodotto/{sviluppatoreId}/{prodottoId}",
            async (AziendaDbContext db, int sviluppatoreId, int prodottoId) =>
            {
                Prodotto? prodotto = await db.Prodotti.FindAsync(prodottoId);
                Sviluppatore? sviluppatore = await db.Sviluppatori.FindAsync(sviluppatoreId);
                if (sviluppatore != null && prodotto != null)
                {
                    //controlla che l'associazione tra sviluppatore e prodotto esista
                    SviluppaProdotto? rigaInTabella = await db.SviluppaProdotti.
                    Where(sp => sp.SviluppatoreId == sviluppatoreId && sp.ProdottoId == prodottoId).
                    FirstOrDefaultAsync();
                    //l'associazione esiste e va eliminata
                    if (rigaInTabella != null)
                    {
                        db.SviluppaProdotti.Remove(rigaInTabella);
                        await db.SaveChangesAsync();
                        return Results.NoContent();
                    }
                    else//l'associazione non esiste
                    {
                        return Results.NotFound();
                    }
                }
                else //non sono stati trovati lo sviluppatore e/o il prodotto
                {
                    return Results.NotFound();
                }
            });
    }

}
```

[^1]: https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql/wiki/Character-Sets-and-Collations

[^2]: https://dev.mysql.com/doc/refman/8.0/en/charset-unicode-utf8mb4.html

[^3]: https://en.wikipedia.org/wiki/UTF-8
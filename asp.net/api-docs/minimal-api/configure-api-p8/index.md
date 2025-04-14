# Eseguire SQL Raw in ASP.NET Minimal API con EF Core e MariaDB

- [Eseguire SQL Raw in ASP.NET Minimal API con EF Core e MariaDB](#eseguire-sql-raw-in-aspnet-minimal-api-con-ef-core-e-mariadb)
  - [Obiettivi](#obiettivi)
  - [Prerequisiti](#prerequisiti)
  - [Introduzione: Perché SQL Raw?](#introduzione-perché-sql-raw)
  - [Metodi EF Core per Query SQL Raw che Restituiscono Tipi Entità](#metodi-ef-core-per-query-sql-raw-che-restituiscono-tipi-entità)
  - [SQL Injection: comprendere il rischio](#sql-injection-comprendere-il-rischio)
  - [Gestire SQL dinamico e limiti della parametrizzazione](#gestire-sql-dinamico-e-limiti-della-parametrizzazione)
  - [Metodi EF Core per Query SQL Raw che restituiscono tipi qualsiasi](#metodi-ef-core-per-query-sql-raw-che-restituiscono-tipi-qualsiasi)
  - [Metodi EF Core per Comandi SQL Raw (Non-Query, ossia per comandi che non restituiscono righe)](#metodi-ef-core-per-comandi-sql-raw-non-query-ossia-per-comandi-che-non-restituiscono-righe)
  - [Raccomandazioni: Quale Metodo Usare?](#raccomandazioni-quale-metodo-usare)
  - [Tabella Riepilogativa dei Metodi](#tabella-riepilogativa-dei-metodi)
  - [Applicazione Pratica: AziendaAPI con SQL Raw](#applicazione-pratica-aziendaapi-con-sql-raw)
  - [Considerazioni sul provider Pomelo](#considerazioni-sul-provider-pomelo)
  - [Conclusioni](#conclusioni)

## Obiettivi

1. Comprendere quando e perché utilizzare SQL raw al posto di LINQ to Entities.
2. Conoscere i diversi metodi offerti da EF Core per eseguire query e comandi SQL raw.
3. Imparare a utilizzare `FromSqlRaw`, `FromSqlInterpolated`, `SqlQueryRaw<T>` e `DbContext.Database.ExecuteSql*`.
4. Capire i rischi di sicurezza (SQL Injection) e come mitigarli.
5. Applicare queste tecniche per implementare endpoint CRUD in una Minimal API collegata a MariaDB tramite Pomelo.

## Prerequisiti

- Conoscenza di base di C# e ASP.NET Minimal API.
- Esperienza con EF Core e LINQ per query su database.
- Comprensione dei concetti base di SQL (SELECT, INSERT, UPDATE, DELETE).
- Familiarità con il setup di un progetto Minimal API con EF Core e Pomelo per MariaDB.

## Introduzione: Perché SQL Raw?

- **Recap veloce:** EF Core traduce LINQ in SQL. Questo è potente e comodo per la maggior parte dei casi (type safety, refactoring facile, astrazione dal DB).
- **Limitazioni di LINQ/EF Core:**
    - **Performance:** A volte, una query SQL scritta a mano può essere più ottimizzata di quella generata da EF Core, specialmente per query molto complesse.
    - **Query Complesse:** Alcune query SQL (es. con funzioni specifiche del DB) possono essere difficili o impossibili da esprimere con LINQ.
    - **Stored Procedure/Funzioni:** Eseguire stored procedure o funzioni definite nel database.
    - **Bulk Operations:** Operazioni massicce (anche se EF Core ha migliorato il batching, SQL raw può dare più controllo).
- **Attenzione alla Sicurezza:** Il rischio principale con SQL raw è l'**SQL Injection**. Vedremo come EF Core ci aiuta a prevenirlo.
- **Contesto:** Useremo EF Core con il provider Pomelo per MariaDB in un'applicazione ASP.NET Minimal API.

## Metodi EF Core per Query SQL Raw che Restituiscono Tipi Entità

- I metodi riportati di seguito sono usati quando il risultato SQL corrisponde a un'entità del `DbContext` (ossia un tipo definito come `DbSet<TEntity>` nel `DbContext`). Si applicano direttamente a `DbSet<TEntity>`.

    - **`FromSqlRaw(string sql, params object[] parameters)`:**

        - Esegue SQL con parametri posizionali (`{0}`, `{1}`).
        - Sicuro contro SQL Injection **SE** si usano i parametri e non si concatena input esterno nella stringa `sql`.
        - Utile per query complesse o quando si necessita controllo fine sui `DbParameter`.
        - **Esempio:**

            ```cs
            string searchName = "Mario Rossi";
            var users = await dbContext.Users
                                     .FromSqlRaw("SELECT * FROM Users WHERE Name = {0}", searchName)
                                     .ToListAsync();
            ```

    - **`FromSql(FormattableString sql)` (per EF Core dalla versione 7.0 in poi) / `FromSqlInterpolated(FormattableString sql)` (per versioni di EF Core precedenti alla 7.0):**

        - Usa l'**interpolazione di stringhe C#** (`$""`) per creare query leggibili e sicure.
        - **Come funziona la sicurezza:** Anche se sembra una normale interpolazione, EF Core **non** inserisce direttamente i valori nella stringa SQL. Invece, analizza la `FormattableString`, crea oggetti `DbParameter` per ogni variabile interpolata (`{variabile}`) e inserisce i nomi dei parametri generati nella stringa SQL (`@p0`, `@p1`, ...). Questo previene automaticamente SQL Injection per i valori interpolati.
        - È il metodo **preferito** per la maggior parte degli scenari di query su entità grazie alla sua sicurezza e leggibilità.
        - **Esempio (dal tuo input - `Book`):**

            ```cs
            // Modelli (semplificati per l'esempio)
            public class Book {
                public int BookId { get; set; }
                public string Title { get; set; }
                // public int AuthorId { get; set; } // Altre proprietà...
            }
            public class LibraryContext : DbContext {
                 public DbSet<Book> Books { get; set; }
                 // ... costruttore e OnConfiguring/AddDbContext
            }

            // Utilizzo in un endpoint o servizio (assumendo EF Core 7+)
            using (var context = new LibraryContext()) // In pratica si usa la Dependency Injection
            {
                // Esempio 1: Valore letterale (comunque parametrizzato da EF Core!)
                // FormattableString sql = $"SELECT * FROM Books WHERE Title = 'Hamlet'"; // Meno comune passare letterali così
                // var book = await context.Books.FromSql(sql).FirstOrDefaultAsync();

                // Esempio 2: Variabile (il caso d'uso standard)
                var title = "Hamlet";
                // 'title' viene trasformato in un DbParameter da EF Core
                var book = await context.Books
                                        .FromSql($"SELECT * FROM Books WHERE Title = {title}")
                                        .FirstOrDefaultAsync();
                 Console.WriteLine($"Trovato libro: {book?.Title}");
            }
            ```

        - **Esecuzione Stored Procedure (Esempio MariaDB/Pomelo):** `FromSql` può chiamare Stored Procedures che restituiscono dati che possono essere mappate all'entità.

            ```cs
            // Parametro specifico per Pomelo/MySQL
            var userParam = new MySqlParameter("@user", "johndoe");

            // Assumendo che 'GetMostPopularBlogsForUser' restituisca colonne che si mappano sull'entità Blog
            var blogs = await context.Blogs // DbSet<Blog>
                                     .FromSql($"CALL GetMostPopularBlogsForUser({userParam})")
                                     // Oppure, se il parametro è semplice: .FromSql($"CALL GetMostPopularBlogsForUser({'johndoe'})")
                                     .ToListAsync();
            ```

            *(Nota: L'uso esatto di `MySqlParameter` o l'interpolazione diretta dipende da come Pomelo gestisce i parametri nelle chiamate SP. L'interpolazione è generalmente preferita se supportata).*
    - **Limitazioni Importanti:**

        - `FromSqlRaw`/`FromSql` funzionano **solo direttamente su un `DbSet<T>`**. Non possono essere applicati dopo altre operazioni LINQ (es. dopo un `.Where()`).

            ```cs
            // ERRORE: FromSql deve essere il primo metodo chiamato sul DbSet
            var query = context.Books.Where(b => b.BookId > 10).FromSql($"SELECT * FROM Books");

            // CORRETTO (se applicabile): LINQ dopo FromSql
            var query = context.Books.FromSql($"SELECT * FROM Books") // Esegue SQL
                                     .Where(b => b.BookId > 10);        // Filtra in memoria o nel DB se possibile
            ```

        - La query SQL deve restituire **tutte le colonne richieste** dall'entità, con nomi corrispondenti (o mappati).

## SQL Injection: comprendere il rischio

- **Cos'è:** Un attacco dove codice SQL malevolo viene "iniettato" tramite l'input dell'utente (es. URL, form, API request) in una query SQL costruita dinamicamente dall'applicazione, alterandone l'esecuzione prevista.
- **Obiettivo dell'Attaccante:** Bypassare controlli, leggere dati riservati, modificare dati, cancellare dati, ottenere controllo sul server database.
- **Esempio vulnerabile (COSA NON FARE MAI): Costruzione di Stringhe:** Supponiamo di avere un endpoint per cercare utenti per nome:

    ```cs
    // Endpoint API (Esempio VULNERABILE)
    app.MapGet("/api/users/search", async (string name, UserDbContext context) => {
        // !!! PERICOLO: Concatenazione diretta dell'input utente !!!
        string sql = "SELECT * FROM Users WHERE Name = '" + name + "'";

        // Questo esegue la query concatenata. Se 'name' è malevolo, è un disastro.
        try {
            var users = await context.Users.FromSqlRaw(sql).ToListAsync(); // Anche FromSqlRaw è vulnerabile se la stringa è già costruita male!
            return Results.Ok(users);
        } catch (Exception ex) {
            // Mascherare l'errore potrebbe nascondere l'attacco
            return Results.Problem("Errore durante la ricerca");
        }
    });
    ```

- **Come Funziona l'Attacco:**
    1. L'utente invia una richiesta a `/api/users/search?name=Alice`. La query diventa: `SELECT * FROM Users WHERE Name = 'Alice'` (OK).
    2. Un attaccante invia: `/api/users/search?name=Alice'%3B%20DROP%20TABLE%20Users%3B%20--` (URL Encoded for `Alice'; DROP TABLE Users; --`)
    3. La stringa `sql` dentro l'applicazione diventa: `SELECT * FROM Users WHERE Name = 'Alice'; DROP TABLE Users; --'`
    4. Il database esegue il primo comando (trova 'Alice'), poi esegue `DROP TABLE Users;` (!!!), e il `--` commenta il resto della query originale. La tabella Users viene cancellata.
    5. Un altro attacco comune per leggere dati è usare `name=' OR '1'='1`. La query diventa `SELECT * FROM Users WHERE Name = '' OR '1'='1'`, restituendo *tutti* gli utenti.
- **Come prevenire (la soluzione corretta): parametrizzazione**
  - Usare i metodi sicuri di EF Core:

    ```cs
    // Endpoint API (Esempio SICURO con FromSql - EF Core - versione da 7 in poi)
    app.MapGet("/api/users/search", async (string name, UserDbContext context) => {
        // Sicuro: 'name' viene trattato come un valore parametrico, non codice SQL.
        var users = await context.Users
                                 .FromSql($"SELECT * FROM Users WHERE Name = {name}")
                                 .ToListAsync();
        return Results.Ok(users);
    });

    // Endpoint API (Esempio SICURO con FromSqlRaw e parametri per versioni di EF Core precedenti alla 7)
    app.MapGet("/api/users/search-raw", async (string name, UserDbContext context) => {
        // Sicuro: 'name' viene passato come parametro separato {0}.
        var users = await context.Users
                                 .FromSqlRaw("SELECT * FROM Users WHERE Name = {0}", name)
                                 .ToListAsync();
        return Results.Ok(users);
    });
    ```

- **Conclusione sulla sicurezza:** Mai costruire query SQL concatenando direttamente input non fidato (utente, file, altre sorgenti esterne). Usare sempre la parametrizzazione offerta da EF Core o altre librerie ADO.NET.

## Gestire SQL dinamico e limiti della parametrizzazione

- **Il Problema:** Come visto, `FromSql` (interpolato) e `FromSqlRaw` (con parametri) sono ottimi per parametrizzare i *valori* nelle clausole `WHERE`, `INSERT`, `UPDATE`. Tuttavia, i database **non permettono di parametrizzare parti dello schema SQL**, come nomi di tabelle, nomi di colonne, o parole chiave SQL (`ORDER BY`, `ASC`/`DESC`).

- Esempio NON funzionante (parametrizzare un nome di una colonna):

    Questo codice fallirà perché si tenta di sostituire {propertyName} con un parametro SQL, cosa non consentita dal database per un nome di colonna.

    ```cs
    // !!! QUESTO CODICE NON FUNZIONA COME PREVISTO !!!
    var propertyName = "Url"; // Potrebbe arrivare dall'utente? Rischioso!
    var propertyValue = "http://example.com";

    // ERRORE: Non si può parametrizzare il nome della colonna 'propertyName'
    var blogs = await context.Blogs
                             .FromSql($"SELECT * FROM Blogs WHERE {propertyName} = {propertyValue}")
                             .ToListAsync();
    ```

- Soluzione: SQL Dinamico - da usare con Cautela usando `FromSqlRaw`

    Se si deve costruire una query dove parti dello schema (come un nome di colonna o la direzione di ordinamento) sono dinamiche, bisogna:

    1. Costruire la stringa SQL dinamicamente in C#.
    2. **Validare/Sanificare rigorosamente** qualsiasi input usato per costruire dinamicamente parti dello schema (es., `propertyName`, `sortDirection`). Questo è il punto CRITICO per la sicurezza. Un buon approccio è confrontare l'input con una lista predefinita di valori consentiti.
    3. Usare `FromSqlRaw` per eseguire la query costruita.
    4. **Parametrizzare sempre i *valori* (`propertyValue` nell'esempio) usando i segnaposto `{0}` o `@nomeParametro` in `FromSqlRaw`.**

    ```cs
    // Esempio: Ordinamento dinamico (con validazione!)
    public async Task<List<Blog>> GetBlogsSortedAsync(string sortByColumn, string sortDirection)
    {
        // --- VALIDAZIONE CRITICA ---
        var allowedColumns = new List<string> { "Name", "Url", "Rating" }; // Colonne consentite
        if (!allowedColumns.Contains(sortByColumn, StringComparer.OrdinalIgnoreCase)) {
            throw new ArgumentException("Colonna di ordinamento non valida.");
        }

        var direction = "ASC"; // Default
        if ("DESC".Equals(sortDirection, StringComparison.OrdinalIgnoreCase)) {
            direction = "DESC";
        }
        // --- FINE VALIDAZIONE ---

        // Costruzione SICURA della stringa SQL:
        // sortByColumn e direction sono stati validati e sono inseriti direttamente.
        // NON provengono direttamente da input utente non controllato.
        string sql = $"SELECT * FROM Blogs ORDER BY {sortByColumn} {direction}";

        // Esecuzione con FromSqlRaw (senza parametri di valore in questo caso, ma potrebbero esserci in un WHERE)
        var blogs = await context.Blogs.FromSqlRaw(sql).ToListAsync();
        return blogs;
    }

    // Esempio: Filtro su colonna dinamica
    public async Task<List<Blog>> FindBlogsByColumnAsync(string columnName, string columnValue)
    {
        // --- VALIDAZIONE CRITICA ---
        var allowedColumns = new List<string> { "Name", "Url", "AuthorName" }; // Colonne ricercabili
        if (!allowedColumns.Contains(columnName, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Nome colonna non valido per la ricerca.");
        }
        // Si potrebbe voler sanificare anche 'columnValue' se necessario, ma la parametrizzazione lo protegge da SQL Injection.
        // --- FINE VALIDAZIONE ---

        // Parametro per il VALORE (sicuro)
        var valueParam = new MySqlParameter("@paramValue", columnValue);

        // Costruzione SQL: columnName validato è inserito, @paramValue è un parametro.
        // NOTA: Usare @nomeParametro in FromSqlRaw è spesso più chiaro dei segnaposto {0} quando si mescolano parti dinamiche.
        string sql = $"SELECT * FROM Blogs WHERE {columnName} = @paramValue";

        // Esecuzione con FromSqlRaw e parametro
        var blogs = await context.Blogs
                                 .FromSqlRaw(sql, valueParam)
                                 .ToListAsync();
        return blogs;
    }
    ```

- **Avvertimento:** Scrivere codice SQL dinamico è complesso e potenzialmente pericoloso. Si dovrebbe fare solo se strettamente necessario e **bisogna essere ossessivi riguardo alla validazione** degli input usati per costruire la struttura della query. Se possibile, preferire sempre alternative completamente parametrizzate.

## Metodi EF Core per Query SQL Raw che restituiscono tipi qualsiasi

- Usati quando il risultato SQL non si mappa su un'entità del `DbContext` (DTO, tipi primitivi). Si accede tramite `DbContext.Database`.

    - **`SqlQueryRaw<T>(string sql, params object[] parameters)`:**

        - Esegue SQL e mappa a un tipo generico `T` (non entità). Usa parametri posizionali `{0}`.
        - **Esempio (DTO):**

            ```cs
            public class BookAuthorSummary { // DTO
                public string BookTitle { get; set; }
                public string AuthorName { get; set; }
            }
            var authorId = 1;
            var summaries = await context.Database.SqlQueryRaw<BookAuthorSummary>(
                @"SELECT b.Title as BookTitle, a.Name as AuthorName
                  FROM Books b JOIN Authors a ON b.AuthorId = a.AuthorId
                  WHERE a.AuthorId = {0}", authorId)
                .ToListAsync();
            ```

    - **`SqlQuery<T>(FormattableString sql)` (per versioni di EF Core dalla 7.0 in poi):**

        - Versione più moderna e sicura con **interpolazione di stringhe** (`$""`) per i parametri.
        - È il metodo **preferito** per query su tipi non-entità.
        - **Esempio (DTO):**

            ```cs
            public class BookAuthorSummary { /* come sopra */ }
            var authorId = 1;
            // Sicuro: authorId diventa un parametro
            var summaries = await context.Database.SqlQuery<BookAuthorSummary>(
                $@"SELECT b.Title as BookTitle, a.Name as AuthorName
                   FROM Books b JOIN Authors a ON b.AuthorId = a.AuthorId
                   WHERE a.AuthorId = {authorId}")
                .ToListAsync();
            ```

        - **Esempio (Tipo scalare):**

            ```cs
            // Esempio Aggiuntivo: Query di un Tipo Scalare (Lista di Stringhe)
            // Recuperiamo solo i titoli dei libri di un certo autore
            var authorId = 1;
            var bookTitles = await context.Database.SqlQuery<string>(
                $"SELECT Title FROM Books WHERE AuthorId = {authorId}")
                .ToListAsync();

            Console.WriteLine("Titoli trovati:");
            foreach (var title in bookTitles) {
                Console.WriteLine($"- {title}");
            }

            // Esempio Aggiuntivo: Query di un Singolo Valore Scalare (Data/Ora Corrente dal DB)
            // La funzione SQL per ottenere la data/ora corrente varia:
            // SQL Server: GETDATE()
            // MariaDB/MySQL/PostgreSQL: NOW()
            // SQLite: DATETIME('now')
            // Assumendo MariaDB/MySQL con Pomelo:
            var databaseTime = await context.Database.SqlQuery<DateTime>(
                $"SELECT NOW()")
                .FirstOrDefaultAsync(); // Usiamo FirstOrDefaultAsync per ottenere un singolo valore

            Console.WriteLine($"Ora corrente del database: {databaseTime}");

            // Esempio Aggiuntivo: Query di un Singolo Valore Scalare (Conteggio)
            var numberOfBooks = await context.Database.SqlQuery<int>(
                $"SELECT COUNT(*) FROM Books")
                .SingleAsync(); // Usiamo SingleAsync perché COUNT(*) restituisce sempre una riga/un valore

            Console.WriteLine($"Numero totale di libri: {numberOfBooks}");
            ```

## Metodi EF Core per Comandi SQL Raw (Non-Query, ossia per comandi che non restituiscono righe)

- Per `INSERT`, `UPDATE`, `DELETE`. Si usa `DbContext.Database`. Restituiscono il numero di righe modificate.

    - **`ExecuteSqlRawAsync(string sql, params object[] parameters)`:**

        - Esegue un comando con parametri posizionali `{0}`.
        - **Esempio (UPDATE):**

            ```cs
            string oldDomain = "@example.com";
            string newDomain = "@newdomain.com";
            int rowsAffected = await context.Database.ExecuteSqlRawAsync(
                "UPDATE Users SET Email = REPLACE(Email, {0}, {1}) WHERE Email LIKE '%' + {0}",
                oldDomain, newDomain);
            ```

    - **`ExecuteSqlAsync(FormattableString sql)` (per versioni di EF Core dalla 7.0 in poi) / `ExecuteSqlInterpolatedAsync(FormattableString sql)` (Per versioni di EF Core precedenti alla 7.0):**

        - Versione più moderna e sicura con **interpolazione di stringhe** (`$""`).
        - È il metodo **preferito** per eseguire comandi.
        - **Esempio (DELETE):**

            ```cs
            int userIdToDelete = 5;
            // Sicuro: userIdToDelete diventa un parametro
            int rowsAffected = await context.Database.ExecuteSqlAsync(
                $"DELETE FROM Users WHERE Id = {userIdToDelete}");
            ```

    - **Importante:** Questi metodi **non tracciano** le entità. Le entità già caricate nel context non verranno aggiornate automaticamente dopo un `ExecuteSql*`.

## Raccomandazioni: Quale Metodo Usare?

- **Scenario Principale: CRUD Base/Query Standard:** Usare **LINQ to Entities**. È più sicuro (type-safe), più facile da scrivere e manutenere, e astrae dai dettagli del DB.
- **Query su Entità con SQL Specifico/Complicato:**
    - **EF Core 7+:** Usare `DbSet<T>.FromSql($"...")` (interpolato).
    - **EF Core < 7:** Usare `DbSet<T>.FromSqlInterpolated($"...")`.
    - *Raramente:* Usare `DbSet<T>.FromSqlRaw("...", params)` se si devono usare i parametri posizionali o costruire `DbParameter` manualmente.
- **Query su Tipi Non-Entità (DTO, Primitivi):**
    - **EF Core 7+:** Usare `Database.SqlQuery<T>($"...")` (interpolato).
    - **EF Core < 7 o necessità di parametri posizionali:** Usare `Database.SqlQueryRaw<T>("...", params)`.
- **Comandi (INSERT/UPDATE/DELETE) con SQL Specifico:**
    - **EF Core 7+:** Usare `Database.ExecuteSqlAsync($"...")` (interpolato).
    - **EF Core < 7:** Usare `Database.ExecuteSqlInterpolatedAsync($"...")`.
    - *Raramente:* Usare `Database.ExecuteSqlRawAsync("...", params)` per parametri posizionali.
- **SQL Dinamico (Nomi colonne/tabelle variabili, ordinamento dinamico):**
    - Usare `FromSqlRaw` (per query) o `ExecuteSqlRawAsync` (per comandi).
    - **MASSIMA ATTENZIONE:** Si costruisca la stringa SQL dinamicamente **validando/sanificando rigorosamente** le parti non parametrizzabili (nomi colonne/tabelle, direzioni ORDER BY).
    - **Parametrizzare sempre i VALORI** usando i segnaposto `{0}` o `@nomeParam` passati come argomenti separati a `FromSqlRaw`/`ExecuteSqlRawAsync`.

**Regola Generale:** **Preferire sempre i metodi interpolati (`FromSql`, `SqlQuery<T>`, `ExecuteSql`) per la loro sicurezza intrinseca e leggibilità.** Ricorrere ai metodi `*Raw` solo quando necessario e con grande cautela se si sta costruendo SQL dinamico.

## Tabella Riepilogativa dei Metodi

| **Metodo** | **Oggetto Target** | **Input Principale** | **Scopo** | **Sicurezza Predefinita (per i valori)** | **Note** |
| --- |  --- |  --- |  --- |  --- |  --- |
| `FromSql(fstring)` | `DbSet<TEntity>` | `FormattableString` | Query SQL per Tipi Entità | **Alta** (Interpolazione sicura) | EF Core 7+. Sostituisce `FromSqlInterpolated`. Solo su DbSet. |
| `FromSqlRaw(sql, params)` | `DbSet<TEntity>` | `string`, `object[]` | Query SQL per Tipi Entità | **Alta** (con parametri posizionali) | Utile per parametri posizionali o SQL dinamico (con cautela). |
| `SqlQuery<T>(fstring)` | `DbContext.Database` | `FormattableString` | Query SQL per Tipi Non-Entità (DTO, etc) | **Alta** (Interpolazione sicura) | EF Core 7+. Preferito per DTO/tipi primitivi. |
| `SqlQueryRaw<T>(sql, params)` | `DbContext.Database` | `string`, `object[]` | Query SQL per Tipi Non-Entità (DTO, etc) | **Alta** (con parametri posizionali) |  |
| `ExecuteSqlAsync(fstring)` | `DbContext.Database` | `FormattableString` | Comando SQL (INSERT, UPDATE, DELETE) | **Alta** (Interpolazione sicura) | EF Core 7+. Async. Sostituisce `ExecuteSqlInterpolatedAsync`. |
| `ExecuteSqlRawAsync(sql, params)` | `DbContext.Database` | `string`, `object[]` | Comando SQL (INSERT, UPDATE, DELETE) | **Alta** (con parametri posizionali) | Async. Utile per parametri posizionali o SQL dinamico (con cautela). |

*(Nota: `fstring` indica `FormattableString`, ovvero una stringa C# preceduta da `$`. `params` indica parametri posizionali `{0}`, `{1}`... passati come argomenti aggiuntivi).*

## Applicazione Pratica: AziendaAPI con SQL Raw

Si consideri il progetto di Minimal API ASP.NET `AziendaAPI` già sviluppato precedentemente con il LINQ. Di seguito si riportano alcune query SQL del progetto [`AziendaAPI Row SQL`](../../../api-samples/minimal-api/AziendaAPIRowSQL/) nel quale le query degli endpoint sono state ottenute mediante codice SQL.

- **Implementazione Endpoint (Usando Metodi Raccomandati - EF Core 7+):**

    - **GET `/api/aziende`:**

        ```cs
         app.MapGet("/api/aziende", async (AziendaDbContext db) =>
        {
            // Seleziona direttamente le colonne necessarie per AziendaDTO
            var aziendeDto = await db.Database.SqlQuery<AziendaDTO>(
                $"SELECT Id, Nome, Indirizzo FROM Aziende")
                .ToListAsync();
            return Results.Ok(aziendeDto);
        });
        ```

    - **GET `/api/aziende/{id}`:**

        ```cs
        app.MapGet("/api/aziende/{id:int}", async (int id, AziendaDbContext db) =>
        {
            var aziendaDto = await db.Database.SqlQuery<AziendaDTO>(
                    $"SELECT Id, Nome, Indirizzo FROM Aziende WHERE Id = {id}")
                    .FirstOrDefaultAsync(); // O SingleOrDefaultAsync
            return aziendaDto is not null ? Results.Ok(aziendaDto) : Results.NotFound();
        });
        ```

    - **POST `/api/aziende`:**

        ```cs
        app.MapPost("/api/aziende", async (AziendaDbContext db, AziendaDTO aziendaDTO) =>
        {
            // Validazione...
            if (aziendaDTO == null) return Results.BadRequest("Dati azienda mancanti.");

            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                // 1. Esegui l'INSERT
                int rowsAffected = await db.Database.ExecuteSqlAsync(
                    $@"INSERT INTO Aziende (Nome,Indirizzo)
                    VALUES ({aziendaDTO.Nome},{aziendaDTO.Indirizzo})");

                int generatedId = 0;

                if (rowsAffected > 0)
                {
                    // 2. Recupera l'ID con l'alias corretto
                    generatedId = await db.Database.SqlQuery<int>(
                        $"SELECT LAST_INSERT_ID() AS Value") // <-- AS Value serve perché EF Core nel caso di query che restituiscono uno scalare effettua wrapping della query in una query esterna su cui invoca il campo Value
                        .SingleAsync();

                    aziendaDTO.Id = generatedId;

                    await transaction.CommitAsync();

                    return Results.Created($"/api/aziende/{generatedId}", aziendaDTO);
                }
                else
                {
                    await transaction.RollbackAsync();
                    return Results.Problem("Creazione fallita (nessuna riga inserita).");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Logga l'errore ex (importante!)
                Console.WriteLine($"ERRORE: {ex.ToString()}"); // Log di base per debug
                return Results.Problem($"Errore durante la creazione: {ex.Message}");
            }
        });
        ```

    - **PUT `/api/aziende/{id}`:**

        ```cs
        app.MapPut("/api/aziende/{id:int}", async (AziendaDbContext db, AziendaDTO updateAzienda, int id) =>
        {
            // Validazione DTO...
            if (updateAzienda == null || string.IsNullOrWhiteSpace(updateAzienda.Nome))
                return Results.BadRequest("Dati azienda non validi.");

            int rowsAffected = await db.Database.ExecuteSqlAsync(
                $@"UPDATE Aziende
                SET Nome = {updateAzienda.Nome}, Indirizzo = {updateAzienda.Indirizzo}
                WHERE Id = {id}");

            // Se rowsAffected è 0, considera l'entità come non trovata.
            return rowsAffected > 0 ? Results.NoContent() : Results.NotFound();
        });
        ```

    - **DELETE `/api/aziende/{id}`:**

        ```cs
        app.MapDelete("/api/aziende/{id:int}", async (int id, AziendaDbContext context) =>
        {
            int rowsAffected = await context.Database.ExecuteSqlAsync( // Usa ExecuteSqlAsync (interpolato)
                $"DELETE FROM Aziende WHERE Id = {id}");
            return rowsAffected > 0 ? Results.NoContent() : Results.NotFound();
        });
        ```

## Considerazioni sul provider Pomelo

- Il provider Pomelo (`Pomelo.EntityFrameworkCore.MySql`) permette a EF Core di comunicare con MariaDB (e MySQL).
- I metodi EF Core per SQL raw (`FromSql*`, `SqlQuery*`, `ExecuteSql*`) sono **indipendenti dal provider**. Funzionano allo stesso modo sia con SQL Server, PostgreSQL, SQLite o MariaDB (con Pomelo).
- Quello che cambia è la **sintassi SQL specifica** che si potrebbe dover usare all'interno delle stringhe SQL raw se si usano funzionalità particolari di MariaDB (es. funzioni specifiche, sintassi particolari). Per query/comandi standard ANSI SQL, non ci sono differenze significative.

## Conclusioni

Abbiamo visto come e perché usare SQL raw con EF Core.

  - `FromSqlRaw`/`FromSqlInterpolated`: Query per tipi entità.
  - `SqlQueryRaw<T>`/`SqlQuery<T>`: Query per tipi non-entità/DTO/primitivi.
  - `ExecuteSqlRaw`/`ExecuteSqlInterpolated`: Comandi non-query (INSERT, UPDATE, DELETE).
  - **Quando usarlo:** Ottimizzazioni, query complesse, stored procedure.
  - **Quando NON usarlo:** Per operazioni CRUD standard, LINQ è spesso più manutenibile, sicuro (type-safe) e leggibile.
  - **Sicurezza:** Priorità assoluta! Usare sempre metodi parametrizzati o interpolati.

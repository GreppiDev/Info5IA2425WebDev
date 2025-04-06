# Argomenti avanzati: Transactions, User Defined Domains, User Defined Variables, Prepared Statements, Triggers, Stored Routines, Cursori

- [Argomenti avanzati: Transactions, User Defined Domains, User Defined Variables, Prepared Statements, Triggers, Stored Routines, Cursori](#argomenti-avanzati-transactions-user-defined-domains-user-defined-variables-prepared-statements-triggers-stored-routines-cursori)
  - [Argomenti avanzati di MySQL/MariaDB](#argomenti-avanzati-di-mysqlmariadb)
    - [Transazioni e Controllo della Concorrenza (ACID)](#transazioni-e-controllo-della-concorrenza-acid)
    - [Table Locking](#table-locking)
    - [Integrità dei Dati: Domini (Standard SQL), Vincoli CHECK, Enum](#integrità-dei-dati-domini-standard-sql-vincoli-check-enum)
    - [Gestione dello Stato: Variabili Utente, di Sistema e Locali, Temporary Tables](#gestione-dello-stato-variabili-utente-di-sistema-e-locali-temporary-tables)
    - [Efficienza e Sicurezza: Prepared Statements](#efficienza-e-sicurezza-prepared-statements)
    - [Automazione nel Database: Triggers ed Eventi Pianificati](#automazione-nel-database-triggers-ed-eventi-pianificati)
    - [Logica Riusabile nel Database: Stored Procedures e Stored Functions](#logica-riusabile-nel-database-stored-procedures-e-stored-functions)
    - [Elaborazione Row-by-Row: Cursor in Stored Routines](#elaborazione-row-by-row-cursor-in-stored-routines)

Questo documento esplora concetti avanzati ma essenziali di MySQL e MariaDB, cruciali per la costruzione di applicazioni backend efficienti, sicure e manutenibili.

## Argomenti avanzati di MySQL/MariaDB

Verranno analizzati i seguenti argomenti dal punto di vista SQL, architetturale e della loro applicazione pratica:

1. **Transazioni e Controllo della Concorrenza (ACID)**
2. **Table Locking**
3. **Integrità dei Dati: Domini (Standard SQL), Vincoli CHECK, Enum**
4. **Gestione dello Stato: Variabili Utente, di Sistema e Locali, Temporary Tables**
5. **Efficienza e Sicurezza: Prepared Statements**
6. **Automazione nel Database: Triggers ed Eventi Pianificati**
7. **Logica Riusabile nel Database: Stored Procedures e Stored Functions**

Al termine della trattazione teorica, verranno illustrati esempi concreti di implementazione di questi concetti in un'applicazione backend ASP.NET Core Minimal API, confrontando tre approcci comuni per l'interazione con il database.

### Transazioni e Controllo della Concorrenza (ACID)

Una **transazione** rappresenta un'unità di lavoro indivisibile composta da una o più operazioni SQL. L'obiettivo primario delle transazioni è garantire l'integrità e la coerenza dei dati, anche in presenza di errori o accessi concorrenti. I motori di storage transazionali, come **InnoDB** (il default per MySQL e MariaDB), implementano le proprietà **ACID**:

* **Atomicità (Atomicity):** Tutte le operazioni all'interno di una transazione vengono eseguite con successo (commit), oppure nessuna di esse viene applicata in modo permanente (rollback). Se un errore impedisce il completamento, il database viene riportato allo stato precedente l'inizio della transazione. Questo garantisce che non ci siano stati intermedi "parziali".
* **Consistenza (Consistency):** Una transazione valida porta il database da uno stato consistente a un altro stato consistente. Ogni transazione completata con successo deve rispettare tutti i vincoli definiti (chiavi primarie, esterne, univoche, CHECK, NOT NULL). Il sistema garantisce che i dati siano sempre in uno stato valido secondo le regole definite.
* **Isolamento (Isolation):** Le transazioni eseguite concorrentemente non dovrebbero influenzarsi a vicenda in modi imprevisti. Idealmente, ogni transazione opera come se fosse l'unica attiva sul sistema. Questo previene fenomeni indesiderati come:
    * *Letture Sporche (Dirty Reads):* Una transazione legge dati modificati da un'altra transazione non ancora confermata (committata).
    * *Letture Non Ripetibili (Non-Repeatable Reads):* Una transazione rilegge gli stessi dati e trova valori diversi perché un'altra transazione li ha modificati e confermati nel frattempo.
    * *Letture Fantasma (Phantom Reads):* Una transazione esegue una query con una clausola WHERE, e rieseguendola successivamente trova *nuove righe* che soddisfano la condizione, inserite e confermate da un'altra transazione.
    Il livello di isolamento definisce quali di questi fenomeni sono permessi. MySQL/MariaDB supportano diversi livelli (es. `READ UNCOMMITTED`, `READ COMMITTED`, `REPEATABLE READ` - default per InnoDB, `SERIALIZABLE`).
* **Durabilità (Durability):** Una volta che una transazione è stata confermata (`COMMIT`), le sue modifiche sono permanenti e sopravvivono a crash del sistema (es. mancanza di corrente, riavvio del server). Questo viene solitamente garantito scrivendo le modifiche in log persistenti prima di confermare la transazione al client.

**Sintassi SQL per il Controllo delle Transazioni:**

MySQL e MariaDB operano di default in modalità `autocommit=1`, dove ogni istruzione SQL viene trattata come una transazione atomica e confermata immediatamente. Per raggruppare più istruzioni:

```sql
-- Metodo 1: Disabilitare autocommit per la sessione
SET autocommit = 0;

-- Operazioni della transazione
INSERT INTO tabella1 (...) VALUES (...);
UPDATE tabella2 SET ... WHERE ...;
-- ... altre operazioni ...

-- Se tutto è andato a buon fine
COMMIT;

-- Se si verifica un errore o una condizione logica richiede l'annullamento
-- ROLLBACK;

-- Riattivare autocommit (opzionale, altrimenti rimane disattivo per la sessione)
SET autocommit = 1;

-- Metodo 2: Usare START TRANSACTION (o BEGIN / BEGIN WORK)
START TRANSACTION; -- Inizia esplicitamente una transazione; autocommit viene temporaneamente ignorato

-- Operazioni della transazione
INSERT INTO tabella1 (...) VALUES (...);
UPDATE tabella2 SET ... WHERE ...;

-- Conferma o annulla
COMMIT;   -- Rende le modifiche permanenti
-- ROLLBACK; -- Annulla le modifiche

-- Dopo COMMIT o ROLLBACK, la modalità autocommit ritorna al suo stato precedente.

-- Savepoints (Punti di salvataggio intermedi)
START TRANSACTION;
INSERT INTO ...; -- Operazione 1
SAVEPOINT punto1; -- Crea un punto di salvataggio
UPDATE ...;      -- Operazione 2 (potrebbe fallire)
-- Se l'operazione 2 fallisce:
-- ROLLBACK TO SAVEPOINT punto1; -- Annulla solo l'Operazione 2, mantenendo l'Operazione 1
-- ... si può poi decidere se fare COMMIT o ROLLBACK completo ...
COMMIT; -- Conferma l'Operazione 1 (e altre successive al savepoint, se presenti)
```

* **Importanza Architetturale e per Backend:**

Le transazioni sono un pilastro fondamentale per qualsiasi applicazione che gestisca dati critici. Nel backend, operazioni complesse come registrare un ordine (verifica disponibilità, aggiorna magazzino, inserisci ordine, processa pagamento), trasferire fondi, o gestire prenotazioni coinvolgono modifiche a più tabelle che devono avvenire in modo atomico. Un errore in una qualsiasi di queste fasi senza una transazione lascerebbe i dati in uno stato inconsistente (es. prodotto scalato dal magazzino ma ordine non registrato). L'uso corretto delle transazioni garantisce l'integrità logica del dominio applicativo riflessa nel database.

* **Ulteriori approfondimenti e risorse online**
    - [ACID: Concurrency Control with Transactions - MariaDB Knowledge Base](https://mariadb.com/kb/en/acid-concurrency-control-with-transactions/)
    - [Transactions - MariaDB Knowledge Base](https://mariadb.com/kb/en/transactions/)
    - [Transactional and Locking Statements - MySQL Manual](https://dev.mysql.com/doc/refman/9.2/en/sql-transactional-statements.html)
    - [MySQL Transactions - mysqltutorial.org](https://www.mysqltutorial.org/mysql-stored-procedure/mysql-transactions/)
    - [MySQL transactions - ZetCode](https://zetcode.com/mysql/transactions/)

### Table Locking

Sebbene distinto dalle transazioni ACID gestite dallo storage engine (come InnoDB), MySQL permette anche il *locking esplicito* delle tabelle tramite `LOCK TABLES`. Questo meccanismo, più datato, consente a una sessione di bloccare intere tabelle in modalità `READ` (lettura condivisa, scrittura bloccata per altri) o `WRITE` (lettura/scrittura esclusiva). Va usato con cautela perché può ridurre drasticamente la concorrenza. InnoDB gestisce la concorrenza in modo molto più granulare (a livello di riga) e generalmente non richiede `LOCK TABLES`.

**Risorse online per il table lock:**

  - [MySQL Table Locking - mysqltutorial.org](https://www.mysqltutorial.org/mysql-basics/mysql-table-locking/)

### Integrità dei Dati: Domini (Standard SQL), Vincoli CHECK, Enum

Garantire che i dati memorizzati siano validi è cruciale. Oltre ai tipi di dato e ai vincoli `NOT NULL`, `UNIQUE`, `PRIMARY KEY`, `FOREIGN KEY`, lo standard SQL offre altri meccanismi.

**User-Defined Domains (Standard SQL):**

Lo standard SQL definisce il concetto di **Dominio** come un tipo di dato personalizzato, riutilizzabile, che incapsula un tipo di dato base e un insieme di vincoli (es. `CHECK`, `DEFAULT`, `NOT NULL`).

```sql
-- Esempio concettuale (Standard SQL)
CREATE DOMAIN EMAIL_ADDRESS AS VARCHAR(255)
  CHECK (VALUE LIKE '%_@__%.__%') -- Vincolo di formato semplice
  DEFAULT 'n/a';

CREATE TABLE Users (
  UserID INT PRIMARY KEY,
  UserEmail EMAIL_ADDRESS -- Usa il dominio definito
);
```

Altro esempio:

```sql
-- Esempio concettuale (Standard SQL)
CREATE DOMAIN CAP AS CHAR(5)
  CHECK (VALUE ~ '^[0-9]{5}$'); -- Verifica che siano 5 cifre numeriche

CREATE TABLE Indirizzi (
  ID INT PRIMARY KEY,
  Via VARCHAR(100),
  CodicePostale CAP NOT NULL -- Usa il dominio definito
);
```

Questo promuove la coerenza (stesse regole applicate ovunque si usi il dominio) e la manutenibilità (modifica la regola in un solo posto).

**Supporto in MySQL/MariaDB:**

* **MySQL:** **Non supporta** `CREATE DOMAIN`. La sintassi non è riconosciuta.
* **MariaDB:** **Supporta** `CREATE DOMAIN` a partire dalla versione **10.2.1**.

**Vincoli CHECK (Alternativa Pratica):**

Dato il supporto limitato o assente per i domini, l'alternativa principale per imporre regole di validità sui valori delle colonne è l'uso dei vincoli `CHECK`. Un vincolo `CHECK` definisce un'espressione booleana che deve essere vera (o `NULL`) per ogni riga della tabella.Possono essere definiti a livello di colonna o di tabella.

**Sintassi dei Vincoli CHECK:**

```sql
CREATE TABLE Products (
    ProductID INT PRIMARY KEY,
    ProductName VARCHAR(100) NOT NULL,
    -- Vincolo CHECK a livello di colonna
    UnitPrice DECIMAL(10, 2) CHECK (UnitPrice > 0),
    StockQuantity INT DEFAULT 0,
    -- Vincolo CHECK a livello di tabella (può riferirsi a più colonne)
    CONSTRAINT chk_stock_positive CHECK (StockQuantity >= 0),
    CONSTRAINT chk_reorder_level CHECK (ReorderLevel < StockQuantity) -- Esempio con più colonne
    -- ... altre colonne e vincoli ...
    ReorderLevel INT
);

-- Aggiungere un vincolo a una tabella esistente
ALTER TABLE Products
ADD CONSTRAINT chk_productname_notempty CHECK (ProductName <> '');
```

Altri esempi:

```sql
CREATE TABLE Ordine (
    IDOrdine INT PRIMARY KEY,
    DataOrdine DATE NOT NULL,
    DataSpedizione DATE,
    Quantita INT CHECK (Quantita > 0), -- Vincolo su colonna
    PrezzoTotale DECIMAL(10,2) CHECK (PrezzoTotale >= 0),
    -- Vincolo a livello tabella
    CONSTRAINT chk_date_spedizione CHECK (DataSpedizione IS NULL OR DataSpedizione >= DataOrdine)
);

CREATE TABLE parts (
    part_no VARCHAR(18) PRIMARY KEY,
    description VARCHAR(40),
    cost DECIMAL(10,2 ) NOT NULL CHECK (cost >= 0),
    price DECIMAL(10,2) NOT NULL CHECK (price >= 0),
    CONSTRAINT parts_chk_price_gt_cost 
        CHECK(price >= cost)
);
```

**Supporto Vincoli CHECK:**

* **MySQL:** Ha introdotto il *supporto effettivo* (cioè la verifica del vincolo) nella versione **8.0.16**. Le versioni precedenti accettavano la sintassi `CHECK` ma la ignoravano silenziosamente.
* **MariaDB:** Supporta e verifica i vincoli `CHECK` dalla versione **10.2.1**.

* **Deferrable Constraints:** Lo standard SQL prevede la possibilità di definire vincoli come `DEFERRABLE`, posticipando la loro verifica alla fine della transazione (`COMMIT`). Questo può essere utile in scenari complessi (es. inserimento circolare di chiavi esterne). Tuttavia, né MySQL né MariaDB implementano pienamente questa funzionalità; i vincoli (inclusi `CHECK` e `FOREIGN KEY`) vengono tipicamente verificati **immediatamente** dopo l'esecuzione dell'istruzione DML che potrebbe violarli. Si vedano al riguardo i links:
  * [Constraint Checking - MariaDB Knowledge Base](https://mariadb.com/kb/en/mariadb-transactions-and-isolation-levels-for-sql-server-users/)
  * [CHECK Constraints - MySQL Manual](https://dev.mysql.com/doc/refman/9.2/en/create-table-check-constraints.html)

* I vincoli `CHECK` sono lo strumento standard SQL più vicino al concetto di dominio per applicare regole di validità arbitrarie.

**Tipo di Dato ENUM (Specifico di MySQL/MariaDB):**

Un altro meccanismo fornito da MySQL e MariaDB per limitare i valori possibili di una colonna è il tipo di dato **ENUM**. Questo tipo permette a una colonna di accettare solo *uno* dei valori stringa specificati esplicitamente nella sua definizione.

```sql
CREATE TABLE CopiaLibro (
    id_copia INT AUTO_INCREMENT PRIMARY KEY,
    isbn VARCHAR(13) NOT NULL,
    -- Definizione della colonna ENUM
    stato_copia ENUM('disponibile', 'prestata', 'in_riparazione', 'smarrita')
        DEFAULT 'disponibile'
        NOT NULL,
    -- ... altre colonne ...
    FOREIGN KEY (isbn) REFERENCES Libro(isbn)
);

-- Inserimento valido
INSERT INTO CopiaLibro (isbn, stato_copia) VALUES ('978...', 'disponibile');

-- Inserimento NON valido (il valore non è nella lista)
-- INSERT INTO CopiaLibro (isbn, stato_copia) VALUES ('978...', 'perso'); -- ERRORE
```

**Caratteristiche di ENUM:**

- **Definizione:** La lista di valori permessi è definita direttamente nella `CREATE TABLE` o `ALTER TABLE`.
- **Valori Ammessi:** Solo i valori stringa presenti nella lista (o `NULL` se la colonna è nullable). I valori sono case-insensitive durante l'inserimento, ma vengono memorizzati con il case specificato nella definizione.
- **Memorizzazione:** Internamente, MySQL/MariaDB memorizzano i valori ENUM come interi (1 per il primo valore della lista, 2 per il secondo, e così via), il che può portare a un risparmio di spazio rispetto a `VARCHAR`. L'indice 0 è riservato per la stringa vuota (`''`) in caso di inserimenti non validi in colonne non-`strict` mode, o per `NULL` se la colonna è nullable.
- **Vantaggi:** Definisce chiaramente i possibili stati/valori per una colonna; può essere efficiente in termini di storage.
- **Svantaggi:**
    - **Inflessibilità:** Modificare la lista di valori permessi richiede un'operazione `ALTER TABLE`, che può essere costosa su tabelle grandi.
    - **Portabilità:** È una caratteristica specifica di MySQL/MariaDB, non fa parte dello standard SQL.
    - **Ordinamento/Confronto:** L'ordinamento e i confronti numerici si basano sugli indici interni, non sull'ordine alfabetico delle stringhe, il che può essere controintuitivo.
    - **Riusabilità:** La lista è definita per colonna, non è un tipo riutilizzabile come un Dominio.

**ENUM vs. CHECK vs. Dominio:**

- `ENUM` è utile per colonne che rappresentano uno **stato** o una **categoria** con un numero **limitato e ben definito** di valori stringa possibili. È una soluzione specifica e pragmatica di MySQL/MariaDB.
- `CHECK` è più **generale e flessibile**, permettendo qualsiasi condizione booleana (controlli su range numerici, formati di stringa complessi con `REGEXP`, confronti tra colonne, etc.). È più vicino al meccanismo di validazione dei Domini standard.
- `CREATE DOMAIN` (dove supportato, es. MariaDB 10.2.1+) offre **riusabilità** del tipo e delle regole associate, promuovendo coerenza nello schema.

**Importanza Architetturale e per Backend:**

I vincoli `CHECK` (e i domini dove supportati) sono essenziali per applicare regole di business e garantire l'integrità dei dati direttamente nel database. Questo approccio:

* **Centralizza la Validazione:** Le regole sono definite una sola volta nel database, non duplicate in ogni applicazione che accede ai dati.
* **Garantisce Coerenza:** Le regole vengono applicate indipendentemente da come i dati vengono inseriti o modificati (applicazione, script, accesso diretto).
* **Semplifica il Codice Applicativo:** L'applicazione può fare affidamento sul database per le validazioni di base, riducendo il codice boilerplate.

È buona pratica definire nel database tutte le regole di integrità che sono intrinseche ai dati stessi.

Sia `CHECK` che `ENUM` sono strumenti validi per rafforzare l'integrità dei dati direttamente nel database, riducendo la necessità di validazioni duplicate nel codice applicativo e garantendo che le regole siano rispettate indipendentemente da come i dati vengono modificati. La scelta tra `CHECK` e `ENUM` (o l'uso di `FOREIGN KEY` verso una tabella di "lookup" per liste più dinamiche) dipende dalla natura specifica dei dati da vincolare e dai requisiti di flessibilità e portabilità.

* **Ulteriori approfondimenti e risorse online**
    - [MySQL CHECK Constraint - mysqltutorial.org](https://www.mysqltutorial.org/mysql-basics/mysql-check-constraint/)
    - [CHECK Constraints - MySQL Manual](https://dev.mysql.com/doc/refman/9.2/en/create-table-check-constraints.html)

### Gestione dello Stato: Variabili Utente, di Sistema e Locali, Temporary Tables

MySQL e MariaDB offrono diversi tipi di variabili per memorizzare valori temporanei o configurare il comportamento del server.

* **User-Defined Variables (Variabili di Sessione):**

  * Prefisso: `@` (es. `@my_var`).
  * Scope: **Sessione**. Ogni variabile utente è privata della connessione client che l'ha creata e viene automaticamente distrutta alla chiusura della connessione. Non sono visibili ad altre sessioni.
  * Assegnazione:
      * `SET @var_name = espressione;` (o `:=`)
      * `SELECT @var_name := espressione;`
      * `SELECT colonna1, @var_name := colonna2 FROM tabella ...;` (Assegnamento all'interno di una query)
      * `SELECT colonna INTO @var_name FROM tabella WHERE ... LIMIT 1;` (Per query che restituiscono una singola riga/colonna)
  * Tipo: Debolmente tipizzate. Possono contenere interi, decimali, stringhe, binari, `NULL`. Il tipo può cambiare durante la sessione. La precisione per tipi numerici può non essere sempre mantenuta.
  * Uso Tipico: Memorizzare valori intermedi in script SQL multi-step o all'interno della stessa sessione interattiva.

  ```sql
  SET @counter = 0;
  SELECT @min_price := MIN(UnitPrice), @max_price := MAX(UnitPrice) FROM Products;
  SELECT @counter := @counter + 1, ProductID FROM Orders WHERE OrderDate = CURDATE();
  SELECT @counter; -- Mostra il numero di ordini di oggi
  ```

  Analizziamo l'istruzione:

  ```sql
  SELECT @counter := @counter + 1, ProductID
  FROM Orders
  WHERE OrderDate = CURDATE();
  ```

  1. **`FROM Orders WHERE OrderDate = CURDATE()`**: Questa parte seleziona tutte le righe dalla tabella `Orders` per le quali la colonna `OrderDate` corrisponde alla data odierna (`CURDATE()` restituisce la data corrente).

  2. **`SELECT ..., ProductID`**: Per ogni riga trovata che soddisfa la condizione `WHERE`, la query seleziona il valore della colonna `ProductID`. Quindi, nel risultato finale, avrai una lista dei `ProductID` degli ordini effettuati oggi.

  3. **`SELECT @counter := @counter + 1, ...`**: Questa è la parte cruciale e forse meno intuitiva. L'operatore `:=` è un operatore di assegnamento in MySQL/MariaDB, utilizzabile all'interno delle espressioni (mentre `=` si usa con `SET` o come operatore di confronto).
      * Per **ogni riga** che viene selezionata dalla query (cioè per ogni ordine di oggi), l'espressione `@counter := @counter + 1` viene valutata.
      * Essa prende il valore *attuale* della variabile di sessione `@counter`, gli **aggiunge 1**, e **assegna il nuovo valore** nuovamente alla variabile `@counter`.
      * Questo avviene *prima* che la riga venga effettivamente restituita nel set di risultati.

  **In Sintesi:**

  Lo scopo principale di includere `@counter := @counter + 1` nella lista `SELECT` è quello di usare la query per **contare** le righe che soddisfano la condizione, modificando la variabile `@counter` come **effetto collaterale**.

  Considerando le istruzioni nel loro insieme:

  ```sql
  -- 1. Inizializza il contatore a zero per questa sessione
  SET @counter = 0;

  -- 2. Seleziona gli ordini di oggi.
  --    PER OGNI ordine trovato:
  --       - Incrementa il valore di @counter di 1
  --       - Seleziona il ProductID di quell'ordine
  SELECT @counter := @counter + 1, ProductID
  FROM Orders
  WHERE OrderDate = CURDATE();

  -- 3. Mostra il valore finale del contatore
  SELECT @counter;
  ```

  Quindi, l'istruzione `SELECT @counter := @counter + 1, ProductID ...` fa due cose:

  1. Restituisce l'elenco dei `ProductID` degli ordini di oggi.
  2. **Conta** quanti ordini sono stati effettuati oggi e memorizza questo conteggio nella variabile `@counter`.

  L'ultima istruzione (`SELECT @counter;`) serve poi a visualizzare il risultato di questo conteggio. È un modo un po' "compatto" per ottenere sia i dettagli (i `ProductID`) sia il conteggio totale con una sola query principale, sfruttando le variabili di sessione e l'assegnamento all'interno del `SELECT`.

  Ovviamente la query dell'esempio poteva essere eseguita usando la funzione `COUNT`:

  ```sql
  SELECT COUNT(*) FROM Orders WHERE OrderDate = CURDATE();
  ```

* **System Variables (Variabili di Sistema):**

  * Prefisso: `@@` (opzionale quando si usa `SET`).
  * Scope:
      * `GLOBAL`: Impostazioni a livello di server. Richiede privilegi specifici (es. `SUPER` o `SYSTEM_VARIABLES_ADMIN`) per essere modificate. Le modifiche influenzano le nuove connessioni.
      * `SESSION`: Impostazioni specifiche della connessione corrente. Ogni sessione eredita i valori globali al momento della connessione, ma può sovrascriverli localmente (se la variabile è modificabile a livello di sessione).
  * Accesso/Modifica:
      * `SHOW GLOBAL VARIABLES LIKE 'pattern';`
      * `SHOW SESSION VARIABLES LIKE 'pattern';`
      * `SELECT @@GLOBAL.var_name;`
      * `SELECT @@SESSION.var_name;` (o solo `@@var_name`)
      * `SET GLOBAL var_name = value;`
      * `SET SESSION var_name = value;` (o solo `SET var_name = value;`)
  * Uso Tipico: Configurare e ottimizzare il comportamento del server (es. `max_connections`, `innodb_buffer_pool_size`, `sql_mode`, `character_set_client`).
    * `SHOW VARIABLES LIKE '%size%';` -- per cercare quelle che contengono la parola `size`.
  
* **Local Variables (Variabili Locali):**

  * Dichiarazione: `DECLARE var_name datatype [DEFAULT value];`
  * Scope: **Blocco `BEGIN...END`** all'interno di stored routines (procedure, funzioni, trigger, eventi). Non sono visibili al di fuori del blocco in cui sono dichiarate.
  * Tipo: Fortemente tipizzate (devono avere un tipo di dato SQL specifico).
  * Assegnazione:
      * `SET var_name = espressione;`
      * `SELECT colonna INTO var_name FROM tabella WHERE ...;`
  * Uso Tipico: Variabili temporanee all'interno della logica di una stored routine.
    * *I titoli dei film dello stesso regista di "Casablanca"*
      Si può risolvere così:
      Il regista del film Casablanca è ottenuto con una query e inserito in una variabile:

      ```sql
      SELECT Regista INTO @regista_casablanca FROM film WHERE Titolo='Casablanca';
      ```

      La query complessiva diventa:

      ```sql
      SELECT * 
      FROM film
      WHERE Regista =@regista_casablanca;
      ```

* **Uso di Temporary Tables per memorizzare i risultati di una query**
    Quando i dati intermedi di un’elaborazione sono strutturati e devono essere momentaneamente memorizzati, conviene ricorrere a tabelle temporanee piuttosto che a variabili di sessione. Ad esempio, la query (che si potrebbe risolvere con subquery):

    ```sql
    -- Per ogni film di fantascienza che non è mai stato proiettato prima del 1 / 1 / 01 
    -- il titolo e l ’ incasso totale di tutte le sue proiezioni
    -- si può sviluppare anche come:
    CREATE TEMPORARY TABLE codici_film_after (CodFilm INT UNSIGNED);

    INSERT INTO codici_film_after SELECT F2.CodFilm 
       FROM film F2, proiezioni P2 
       WHERE F2.CodFilm = P2.CodiceFilm AND F2.Genere='Fantascienza' AND P2.DataProiezione>'2001-01-01';

    SELECT F.Codfilm, F.Titolo, sum(P.Incasso) IncassoTotale
    FROM film F, proiezioni P 
    WHERE F.CodFilm = P.CodiceFilm AND F.Genere='Fantascienza' AND F.CodFilm NOT IN (SELECT * FROM codici_film_after)
    GROUP BY F.Codfilm; 
    ```

**Importanza Architetturale e per Backend:**

* **Variabili Utente:** Utili in script o sessioni interattive, ma meno comuni nel codice applicativo backend moderno, dove la gestione dello stato avviene tipicamente a livello di applicazione. Possono essere usate per passare valori semplici tra chiamate SQL all'interno della stessa richiesta, ma spesso è preferibile gestire la logica nell'applicazione.
* **Variabili di Sistema:** Fondamentali per l'amministrazione e il tuning del database. L'applicazione backend potrebbe occasionalmente leggere variabili di sessione (es. `@@identity` o `LAST_INSERT_ID()` dopo un `INSERT`) o modificare impostazioni di sessione (es. `sql_mode`) se necessario, ma raramente modifica variabili globali.
* **Variabili Locali:** Essenziali per scrivere logica complessa all'interno di stored routines.

**Alternative:** Per dati intermedi più complessi o voluminosi rispetto a quanto gestibile con le variabili utente, le **Tabelle Temporanee** (`CREATE TEMPORARY TABLE ...`) sono spesso una soluzione migliore. Sono anch'esse limitate alla sessione e vengono eliminate automaticamente alla disconnessione.

### Efficienza e Sicurezza: Prepared Statements

Le **Prepared Statements** sono un meccanismo fondamentale per l'interazione tra applicazioni e database, offrendo vantaggi cruciali in termini di performance e sicurezza.

**Concetto:**

L'idea è separare l'istruzione SQL dalla sua esecuzione con dati specifici:

1. **Preparazione (Prepare):** L'applicazione invia al database un'istruzione SQL "template" contenente dei segnaposto (tipicamente `?` in MySQL/MariaDB, ma i driver possono supportare anche segnaposto nominali come `@param_name`) al posto dei valori letterali. Il database analizza (parse), valida e ottimizza questa istruzione *una sola volta*, creando un piano di esecuzione compilato.
2. **Esecuzione (Execute):** L'applicazione invia i valori effettivi da sostituire ai segnaposto. Il database utilizza il piano precompilato per eseguire l'istruzione con i valori forniti. Questo passo può essere ripetuto molte volte con set di parametri diversi, senza dover rianalizzare l'SQL ogni volta.
3. **Rilascio (Deallocate/Close):** L'applicazione (o il driver/libreria) rilascia le risorse associate all'istruzione preparata quando non è più necessaria.

**Sintassi SQL (Uso Diretto - Raro in Applicazioni):**

```sql
-- 1. Preparazione
PREPARE stmt1 FROM 'SELECT ProductID, ProductName FROM Products WHERE UnitPrice > ? AND StockQuantity > ?';

-- 2. Impostazione parametri (tramite variabili utente)
SET @min_price = 50.00;
SET @min_stock = 10;

-- 3. Esecuzione con i parametri
EXECUTE stmt1 USING @min_price, @min_stock;

-- 4. (Opzionale) Nuovi parametri e ri-esecuzione
SET @min_price = 100.00;
SET @min_stock = 5;
EXECUTE stmt1 USING @min_price, @min_stock;

-- 5. Rilascio
DEALLOCATE PREPARE stmt1;
```

**Vantaggi Chiave:**

1. **Performance:** Evita il sovraccarico di parsing e ottimizzazione per ogni esecuzione di query simili. Il database riutilizza il piano di esecuzione ottimizzato, inviando solo i parametri variabili sulla rete. Questo è particolarmente efficace per query DML (INSERT, UPDATE, DELETE) o SELECT ripetitive all'interno di cicli o richieste frequenti.
2. **Sicurezza (Prevenzione [SQL Injection](https://www.w3schools.com/sql/sql_injection.asp)):** Questo è il vantaggio *più importante*. I valori dei parametri vengono trattati dal database come *dati* e non come parte del codice SQL eseguibile. Anche se un parametro contenesse caratteri speciali SQL (es. `'`, `;`, `--`), questi non verrebbero interpretati come comandi SQL, ma inseriti/confrontati come valori letterali. Questo elimina virtualmente il rischio di attacchi SQL Injection, una delle vulnerabilità più gravi delle applicazioni web.

**Importanza Architetturale e per Backend:**

L'uso dei Prepared Statements (o delle query parametrizzate, che sono il modo in cui le librerie li implementano) è **obbligatorio** nello sviluppo backend moderno per i seguenti motivi:

* **Sicurezza:** È la difesa principale contro `SQL Injection`. Costruire query concatenando stringhe con input utente è estremamente pericoloso e inaccettabile.
* **Standard de Facto:** Tutte le librerie di accesso ai dati mature (Entity Framework Core, Dapper, ADO.NET, JDBC, PDO, etc.) si basano su questo meccanismo. Utilizzare queste librerie correttamente significa utilizzare implicitamente le prepared statements.
* **Performance:** Migliora l'efficienza per operazioni comuni.

Nel codice backend (es. C#, Java, Python), non si usano i comandi `PREPARE`/`EXECUTE` SQL direttamente. Si utilizzano le API della libreria scelta, che gestiscono la preparazione, l'esecuzione con parametri e il rilascio in modo trasparente. Esempi includono `ExecuteSqlInterpolatedAsync` in EF Core o il passaggio di oggetti anonimi/`DynamicParameters` a Dapper.

### Automazione nel Database: Triggers ed Eventi Pianificati

Il database può eseguire automaticamente codice SQL in risposta a eventi specifici.

**Triggers:**

Un **Trigger** è un blocco di codice SQL nominato, associato a una tabella specifica, che viene eseguito automaticamente dal database *prima* (`BEFORE`) o *dopo* (`AFTER`) che un evento DML (`INSERT`, `UPDATE`, `DELETE`) si verifica su quella tabella.

**Componenti Chiave di un Trigger:**

* **Nome:** Identificativo univoco per il trigger nello schema.
* **Timing:** `BEFORE` (eseguito prima della modifica della riga) o `AFTER` (eseguito dopo la modifica della riga).
* **Evento:** `INSERT`, `UPDATE`, o `DELETE`.
* **Tabella:** La tabella a cui il trigger è associato.
* **Corpo:** Il codice SQL da eseguire. Per trigger multi-istruzione, si usa `BEGIN...END;`. È necessario cambiare temporaneamente il `DELIMITER` per definire il corpo.
* **`FOR EACH ROW`:** Indica che il corpo del trigger viene eseguito una volta per ogni riga interessata dall'istruzione DML. (Questo è l'unico tipo supportato da MySQL/MariaDB; lo standard SQL prevede anche trigger a livello di istruzione).
* **Pseudo-Righe `NEW` e `OLD`:** All'interno del corpo del trigger `FOR EACH ROW`:
    * `INSERT`: `NEW.colonna` contiene i valori della nuova riga da inserire.
    * `UPDATE`: `OLD.colonna` contiene i valori della riga *prima* della modifica, `NEW.colonna` contiene i valori *dopo* la modifica. È possibile modificare i valori in `NEW` in un trigger `BEFORE UPDATE`.
    * `DELETE`: `OLD.colonna` contiene i valori della riga che sta per essere eliminata.

**Sintassi Esempio:**

```sql
CREATE TABLE Products (
    ProductID INT AUTO_INCREMENT PRIMARY KEY,
    ProductName VARCHAR(255) NOT NULL,
    Description TEXT,
    UnitPrice DECIMAL(10, 2) NOT NULL,
    SKU VARCHAR(50) UNIQUE,
 -- Potrebbero esserci altre colonne come Category, SupplierID, ecc.
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Tabella per mantenere un log delle modifiche ai prezzi
CREATE TABLE ProductPriceLog (
    LogID INT AUTO_INCREMENT PRIMARY KEY,
    ProductID INT,
    OldPrice DECIMAL(10, 2),
    NewPrice DECIMAL(10, 2),
    ChangeTimestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- definizione di un trigger per monitorare il cambiamento dei prezzi automaticamente
-- viene cambiato il delimitatore standard di statement SQL 
DELIMITER //
CREATE TRIGGER trg_LogPriceChange
AFTER UPDATE ON Products
FOR EACH ROW
BEGIN
    -- Esegui solo se il prezzo è effettivamente cambiato
    IF OLD.UnitPrice <> NEW.UnitPrice THEN
        INSERT INTO ProductPriceLog (ProductID, OldPrice, NewPrice)
        VALUES (OLD.ProductID, OLD.UnitPrice, NEW.UnitPrice);
    END IF;
END //
--viene ripristinato il delimitatore standard
DELIMITER ;

-- Trigger per impostare un valore di default complesso prima dell'inserimento
DELIMITER //
CREATE TRIGGER trg_SetDefaultSKU
BEFORE INSERT ON Products
FOR EACH ROW
BEGIN
    IF NEW.SKU IS NULL THEN
        -- Genera SKU basato su categoria (ipotetica) e ID (non disponibile qui, esempio semplificato)
        SET NEW.SKU = CONCAT('PROD-', UPPER(SUBSTRING(NEW.ProductName, 1, 3)), '-', FLOOR(RAND() * 1000));
    END IF;
END //
DELIMITER ;

-- Visualizzare/Eliminare Trigger
SHOW TRIGGERS LIKE 'trg%';
DROP TRIGGER IF EXISTS trg_LogPriceChange;
```

:memo:**Nota:** La differenza principale tra `ProductID` e `SKU (Stock Keeping Unit)` risiede nel loro scopo e nel modo in cui vengono utilizzati:

- `ProductID (ID Prodotto)`:
    - Scopo principale: Identificare in modo univoco un prodotto all'interno del sistema di database dell'azienda. È un identificatore interno.
- `SKU (Stock Keeping Unit)`:
    - Scopo principale: Identificare in modo univoco una specifica variante di un prodotto a fini di inventario, tracciamento e vendita. È un identificatore più orientato al business e al cliente.

:memo: **Nota importante**: Il cambio del delimitatore (`DELIMITER //`) nella definizione di un trigger (o di stored procedure, funzioni, ecc.) è necessario per evitare conflitti con il delimitatore predefinito delle istruzioni SQL, che è il punto e virgola (`;`). I motivi sono:

  1. **Delimitatore predefinito:** Di solito, quando si eseguono comandi SQL, ogni comando termina con un punto e virgola. Il client SQL (come `mysql` da riga di comando o un'interfaccia grafica come phpMyAdmin) usa il punto e virgola per capire dove finisce un'istruzione e dove ne inizia un'altra.

  2. **Corpo del trigger contiene istruzioni SQL:** Il corpo di un trigger (il codice racchiuso tra `BEGIN` e `END`) può contenere una o più istruzioni SQL. Ad esempio, se c'è un'istruzione `IF` e un'istruzione `SET`. Ognuna di queste istruzioni interne deve essere terminata con un punto e virgola.

  3. **Conflitto con il delimitatore predefinito:** Se si cambiasse il delimitatore, il client SQL interpreterebbe il primo punto e virgola che incontra all'interno del blocco `BEGIN...END` come la fine dell'istruzione `CREATE TRIGGER` stessa. Questo porterebbe a un errore di sintassi perché il client si aspetterebbe di trovare la fine dell'istruzione `CREATE TRIGGER` solo dopo aver letto tutto il corpo del trigger.

  4. **Cambio del delimitatore:** Per risolvere questo problema, temporaneamente si cambia il delimitatore da `;` a qualcos'altro, come `//` (o `$$` o qualsiasi altro simbolo che non venga utilizzato all'interno del corpo del trigger). In questo modo, il client SQL sa che l'istruzione `CREATE TRIGGER` continua fino a quando non incontra il nuovo delimitatore.

**Ordine di Esecuzione:** Se ci sono più trigger per lo stesso evento/timing/tabella, MySQL e MariaDB li eseguono nell'ordine di creazione per default. È possibile specificare un ordine esplicito con `FOLLOWS` o `PRECEDES` nella definizione del trigger (da MySQL 5.7.2+).

**Limitazioni:**

* Non possono eseguire comandi di controllo transazione espliciti (`COMMIT`, `ROLLBACK`, `START TRANSACTION`), ma possono operare all'interno della transazione dell'istruzione DML che li ha attivati.
* Non possono restituire result set direttamente al client.
* Non possono usare SQL dinamico (`PREPARE`, `EXECUTE`).
* Non vengono attivati da modifiche causate da `FOREIGN KEY ... ON CASCADE`.

**Ulteriori esempi di trigger online:**

  - [MySQL BEFORE INSERT Trigger](https://www.mysqltutorial.org/mysql-triggers/mysql-before-insert-trigger/)

  - [MySQL AFTER INSERT Trigger](https://www.mysqltutorial.org/mysql-triggers/mysql-after-insert-trigger/)

  - [MySQL BEFORE UPDATE Trigger](https://www.mysqltutorial.org/mysql-triggers/mysql-before-update-trigger/)

  - [MySQL AFTER UPDATE Trigger](https://www.mysqltutorial.org/mysql-triggers/mysql-after-update-trigger/)

**Scheduled Events (Eventi Pianificati):**

Simili ai trigger, ma attivati dal **tempo** invece che da eventi DML. Sono task SQL eseguiti secondo una pianificazione (una tantum o ricorrente).

```sql
-- Abilitare l'Event Scheduler (se non già attivo)
SET GLOBAL event_scheduler = ON;

-- Esempio: Evento per archiviare ordini vecchi ogni notte
CREATE EVENT evt_ArchiveOldOrders
ON SCHEDULE EVERY 1 DAY
STARTS CURRENT_TIMESTAMP + INTERVAL 1 DAY -- Inizia domani
DO
BEGIN
    INSERT INTO ArchivedOrders (...)
    SELECT ... FROM Orders WHERE OrderDate < CURDATE() - INTERVAL 1 YEAR;

    DELETE FROM Orders WHERE OrderDate < CURDATE() - INTERVAL 1 YEAR;
END;

-- Gestione Eventi
SHOW EVENTS;
ALTER EVENT evt_ArchiveOldOrders DISABLE; -- Disabilita
DROP EVENT IF EXISTS evt_ArchiveOldOrders;
```

**Ulteriori dettagli sugli scheduled events si trovano ai link seguenti:**

- [MySQL Events](https://www.mysqltutorial.org/mysql-triggers/working-mysql-scheduled-event/)
- [MySQL ALTER EVENT](https://www.mysqltutorial.org/mysql-triggers/modifying-mysql-events/)

**Importanza Architetturale e per Backend:**

* **Triggers:** Utili per:
    * **Auditing/Logging:** Registrare chi ha modificato cosa e quando.
    * **Validazione Complessa:** Applicare regole di business che non possono essere espresse con semplici `CHECK`.
    * **Mantenimento Integrità Derivata:** Aggiornare automaticamente dati ridondanti o aggregati (es. totale ordine in testata quando si aggiunge una riga dettaglio).
    * **Applicazione di Default Complessi:** Impostare valori predefiniti basati su logica o altre colonne.
    **Vantaggi:** Logica eseguita atomicamente con la modifica, garantita indipendentemente dall'applicazione.
    **Svantaggi:** Logica "nascosta" nel DB, potenziale impatto sulle performance, debugging più complesso, possibili effetti a catena.

* **Scheduled Events:** Utili per:
    * **Manutenzione Periodica:** Pulizia log, ottimizzazione tabelle.
    * **Batch Processing:** Generazione report notturni, aggregazioni dati.
    * **Archiviazione Dati.**
    **Vantaggi:** Automatizzano task ripetitivi a livello DB.
    **Svantaggi:** Richiedono l'Event Scheduler attivo, il debugging può richiedere l'analisi dei log del DB.

:memo::fire:**Importante: L'uso di trigger ed eventi deve essere valutato attentamente perché sono potenti ma possono rendere il sistema meno trasparente. Non sempre sono la scelta ottimale!**

### Logica Riusabile nel Database: Stored Procedures e Stored Functions

Le **Stored Routines** sono blocchi di codice SQL precompilato, memorizzati nel database e richiamabili per nome. Permettono di incapsulare logica complessa o operazioni ripetitive.

**Stored Procedures:**

* **Definizione:** `CREATE PROCEDURE nome_procedura ([param_mode] param_name datatype, ...)`
    * `param_mode`: `IN` (valore passato alla procedura, default), `OUT` (valore restituito dalla procedura al chiamante), `INOUT` (passato e potenzialmente modificato/restituito).
* **Chiamata:** `CALL nome_procedura(argomenti);`
* **Corpo:** Blocco `BEGIN...END` contenente istruzioni SQL, variabili locali (`DECLARE`), strutture di controllo (IF, CASE, LOOP, WHILE, REPEAT), cursori, gestione degli errori (`DECLARE HANDLER`), chiamate ad altre procedure/funzioni.
* **Restituzione Valori:** Tramite parametri `OUT`/`INOUT` o restituendo uno o più **result set** (risultati di query `SELECT`).
* **Ricorsione:** Possibile, ma limitata dalla variabile `max_sp_recursion_depth` (default 0, disabilitata).

**Sintassi Stored Procedure (Esempio):**

```sql
CREATE TABLE Orders (
    OrderID INT AUTO_INCREMENT PRIMARY KEY,
    CustomerID INT NOT NULL,
    OrderDate DATE NOT NULL,
    TotalAmount DECIMAL(10, 2) NOT NULL,
    -- Potrebbero esserci altre colonne come ShippingAddress, BillingAddress, OrderStatus, ecc.
    OrderTimestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    LastUpdated TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID) -- Ipotizzando una tabella Customers
);

DELIMITER //
CREATE PROCEDURE sp_GetCustomerOrders (
    IN customerId INT,
    OUT totalOrders INT
)
BEGIN
    -- Conta gli ordini
    SELECT COUNT(*) INTO totalOrders FROM Orders WHERE CustomerID = customerId;

    -- Restituisce il dettaglio degli ordini come result set
    SELECT OrderID, OrderDate, TotalAmount
    FROM Orders
    WHERE CustomerID = customerId
    ORDER BY OrderDate DESC;
END //
DELIMITER ;

-- Chiamata
CALL sp_GetCustomerOrders(123, @orderCount);
SELECT @orderCount; -- Mostra il valore restituito tramite parametro OUT
-- Il client riceverà anche il result set con l'elenco degli ordini
```

**Stored Functions:**

* **Definizione:** `CREATE FUNCTION nome_funzione ([param_name datatype, ...]) RETURNS return_datatype [characteristic ...]`
    * `characteristic`: Es. `DETERMINISTIC` (stesso input -> stesso output), `READS SQL DATA` (legge dati), `MODIFIES SQL DATA` (modifica dati, sconsigliato per funzioni), `NOT DETERMINISTIC` (default).
* **Chiamata:** Usata all'interno di espressioni SQL, come le funzioni built-in (es. `SELECT nome_funzione(colonna) FROM ...`).
* **Parametri:** Solo `IN` (la parola chiave `IN` è implicita e non si scrive).
* **Restituzione Valori:** **Deve** restituire un **singolo valore scalare** del tipo specificato in `RETURNS`, usando l'istruzione `RETURN valore;`. Non può restituire result set.
* **Ricorsione:** Non permessa.

**Sintassi Stored Function (Esempio):**

```sql
DELIMITER //
CREATE FUNCTION fn_CalculateDiscount (
    originalPrice DECIMAL(10, 2),
    discountPercentage INT
)
RETURNS DECIMAL(10, 2)
DETERMINISTIC
BEGIN
    DECLARE discountedPrice DECIMAL(10, 2);
    IF discountPercentage < 0 OR discountPercentage > 100 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Percentuale sconto non valida'; -- Solleva errore
    END IF;
    SET discountedPrice = originalPrice * (1 - (discountPercentage / 100.0));
    RETURN discountedPrice;
END //
DELIMITER ;

-- Utilizzo
SELECT
    ProductName,
    UnitPrice,
    fn_CalculateDiscount(UnitPrice, 15) AS PriceWith15PercentDiscount
FROM Products;
```

**Gestione:**

* `DROP PROCEDURE [IF EXISTS] nome_procedura;`
* `DROP FUNCTION [IF EXISTS] nome_funzione;`
* `ALTER PROCEDURE/FUNCTION ...` (Modifica caratteristiche, non corpo/parametri)
* `SHOW CREATE PROCEDURE/FUNCTION ...`
* `SHOW PROCEDURE/FUNCTION STATUS;`

**Importanza Architetturale e per Backend:**

Le stored routines offrono numerosi vantaggi per le applicazioni backend:

* **Incapsulamento e Riusabilità:** Nascondono la complessità, forniscono un'API stabile al database.
* **Performance:** Codice precompilato, riduzione del traffico di rete (una chiamata per eseguire molte istruzioni).
* **Sicurezza:** `GRANT EXECUTE` permette di eseguire la logica senza dare permessi diretti sulle tabelle sottostanti.
* **Manutenibilità:** La logica può essere aggiornata nel database senza ridistribuire l'applicazione (ma attenzione alla compatibilità).
* **Coerenza:** Assicura che la stessa logica venga applicata ovunque venga richiamata.

**Svantaggi:**

* **Testabilità:** Più difficile eseguire unit test sulla logica nel DB rispetto al codice applicativo.
* **Versionamento:** La logica nel DB deve essere versionata insieme all'applicazione che la usa.
* **Complessità Linguaggio:** SQL procedurale è meno espressivo/flessibile dei linguaggi applicativi.
* **Accoppiamento:** Rende l'applicazione più dipendente dalla specifica implementazione del DB.
* **Debugging:** Può essere più complesso rispetto al debugging del codice applicativo.

:memo::fire:**Importante: La scelta di implementare funzionalità in stored routines oppure in codice applicativo dipende da fattori come complessità, requisiti di performance, sicurezza, manutenibilità e competenze del team. Le stored routines e le stored functions sono spesso ideali per operazioni complesse, logica di business critica condivisa, reportistica e task ETL (acronimo di Extract, Transform, Load). Prima di creare codice in stored routines o stored functions è bene valutare con attenzione se siano effettivamente la scelta ottimale, dal momento che oltre ad alcuni vantaggi, hanno anche diversi svantaggi!**

**Altri tutorial su stored procedures e stored functions:**

- [MySQL Stored Procedures - mysqltutorial.org](https://www.mysqltutorial.org/mysql-stored-procedure-tutorial.aspx)
- [MySQL Stored Function - mysqltutorial.org](https://www.mysqltutorial.org/mysql-stored-function/)

### Elaborazione Row-by-Row: Cursor in Stored Routines

Mentre SQL è intrinsecamente orientato ai set (le operazioni `SELECT`, `INSERT`, `UPDATE`, `DELETE` agiscono tipicamente su insiemi di righe), ci sono scenari all'interno della logica delle stored routines (procedure, funzioni, trigger) in cui è necessario elaborare le righe restituite da una query una alla volta. Per questo scopo, MySQL e MariaDB forniscono i **cursori**.

**Cos'è un Cursore?**

Un cursore è un costrutto del database, utilizzabile all'interno di un blocco `BEGIN...END` di una stored routine, che permette di "puntare" a una specifica riga all'interno del result set generato da un'istruzione `SELECT`. **Consente di scorrere questo result set, recuperare i dati di ogni riga in variabili locali e eseguire operazioni specifiche basate su quei dati.**

**Perché Usare i Cursori?**

L'uso principale dei cursori si ha quando:

1. È richiesta un'elaborazione complessa e specifica per ogni singola riga restituita da una query.
2. Il risultato dell'elaborazione di una riga influenza le azioni da compiere successivamente (ad esempio, aggiornare altre tabelle in base ai valori della riga corrente).
3. Si deve chiamare un'altra stored procedure o funzione per ogni riga del result set.

:memo::fire:**Importante:** I cursori implicano un'elaborazione **row-by-row**, che è generalmente **meno performante** delle operazioni SQL standard basate su set. Dovrebbero essere utilizzati solo quando un approccio set-based non è pratico o possibile. **Prima di implementare un cursore, è sempre consigliabile valutare se lo stesso risultato può essere ottenuto con un singolo `UPDATE`, `INSERT` o `DELETE` con clausole `WHERE` appropriate o tramite join.**

**Ciclo di Vita di un Cursore:**

L'utilizzo di un cursore segue un ciclo di vita ben definito all'interno di un blocco `BEGIN...END`:

1. **Dichiarazione del Cursore (`DECLARE CURSOR`):**

    - Associa un nome a un'istruzione `SELECT` che definisce il result set su cui il cursore opererà.
    - **Sintassi:** `DECLARE nome_cursore CURSOR FOR istruzione_select;`
    - **Posizionamento:** La dichiarazione del cursore deve avvenire *dopo* qualsiasi dichiarazione di variabile locale (`DECLARE variabile ...`) ma *prima* della dichiarazione di qualsiasi handler (`DECLARE HANDLER ...`).
2. **Dichiarazione dell'Handler `NOT FOUND` (`DECLARE HANDLER`):**

    - **Fondamentale** per controllare il ciclo di fetch. Quando l'istruzione `Workspace` tenta di leggere oltre l'ultima riga del result set, il database segnala una condizione speciale (SQLSTATE '02000', o semplicemente `NOT FOUND`).
    - Si dichiara un "handler" per questa condizione, che tipicamente imposta una variabile booleana locale (spesso chiamata `done` o `no_more_rows`) a `TRUE`. Questa variabile verrà usata per terminare il ciclo di lettura.
    - **Sintassi:** `DECLARE CONTINUE HANDLER FOR NOT FOUND SET variabile_flag = TRUE;`
        - `CONTINUE`: Indica che dopo l'esecuzione dell'handler (l'assegnamento `SET`), l'esecuzione della stored routine deve continuare dalla istruzione successiva a quella che ha causato l'errore/condizione. È l'opzione tipica per i loop con cursore. (`EXIT` invece terminerebbe il blocco `BEGIN...END` corrente).
3. **Apertura del Cursore (`OPEN`):**

    - Esegue l'istruzione `SELECT` associata al cursore e popola il result set, posizionando un puntatore logico *prima* della prima riga.
    - **Sintassi:** `OPEN nome_cursore;`
4. **Recupero della Riga (`Workspace`):**

    - Recupera i dati della riga successiva nel result set e li assegna a variabili locali definite precedentemente.
    - **Sintassi:** `Workspace nome_cursore INTO variabile1, variabile2, ...;`
    - Il numero, l'ordine e i tipi delle variabili locali devono corrispondere alle colonne selezionate nell'istruzione `SELECT` del cursore.
    - Ogni `Workspace` sposta il puntatore alla riga successiva. Quando non ci sono più righe, il `Workspace` successivo solleva la condizione `NOT FOUND`.
5. **Ciclo di Elaborazione (Loop):**

    - Si utilizza una struttura di loop (come `LOOP`, `REPEAT ... UNTIL`, `WHILE ... DO`) per eseguire ripetutamente l'operazione `Workspace`.
    - Il loop viene terminato quando la variabile flag impostata dall'handler `NOT FOUND` diventa `TRUE`.
    - All'interno del loop, dopo il `Workspace` e prima del controllo di uscita, si inserisce la logica per elaborare i dati contenuti nelle variabili locali popolate dal `Workspace`.
6. **Chiusura del Cursore (`CLOSE`):**

    - Rilascia le risorse interne utilizzate dal cursore (principalmente la memoria per il result set).
    - **Sintassi:** `CLOSE nome_cursore;`
    - È buona norma chiudere esplicitamente i cursori al termine del loro utilizzo. Vengono comunque chiusi automaticamente alla fine del blocco `BEGIN...END` in cui sono stati dichiarati.

**Esempio Pratico (Database BibliotecaUniversitaria):**

Supponiamo di voler creare una procedura che scorre tutti i prestiti scaduti (`stato_prestito = 'scaduto'`) e, per ciascuno, invia una notifica (simulata qui con un `SELECT`) e magari aggiorna un campo (ipotetico) sulla tabella `Studente`.

```sql
DELIMITER //

CREATE PROCEDURE sp_ProcessaPrestitiScaduti()
BEGIN
    -- Variabili locali per contenere i dati del prestito
    DECLARE v_id_prestito INT;
    DECLARE v_matricola_studente VARCHAR(10);
    DECLARE v_id_copia INT;
    DECLARE v_data_scadenza DATETIME;

    -- Variabile flag per controllare la fine del cursore
    DECLARE done BOOLEAN DEFAULT FALSE;

    -- 1. Dichiarazione del Cursore
    DECLARE curPrestitiScaduti CURSOR FOR
        SELECT id_prestito, matricola_studente, id_copia, data_scadenza
        FROM Prestito
        WHERE stato_prestito = 'scaduto' AND data_restituzione IS NULL; -- Assicuriamoci non siano stati restituiti nel frattempo

    -- 2. Dichiarazione dell'Handler NOT FOUND
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;

    -- 3. Apertura del Cursore
    OPEN curPrestitiScaduti;

    -- 4. Ciclo di Elaborazione (LOOP con LEAVE)
    process_loop: LOOP
        -- 5. Recupero della Riga
        FETCH curPrestitiScaduti INTO v_id_prestito, v_matricola_studente, v_id_copia, v_data_scadenza;

        -- Controlla se il flag 'done' è stato impostato dall'handler
        IF done THEN
            LEAVE process_loop; -- Esce dal loop
        END IF;

        -- === Logica di Elaborazione per la riga corrente ===
        -- Simula l'invio di una notifica
        SELECT CONCAT('Notifica: Prestito ID ', v_id_prestito, ' per studente ', v_matricola_studente, ' scaduto il ', DATE(v_data_scadenza)) AS Azione;

        -- Aggiorna un campo ipotetico sulla tabella Studente
        -- UPDATE Studente SET flag_prestiti_scaduti = 1 WHERE matricola_studente = v_matricola_studente;
        -- === Fine Logica di Elaborazione ===

    END LOOP process_loop;

    -- 6. Chiusura del Cursore
    CLOSE curPrestitiScaduti;

END //

DELIMITER ;

-- Per eseguire la procedura:
-- CALL sp_ProcessaPrestitiScaduti();
```

**Ulteriori risorse per i cursori**:

- [MySQL Cursor - mysqltutorial.org](https://www.mysqltutorial.org/mysql-stored-procedure/sql-cursor-in-stored-procedures/)

:memo::fire:**Importante: i cursori sono uno strumento potente ma da usare con cautela a causa delle potenziali implicazioni sulle performance**. Offrono la flessibilità dell'elaborazione row-by-row indispensabile in certi scenari complessi all'interno delle stored routines, ma è sempre preferibile cercare soluzioni basate su set quando possibile. La corretta gestione del ciclo di vita, in particolare l'uso dell'handler `NOT FOUND` per terminare il loop, è essenziale per evitare cicli infiniti.

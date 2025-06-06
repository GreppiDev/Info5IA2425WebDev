# Analisi Approfondita delle Strategie di Progettazione SQL: Un Confronto tra Tre Modelli

- [Analisi Approfondita delle Strategie di Progettazione SQL: Un Confronto tra Tre Modelli](#analisi-approfondita-delle-strategie-di-progettazione-sql-un-confronto-tra-tre-modelli)
  - [Introduzione](#introduzione)
  - [2. Due Filosofie a Confronto: Denormalizzazione Controllata vs. Normalizzazione Pura](#2-due-filosofie-a-confronto-denormalizzazione-controllata-vs-normalizzazione-pura)
  - [3. Analisi Approfondita e Confronto Diretto degli Esercizi](#3-analisi-approfondita-e-confronto-diretto-degli-esercizi)
    - [3.1. Gestione dei Dati Calcolati: Un Percorso a Tre Bivi](#31-gestione-dei-dati-calcolati-un-percorso-a-tre-bivi)
      - [Confronto nel Dettaglio](#confronto-nel-dettaglio)
    - [3.2. Il Ruolo dei Trigger: Sincronizzatori vs. Guardiani](#32-il-ruolo-dei-trigger-sincronizzatori-vs-guardiani)
    - [3.3. Impatto sulla Programmazione: Analisi delle Stored Procedure](#33-impatto-sulla-programmazione-analisi-delle-stored-procedure)
  - [4. Tecniche SQL Avanzate: Oltre le Basi di MariaDB](#4-tecniche-sql-avanzate-oltre-le-basi-di-mariadb)
    - [4.1. Common Table Expressions (CTE): Organizzare la Complessità](#41-common-table-expressions-cte-organizzare-la-complessità)
    - [4.2. Viste Materializzate: La "Cache" del Database](#42-viste-materializzate-la-cache-del-database)
  - [5. Sintesi e Guida alla Scelta Progettuale](#5-sintesi-e-guida-alla-scelta-progettuale)
    - [Tendenze e Consigli per lo Sviluppatore Moderno](#tendenze-e-consigli-per-lo-sviluppatore-moderno)

## Introduzione

L'analisi comparativa dei tre esercizi --- **Gestione Biblioteca**, **Gestione E-commerce** e **Gestione Campionato Sportivo** --- è uno strumento potente per comprendere le diverse filosofie di progettazione di un database. Sebbene tutti e tre i modelli siano funzionali, adottano approcci radicalmente diversi per risolvere un problema comune: il bilanciamento tra **prestazioni**, **integrità dei dati** e **complessità di manutenzione**.

Questo documento si propone di andare oltre una discussione teorica, analizzando nel dettaglio le scelte implementative di ciascun esercizio, confrontando direttamente il codice SQL per illustrare i pro e i contro di ogni strategia.

## 2. Due Filosofie a Confronto: Denormalizzazione Controllata vs. Normalizzazione Pura

Il cuore della differenza tra i modelli risiede nella gestione della **ridondanza dei dati**, un concetto noto anche come **denormalizzazione**.

- **Denormalizzazione Controllata (Biblioteca & E-commerce):** Si sceglie deliberatamente di violare parzialmente le regole della normalizzazione introducendo campi ridondanti (che potrebbero essere calcolati) per un fine preciso: ottimizzare le prestazioni delle operazioni di lettura. La "denormalizzazione" è "controllata" perché l'integrità di questi dati ridondanti è affidata a meccanismi automatici come i **trigger**.

- **Normalizzazione Pura (Campionato Sportivo):** Si persegue l'obiettivo di eliminare ogni forma di ridondanza. Ogni dato esiste in un unico posto (Single Source of Truth) e tutte le informazioni derivate vengono calcolate dinamicamente al momento della richiesta, tipicamente attraverso **Viste** e **Common Table Expressions (CTE)**.

## 3. Analisi Approfondita e Confronto Diretto degli Esercizi

Analizziamo ora come queste due filosofie si traducono in pratica, confrontando le soluzioni specifiche adottate nei tre scenari.

### 3.1. Gestione dei Dati Calcolati: Un Percorso a Tre Bivi

Il modo in cui ogni database gestisce i dati "calcolabili" è l'indicatore più chiaro della sua filosofia progettuale.

| **Esercizio** | **Dato Calcolato** | **Implementazione** | **Pro** | **Contro** |
| --- |  --- |  --- |  --- |  --- |
| **Biblioteca** | `StatoCopia` | **Flag Ridondante** (`ENUM`) aggiornato da trigger. | Query di ricerca semplicissime e ultra-rapide. | Rischio di inconsistenza se un trigger fallisce. |
| **E-commerce** | `TotaleOrdine`, `Giacenza` | **Valori Aggregati Ridondanti** (`DECIMAL`, `INT`) aggiornati da trigger. | Evita ricalcoli costosi su dati numerici. | Logica dei trigger più complessa (gestione UPDATE/DELETE). |
| **Campionato** | `Risultati`, `Classifica` | **Calcolo Dinamico** tramite Viste e CTE. Nessun dato memorizzato. | Massima integrità e flessibilità. | Prestazioni inferiori sulle letture complesse. |

#### Confronto nel Dettaglio

- Biblioteca: La semplicità di un flag

    La query per trovare una copia disponibile è triviale:

    ```sql
    SELECT IDCopia, LibroISBN FROM CopieLibro WHERE StatoCopia = 'Disponibile';
    ```

    Il trigger che la gestisce è altrettanto semplice:

    ```sql
    -- Dopo l'inserimento di un prestito, imposta lo stato. Semplice e diretto.
    UPDATE CopieLibro SET StatoCopia = 'In Prestito' WHERE IDCopia = NEW.IDCopia;
    ```

    Questa è la denormalizzazione nella sua forma più basilare ed efficace: un piccolo campo ridondante che semplifica drasticamente l'operazione più comune.

- E-commerce: La necessità di aggregati stabili

    Qui la denormalizzazione è più complessa. Il TotaleOrdine non è un semplice stato, ma il risultato di un calcolo (SUM). Il trigger deve gestire non solo gli INSERT ma anche gli UPDATE e i DELETE sui DettagliOrdine, ricalcolando le differenze:

    ```sql
    -- Trigger per l'aggiornamento di un dettaglio ordine
    CREATE TRIGGER AggiornaDatiDopoUpdateDettaglio
    AFTER UPDATE ON DettagliOrdine
    FOR EACH ROW
    BEGIN
        -- ... Logica per ricalcolare la differenza di quantità e importo ...
        UPDATE Ordini SET TotaleOrdine = TotaleOrdine + differenzaImporto WHERE IDOrdine = NEW.IDOrdine;
    END;
    ```

    La scelta è giustificata dal fatto che un totale d'ordine, una volta confermato, è un dato "storico" che viene letto molto più spesso di quanto venga modificato. Ricalcolarlo ogni volta sarebbe inefficiente.

- Campionato: L'eleganza della normalizzazione

    Qui, la richiesta apparentemente semplice "Mostrami il risultato della partita X" non ha una risposta diretta nel DB. La risposta viene costruita al momento:

    ```sql
    -- Parte della VistaCalendarioCompleto per calcolare i gol
    WITH GolCalcolatiPerPartita AS (
        SELECT
            p.IDPartita,
            -- Logica complessa con CASE per gestire gol e autogol
            SUM(CASE WHEN mp.TipoGol <> 'Autogol' AND g.IDSquadraAttuale = p.IDSquadraCasa THEN 1 ... END) AS GolCasaEffettivi,
            SUM(CASE WHEN mp.TipoGol <> 'Autogol' AND g.IDSquadraAttuale = p.IDSquadraOspite THEN 1 ... END) AS GolOspiteEffettivi
        FROM Partite p ...
        GROUP BY p.IDPartita
    )
    SELECT ... FROM Partite p LEFT JOIN GolCalcolatiPerPartita gc ON p.IDPartita = gc.IDPartita;

    ```

    Sebbene la query sia complessa, garantisce che il risultato sia **sempre e solo** la somma dei marcatori inseriti. Non c'è alcuna possibilità che la tabella `Partite` riporti `2-1` e la tabella `MarcatoriPartita` contenga solo due gol.

### 3.2. Il Ruolo dei Trigger: Sincronizzatori vs. Guardiani

È fondamentale notare che i trigger sono stati usati con due scopi completamente diversi:

1. Scopo di Sincronizzazione (Biblioteca / E-commerce):

    Il loro unico obiettivo è mantenere allineati i dati ridondanti. Sono il "collante" che tiene insieme la denormalizzazione. La loro logica è puramente tecnica: se X cambia, aggiorna Y.

2. Scopo di Validazione (Campionato):

    Nel modello normalizzato del campionato, i trigger non sincronizzano dati (perché non ce ne sono di ridondanti). Il loro ruolo è invece quello di imporre vincoli di integrità che il DBMS non può gestire nativamente.

    ```sql
    -- Trigger dal Campionato: non sincronizza dati, ma applica una regola di business.
    CREATE TRIGGER CHK_SquadreDiverse_INSERT
    BEFORE INSERT ON Partite
    FOR EACH ROW
    BEGIN
        -- Controlla se la squadra gioca contro se stessa
        IF NEW.IDSquadraCasa = NEW.IDSquadraOspite THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = '...';
        END IF;
    END;
    ```

    Questo è un uso molto più "pulito" e meno controverso dei trigger, accettato anche nelle architetture più moderne.

### 3.3. Impatto sulla Programmazione: Analisi delle Stored Procedure

Il modo migliore per "sentire" l'impatto di una scelta di design è vedere come influenza il codice che interagisce con il database. Confrontiamo le stored procedure principali dei tre esercizi.

- **`RegistraPrestitoLibro` (Biblioteca):** Questa procedura **si affida totalmente** al campo ridondante `StatoCopia` per funzionare in modo efficiente.

    ```sql
    -- La ricerca della copia è banale grazie al campo ridondante
    SELECT IDCopia INTO v_IDCopiaDisponibile
    FROM CopieLibro
    WHERE LibroISBN = p_LibroISBN AND StatoCopia = 'Disponibile'
    LIMIT 1;
    ```

    Se `StatoCopia` non esistesse, questa query dovrebbe eseguire un complesso `LEFT JOIN` con `Prestiti` per escludere le copie già in prestito, diventando molto più lenta.

- **`CreaNuovoOrdine` (E-commerce):** Questa procedura è un esempio di **orchestrazione**. Esegue gli `INSERT` principali e poi **si fida dei trigger** per fare il "lavoro sporco" (aggiornare `TotaleOrdine` e `Giacenza`).

    ```sql
    -- 1. Inserisce l'ordine
    INSERT INTO Ordini ... ;
    SET v_IDOrdineCreato = LAST_INSERT_ID();

    -- 2. Inserisce il dettaglio. A questo punto, i trigger si attivano automaticamente
    --  e aggiornano TotaleOrdine e Giacenza in background.
    INSERT INTO DettagliOrdine (IDOrdine, IDProdotto, Quantita, ...) ... ;

    -- 3. Inserisce il pagamento, leggendo il TotaleOrdine che è stato appena aggiornato dal trigger.
    SELECT TotaleOrdine INTO v_TotaleCalcolatoOrdine FROM Ordini WHERE IDOrdine = v_IDOrdineCreato;
    INSERT INTO Pagamenti (IDOrdine, ImportoPagato, ...) VALUES (v_IDOrdineCreato, v_TotaleCalcolatoOrdine, ...);

    ```

- **`MostraClassificaStagione` (Campionato):** Questa procedura è l'emblema della semplicità, ma è una semplicità "ingannevole".

    ```sql
    CREATE PROCEDURE MostraClassificaStagione(IN p_IDStagione INT)
    BEGIN
        SELECT NomeSquadra, PartiteGiocate, Vittorie, ...
        FROM VistaClassificaStagione -- Tutta la magia è qui!
        WHERE IDStagione = p_IDStagione;
    END;

    ```

    La procedura è banale perché tutta la logica di calcolo, estremamente complessa, è stata **incapsulata e nascosta** all'interno della vista `VistaClassificaStagione`. Questo è un ottimo esempio di astrazione.

## 4. Tecniche SQL Avanzate: Oltre le Basi di MariaDB

Gli esercizi, in particolare quello sul campionato, introducono tecniche che vanno oltre le query di base.

### 4.1. Common Table Expressions (CTE): Organizzare la Complessità

Come già introdotto, una **CTE** (`WITH ... AS (...)`) è uno strumento per rendere leggibili query complesse. È come scomporre un problema matematico difficile in passaggi intermedi più semplici. Senza le CTE, la `VistaClassificaStagione` sarebbe un groviglio quasi incomprensibile di subquery annidate. Sono uno strumento essenziale per ogni sviluppatore SQL moderno.

### 4.2. Viste Materializzate: La "Cache" del Database

Abbiamo visto che una vista standard ricalcola tutto ogni volta. Questo può essere un problema. DBMS più avanzati come **PostgreSQL** offrono le **Viste Materializzate**: il risultato della query viene salvato su disco e le letture successive sono istantanee. L'aggiornamento non è automatico ma deve essere richiesto (`REFRESH`).

**Perché è importante conoscerle?** Perché rappresentano un perfetto **ibrido** tra i due approcci discussi:

1. Si progetta il database in modo **completamente normalizzato** (come nel Campionato), garantendo la massima integrità.

2. Si crea una **vista materializzata** sulla classifica.

3. Si interroga sempre la vista materializzata per avere prestazioni di lettura istantanee (come nel modello della Biblioteca).

4. Si esegue un `REFRESH` della vista solo quando necessario (es. dopo la fine di una partita).

In questo modo, si ottengono i vantaggi di entrambi i mondi: **integrità del modello normalizzato e prestazioni del modello denormalizzato**. Per MariaDB, questo approccio si simula con una tabella di cache e una procedura di aggiornamento.

## 5. Sintesi e Guida alla Scelta Progettuale

La scelta tra denormalizzazione e normalizzazione non è una questione di "giusto" o "sbagliato", ma di **trade-off consapevoli**.

| **Criterio** | **Approccio Denormalizzato + Trigger (Biblioteca/E-commerce)** | **Approccio Normalizzato + Viste/CTE (Campionato)** |
| --- |  --- |  --- |
| **Obiettivo Primario** | **Prestazioni in lettura.** Rispondere velocemente alle domande più comuni. | **Integrità e flessibilità.** Garantire che i dati siano sempre corretti e le regole modificabili. |
| **Query di Lettura** | Semplici e veloci (`SELECT ... WHERE campo_ridondante = ...`). | Potenzialmente complesse e lente (richiedono calcoli `on-the-fly`). |
| **Query di Scrittura** | Rallentate dall'overhead dei trigger. | Veloci e atomiche. |
| **Rischio Maggiore** | **Inconsistenza dei dati** se un trigger contiene un bug o non copre tutti i casi. | **Scarse prestazioni** se il volume di dati cresce e le query non sono ottimizzate. |
| **Manutenzione** | Complessa a causa della logica "nascosta" nei trigger. | Complessa a causa di Viste e CTE articolate, ma la logica è esplicita. |

### Tendenze e Consigli per lo Sviluppatore Moderno

Nel mondo dello sviluppo software moderno (specialmente web), c'è una forte tendenza a **mantenere i database il più "semplici" possibile** e a spostare la logica di business complessa a livello applicativo (es. nel codice Java, Python, C#).

**Perché?**

1. **Testabilità:** È molto più facile scrivere test automatici (unit test) per una classe `OrderService` che per un trigger o una stored procedure del database.

2. **Portabilità:** Se la logica è nell'applicazione, migrare da MariaDB a PostgreSQL o un altro DBMS è più semplice.

3. **Scalabilità:** Nelle architetture a microservizi, la logica è contenuta all'interno dei servizi stessi.

**Questo significa che i trigger sono da evitare? No.** Sono lo strumento giusto per:

- **Imporre vincoli di integrità** che il DBMS non supporta (come `CHK_SquadreDiverse`).

- **Implementare logiche di auditing** (es. salvare ogni modifica in una tabella di log).

- **Gestire semplici denormalizzazioni** quando le prestazioni sono critiche e la logica è stabile, come nel caso della biblioteca.

La vera competenza nella progettazione di database non sta nel sostenere dogmaticamente che un approccio sia sempre superiore all'altro, ma nel possedere la maturità tecnica per **analizzare i requisiti** di un progetto e, di conseguenza, **scegliere** e **giustificare** la strategia più adatta a quel contesto specifico. Questo processo di analisi richiede di porsi domande cruciali: l'applicazione eseguirà milioni di letture al secondo e poche scritture, privilegiando la velocità di accesso (come nella Biblioteca)? L'integrità del dato è talmente critica e le regole di calcolo così soggette a cambiamenti da rendere la normalizzazione l'unica via sicura (come per la classifica del Campionato)? È necessario gestire transazioni complesse la cui affidabilità giustifica l'uso di trigger (come nell'E-commerce)? La capacità di articolare una risposta chiara a queste domande, motivando perché un modello denormalizzato con trigger è ideale per un caso e un modello normalizzato con Viste per un altro, è ciò che distingue un esecutore da un vero progettista di database.

# Progetto "Educational Games"

- [Progetto "Educational Games"](#progetto-educational-games)
  - [Punto 1: Analisi della Realtà di Riferimento, Requisiti, Schema Concettuale](#punto-1-analisi-della-realtà-di-riferimento-requisiti-schema-concettuale)
  - [Punto 2: Schema Logico Relazionale](#punto-2-schema-logico-relazionale)
  - [Punto 3: Definizione SQL (MariaDB) - Sottoinsieme con Vincoli](#punto-3-definizione-sql-mariadb---sottoinsieme-con-vincoli)
  - [Punto 4: Interrogazioni SQL](#punto-4-interrogazioni-sql)
  - [Punto 5: Progetto di Massima dell'Applicazione Web](#punto-5-progetto-di-massima-dellapplicazione-web)
    - [Architettura software/hardware](#architettura-softwarehardware)
    - [Moduli Principali](#moduli-principali)
    - [Diagrammi dei Casi d'Uso (Use Case)](#diagrammi-dei-casi-duso-use-case)
    - [Possibili stack implementativi](#possibili-stack-implementativi)
  - [Fase 6: Parte Significativa dell'Applicazione Web (Esempio)](#fase-6-parte-significativa-dellapplicazione-web-esempio)

## Punto 1: Analisi della Realtà di Riferimento, Requisiti, Schema Concettuale

Identifichiamo prima i requisiti basandoci sulla traccia, distinguendo tra quelli che definiscono i *dati* da memorizzare (utili per il modello E/R) e quelli che descrivono le *funzionalità* del sistema (utili per i casi d'uso).

**1.1 Requisiti di Dato (Cosa dobbiamo memorizzare?):**

- **RD1:** Dati degli utenti (Docenti e Studenti), inclusi dati per la registrazione/autenticazione.
- **RD2:** Dati delle classi virtuali: nome, materia di pertinenza, docente creatore, codice univoco di iscrizione (link/QR code).
- **RD3:** Dati del catalogo dei videogiochi didattici: titolo, descrizione breve (max 160 caratteri), descrizione estesa, numero massimo di monete virtuali ottenibili, fino a tre immagini associate, argomenti di classificazione.
- **RD4:** Elenco predefinito di argomenti per classificare i giochi.
- **RD5:** Elenco predefinito di materie associate alle classi.
- **RD6:** Associazione tra docenti e classi virtuali create da loro.
- **RD7:** Associazione tra studenti e classi virtuali a cui sono iscritti.
- **RD8:** Associazione tra classi virtuali e videogiochi inclusi in esse dal docente.
- **RD9:** Associazione tra videogiochi e argomenti che li classificano.
- **RD10:** Tracciamento delle monete raccolte da uno studente specifico, per un videogioco specifico, all'interno di una classe specifica.

**1.2 Requisiti Funzionali (Cosa deve fare il sistema?):**

- **RF1:** Permettere la registrazione di nuovi utenti (Docenti e Studenti).
- **RF2:** Permettere l'autenticazione degli utenti registrati.
- **RF3:** Permettere ai docenti di creare classi virtuali, specificando nome e materia.
- **RF4:** Generare e fornire un codice di iscrizione univoco (e/o link/QR code) per ogni classe virtuale creata.
- **RF5:** Permettere ai docenti di sfogliare il catalogo dei videogiochi didattici.
- **RF6:** Permettere ai docenti di filtrare/cercare i giochi per argomento. (Implicito da RD3/RD4, utile per query 4a)
- **RF7:** Permettere ai docenti di aggiungere uno o più videogiochi alle proprie classi virtuali.
- **RF8:** Permettere agli studenti di iscriversi a una classe virtuale usando il codice di iscrizione.
- **RF9:** Mostrare agli studenti iscritti i videogiochi associati a ciascuna delle loro classi.
- **RF10:** Registrare le monete ottenute da uno studente mentre gioca a un videogioco specifico nel contesto di una classe. (L'interfaccia di gioco è esterna, ma la piattaforma deve ricevere e salvare questo dato).
- **RF11:** Calcolare e visualizzare la classifica degli studenti per un singolo gioco all'interno di una classe, basata sulle monete raccolte.
- **RF12:** Calcolare e visualizzare la classifica generale degli studenti per una classe, sommando le monete di tutti i giochi in quella classe.
- **RF13:** Permettere ai docenti di monitorare l'andamento (monete raccolte) degli studenti nelle loro classi.
- **RF14:** Visualizzare l'elenco dei giochi per un argomento specifico. (Query 4a)
- **RF15:** Visualizzare la classifica degli studenti per un gioco in una classe. (Query 4b)
- **RF16:** Calcolare e visualizzare quante classi utilizzano ciascun videogioco. (Query 4c)

**1.3 Ipotesi di Lavoro (e Aggiuntive):**

- **Utenti:** Si assume un'unica entità per gli utenti (Docenti e Studenti) con un attributo `Ruolo` per distinguerli. Ogni utente ha credenziali univoche (es. email) e una password (memorizzata in modo sicuro, es. hash).
- **Materie e Argomenti:** Si ipotizza che `Materia` e `Argomento` siano entità separate con un proprio ID e nome, per garantire consistenza e permettere future gestioni (es. aggiunta/rimozione centralizzata).
- **Codice Iscrizione:** Sarà un codice alfanumerico univoco a livello globale per identificare l'invito a una classe. La generazione di link/QR code è una funzionalità dell'interfaccia basata su questo codice.
- **Immagini Gioco:** Memorizzeremo i percorsi (path o URL) delle immagini, non i file binari direttamente nel database. Ipotizziamo 3 campi distinti per i path delle immagini (`Immagine1`, `Immagine2`, `Immagine3`), che possono essere `NULL`.
- **Monete Raccolte:** Il numero di monete è specifico per la *combinazione* `Studente` - `Videogioco` - `ClasseVirtuale`. Uno studente potrebbe giocare lo stesso gioco in classi diverse (se assegnato) con progressi separati. Il valore massimo di monete è una proprietà del gioco, mentre quelle raccolte sono del progresso dello studente.
- **Requisiti di Unicità:**
    - Email utente: Univoca.
    - Nome Classe: Univoco *per Docente*. Una coppia (ID_Docente, NomeClasse) deve essere unica.
    - Titolo Videogioco: Univoco.
    - Nome Materia: Univoco.
    - Nome Argomento: Univoco.
    - Codice Iscrizione Classe: Univoco.
- **Relazioni M:N:** Le relazioni M:N (Studente-Classe, Classe-Gioco, Gioco-Argomento) saranno implementate con tabelle associative.
- **Integrità Referenziale:** Verranno usati vincoli di chiave esterna per garantire la coerenza tra le tabelle collegate. Si ipotizzano politiche `RESTRICT` o `NO ACTION` per `ON DELETE` / `ON UPDATE` come default sicuro, a meno che non sia logico propagare le modifiche (es. `CASCADE` se si cancella un utente, forse si cancellano le classi che ha creato? O si impedisce la cancellazione se ha classi? Ipotizziamo `RESTRICT` per sicurezza).

**1.4 Progettazione Concettuale (Modello E/R):**

- Modello E/R iniziale

    Basandoci sui requisiti di dato (RD) e le ipotesi, definiamo le entità e le associazioni.

    - **Entità:**
        - `UTENTE` (ID_Utente PK, Nome, Cognome, Email UNIQUE, PasswordHash, Ruolo ENUM('Docente', 'Studente'))
        - `MATERIA` (ID_Materia PK, NomeMateria UNIQUE)
        - `ARGOMENTO` (ID_Argomento PK, NomeArgomento UNIQUE)
        - `VIDEOGIOCO` (ID_Gioco PK, Titolo UNIQUE, DescrizioneBreve, DescrizioneEstesa, MaxMonete, Immagine1, Immagine2, Immagine3, DefinizioneGioco)
        - `CLASSE_VIRTUALE` (ID_Classe PK, NomeClasse, CodiceIscrizione UNIQUE, ID_Docente FK -> UTENTE, ID_Materia FK -> MATERIA) - Vincolo UNIQUE su (ID_Docente, NomeClasse)
    - **Associazioni (molti a molti M:N):**
        - `ISCRIZIONE` (Studente si iscrive a Classe - M:N): (ID_Studente FK -> UTENTE, ID_Classe FK -> CLASSE_VIRTUALE) - PK composita (ID_Studente, ID_Classe)
        - `CLASSE_GIOCO` (Classe include Gioco - M:N): (ID_Classe FK -> CLASSE_VIRTUALE, ID_Gioco FK -> VIDEOGIOCO) - PK composita (ID_Classe, ID_Gioco)
        - `GIOCO_ARGOMENTO` (Gioco appartiene a Argomento - M:N): (ID_Gioco FK -> VIDEOGIOCO, ID_Argomento FK -> ARGOMENTO) - PK composita (ID_Gioco, ID_Argomento)
        - `PROGRESSO_STUDENTE` (Tracciamento monete - relazione ternaria Studente-Gioco-Classe): (ID_Studente FK -> UTENTE, ID_Gioco FK -> VIDEOGIOCO, ID_Classe FK -> CLASSE_VIRTUALE, MoneteRaccolte INT >= 0 DEFAULT 0) - PK composita (ID_Studente, ID_Gioco, ID_Classe)

    *Nota sull'E/R Diagram:* Un diagramma grafico mostrerebbe queste entità come rettangoli, attributi come ovali (con PK sottolineato), e relazioni come rombi collegati alle entità con linee indicanti le cardinalità (1:N, M:N). Le tabelle associative sopra derivano direttamente dalla risoluzione delle relazioni M:N e della relazione ternaria nel modello logico.

- Raffinamento del modello concettuale (Modello E/R raffinato)

  - **Gestione caricamento videogiochi:**

      L'entità `VIDEOGIOCHI` non dovrebbe contenere solo metadati, ma anche un descrittore del videogioco, in modo che la piattaforma possa effettivamente erogare i videogiochi. Per Semplicità si assume che ciascun videogioco sia costituito da un quiz a risposta multipla, definito mediante un oggetto json. L'ipotesi di considerare i **quiz a risposta multipla definiti tramite un oggetto JSON** risulta molto pratica e adatta al contesto, per i seguenti motivi:

      1. **Flessibilità:** JSON permette di definire strutture dati anche complesse (domande, risposte multiple, indicazione della risposta corretta, risorse associate come link o immagini, punteggi parziali, ecc.) in modo leggibile e standard.
      2. **Creazione da Parte del Docente/Admin:** Si può immaginare un'interfaccia web all'interno della piattaforma (probabilmente accessibile ai Docenti o agli Admin) che guidi l'utente nella creazione del quiz (inserimento domande, risposte, ecc.) e che, al salvataggio, **generi questo oggetto JSON**.
      3. **Memorizzazione:** Questo JSON, che descrive l'intero gioco/quiz, deve essere salvato nel database.
      4. **Esecuzione Lato Client:** Una pagina web dedicata al gioco (accessibile dallo studente) può, tramite JavaScript:
          - **Scaricare** l'oggetto JSON relativo a quel gioco specifico dal backend.
          - **Interpretare** la struttura JSON.
          - **Renderizzare dinamicamente** l'interfaccia del quiz (mostrare una domanda alla volta, le opzioni di risposta, eventuali immagini/video associati).
          - **Gestire l'interazione** dello studente (selezione delle risposte).
          - **Verificare** le risposte confrontandole con quelle corrette definite nel JSON.
          - **Calcolare un punteggio** (es. percentuale di risposte corrette).
          - **Tradurre il punteggio in "Monete Raccolte"**, magari in proporzione al `MaxMonete` definito nei metadati del gioco.
          - **Inviare il risultato** (le `MoneteRaccolte`) al backend per la registrazione nella tabella `PROGRESSI_STUDENTI`.

      5. **Workflow Dettagliato relativo a questa ipotesi aggiuntiva (videogiochi creati a partire da un oggetto json):**

         1. **Creazione Gioco (Docente/Admin):**
             - Usa un'interfaccia web dedicata sulla piattaforma.
             - Inserisce metadati (Titolo, Descrizione, MaxMonete...).
             - Usa un editor visuale o form per definire le domande, le risposte (segnando quella corretta), le risorse (URL immagini/video).
             - Al salvataggio, il backend:
                 - Genera la stringa JSON che rappresenta la struttura del quiz.
                 - Salva i metadati e la stringa JSON nella tabella `VIDEOGIOCHI`.
         2. **Selezione Gioco (Studente):**
             - Lo studente vede l'elenco dei giochi assegnati alla classe (es. link con Titolo e DescrizioneBreve).
         3. **Avvio Gioco (Studente):**
             - Clicca sul link del gioco.
             - Viene caricata una pagina "player" generica nel frontend.
             - Il JavaScript di questa pagina esegue una `Workspace` a un endpoint API (es. `GET /api/giochi/{ID_Gioco}/play`), passando l'ID del gioco (e magari l'ID della classe per contesto).
         4. **Recupero Dati Gioco (Backend):**
             - L'endpoint API recupera dalla tabella `VIDEOGIOCHI` sia i metadati sia il contenuto della colonna `DefinizioneGioco` per l'ID richiesto.
             - Restituisce questi dati (probabilmente come un unico oggetto JSON) al frontend.
         5. **Rendering ed Esecuzione (Frontend JS):**
             - Il JavaScript nella pagina player riceve i dati.
             - Legge la `DefinizioneGioco` JSON.
             - Costruisce dinamicamente l'HTML per visualizzare la prima domanda, le opzioni di risposta, i link alle risorse, ecc.
             - Gestisce la navigazione tra le domande (se multi-domanda).
             - Registra le risposte dell'utente.
         6. **Valutazione e Invio Punteggio (Frontend JS):**
             - Al termine del quiz (o man mano), il JS confronta le risposte date con quelle corrette nel JSON.
             - Calcola la percentuale di successo.
             - Determina le `MoneteRaccolte` (es. `percentuale * MaxMonete / 100`).
             - Esegue una `Workspace` all'endpoint API per salvare il progresso (es. `POST /api/progressi`), inviando `{ ID_Studente, ID_Gioco, ID_Classe, MoneteRaccolte }`.
         7. **Salvataggio Progresso (Backend):**
             - L'endpoint API riceve i dati del progresso.
             - Li valida (es. l'utente è iscritto a quella classe? Il gioco è assegnato? Le monete non superano `MaxMonete`?).
             - Salva o aggiorna il record nella tabella `PROGRESSI_STUDENTI`.

         Questo approccio sposta la logica di *esecuzione* del gioco/quiz sul frontend (JavaScript), rendendolo molto flessibile, mentre il backend si occupa di orchestrare, memorizzare le definizioni e registrare i risultati. La tua intuizione di usare JSON è quindi un'ottima strada per implementare la funzionalità mancante.

  Qui si riporta il diagramma E/R ristrutturato.

## Punto 2: Schema Logico Relazionale

Traduciamo il modello E/R in uno schema relazionale (praticamente già delineato sopra):

1. `UTENTI` (<u>ID_Utente</u> INT AUTO_INCREMENT, Nome VARCHAR(50) NOT NULL, Cognome VARCHAR(50) NOT NULL, Email VARCHAR(100) NOT NULL UNIQUE, PasswordHash VARCHAR(255) NOT NULL, Ruolo ENUM('Docente', 'Studente') NOT NULL)
2. `MATERIE` (<u>ID_Materia</u> INT AUTO_INCREMENT, NomeMateria VARCHAR(50) NOT NULL UNIQUE)
3. `ARGOMENTI` (<u>ID_Argomento</u> INT AUTO_INCREMENT, NomeArgomento VARCHAR(100) NOT NULL UNIQUE)
4. `VIDEOGIOCHI` (<u>ID_Gioco</u> INT AUTO_INCREMENT, Titolo VARCHAR(100) NOT NULL UNIQUE, DescrizioneBreve VARCHAR(160), DescrizioneEstesa TEXT, MaxMonete INT UNSIGNED NOT NULL DEFAULT 0, Immagine1 VARCHAR(255), Immagine2 VARCHAR(255), Immagine3 VARCHAR(255), DefinizioneGioco JSON)
5. `CLASSI_VIRTUALI` (<u>ID_Classe</u> INT AUTO_INCREMENT, NomeClasse VARCHAR(50) NOT NULL, CodiceIscrizione VARCHAR(20) NOT NULL UNIQUE, *ID_Docente* INT NOT NULL, *ID_Materia* INT NOT NULL, FOREIGN KEY (ID_Docente) REFERENCES UTENTI(ID_Utente) ON DELETE RESTRICT, FOREIGN KEY (ID_Materia) REFERENCES MATERIE(ID_Materia) ON DELETE RESTRICT, UNIQUE KEY (ID_Docente, NomeClasse))
6. `ISCRIZIONI` (<u>*ID_Studente*</u> INT NOT NULL, <u>*ID_Classe*</u> INT NOT NULL, DataIscrizione TIMESTAMP DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY (ID_Studente, ID_Classe), FOREIGN KEY (ID_Studente) REFERENCES UTENTI(ID_Utente) ON DELETE CASCADE, FOREIGN KEY (ID_Classe) REFERENCES CLASSI_VIRTUALI(ID_Classe) ON DELETE CASCADE) - *Nota: Assumiamo CASCADE qui: se studente o classe vengono rimossi, l'iscrizione non ha più senso.*
7. `CLASSI_GIOCHI` (<u>*ID_Classe*</u> INT NOT NULL, <u>*ID_Gioco*</u> INT NOT NULL, PRIMARY KEY (ID_Classe, ID_Gioco), FOREIGN KEY (ID_Classe) REFERENCES CLASSI_VIRTUALI(ID_Classe) ON DELETE CASCADE, FOREIGN KEY (ID_Gioco) REFERENCES VIDEOGIOCHI(ID_Gioco) ON DELETE CASCADE) - *Nota: Assumiamo CASCADE qui: se la classe o il gioco vengono rimossi, l'associazione non ha senso.*
8. `GIOCHI_ARGOMENTI` (<u>*ID_Gioco*</u> INT NOT NULL, <u>*ID_Argomento*</u> INT NOT NULL, PRIMARY KEY (ID_Gioco, ID_Argomento), FOREIGN KEY (ID_Gioco) REFERENCES VIDEOGIOCHI(ID_Gioco) ON DELETE CASCADE, FOREIGN KEY (ID_Argomento) REFERENCES ARGOMENTI(ID_Argomento) ON DELETE CASCADE) - *Nota: CASCADE: se gioco o argomento spariscono, la classificazione sparisce.*
9. `PROGRESSI_STUDENTI` (<u>*ID_Studente*</u> INT NOT NULL, <u>*ID_Gioco*</u> INT NOT NULL, <u>*ID_Classe*</u> INT NOT NULL, MoneteRaccolte INT UNSIGNED NOT NULL DEFAULT 0, UltimoAggiornamento TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, PRIMARY KEY (ID_Studente, ID_Gioco, ID_Classe), FOREIGN KEY (ID_Studente) REFERENCES UTENTI(ID_Utente) ON DELETE CASCADE, FOREIGN KEY (ID_Gioco) REFERENCES VIDEOGIOCHI(ID_Gioco) ON DELETE CASCADE, FOREIGN KEY (ID_Classe) REFERENCES CLASSI_VIRTUALI(ID_Classe) ON DELETE CASCADE, CHECK (MoneteRaccolte >= 0)) - *Nota: CASCADE qui. Il check su MoneteRaccolte >= 0 è un vincolo di dominio.*

## Punto 3: Definizione SQL (MariaDB) - Sottoinsieme con Vincoli

Creiamo le tabelle del database in SQL. Le stesse tabelle potrebbero essere create anche a partire da una migrazione del modello dei dati dal codice applicativo (usando EF Core).

```sql

-- creazione del database

CREATE DATABASE IF NOT EXISTS educational_games;
USE educational_games;

-- --- TABELLE PRINCIPALI ---

-- Tabella Utenti (Docenti e Studenti)
CREATE TABLE UTENTI (
    ID_Utente INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(50) NOT NULL,
    Cognome VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL COMMENT 'Contiene l hash sicuro della password',
    Ruolo ENUM('Docente', 'Studente', 'Admin') NOT NULL COMMENT 'Ruolo dell utente nella piattaforma'
    -- L ENGINE, CHARSET e COLLATE useranno i default del database/server MariaDB
);

-- Tabella Materie
CREATE TABLE MATERIE (
    ID_Materia INT AUTO_INCREMENT PRIMARY KEY,
    NomeMateria VARCHAR(50) NOT NULL UNIQUE COMMENT 'Nome univoco della materia'
);

-- Tabella Argomenti per i videogiochi
CREATE TABLE ARGOMENTI (
    ID_Argomento INT AUTO_INCREMENT PRIMARY KEY,
    NomeArgomento VARCHAR(100) NOT NULL UNIQUE COMMENT 'Nome univoco dell argomento'
);

-- Tabella Videogiochi didattici
CREATE TABLE VIDEOGIOCHI (
    ID_Gioco INT AUTO_INCREMENT PRIMARY KEY,
    Titolo VARCHAR(100) NOT NULL UNIQUE COMMENT 'Titolo univoco del videogioco',
    DescrizioneBreve VARCHAR(160) COMMENT 'Descrizione breve (max 160 caratteri)',
    DescrizioneEstesa TEXT COMMENT 'Descrizione dettagliata del gioco',
    MaxMonete INT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Massimo numero di monete ottenibili nel gioco',
    Immagine1 VARCHAR(255) COMMENT 'URL o path della prima immagine',
    Immagine2 VARCHAR(255) COMMENT 'URL o path della seconda immagine',
    Immagine3 VARCHAR(255) COMMENT 'URL o path della terza immagine',
     -- COLONNA PER LA DEFINIZIONE DEL GIOCO/QUIZ --
    -- Usiamo il tipo JSON per MariaDB (consigliato per validazione e query)
    -- Altrimenti per MySQL, usiamo TEXT (più generico).
    -- https://mariadb.com/kb/en/json/
    DefinizioneGioco JSON NULL COMMENT 'Struttura JSON che definisce il contenuto/regole del gioco (es. quiz)'
    -- DefinizioneGioco TEXT NULL COMMENT 'Alternativa se il tipo JSON non è disponibile/voluto'

);

-- Tabella Classi Virtuali create dai docenti
CREATE TABLE CLASSI_VIRTUALI (
    ID_Classe INT AUTO_INCREMENT PRIMARY KEY,
    NomeClasse VARCHAR(50) NOT NULL COMMENT 'Nome della classe virtuale',
    CodiceIscrizione VARCHAR(20) NOT NULL UNIQUE COMMENT 'Codice univoco per l iscrizione degli studenti',
    ID_Docente INT NOT NULL COMMENT 'FK Riferimento al docente che ha creato la classe',
    ID_Materia INT NOT NULL COMMENT 'FK Riferimento alla materia della classe',

    -- Vincolo di integrità referenziale verso il docente creatore
    FOREIGN KEY (ID_Docente) REFERENCES UTENTI (ID_Utente) 
        ON DELETE RESTRICT -- Non permette di cancellare un docente se ha creato classi
        ON UPDATE CASCADE, -- Se l'ID del docente cambia (improbabile), aggiorna la FK

    -- Vincolo di integrità referenziale verso la materia
    FOREIGN KEY (ID_Materia) REFERENCES MATERIE (ID_Materia) 
        ON DELETE RESTRICT -- Non permette di cancellare una materia se usata in una classe
        ON UPDATE CASCADE,

    -- Vincolo di unicità: un docente non può avere due classi con lo stesso nome
    UNIQUE KEY (ID_Docente, NomeClasse) 
);

-- --- TABELLE ASSOCIATIVE (Molti-a-Molti e Progressi) ---

-- Tabella Iscrizioni (collega Studenti a Classi Virtuali - M:N)
CREATE TABLE ISCRIZIONI (
    ID_Studente INT NOT NULL COMMENT 'FK Riferimento allo studente iscritto (Utente con Ruolo=Studente)',
    ID_Classe INT NOT NULL COMMENT 'FK Riferimento alla classe a cui è iscritto',
    DataIscrizione DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT 'Data e ora di iscrizione (ora locale del server)',
    PRIMARY KEY (ID_Studente, ID_Classe),
    FOREIGN KEY (ID_Studente) REFERENCES UTENTI (ID_Utente) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (ID_Classe) REFERENCES CLASSI_VIRTUALI (ID_Classe) ON DELETE CASCADE ON UPDATE CASCADE
);

-- Tabella ClassiGiochi (collega Classi Virtuali a Videogiochi - M:N)
-- Rappresenta quali giochi sono stati assegnati a quali classi
CREATE TABLE CLASSI_GIOCHI (
    ID_Classe INT NOT NULL COMMENT 'FK Riferimento alla classe virtuale',
    ID_Gioco INT NOT NULL COMMENT 'FK Riferimento al videogioco assegnato',

    -- Chiave primaria composta
    PRIMARY KEY (ID_Classe, ID_Gioco),

    -- Vincoli di integrità referenziale
    FOREIGN KEY (ID_Classe) REFERENCES CLASSI_VIRTUALI(ID_Classe)
    ON DELETE CASCADE   -- Se la classe viene cancellata, rimuovi le associazioni ai giochi
    ON UPDATE CASCADE,
    FOREIGN KEY (ID_Gioco) REFERENCES VIDEOGIOCHI(ID_Gioco)
    ON DELETE CASCADE   -- Se il gioco viene cancellato, rimuovilo dalle classi
    ON UPDATE CASCADE
);

-- Tabella GiochiArgomenti (collega Videogiochi a Argomenti - M:N)
CREATE TABLE GIOCHI_ARGOMENTI (
    ID_Gioco INT NOT NULL COMMENT 'FK Riferimento al videogioco',
    ID_Argomento INT NOT NULL COMMENT 'FK Riferimento all argomento associato',
    -- Chiave primaria composta
    PRIMARY KEY (ID_Gioco, ID_Argomento),
    -- Vincoli di integrità referenziale
    FOREIGN KEY (ID_Gioco) REFERENCES VIDEOGIOCHI(ID_Gioco)
    ON DELETE CASCADE   -- Se il gioco viene cancellato, rimuovi le sue classificazioni per argomento
    ON UPDATE CASCADE,
    FOREIGN KEY (ID_Argomento) REFERENCES ARGOMENTI(ID_Argomento)
    ON DELETE CASCADE   -- Se l'argomento viene cancellato, rimuovi le classificazioni relative
    ON UPDATE CASCADE
);

-- Tabella Progressi Studenti (traccia le monete raccolte da uno studente per un gioco in una classe)
CREATE TABLE PROGRESSI_STUDENTI (
    ID_Studente INT NOT NULL COMMENT 'FK Riferimento allo studente (Utente con Ruolo=Studente)',
    ID_Gioco INT NOT NULL COMMENT 'FK Riferimento al videogioco giocato',
    ID_Classe INT NOT NULL COMMENT 'FK Riferimento alla classe in cui il gioco è stato giocato',
    MoneteRaccolte INT UNSIGNED NOT NULL DEFAULT 0 COMMENT 'Numero di monete raccolte (non può essere negativo)',
    UltimoAggiornamento DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Data e ora dell ultimo aggiornamento (ora locale del server)',
    PRIMARY KEY (
        ID_Studente,
        ID_Gioco,
        ID_Classe
    ),
    FOREIGN KEY (ID_Studente) REFERENCES UTENTI (ID_Utente) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (ID_Gioco) REFERENCES VIDEOGIOCHI (ID_Gioco) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (ID_Classe) REFERENCES CLASSI_VIRTUALI (ID_Classe) ON DELETE CASCADE ON UPDATE CASCADE
    -- CONSTRAINT chk_monete_non_negative CHECK (MoneteRaccolte >= 0) -- Opzionale
);

-- --- FINE DELLO SCHEMA ---
```

## Punto 4: Interrogazioni SQL

a) **Elenco giochi per argomento (es. 'Legge di Ohm') in ordine alfabetico:**

```sql
SELECT
    V.Titolo,
    V.DescrizioneBreve,
    V.MaxMonete
FROM
    VIDEOGIOCHI AS V
JOIN
    GIOCHI_ARGOMENTI AS GA ON V.ID_Gioco = GA.ID_Gioco
JOIN
    ARGOMENTI AS A ON GA.ID_Argomento = A.ID_Argomento
WHERE
    A.NomeArgomento = 'Legge di Ohm' -- Sostituire con l'argomento desiderato
ORDER BY
    V.Titolo ASC;
```

b) **Classifica studenti per un gioco (es. Gioco ID 5) in una classe (es. Classe ID 10):**

```sql
SELECT
    U.Nome,
    U.Cognome,
    PS.MoneteRaccolte
FROM
    PROGRESSI_STUDENTI AS PS
JOIN
    UTENTI AS U ON PS.ID_Studente = U.ID_Utente
WHERE
    PS.ID_Classe = 10 -- Sostituire con l'ID della classe desiderata
    AND PS.ID_Gioco = 5 -- Sostituire con l'ID del gioco desiderato
    AND U.Ruolo = 'Studente' -- Assicuriamoci siano studenti
ORDER BY
    PS.MoneteRaccolte DESC, -- Ordine decrescente per classifica
    U.Cognome ASC,           -- A parità di monete, ordine alfabetico
    U.Nome ASC;
```

c) **Numero di classi in cui è utilizzato ciascun videogioco:**

```sql
SELECT
    V.Titolo,
    COUNT(DISTINCT CG.ID_Classe) AS NumeroClassiUtilizzo
FROM
    VIDEOGIOCHI AS V
LEFT JOIN -- Usiamo LEFT JOIN per includere anche giochi non usati in nessuna classe (conteggio 0)
    CLASSI_GIOCHI AS CG ON V.ID_Gioco = CG.ID_Gioco
GROUP BY
    V.ID_Gioco, V.Titolo -- Raggruppiamo per gioco
ORDER BY
    NumeroClassiUtilizzo DESC, -- Opzionale: ordina per i più usati
    V.Titolo ASC;
```

## Punto 5: Progetto di Massima dell'Applicazione Web

### Architettura software/hardware

- Applicazione Web multi-tier
    - **Client-Side (Browser):** HTML per la struttura, CSS (es. Bootstrap) per lo stile, JavaScript (es. Vanilla JS, React, Vue, Angular) per l'interattività, gestione eventi, chiamate API asincrone (Fetch API).
    - **Server-Side (Backend):**
        - Web Server (es. Nginx, Apache) per gestire richieste HTTP.
        - Application Server con un linguaggio/framework (es. ASP.NET Core) per gestire la logica di business, l'autenticazione/autorizzazione, l'interazione col database.
        - API RESTful per permettere la comunicazione tra client e server.
    - **Database:** MariaDB o MySQL.

### Moduli Principali

- Componenti Funzionali richiesti dalla traccia
    - **Gestione Utenti:** Registrazione, Login, Gestione Profilo, Distinzione Ruoli (Docente/Studente).
    - **Gestione Classi (Docente):** Creazione, Visualizzazione, Generazione Codice Iscrizione, Associazione Giochi.
    - **Gestione Catalogo Giochi:** Visualizzazione, Ricerca/Filtro per Argomento (potrebbe esserci un'area admin per aggiungere giochi/argomenti).
    - **Gestione Iscrizioni (Studente):** Inserimento Codice, Visualizzazione Classi e Giochi.
    - **Interfaccia di Gioco/Progresso:** La traccia non chiede di *creare* i giochi, ma l'applicazione deve poter:
        - Fornire il link/accesso al gioco esterno.
        - Ricevere (probabilmente via API) aggiornamenti sulle monete raccolte dallo studente in quel gioco/classe.
    - **Visualizzazione Classifiche:** Per gioco e generale per classe (sia per studenti che per docenti).
    - **Dashboard Docente:** Riepilogo classi, monitoraggio progressi studenti.
    - **Dashboard Studente:** Riepilogo classi, giochi da svolgere, progressi personali.

- **Simulazione della creazione dei giochi (non richiesta dalla traccia, ma utile per la creazione del prototipo**
  - Per la scrittura del prototipo dell'applicazione si può ipotizzare che la creazione del gioco sia fatta semplicemente con il caricamento di un oggetto JSON che contiene la struttura del quiz, con le domande, le possibili risposte e l'indicazione delle risposte corrette.

### Diagrammi dei Casi d'Uso (Use Case)

Mostriamo l'interazione degli attori (Docente, Studente) con le funzionalità principali (derivate da RF).

- **Attori:**

    - Docente
    - Studente
    - Admin
- **Sistema:** Piattaforma Educational Games

- **Casi d'Uso Principali:**

    - `Registra Utente` (accessibile a entrambi, ma porta a ruoli diversi)
    - `Autentica Utente` (accessibile a entrambi)
    - **(Docente)** `Crea Classe Virtuale`
    - **(Docente)** `Gestisci Classe` (include: visualizza codice, aggiungi/rimuovi giochi)
    - **(Docente)** `Aggiungi Gioco a Classe` (potrebbe includere `Sfoglia Catalogo Giochi`)
    - **(Docente)** `Visualizza Progressi Studenti` (include `Visualizza Classifica Gioco` e `Visualizza Classifica Generale`)
    - **(Studente)** `Iscriviti a Classe` (usa codice)
    - **(Studente)** `Visualizza Classi e Giochi`
    - **(Studente)** `Avvia Videogioco` (l'interazione *durante* il gioco è esterna al backend, ma l'avvio parte da qui)
    - **(Sistema -> Sistema)** `Registra Monete Raccolte` (API chiamata dal gioco per registrare il progresso delle monete raccolte)
    - **(Studente)** `Visualizza Mie Classifiche`
    - **(Studente/Docente)** `Sfoglia Catalogo Giochi` (potrebbe essere accessibile anche prima del login o con permessi diversi)
    - **(Studente/Docente)** `Filtra Giochi per Argomento`
- **Casi d'Uso Aggiuntivi (non previsti dalla traccia, ma utili al prototipo)**
    - **(Docente/Admin)** `Crea videogioco` (crea l'oggetto che contiene i metadati e il JSON che definisce il videogioco)

*Diagramma (descrizione testuale):* Immagina un rettangolo (il Sistema). All'esterno, due figure stilizzate (Docente, Studente). All'interno, ovali per ogni caso d'uso.

- Linee collegano `Docente` a: `Autentica Utente`, `Crea Classe Virtuale`, `Gestisci Classe`, `Aggiungi Gioco a Classe`, `Visualizza Progressi Studenti`, `Sfoglia Catalogo Giochi`, `Filtra Giochi per Argomento`.
- Linee collegano `Studente` a: `Autentica Utente`, `Iscriviti a Classe`, `Visualizza Classi e Giochi`, `Avvia Videogioco`, `Visualizza Mie Classifiche`, `Sfoglia Catalogo Giochi`, `Filtra Giochi per Argomento`.
- `Registra Utente` è accessibile da "Visitatore" (o da entrambi gli attori prima del login).
- `Registra Monete Raccolte` potrebbe essere collegato a `Avvia Videogioco` o essere un caso d'uso separato attivato da un attore "Sistema Esterno (Gioco)".
- Potrebbero esserci relazioni `<<include>>` (es. `Autentica Utente` è inclusa in quasi tutte le azioni post-login) o `<<extend>>`.

### Possibili stack implementativi

Questo richiederebbe la scelta di tecnologie specifiche (es. ASP.NET Core + HTML/JS/Fetch API) e la scrittura di codice sia client che server. Ad esempio:

- **Per un progetto con frontend ti tipo SPA** si potrebbe ipotizzare un'architettura web moderna e disaccoppiata, composta da due applicazioni distinte che comunicano tramite API:

    1. **Backend API (ASP.NET Core Minimal API):**

        - **Tecnologia:** Sviluppata utilizzando ASP.NET Core con il pattern Minimal API. Questo approccio favorisce la creazione di endpoint HTTP leggeri e performanti con una sintassi concisa.
        - **Responsabilità:**
            - Implementazione di tutta la logica di business (creazione classi, gestione iscrizioni, calcolo classifiche, ecc.).
            - Gestione della sicurezza: autenticazione degli utenti (es. tramite token JWT rilasciati al login) e autorizzazione per l'accesso alle diverse funzionalità/dati in base al ruolo (Docente/Studente).
            - Interazione con il database: accesso e manipolazione dei dati nel database MariaDB. Si può utilizzare un ORM come Entity Framework Core (con il provider `Pomelo.EntityFrameworkCore.MySql` per la compatibilità con MariaDB) oppure un micro-ORM come Dapper per mappare i dati tra il database e gli oggetti C#.
            - Esposizione dei dati e delle funzionalità tramite endpoint RESTful ben definiti (es. `POST /api/utenti/registra`, `POST /api/utenti/login`, `GET /api/classi`, `POST /api/classi`, `GET /api/classi/{idClasse}/giochi`, `POST /api/classi/{idClasse}/iscrivi`, `GET /api/classifiche/classe/{idClasse}/gioco/{idGioco}`, `POST /api/progressi`).
        - **CORS (Cross-Origin Resource Sharing):** Essendo un'API separata dal frontend (potenzialmente su domini o porte diverse), sarà necessario configurare le policy CORS nell'applicazione Minimal API per permettere esplicitamente le richieste provenienti dall'origine (dominio/porta) dell'applicazione frontend.
    2. **Frontend Application (Separata):**

        - **Tecnologia (Opzione A - SPA-like con ASP.NET Server):** Un'applicazione ASP.NET Core configurata principalmente per servire file statici. L'applicazione frontend vera e propria risiede nel browser ed è costituita da:
            - `index.html`: Punto di ingresso principale.
            - CSS: Fogli di stile per la presentazione (es. utilizzando framework come Bootstrap o Tailwind CSS, o CSS custom).
            - JavaScript: Logica dell'interfaccia utente, gestione dello stato (semplice o con librerie), routing lato client (usando History API o hash routing per simulare la navigazione tra pagine senza ricaricamenti completi) e comunicazione asincrona (`Fetch` API o librerie come Axios) con il backend Minimal API per recuperare/inviare dati.
        - **Tecnologia (Opzione B - Framework Dedicato):** Un'applicazione sviluppata interamente con un framework/libreria JavaScript moderno come React, Angular, Vue.js, o Svelte, oppure con Blazor WebAssembly.
            - Questi framework offrono un approccio basato su componenti, gestione dello stato avanzata, routing integrato e un ecosistema ricco.
            - Il risultato del processo di build sono file statici (HTML, CSS, JS) che possono essere ospitati su un server web semplice (Nginx, Apache, CDN) o anche tramite un'app ASP.NET Core configurata per servire file statici.
        - **Responsabilità (Comune a Opzione A e B):**
            - Rendering dell'interfaccia utente basata sui dati ricevuti dall'API.
            - Gestione dell'interazione utente.
            - Invio di richieste all'API backend per eseguire azioni (es. login, creazione classe, iscrizione, invio progressi) e recuperare dati.
            - Gestione del token di autenticazione (es. memorizzazione sicura in `localStorage` o `sessionStorage` e invio nell'header `Authorization` delle richieste API).
    3. **Comunicazione e Deployment:**

        - **Comunicazione:** Avviene esclusivamente tramite chiamate HTTP/S tra il Frontend e il Backend API, utilizzando il formato JSON per lo scambio dei dati. L'API definisce il "contratto" che il frontend deve rispettare.
        - **Deployment:** Le due applicazioni sono installate in modo indipendente.
            - Il Backend API può essere ospitato su un server (IIS, Kestrel dietro reverse proxy come Nginx/Apache), in un container Docker, o su piattaforme cloud (es. Azure App Service, AWS Elastic Beanstalk).
            - Il Frontend (specialmente se basato su framework JS o Blazor Wasm) può essere installato come un insieme di file statici su servizi di hosting statico (es. Netlify, Vercel, GitHub Pages, Azure Static Web Apps, AWS S3/CloudFront) o servito da un web server tradizionale o da un'app ASP.NET Core.
        - **Domini/Porte:** Possono risiedere su domini, sottodomini o porte differenti (es. backend su `api.miosito.com` e frontend su `app.miosito.com`, oppure `localhost:7001` per l'API e `localhost:5173` per il frontend durante lo sviluppo), rendendo la configurazione CORS essenziale.

- **Per una soluzione con fronted di tipo Multi Page Application (MPA)** si potrebbe ipotizzare un'architettura web moderna (del tipo **Backend API Separato e Frontend Server Dedicato (BFF - Backend For Frontend)**) nella quale si ha una Minimal API strettamente come backend RESTful (che idealmente usa token JWT per autenticazione stateless), e un'altra applicazione web (es. un'app ASP.NET Core MVC o Razor Pages più tradizionale) che funge da Frontend Server, oppure una soluzione basata su una **Applicazione Unificata (Minimal API serve sia API che Pagine)**. Per lo svolgimento di questo esempio verrà proposta quest'ultima soluzione.

 1. **Applicazione Unificata (Minimal API serve sia API che Pagine):**
    - In questo scenario, l'applicazione ASP.NET Core Minimal API si occupa *sia* di esporre gli endpoint `/api/...` *sia* di servire le pagine HTML/CSS/JS tradizionali.

      1. **Configurazione:** bisogna configurare la Minimal API per servire anche pagine:
          - Servire file statici (HTML/CSS/JS) da una cartella `wwwroot`.
      2. **Autenticazione:** Utilizzare **l'autenticazione basata su Cookie** all'interno della stessa applicazione Minimal API.
          - **Login:** Si crea, ad esempio, un endpoint (es. `POST /account/login`, gestito da Minimal API) che riceve le credenziali, le valida contro il DB (tabella `UTENTI`), e se valide, usa i servizi di autenticazione di ASP.NET Core per creare il principal dell'utente (con i ruoli) ed **emettere il Cookie di Autenticazione**.
          - **Protezione API:** Gli endpoint API (es. `GET /api/classi`) vengono protetti usando la sintassi di Minimal API: `app.MapGet("/api/classi", ...).RequireAuthorization();` o `.RequireAuthorization(policy => policy.RequireRole("Docente"));`. Il middleware ASP.NET Core **validerà il cookie** inviato automaticamente dal browser con le richieste `Workspace` fatte dal JavaScript delle pagine.
          - **Protezione Pagine:** Anche le richieste per le *pagine* HTML (es. una richiesta `GET /CreaClasse`) vengono gestite dalla stessa applicazione. Si può definire un endpoint Minimal API che servono queste pagine e proteggerli allo stesso modo: `app.MapGet("/CreaClasse", (HttpContext context) => { ... return Results.File(...); }).RequireAuthorization(policy => policy.RequireRole("Docente"));`. Il middleware di autenticazione basato su cookie intercetterà la richiesta della pagina, verificherà il cookie e reindirizzerà al login se l'utente non è autenticato o alla pagina "Accesso Negato" se non ha il ruolo corretto.
      3. **Vantaggi:** Applicazione singola da gestire e installare, meccanismo di autenticazione (cookie) unificato sia per le chiamate API dal frontend JS sia per l'accesso diretto alle pagine.
      4. **Svantaggi:** L'applicazione Minimal API diventa un po' meno "minimal", dovendo gestire anche la logica di serving delle pagine. La separazione tra frontend e backend è meno netta, me in compenso l'applicazione è semplice da gestire.

## Fase 6: Parte Significativa dell'Applicazione Web (Esempio)

Per lo sviluppo di questo esempio verrà mostrato il codice necessario alla realizzazione di una **Applicazione Unificata (Minimal API serve sia API che Pagine)**. Realizziamo una struttura di progetto ASP.NET Core Minimal API che funge sia da backend API sia da server per le pagine HTML statiche, utilizzando l'autenticazione basata su cookie.

**1. Struttura del Progetto (cartelle e file principali):**

```text
EducationalGames/
│
├── Dependencies/                     # Dipendenze del progetto (NuGet, etc.)
│
├── Properties/
│   └── launchSettings.json           # Impostazioni di avvio per IIS Express, Kestrel
│
├── Auth/                             # Classi relative all'autenticazione esterna
│   ├── GoogleAuthEvents.cs           # Logica per eventi callback Google
│   └── MicrosoftAuthEvents.cs        # Logica per eventi callback Microsoft
│
├── Data/                             # Classi relative all'accesso ai dati
│   ├── AppDbContext.cs               # Contesto Entity Framework Core
│   └── DatabaseInitializer.cs        # Logica per migrazioni e seeding DB
│
├── DataProtection-Keys/              # Cartella per le chiavi di Data Protection
│
├── Endpoints/                        # Definizioni degli endpoint Minimal API
│   ├── AccountEndpoints.cs           # Endpoint relativi all'account (/api/account/*)
│   └── PageEndpoints.cs              # Endpoint per altre pagine/API 
│
├── Middlewares/                      # Middleware personalizzati
│   └── StatusCodeMiddleware.cs       # Middleware per gestione errori API
│
├── Migrations/                       # File generati da EF Core Migrations
│
├── Models/                           # Classi dei modelli del dominio/database
│
├── ModelsDTO/                        # Classi Data Transfer Object (se usate)
│
├── Utils/                            # Classi di utilità
│   └── RoleUtils.cs                  # Funzioni helper per i ruoli
│
├── wwwroot/                          # Radice per i file statici serviti dal web server
│   ├── assets/                       # Immagini, favicon, etc.
│   ├── components/                   # Componenti HTML riutilizzabili
│   │   ├── navbar.html
│   │   └── footer.html
│   ├── css/                          # Fogli di stile CSS
│   │   └── styles.css
│   ├── js/                           # Script JavaScript
│   │   ├── navbar.js                 # Logica comune navbar e logout
│   │   └── template-loader.js        # Script per caricare componenti HTML
│   ├── index.html                    # Pagina principale/Home
│   ├── profile.html                 # Pagina profilo utente loggato
│   ├── login-failed.html             # Pagina errore login esterno
│   ├── login-page.html               # Pagina dedicata al login
│   ├── not-found.html                # Pagina errore 404
│   └── register.html                 # Pagina dedicata alla registrazione
│
├── appsettings.json                  # File di configurazione principale
├── EducationalGames.http             # File per testare API con estensione REST Client (VS Code)
└── Program.cs                        # File principale di avvio e configurazione dell'applicazione Minimal API
```

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


-- Tabella UTENTI nel prototipo (per testare il login, cambio password, ecc.)
    /* CREATE TABLE UTENTI (
        -- Colonne esistenti
        ID_Utente INT AUTO_INCREMENT PRIMARY KEY,
        Nome VARCHAR(50) NOT NULL,
        Cognome VARCHAR(50) NOT NULL,
        Email VARCHAR(100) NOT NULL UNIQUE,
        PasswordHash VARCHAR(255) NOT NULL COMMENT 'Contiene l hash sicuro della password',
        Ruolo ENUM('Admin', 'Docente', 'Studente') NOT NULL COMMENT 'Ruolo dell utente nella piattaforma', -- Assicurati che l'ordine corrisponda all'enum C#

    -- Nuove colonne per verifica email
    EmailVerificata BOOLEAN NOT NULL DEFAULT FALSE COMMENT 'Indica se l email è stata verificata',
    TokenVerificaEmail VARCHAR(100) NULL DEFAULT NULL COMMENT 'Token inviato per la verifica email',
    ScadenzaTokenVerificaEmail DATETIME NULL DEFAULT NULL COMMENT 'Scadenza del token di verifica email (UTC)',

    -- Nuove colonne per reset password
    TokenResetPassword VARCHAR(100) NULL DEFAULT NULL COMMENT 'Token inviato per il reset password',
    ScadenzaTokenResetPassword DATETIME NULL DEFAULT NULL COMMENT 'Scadenza del token di reset password (UTC)'

    -- L ENGINE, CHARSET e COLLATE useranno i default del database/server MariaDB/MySQL
    ) COMMENT 'Tabella degli utenti della piattaforma';

    -- Indici aggiuntivi (consigliati per performance)
    -- Indice sul token di verifica per velocizzare la ricerca
    CREATE INDEX IX_UTENTI_TokenVerificaEmail ON UTENTI (TokenVerificaEmail);
    -- Indice sul token di reset per velocizzare la ricerca
    CREATE INDEX IX_UTENTI_TokenResetPassword ON UTENTI (TokenResetPassword); */
    --

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
     -- NUOVA COLONNA PER LA DEFINIZIONE DEL GIOCO/QUIZ --
    -- Usa il tipo JSON se il tuo MariaDB lo supporta (consigliato per validazione e query)
    -- Altrimenti per MySQL, usa TEXT (più generico).
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
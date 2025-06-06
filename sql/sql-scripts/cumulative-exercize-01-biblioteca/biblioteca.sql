-- ##############################################################################
-- # Esercitazione SQL: Gestione Biblioteca Universitaria - Svolgimento #
-- ##############################################################################

-- ############################################
-- # 1. Creazione dello Schema Fisico (DDL)   #
-- ############################################

CREATE DATABASE IF NOT EXISTS BibliotecaUniversitariaDB 
    CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE BibliotecaUniversitariaDB;

-- Tabella: Autori
CREATE TABLE IF NOT EXISTS Autori (
    IDAutore INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(50) NOT NULL,
    Cognome VARCHAR(50) NOT NULL,
    DataNascita DATE,
    Nazionalita VARCHAR(30)
);

-- Tabella: Libri (Opere/Titoli)
CREATE TABLE IF NOT EXISTS Libri (
    ISBN VARCHAR(13) PRIMARY KEY,
    Titolo VARCHAR(200) NOT NULL,
    AnnoPubblicazione INT(4),
    Editore VARCHAR(100),
    NumeroPagine INT,
    Lingua VARCHAR(20) DEFAULT 'Italiano'
);

-- Tabella: CopieLibro
CREATE TABLE IF NOT EXISTS CopieLibro (
    IDCopia INT AUTO_INCREMENT PRIMARY KEY,
    LibroISBN VARCHAR(13) NOT NULL,
    Collocazione VARCHAR(50),
    StatoCopia ENUM('Disponibile', 'In Prestito', 'In Manutenzione', 'Smarrito') DEFAULT 'Disponibile' NOT NULL,
    FOREIGN KEY (LibroISBN) REFERENCES Libri(ISBN) ON DELETE CASCADE ON UPDATE CASCADE
);

-- Tabella: LibriAutori (Relazione N:M tra Libri e Autori)
CREATE TABLE IF NOT EXISTS LibriAutori (
    IDLibroAutore INT AUTO_INCREMENT PRIMARY KEY,
    LibroISBN VARCHAR(13) NOT NULL,
    AutoreID INT NOT NULL,
    FOREIGN KEY (LibroISBN) REFERENCES Libri(ISBN) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (AutoreID) REFERENCES Autori(IDAutore) ON DELETE CASCADE ON UPDATE CASCADE,
    UNIQUE (LibroISBN, AutoreID)
);

-- Tabella: Utenti
CREATE TABLE IF NOT EXISTS Utenti (
    IDUtente INT AUTO_INCREMENT PRIMARY KEY,
    Matricola VARCHAR(20) NOT NULL UNIQUE,
    Nome VARCHAR(50) NOT NULL,
    Cognome VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    TipoUtente ENUM('Studente', 'Docente') NOT NULL,
    DataRegistrazione DATE DEFAULT (CURDATE())
);

-- Tabella: Prestiti
CREATE TABLE IF NOT EXISTS Prestiti (
    IDPrestito INT AUTO_INCREMENT PRIMARY KEY,
    IDCopia INT NOT NULL, -- Modificato da LibroISBN a IDCopia
    UtenteID INT NOT NULL,
    DataPrestito DATE NOT NULL DEFAULT (CURDATE()),
    DataRestituzionePrevista DATE NOT NULL,
    DataRestituzioneEffettiva DATE,
    FOREIGN KEY (IDCopia) REFERENCES CopieLibro(IDCopia) ON DELETE RESTRICT ON UPDATE CASCADE,
    FOREIGN KEY (UtenteID) REFERENCES Utenti(IDUtente) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT CHK_DateRestituzione CHECK (DataRestituzioneEffettiva IS NULL OR DataRestituzioneEffettiva >= DataPrestito)
);

-- ####################################################################################
-- # DEFINIZIONE DEI TRIGGER (ANTECEDENTE AL POPOLAMENTO DEI PRESTITI)                #
-- # NOTA PER GLI STUDENTI:                                                         #
-- # I trigger vengono definiti qui, prima di popolare la tabella Prestiti.          #
-- # Questo assicura che la logica di aggiornamento automatico dello StatoCopia       #
-- # sia attiva DURANTE l'inserimento dei dati di esempio dei prestiti.             #
-- # Il trigger DopoNuovoPrestito è stato modificato per gestire correttamente       #
-- # anche i prestiti inseriti come già restituiti.                                 #
-- ####################################################################################

DELIMITER //

-- Trigger `DopoNuovoPrestito`
CREATE TRIGGER DopoNuovoPrestito
AFTER INSERT ON Prestiti
FOR EACH ROW
BEGIN
    IF NEW.DataRestituzioneEffettiva IS NOT NULL THEN
        -- Se il prestito viene inserito come già restituito, la copia è 'Disponibile'
        UPDATE CopieLibro SET StatoCopia = 'Disponibile' WHERE IDCopia = NEW.IDCopia;
    ELSE
        -- Se il prestito è nuovo e in corso, la copia è 'In Prestito'
        UPDATE CopieLibro SET StatoCopia = 'In Prestito' WHERE IDCopia = NEW.IDCopia;
    END IF;
END;
//

-- Trigger `DopoRestituzioneLibro`
CREATE TRIGGER DopoRestituzioneLibro
AFTER UPDATE ON Prestiti
FOR EACH ROW
BEGIN
    -- Questo trigger si attiva quando un prestito ESISTENTE viene aggiornato
    -- e la DataRestituzioneEffettiva passa da NULL a un valore (cioè il libro viene restituito).
    IF NEW.DataRestituzioneEffettiva IS NOT NULL AND OLD.DataRestituzioneEffettiva IS NULL THEN
        UPDATE CopieLibro SET StatoCopia = 'Disponibile' WHERE IDCopia = NEW.IDCopia;
    END IF;
    -- Se un prestito viene aggiornato e DataRestituzioneEffettiva diventa NULL (caso raro, es. errore correzione)
    -- lo stato della copia potrebbe dover tornare a 'In Prestito'.
    IF NEW.DataRestituzioneEffettiva IS NULL AND OLD.DataRestituzioneEffettiva IS NOT NULL THEN
        UPDATE CopieLibro SET StatoCopia = 'In Prestito' WHERE IDCopia = NEW.IDCopia;
    END IF;
END;
//

DELIMITER ; -- Ripristina il delimitatore standard


-- ############################################
-- # 2. Popolamento del Database (DML)        #
-- ############################################

-- Inserimento Autori
INSERT INTO Autori (Nome, Cognome, DataNascita, Nazionalita) VALUES
('Alessandro', 'Manzoni', '1785-03-07', 'Italiana'),
('Umberto', 'Eco', '1932-01-05', 'Italiana'),
('Italo', 'Calvino', '1923-10-15', 'Italiana'),
('George', 'Orwell', '1903-06-25', 'Britannica'),
('J.R.R.', 'Tolkien', '1892-01-03', 'Britannica');

-- Inserimento Libri (Titoli)
INSERT INTO Libri (ISBN, Titolo, AnnoPubblicazione, Editore, NumeroPagine, Lingua) VALUES
('9788804664291', 'I Promessi Sposi', 1827, 'Mondadori', 720, 'Italiano'),
('9788845248790', 'Il Nome della Rosa', 1980, 'Bompiani', 512, 'Italiano'),
('9780141187761', 'Nineteen Eighty-Four', 1949, 'Penguin Books', 328, 'Inglese'),
('9788804391302', 'Il Barone Rampante', 1957, 'Mondadori', 280, 'Italiano'),
('9780618260274', 'The Lord of the Rings', 1954, 'Houghton Mifflin', 1178, 'Inglese'),
('9788806173673', 'Se una notte d''inverno un viaggiatore', 1979, 'Einaudi', 260, 'Italiano'),
('9780451524935', 'Animal Farm', 1945, 'Signet Classics', 144, 'Inglese'),
('9788845292311', 'Il Pendolo di Foucault', 1988, 'Bompiani', 640, 'Italiano');

-- Inserimento CopieLibro
-- NOTA: Ora tutte le copie inizialmente destinate ai prestiti sono 'Disponibile'.
-- I trigger gestiranno il cambio di stato durante l'inserimento dei prestiti.
-- Copie per 'I Promessi Sposi'
INSERT INTO CopieLibro (LibroISBN, Collocazione, StatoCopia) VALUES
('9788804664291', 'A-101', 'Disponibile'), -- Usata in Prestito 4 (in ritardo)
('9788804664291', 'A-102', 'Disponibile'), -- Usata in Prestito 8
('9788804664291', 'A-103', 'In Manutenzione');
-- Copie per 'Il Nome della Rosa'
INSERT INTO CopieLibro (LibroISBN, Collocazione, StatoCopia) VALUES
('9788845248790', 'B-201', 'Disponibile'), -- Usata in Prestito 9
('9788845248790', 'B-202', 'Disponibile'); -- Usata in Prestito 1 (restituita)
-- Copie per 'Nineteen Eighty-Four'
INSERT INTO CopieLibro (LibroISBN, Collocazione, StatoCopia) VALUES
('9780141187761', 'C-301', 'Disponibile'), -- Usata in Prestito 10 (restituita)
('9780141187761', 'C-302', 'Disponibile'); -- Usata in Prestito 2 (in corso)
-- Copie per 'Il Barone Rampante'
INSERT INTO CopieLibro (LibroISBN, Collocazione, StatoCopia) VALUES
('9788804391302', 'D-401', 'Smarrito'),
('9788804391302', 'D-402', 'Disponibile');
-- Copie per 'The Lord of the Rings'
INSERT INTO CopieLibro (LibroISBN, Collocazione, StatoCopia) VALUES
('9780618260274', 'E-501', 'Disponibile'); -- Usata in Prestito 3 (in corso)
-- Copie per 'Se una notte d''inverno un viaggiatore'
INSERT INTO CopieLibro (LibroISBN, Collocazione, StatoCopia) VALUES
('9788806173673', 'F-601', 'Disponibile'), -- Usata in Prestito 5 (restituita)
('9788806173673', 'F-602', 'Disponibile');
-- Copie per 'Animal Farm'
INSERT INTO CopieLibro (LibroISBN, Collocazione, StatoCopia) VALUES
('9780451524935', 'G-701', 'Disponibile'); -- Usata in Prestito 6 (in corso)
-- Copie per 'Il Pendolo di Foucault'
INSERT INTO CopieLibro (LibroISBN, Collocazione, StatoCopia) VALUES
('9788845292311', 'H-801', 'Disponibile'); -- Usata in Prestito 7 (restituita)

-- Associazione Libri-Autori
INSERT INTO LibriAutori (LibroISBN, AutoreID) VALUES
('9788804664291', (SELECT IDAutore FROM Autori WHERE Cognome = 'Manzoni')),
('9788845248790', (SELECT IDAutore FROM Autori WHERE Cognome = 'Eco')),
('9780141187761', (SELECT IDAutore FROM Autori WHERE Cognome = 'Orwell')),
('9788804391302', (SELECT IDAutore FROM Autori WHERE Cognome = 'Calvino')),
('9780618260274', (SELECT IDAutore FROM Autori WHERE Cognome = 'Tolkien')),
('9788806173673', (SELECT IDAutore FROM Autori WHERE Cognome = 'Calvino')),
('9780451524935', (SELECT IDAutore FROM Autori WHERE Cognome = 'Orwell')),
('9788845292311', (SELECT IDAutore FROM Autori WHERE Cognome = 'Eco'));

-- Inserimento Utenti
INSERT INTO Utenti (Matricola, Nome, Cognome, Email, TipoUtente, DataRegistrazione) VALUES
('S1001', 'Mario', 'Rossi', 'mario.rossi@example.com', 'Studente', '2023-09-15'),
('S1002', 'Laura', 'Bianchi', 'laura.bianchi@example.com', 'Studente', '2023-10-01'),
('D2001', 'Paolo', 'Verdi', 'paolo.verdi@example.edu', 'Docente', '2022-05-20'),
('S1003', 'Luca', 'Neri', 'luca.neri@example.com', 'Studente', '2024-01-10'),
('D2002', 'Anna', 'Gialli', 'anna.gialli@example.edu', 'Docente', '2021-11-05'),
('S1004', 'Giulia', 'Bruni', 'giulia.bruni@example.com', 'Studente', '2023-11-20'),
('S1005', 'Marco', 'Ferrari', 'marco.ferrari@example.com', 'Studente', '2024-02-01'),
('D2003', 'Roberto', 'Conti', 'roberto.conti@example.edu', 'Docente', '2023-03-10');

-- Inserimento Prestiti (circa 10)
-- I trigger DopoNuovoPrestito e DopoRestituzioneLibro gestiranno lo StatoCopia.

-- Prestito 1: Rossi, Il Nome della Rosa (copia B-202, ISBN '9788845248790', inserito come restituito)
SET @idCopiaPrestito1 = (SELECT IDCopia FROM CopieLibro WHERE LibroISBN = '9788845248790' AND Collocazione = 'B-202');
INSERT INTO Prestiti (IDCopia, UtenteID, DataPrestito, DataRestituzionePrevista, DataRestituzioneEffettiva)
SELECT @idCopiaPrestito1, U.IDUtente, '2024-03-01', DATE_ADD('2024-03-01', INTERVAL 30 DAY), '2024-03-25'
FROM Utenti U WHERE U.Matricola = 'S1001';
-- Trigger DopoNuovoPrestito imposterà StatoCopia a 'Disponibile' per @idCopiaPrestito1.

-- Prestito 2: Bianchi, Nineteen Eighty-Four (copia C-302, ISBN '9780141187761', inserito come in corso)
SET @idCopiaPrestito2 = (SELECT IDCopia FROM CopieLibro WHERE LibroISBN = '9780141187761' AND Collocazione = 'C-302');
INSERT INTO Prestiti (IDCopia, UtenteID, DataPrestito, DataRestituzionePrevista)
SELECT @idCopiaPrestito2, U.IDUtente, '2024-05-10', DATE_ADD('2024-05-10', INTERVAL 30 DAY)
FROM Utenti U WHERE U.Matricola = 'S1002';
-- Trigger DopoNuovoPrestito imposterà StatoCopia a 'In Prestito' per @idCopiaPrestito2.

-- Prestito 3: Verdi, The Lord of the Rings (copia E-501, ISBN '9780618260274', inserito come in corso)
SET @idCopiaPrestito3 = (SELECT IDCopia FROM CopieLibro WHERE LibroISBN = '9780618260274' AND Collocazione = 'E-501');
INSERT INTO Prestiti (IDCopia, UtenteID, DataPrestito, DataRestituzionePrevista)
SELECT @idCopiaPrestito3, U.IDUtente, '2024-04-15', DATE_ADD('2024-04-15', INTERVAL 90 DAY)
FROM Utenti U WHERE U.Matricola = 'D2001';

-- Prestito 4: Neri, I Promessi Sposi (copia A-101, ISBN '9788804664291', in corso e in ritardo)
SET @idCopiaPrestito4 = (SELECT IDCopia FROM CopieLibro WHERE LibroISBN = '9788804664291' AND Collocazione = 'A-101');
INSERT INTO Prestiti (IDCopia, UtenteID, DataPrestito, DataRestituzionePrevista)
SELECT @idCopiaPrestito4, U.IDUtente, '2024-02-01', DATE_ADD('2024-02-01', INTERVAL 30 DAY) -- Prevista per 2024-03-02
FROM Utenti U WHERE U.Matricola = 'S1003';

-- Prestito 5: Gialli, Se una notte d'inverno un viaggiatore (copia F-601, ISBN '9788806173673', inserito come restituito)
SET @idCopiaPrestito5 = (SELECT IDCopia FROM CopieLibro WHERE LibroISBN = '9788806173673' AND Collocazione = 'F-601');
INSERT INTO Prestiti (IDCopia, UtenteID, DataPrestito, DataRestituzionePrevista, DataRestituzioneEffettiva)
SELECT @idCopiaPrestito5, U.IDUtente, '2024-01-10', DATE_ADD('2024-01-10', INTERVAL 90 DAY), '2024-04-01'
FROM Utenti U WHERE U.Matricola = 'D2002';

-- Prestito 6: Rossi, Animal Farm (copia G-701, ISBN '9780451524935', in corso)
SET @idCopiaPrestito6 = (SELECT IDCopia FROM CopieLibro WHERE LibroISBN = '9780451524935' AND Collocazione = 'G-701');
INSERT INTO Prestiti (IDCopia, UtenteID, DataPrestito, DataRestituzionePrevista)
SELECT @idCopiaPrestito6, U.IDUtente, CURDATE() - INTERVAL 5 DAY, DATE_ADD(CURDATE() - INTERVAL 5 DAY, INTERVAL 30 DAY)
FROM Utenti U WHERE U.Matricola = 'S1001';

-- Prestito 7: Bianchi, Il Pendolo di Foucault (copia H-801, ISBN '9788845292311', inserito come restituito)
SET @idCopiaPrestito7 = (SELECT IDCopia FROM CopieLibro WHERE LibroISBN = '9788845292311' AND Collocazione = 'H-801');
INSERT INTO Prestiti (IDCopia, UtenteID, DataPrestito, DataRestituzionePrevista, DataRestituzioneEffettiva)
SELECT @idCopiaPrestito7, U.IDUtente, '2024-03-15', DATE_ADD('2024-03-15', INTERVAL 30 DAY), '2024-04-10'
FROM Utenti U WHERE U.Matricola = 'S1002';

-- Prestito 8: Verdi, I Promessi Sposi (copia A-102, ISBN '9788804664291', in corso)
SET @idCopiaPrestito8 = (SELECT IDCopia FROM CopieLibro WHERE LibroISBN = '9788804664291' AND Collocazione = 'A-102');
INSERT INTO Prestiti (IDCopia, UtenteID, DataPrestito, DataRestituzionePrevista)
SELECT @idCopiaPrestito8, U.IDUtente, CURDATE() - INTERVAL 10 DAY, DATE_ADD(CURDATE() - INTERVAL 10 DAY, INTERVAL 90 DAY)
FROM Utenti U WHERE U.Matricola = 'D2001';

-- Prestito 9: Neri, Il Nome della Rosa (copia B-201, ISBN '9788845248790', in corso)
SET @idCopiaPrestito9 = (SELECT IDCopia FROM CopieLibro WHERE LibroISBN = '9788845248790' AND Collocazione = 'B-201');
INSERT INTO Prestiti (IDCopia, UtenteID, DataPrestito, DataRestituzionePrevista)
SELECT @idCopiaPrestito9, U.IDUtente, CURDATE() - INTERVAL 2 DAY, DATE_ADD(CURDATE() - INTERVAL 2 DAY, INTERVAL 30 DAY)
FROM Utenti U WHERE U.Matricola = 'S1003';

-- Prestito 10: Gialli, Nineteen Eighty-Four (copia C-301, ISBN '9780141187761', inserito come restituito)
SET @idCopiaPrestito10 = (SELECT IDCopia FROM CopieLibro WHERE LibroISBN = '9780141187761' AND Collocazione = 'C-301');
INSERT INTO Prestiti (IDCopia, UtenteID, DataPrestito, DataRestituzionePrevista, DataRestituzioneEffettiva)
SELECT @idCopiaPrestito10, U.IDUtente, '2023-12-01', DATE_ADD('2023-12-01', INTERVAL 90 DAY), '2024-02-20'
FROM Utenti U WHERE U.Matricola = 'D2002';


-- ############################################
-- # 3. Interrogazioni SQL                    #
-- ############################################

-- ## Query di Base ##

-- 1. Selezionare tutti i libri (titoli) pubblicati dopo il 1970, ordinati per anno di pubblicazione decrescente.
SELECT Titolo, AnnoPubblicazione, Editore, ISBN
FROM Libri
WHERE AnnoPubblicazione > 1970
ORDER BY AnnoPubblicazione DESC;
-- Spiegazione: Seleziona i dettagli dei titoli (opere) dalla tabella Libri.
-- Filtra per AnnoPubblicazione > 1970 e ordina.

-- 2. Selezionare nome, cognome ed email di tutti gli utenti di tipo 'Docente'.
SELECT Nome, Cognome, Email
FROM Utenti
WHERE TipoUtente = 'Docente';
-- Spiegazione: Seleziona i dettagli degli utenti Docenti.

-- 3. Trovare tutte le copie di libri con `StatoCopia` = 'In Manutenzione',
--    visualizzando Titolo del libro, ISBN e Collocazione della copia.
SELECT L.Titolo, L.ISBN, CL.IDCopia, CL.Collocazione
FROM CopieLibro CL
JOIN Libri L ON CL.LibroISBN = L.ISBN
WHERE CL.StatoCopia = 'In Manutenzione';
-- Spiegazione: Unisce CopieLibro e Libri per mostrare informazioni sul titolo.
-- Filtra per le copie in stato 'In Manutenzione'.

-- 4. Elencare tutte le copie disponibili (`StatoCopia` = 'Disponibile') del libro con ISBN '9788845248790',
--    mostrando `IDCopia` e `Collocazione`.
SELECT CL.IDCopia, CL.Collocazione, L.Titolo
FROM CopieLibro CL
JOIN Libri L ON CL.LibroISBN = L.ISBN
WHERE CL.LibroISBN = '9788845248790' AND CL.StatoCopia = 'Disponibile';
-- Spiegazione: Trova le copie disponibili per un ISBN specifico.

-- ## Query con JOIN ##

-- 5. Visualizzare il titolo del libro, la collocazione della copia, il nome e cognome dell'utente
--    che ha preso in prestito ogni copia attualmente non restituita.
SELECT L.Titolo, CL.Collocazione, CL.IDCopia, U.Nome, U.Cognome, P.DataPrestito, P.DataRestituzionePrevista
FROM Prestiti P
JOIN CopieLibro CL ON P.IDCopia = CL.IDCopia
JOIN Libri L ON CL.LibroISBN = L.ISBN
JOIN Utenti U ON P.UtenteID = U.IDUtente
WHERE P.DataRestituzioneEffettiva IS NULL;
-- Spiegazione: Unisce Prestiti, CopieLibro, Libri e Utenti.
-- Mostra i dettagli dei prestiti attivi (copie non restituite).

-- 6. Elencare tutti i libri (titoli) scritti da "Italo Calvino".
SELECT L.Titolo, L.AnnoPubblicazione, L.Editore
FROM Libri L
JOIN LibriAutori LA ON L.ISBN = LA.LibroISBN
JOIN Autori A ON LA.AutoreID = A.IDAutore
WHERE A.Nome = 'Italo' AND A.Cognome = 'Calvino';
-- Spiegazione: Identica alla precedente, in quanto riguarda i titoli.

-- 7. Mostrare i dettagli dei prestiti (IDPrestito, Titolo Libro, IDCopia, Nome Utente, Data Prestito, Data Restituzione Prevista)
--    per i prestiti effettuati nel mese corrente.
SELECT P.IDPrestito, L.Titolo AS TitoloLibro, P.IDCopia, CONCAT(U.Nome, ' ', U.Cognome) AS NomeUtente, P.DataPrestito, P.DataRestituzionePrevista
FROM Prestiti P
JOIN CopieLibro CL ON P.IDCopia = CL.IDCopia
JOIN Libri L ON CL.LibroISBN = L.ISBN
JOIN Utenti U ON P.UtenteID = U.IDUtente
WHERE MONTH(P.DataPrestito) = MONTH(CURDATE()) AND YEAR(P.DataPrestito) = YEAR(CURDATE());
-- Spiegazione: Simile alla precedente, ma il join passa per CopieLibro per arrivare a Libri.

-- ## Query con Funzioni Aggregate e Raggruppamento ##

-- 8. Contare quante copie di ciascun libro (titolo) sono presenti in biblioteca.
--    Visualizzare ISBN, Titolo del libro e il numero di copie.
SELECT L.ISBN, L.Titolo, COUNT(CL.IDCopia) AS NumeroTotaleCopie
FROM Libri L
LEFT JOIN CopieLibro CL ON L.ISBN = CL.LibroISBN -- LEFT JOIN per includere libri senza copie
GROUP BY L.ISBN, L.Titolo
ORDER BY NumeroTotaleCopie DESC;
-- Spiegazione: Raggruppa per libro (titolo) e conta le sue copie. Usato LEFT JOIN per mostrare anche libri senza copie.

-- 9. Calcolare il numero totale di prestiti effettuati da ciascun utente.
--    Visualizzare Matricola, Nome, Cognome dell'utente e il numero di prestiti.
SELECT U.Matricola, U.Nome, U.Cognome, COUNT(P.IDPrestito) AS NumeroPrestiti
FROM Utenti U
LEFT JOIN Prestiti P ON U.IDUtente = P.UtenteID
GROUP BY U.IDUtente, U.Matricola, U.Nome, U.Cognome
ORDER BY NumeroPrestiti DESC;
-- Spiegazione: Identica, i prestiti sono sempre legati agli utenti.

-- 10. Trovare il numero di copie disponibili per ogni libro (titolo).
--     Visualizzare ISBN, Titolo e numero di copie con `StatoCopia` = 'Disponibile'.
SELECT L.ISBN, L.Titolo, COUNT(CL.IDCopia) AS NumeroCopieDisponibili
FROM Libri L
LEFT JOIN CopieLibro CL ON L.ISBN = CL.LibroISBN AND CL.StatoCopia = 'Disponibile'
GROUP BY L.ISBN, L.Titolo
ORDER BY NumeroCopieDisponibili DESC, L.Titolo;
-- Spiegazione: Conta le copie disponibili per ogni titolo. Usato LEFT JOIN e condizione nel JOIN.

-- ## Subquery e Query Complesse ##

-- 11. Elencare gli utenti che non hanno mai effettuato un prestito.
SELECT U.Matricola, U.Nome, U.Cognome, U.Email
FROM Utenti U
WHERE U.IDUtente NOT IN (SELECT DISTINCT P.UtenteID FROM Prestiti P);
-- Spiegazione: Identica.

-- 12. Trovare i libri (titoli) che hanno almeno una copia che è stata presa in prestito più di 1 volta.
SELECT L.ISBN, L.Titolo, COUNT(P.IDPrestito) AS NumeroPrestitiPerLibro
FROM Libri L
JOIN CopieLibro CL ON L.ISBN = CL.LibroISBN
JOIN Prestiti P ON CL.IDCopia = P.IDCopia
GROUP BY L.ISBN, L.Titolo
HAVING COUNT(P.IDPrestito) > 1 
ORDER BY NumeroPrestitiPerLibro DESC;
-- Spiegazione: Conta il numero totale di prestiti per ciascun titolo.

-- ############################################
-- # 4. Viste (View)                          #
-- ############################################

-- 1. Creare una vista chiamata `VistaTitoliConCopieDisponibili` che mostri ISBN, Titolo, Editore
--    e il numero di copie attualmente disponibili per ogni libro che ha almeno una copia disponibile.
CREATE OR REPLACE VIEW VistaTitoliConCopieDisponibili AS
SELECT L.ISBN, L.Titolo, L.Editore, L.AnnoPubblicazione,
       (SELECT COUNT(*) FROM CopieLibro CL WHERE CL.LibroISBN = L.ISBN AND CL.StatoCopia = 'Disponibile') AS NumeroCopieDisponibili
FROM Libri L
WHERE EXISTS (SELECT 1 FROM CopieLibro CL WHERE CL.LibroISBN = L.ISBN AND CL.StatoCopia = 'Disponibile');

-- Per interrogare la vista:
SELECT * FROM VistaTitoliConCopieDisponibili ORDER BY Titolo;
-- Spiegazione: Mostra i titoli che hanno almeno una copia disponibile, e quante ne hanno.

-- 2. Creare una vista chiamata `VistaPrestitiInRitardo` che elenchi IDPrestito, Titolo del libro, IDCopia,
--    Nome e Cognome dell'utente, Email dell'utente, DataRestituzionePrevista per tutti i prestiti
--    la cui DataRestituzioneEffettiva è NULL e la DataRestituzionePrevista è passata rispetto alla data attuale.
CREATE OR REPLACE VIEW VistaPrestitiInRitardo AS
SELECT P.IDPrestito, L.Titolo AS TitoloLibro, P.IDCopia, CL.Collocazione,
       U.Nome AS NomeUtente, U.Cognome AS CognomeUtente, U.Email AS EmailUtente,
       P.DataRestituzionePrevista
FROM Prestiti P
JOIN CopieLibro CL ON P.IDCopia = CL.IDCopia
JOIN Libri L ON CL.LibroISBN = L.ISBN
JOIN Utenti U ON P.UtenteID = U.IDUtente
WHERE P.DataRestituzioneEffettiva IS NULL AND P.DataRestituzionePrevista < CURDATE();

-- Per interrogare la vista:
SELECT * FROM VistaPrestitiInRitardo;
-- Spiegazione: Mostra i prestiti in ritardo, includendo l'IDCopia e la sua collocazione.

-- ############################################
-- # 5. Trigger (Definiti prima del DML Prestiti) #
-- ############################################
-- (Le definizioni dei trigger sono state spostate più in alto nello script)


-- ############################################
-- # 6. Stored Procedure/Function (Rivedute)  #
-- ############################################

DELIMITER //

-- 1. Stored Procedure `RegistraPrestitoLibro`
CREATE PROCEDURE RegistraPrestitoLibro(
    IN p_LibroISBN VARCHAR(13),
    IN p_UtenteID INT,
    OUT p_IDPrestito INT,    -- Parametro OUT per restituire l'ID del prestito
    OUT p_IDCopiaPrestata INT -- Parametro OUT per restituire l'ID della copia prestata
)
BEGIN
    DECLARE v_IDCopiaDisponibile INT DEFAULT NULL;
    DECLARE v_utenteEsiste INT;
    DECLARE v_tipoUtente VARCHAR(10);
    DECLARE v_dataRestituzionePrevista DATE;
    DECLARE v_errorMessage VARCHAR(255);

    -- Controlla se l'utente esiste e recupera il tipo
    SELECT COUNT(*), TipoUtente INTO v_utenteEsiste, v_tipoUtente FROM Utenti WHERE IDUtente = p_UtenteID;
    IF v_utenteEsiste = 0 THEN
        SET v_errorMessage = CONCAT('Errore: Utente con ID ', p_UtenteID, ' non trovato.');
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = v_errorMessage;
    END IF;

    -- Trova una copia disponibile per l'ISBN fornito
    SELECT IDCopia INTO v_IDCopiaDisponibile
    FROM CopieLibro
    WHERE LibroISBN = p_LibroISBN AND StatoCopia = 'Disponibile'
    ORDER BY IDCopia -- Aggiunto ORDER BY per rendere la selezione deterministica se più copie sono disponibili
    LIMIT 1; -- Prende la prima disponibile

    IF v_IDCopiaDisponibile IS NULL THEN
        SET v_errorMessage = CONCAT('Errore: Nessuna copia disponibile per il libro ISBN ', p_LibroISBN, '.');
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = v_errorMessage;
    END IF;

    -- Calcola DataRestituzionePrevista
    IF v_tipoUtente = 'Studente' THEN
        SET v_dataRestituzionePrevista = DATE_ADD(CURDATE(), INTERVAL 30 DAY);
    ELSEIF v_tipoUtente = 'Docente' THEN
        SET v_dataRestituzionePrevista = DATE_ADD(CURDATE(), INTERVAL 90 DAY);
    ELSE
        SET v_errorMessage = 'Errore: Tipo utente non valido.';
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = v_errorMessage;
    END IF;

    -- Inserisce il nuovo prestito
    INSERT INTO Prestiti (IDCopia, UtenteID, DataPrestito, DataRestituzionePrevista)
    VALUES (v_IDCopiaDisponibile, p_UtenteID, CURDATE(), v_dataRestituzionePrevista);

    SET p_IDPrestito = LAST_INSERT_ID();
    SET p_IDCopiaPrestata = v_IDCopiaDisponibile;

END;
//
-- Spiegazione:
-- La procedura ora cerca una IDCopia disponibile per un dato ISBN.
-- Se la trova, procede con la registrazione del prestito.
-- Restituisce l'ID del prestito e l'ID della copia prestata tramite parametri OUT.

-- Test della Stored Procedure:
-- SET @out_id_prestito = NULL; SET @out_id_copia = NULL;
-- CALL RegistraPrestitoLibro('9788806173673', (SELECT IDUtente FROM Utenti WHERE Matricola = 'S1004'), @out_id_prestito, @out_id_copia);
-- SELECT @out_id_prestito AS IDNuovoPrestito, @out_id_copia AS IDCopiaImpegnata;
-- SELECT * FROM CopieLibro WHERE IDCopia = @out_id_copia; -- Per verificare lo stato

-- SET @out_id_prestito_err = NULL; SET @out_id_copia_err = NULL;
-- Tentativo di prestare un libro con solo copie non disponibili (es. 'Il Barone Rampante' ISBN '9788804391302' se D-402 è già in prestito e D-401 è 'Smarrito')
-- Se D-402 fosse disponibile:
-- CALL RegistraPrestitoLibro('9788804391302', (SELECT IDUtente FROM Utenti WHERE Matricola = 'S1001'), @out_id_prestito_err, @out_id_copia_err);
-- SELECT @out_id_prestito_err, @out_id_copia_err;

-- 2. Funzione `ContaCopieDisponibiliLibro`
CREATE FUNCTION ContaCopieDisponibiliLibro(
    f_LibroISBN VARCHAR(13)
)
RETURNS INT
DETERMINISTIC
READS SQL DATA
BEGIN
    DECLARE v_numeroCopieDisponibili INT;

    SELECT COUNT(*) INTO v_numeroCopieDisponibili
    FROM CopieLibro
    WHERE LibroISBN = f_LibroISBN AND StatoCopia = 'Disponibile';

    RETURN v_numeroCopieDisponibili;
END;
//

DELIMITER ; -- Ripristina il delimitatore standard

-- Spiegazione:
-- Conta il numero di copie con StatoCopia = 'Disponibile' per un dato ISBN.

-- Test della Funzione:
SELECT Titolo, ContaCopieDisponibiliLibro(ISBN) AS NumCopieDisponibili
FROM Libri
WHERE ISBN = '9788804664291'; -- I Promessi Sposi

SELECT Titolo, ContaCopieDisponibiliLibro(ISBN) AS NumCopieDisponibili
FROM Libri
WHERE ISBN = '9788804391302'; -- Il Barone Rampante


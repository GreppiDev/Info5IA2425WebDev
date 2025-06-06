-- ####################################################################################################
-- # Esercitazione SQL: Gestione Campionato Sportivo con Stagioni e Allenatori - Svolgimento          #
-- ####################################################################################################

-- ############################################
-- # 1. Creazione dello Schema Fisico (DDL)   #
-- ############################################

CREATE DATABASE IF NOT EXISTS CampionatoDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE CampionatoDB;

-- Tabella: Stagioni
CREATE TABLE IF NOT EXISTS Stagioni (
    IDStagione INT AUTO_INCREMENT PRIMARY KEY,
    Descrizione VARCHAR(50) NOT NULL UNIQUE,
    AnnoInizio YEAR NOT NULL,
    AnnoFine YEAR NOT NULL,
    CONSTRAINT CHK_AnniStagione CHECK (AnnoFine >= AnnoInizio)
);

-- Tabella: Squadre
CREATE TABLE IF NOT EXISTS Squadre (
    IDSquadra INT AUTO_INCREMENT PRIMARY KEY,
    NomeSquadra VARCHAR(100) NOT NULL UNIQUE,
    Citta VARCHAR(50),
    Stadio VARCHAR(100),
    AnnoFondazione INT(4),
    ColoriSociali VARCHAR(50)
);

-- Tabella: Allenatori
CREATE TABLE IF NOT EXISTS Allenatori (
    IDAllenatore INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(50) NOT NULL,
    Cognome VARCHAR(50) NOT NULL,
    DataNascita DATE,
    Nazionalita VARCHAR(30)
);

-- Tabella: SquadrePerStagione
CREATE TABLE IF NOT EXISTS SquadrePerStagione (
    IDSquadraStagione INT AUTO_INCREMENT PRIMARY KEY,
    IDStagione INT NOT NULL,
    IDSquadra INT NOT NULL,
    IDAllenatore INT NULL,
    FOREIGN KEY (IDStagione) REFERENCES Stagioni(IDStagione) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (IDSquadra) REFERENCES Squadre(IDSquadra) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (IDAllenatore) REFERENCES Allenatori(IDAllenatore) ON DELETE SET NULL ON UPDATE CASCADE,
    UNIQUE (IDStagione, IDSquadra)
);

-- Tabella: Giocatori
CREATE TABLE IF NOT EXISTS Giocatori (
    IDGiocatore INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(50) NOT NULL,
    Cognome VARCHAR(50) NOT NULL,
    DataNascita DATE,
    Nazionalita VARCHAR(30),
    Ruolo ENUM('Portiere', 'Difensore', 'Centrocampista', 'Attaccante') NOT NULL,
    NumeroMaglia INT,
    IDSquadraAttuale INT NOT NULL,
    Attivo BOOLEAN DEFAULT TRUE,
    CONSTRAINT CHK_NumeroMaglia CHECK (NumeroMaglia IS NULL OR (NumeroMaglia >= 1 AND NumeroMaglia <= 99)),
    FOREIGN KEY (IDSquadraAttuale) REFERENCES Squadre(IDSquadra) ON DELETE RESTRICT ON UPDATE CASCADE
);

-- Tabella: Partite (MODIFICATA: rimosso CHK_SquadreDiverse)
CREATE TABLE IF NOT EXISTS Partite (
    IDPartita INT AUTO_INCREMENT PRIMARY KEY,
    IDStagione INT NOT NULL,
    DataOraPartita DATETIME NOT NULL,
    IDSquadraCasa INT NOT NULL,
    IDSquadraOspite INT NOT NULL,
    StadioPartita VARCHAR(100),
    NotePartita TEXT,
    Disputata BOOLEAN DEFAULT FALSE,
    FOREIGN KEY (IDStagione) REFERENCES Stagioni(IDStagione) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (IDSquadraCasa) REFERENCES Squadre(IDSquadra) ON DELETE RESTRICT ON UPDATE CASCADE,
    FOREIGN KEY (IDSquadraOspite) REFERENCES Squadre(IDSquadra) ON DELETE RESTRICT ON UPDATE CASCADE
    -- Il vincolo CHK_SquadreDiverse è sostituito da trigger perché MariaDB non supporta i vincoli CHECK
    -- che confrontano valori di colonne diverse in alcuni casi come questo per motivi legati all'implementazione interna.
);

-- Tabella: MarcatoriPartita
CREATE TABLE IF NOT EXISTS MarcatoriPartita (
    IDMarcatorePartita INT AUTO_INCREMENT PRIMARY KEY,
    IDPartita INT NOT NULL,
    IDGiocatore INT NOT NULL,
    MinutoGol INT,
    TipoGol ENUM('Azione', 'Rigore', 'Autogol', 'Punizione') DEFAULT 'Azione',
    FOREIGN KEY (IDPartita) REFERENCES Partite(IDPartita) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (IDGiocatore) REFERENCES Giocatori(IDGiocatore) ON DELETE RESTRICT ON UPDATE CASCADE
);


-- anticipiamo la creazione dei trigger per controllare che i vincoli siano rispettati anche con i valori di esempio inseriti
-- ############################################
-- # 5. Trigger                               #
-- ############################################
DELIMITER //

-- Trigger per sostituire il vincolo CHK_SquadreDiverse
CREATE TRIGGER CHK_SquadreDiverse_INSERT
BEFORE INSERT ON Partite
FOR EACH ROW
BEGIN
    IF NEW.IDSquadraCasa = NEW.IDSquadraOspite THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Errore: La squadra di casa e la squadra ospite devono essere diverse.';
    END IF;
END;
//

CREATE TRIGGER CHK_SquadreDiverse_UPDATE
BEFORE UPDATE ON Partite
FOR EACH ROW
BEGIN
    IF NEW.IDSquadraCasa = NEW.IDSquadraOspite THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Errore: La squadra di casa e la squadra ospite devono essere diverse.';
    END IF;
END;
//

CREATE TRIGGER VerificaMarcatoreSquadra BEFORE INSERT ON MarcatoriPartita FOR EACH ROW
BEGIN
    DECLARE v_squadra_marcatore INT; 
    DECLARE v_squadra_casa_partita INT; 
    DECLARE v_squadra_ospite_partita INT; 
    DECLARE v_giocatore_appartiene BOOLEAN DEFAULT FALSE;
    SELECT IDSquadraAttuale INTO v_squadra_marcatore FROM Giocatori WHERE IDGiocatore = NEW.IDGiocatore;
    SELECT IDSquadraCasa, IDSquadraOspite INTO v_squadra_casa_partita, v_squadra_ospite_partita FROM Partite WHERE IDPartita = NEW.IDPartita;
    IF v_squadra_marcatore = v_squadra_casa_partita OR v_squadra_marcatore = v_squadra_ospite_partita THEN SET v_giocatore_appartiene = TRUE; END IF;
    IF NOT v_giocatore_appartiene THEN SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Errore: Il marcatore non appartiene a nessuna delle due squadre della partita.'; END IF;
END;
//
DELIMITER ;

-- ############################################
-- # 2. Popolamento del Database (DML)        #
-- ############################################

-- Inserimento Stagioni
INSERT INTO Stagioni (Descrizione, AnnoInizio, AnnoFine) VALUES
('Stagione 2023/2024', 2023, 2024),
('Stagione 2024/2025', 2024, 2025);
SET @stagione1ID = (SELECT IDStagione FROM Stagioni WHERE Descrizione = 'Stagione 2023/2024');
SET @stagione2ID = (SELECT IDStagione FROM Stagioni WHERE Descrizione = 'Stagione 2024/2025');

-- Inserimento Squadre
INSERT INTO Squadre (NomeSquadra, Citta, Stadio, AnnoFondazione, ColoriSociali) VALUES
('Juventus FC', 'Torino', 'Allianz Stadium', 1897, 'Bianco-Nero'),
('AC Milan', 'Milano', 'San Siro', 1899, 'Rosso-Nero'),
('Inter Milano', 'Milano', 'San Siro', 1908, 'Nero-Azzurro'),
('AS Roma', 'Roma', 'Stadio Olimpico', 1927, 'Giallo-Rosso'),
('SSC Napoli', 'Napoli', 'Stadio Diego Armando Maradona', 1926, 'Azzurro');
SET @juveID = (SELECT IDSquadra FROM Squadre WHERE NomeSquadra = 'Juventus FC');
SET @milanID = (SELECT IDSquadra FROM Squadre WHERE NomeSquadra = 'AC Milan');
SET @interID = (SELECT IDSquadra FROM Squadre WHERE NomeSquadra = 'Inter Milano');
SET @romaID = (SELECT IDSquadra FROM Squadre WHERE NomeSquadra = 'AS Roma');
SET @napoliID = (SELECT IDSquadra FROM Squadre WHERE NomeSquadra = 'SSC Napoli');


-- Inserimento Allenatori
INSERT INTO Allenatori (Nome, Cognome, DataNascita, Nazionalita) VALUES
('Massimiliano', 'Allegri', '1967-08-11', 'Italiana'),
('Stefano', 'Pioli', '1965-10-20', 'Italiana'),
('Simone', 'Inzaghi', '1976-04-05', 'Italiana'),
('José', 'Mourinho', '1963-01-26', 'Portoghese'),
('Luciano', 'Spalletti', '1959-03-07', 'Italiana'),
('Antonio', 'Conte', '1969-07-31', 'Italiana');
SET @allegriID = (SELECT IDAllenatore FROM Allenatori WHERE Cognome = 'Allegri');
SET @pioliID = (SELECT IDAllenatore FROM Allenatori WHERE Cognome = 'Pioli');
SET @simoneInzaghiID = (SELECT IDAllenatore FROM Allenatori WHERE Cognome = 'Inzaghi' AND Nome = 'Simone');
SET @mourinhoID = (SELECT IDAllenatore FROM Allenatori WHERE Cognome = 'Mourinho');
SET @spallettiID = (SELECT IDAllenatore FROM Allenatori WHERE Cognome = 'Spalletti');
SET @conteID = (SELECT IDAllenatore FROM Allenatori WHERE Cognome = 'Conte');

-- Inserimento SquadrePerStagione
-- Stagione 1 (2023/2024)
INSERT INTO SquadrePerStagione (IDStagione, IDSquadra, IDAllenatore) VALUES
(@stagione1ID, @juveID, @allegriID),
(@stagione1ID, @milanID, @pioliID),
(@stagione1ID, @interID, @simoneInzaghiID),
(@stagione1ID, @romaID, @mourinhoID),
(@stagione1ID, @napoliID, @spallettiID);
-- Stagione 2 (2024/2025) - Ipotizziamo qualche cambio
INSERT INTO SquadrePerStagione (IDStagione, IDSquadra, IDAllenatore) VALUES
(@stagione2ID, @juveID, @allegriID),
(@stagione2ID, @milanID, @conteID), -- Conte al Milan
(@stagione2ID, @interID, @simoneInzaghiID),
(@stagione2ID, @romaID, @mourinhoID),
(@stagione2ID, @napoliID, NULL); -- Napoli senza allenatore definito all'inizio

-- Inserimento Giocatori (assegnati a IDSquadraAttuale)
INSERT INTO Giocatori (Nome, Cognome, DataNascita, Nazionalita, Ruolo, NumeroMaglia, IDSquadraAttuale) VALUES
('Wojciech', 'Szczęsny', '1990-04-18', 'Polacca', 'Portiere', 1, @juveID),
('Dušan', 'Vlahović', '2000-01-28', 'Serba', 'Attaccante', 9, @juveID),
('Federico', 'Chiesa', '1997-10-25', 'Italiana', 'Attaccante', 7, @juveID),
('Mike', 'Maignan', '1995-07-03', 'Francese', 'Portiere', 16, @milanID),
('Rafael', 'Leão', '1999-06-10', 'Portoghese', 'Attaccante', 17, @milanID),
('Olivier', 'Giroud', '1986-09-30', 'Francese', 'Attaccante', 9, @milanID),
('Lautaro', 'Martínez', '1997-08-22', 'Argentina', 'Attaccante', 10, @interID),
('Nicolò', 'Barella', '1997-02-07', 'Italiana', 'Centrocampista', 23, @interID),
('Romelu', 'Lukaku', '1993-05-13', 'Belga', 'Attaccante', 90, @interID),
('Paulo', 'Dybala', '1993-11-15', 'Argentina', 'Attaccante', 21, @romaID),
('Lorenzo', 'Pellegrini', '1996-06-19', 'Italiana', 'Centrocampista', 7, @romaID),
('Tammy', 'Abraham', '1997-10-02', 'Inglese', 'Attaccante', 9, @romaID),
('Victor', 'Osimhen', '1998-12-29', 'Nigeriana', 'Attaccante', 9, @napoliID),
('Khvicha', 'Kvaratskhelia', '2001-02-12', 'Georgiana', 'Attaccante', 77, @napoliID),
('Giovanni', 'Di Lorenzo', '1993-08-04', 'Italiana', 'Difensore', 22, @napoliID),
('Manuel', 'Locatelli', '1998-01-08', 'Italiana', 'Centrocampista', 27, @juveID),
('Theo', 'Hernández', '1997-10-06', 'Francese', 'Difensore', 19, @milanID),
('Milan', 'Škriniar', '1995-02-11', 'Slovacca', 'Difensore', 37, @interID),
('Chris', 'Smalling', '1989-11-22', 'Inglese', 'Difensore', 6, @romaID),
('André-Frank', 'Zambo Anguissa', '1995-11-16', 'Camerunense', 'Centrocampista', 99, @napoliID);


-- Inserimento Partite (distribuite su 2 stagioni, senza colonne dei gol)
-- Stagione 1
INSERT INTO Partite (IDStagione, DataOraPartita, IDSquadraCasa, IDSquadraOspite, Disputata) VALUES
(@stagione1ID, '2023-09-15 20:45:00', @juveID, @milanID, TRUE),
(@stagione1ID, '2023-09-16 15:00:00', @interID, @romaID, TRUE),
(@stagione1ID, '2023-09-22 18:00:00', @romaID, @juveID, TRUE),
(@stagione1ID, '2023-09-23 20:45:00', @milanID, @interID, TRUE),
(@stagione1ID, '2023-10-01 15:00:00', @napoliID, @juveID, TRUE);

-- Stagione 2
INSERT INTO Partite (IDStagione, DataOraPartita, IDSquadraCasa, IDSquadraOspite, Disputata) VALUES
(@stagione2ID, '2024-08-20 20:45:00', @milanID, @napoliID, TRUE),
(@stagione2ID, '2024-08-21 18:00:00', @interID, @juveID, TRUE),
(@stagione2ID, CURDATE() + INTERVAL 7 DAY + INTERVAL '15:00' HOUR_MINUTE, @juveID, @romaID, FALSE),
(@stagione2ID, CURDATE() + INTERVAL 8 DAY + INTERVAL '20:45' HOUR_MINUTE, @milanID, @interID, FALSE),
(@stagione2ID, CURDATE() + INTERVAL 9 DAY + INTERVAL '18:00' HOUR_MINUTE, @napoliID, @romaID, FALSE);

-- Inserimento MarcatoriPartita
-- Partita 1 (Juve-Milan 2-1, Stagione 1) - Con esempio di Autogol
SET @partita1_s1 = (SELECT IDPartita FROM Partite WHERE IDStagione=@stagione1ID AND IDSquadraCasa=@juveID AND IDSquadraOspite=@milanID);
INSERT INTO MarcatoriPartita (IDPartita, IDGiocatore, MinutoGol) VALUES
(@partita1_s1, (SELECT IDGiocatore FROM Giocatori WHERE Cognome = 'Vlahović'), 25);
-- Il secondo gol della Juve è un autogol di un giocatore del Milan
INSERT INTO MarcatoriPartita (IDPartita, IDGiocatore, MinutoGol, TipoGol) VALUES
(@partita1_s1, (SELECT IDGiocatore FROM Giocatori WHERE Cognome = 'Hernández'), 60, 'Autogol');
-- Il gol del Milan
INSERT INTO MarcatoriPartita (IDPartita, IDGiocatore, MinutoGol) VALUES
(@partita1_s1, (SELECT IDGiocatore FROM Giocatori WHERE Cognome = 'Giroud'), 75);

-- Partita 2 (Inter-Roma 3-3, Stagione 1)
SET @partita2_s1 = (SELECT IDPartita FROM Partite WHERE IDStagione=@stagione1ID AND IDSquadraCasa=@interID AND IDSquadraOspite=@romaID);
INSERT INTO MarcatoriPartita (IDPartita, IDGiocatore, MinutoGol, TipoGol) VALUES
(@partita2_s1, (SELECT IDGiocatore FROM Giocatori WHERE Cognome = 'Martínez'), 10, 'Azione'),
(@partita2_s1, (SELECT IDGiocatore FROM Giocatori WHERE Cognome = 'Lukaku'), 33, 'Rigore'),
(@partita2_s1, (SELECT IDGiocatore FROM Giocatori WHERE Cognome = 'Barella'), 88, 'Azione'),
(@partita2_s1, (SELECT IDGiocatore FROM Giocatori WHERE Cognome = 'Dybala'), 15, 'Punizione'),
(@partita2_s1, (SELECT IDGiocatore FROM Giocatori WHERE Cognome = 'Abraham'), 40, 'Azione'),
(@partita2_s1, (SELECT IDGiocatore FROM Giocatori WHERE Cognome = 'Pellegrini'), 90, 'Azione');

-- Partita 3 (Roma-Juve 0-1, Stagione 1)
SET @partita3_s1 = (SELECT IDPartita FROM Partite WHERE IDStagione=@stagione1ID AND IDSquadraCasa=@romaID AND IDSquadraOspite=@juveID);
INSERT INTO MarcatoriPartita (IDPartita, IDGiocatore, MinutoGol) VALUES
(@partita3_s1, (SELECT IDGiocatore FROM Giocatori WHERE Cognome = 'Locatelli'), 55);

-- Partita 6 (Milan-Napoli 3-1, Stagione 2)
SET @partita1_s2 = (SELECT IDPartita FROM Partite WHERE IDStagione=@stagione2ID AND IDSquadraCasa=@milanID AND IDSquadraOspite=@napoliID);
INSERT INTO MarcatoriPartita (IDPartita, IDGiocatore, MinutoGol) VALUES
(@partita1_s2, (SELECT IDGiocatore FROM Giocatori WHERE Cognome = 'Leão'), 15),
(@partita1_s2, (SELECT IDGiocatore FROM Giocatori WHERE Cognome = 'Leão'), 55),
(@partita1_s2, (SELECT IDGiocatore FROM Giocatori WHERE Cognome = 'Giroud'), 70),
(@partita1_s2, (SELECT IDGiocatore FROM Giocatori WHERE Cognome = 'Osimhen'), 85);

-- ############################################
-- # 3. Interrogazioni SQL                    #
-- ############################################

-- ## Query di Base ##
-- 1. Elencare tutte le stagioni registrate, con descrizione, anno di inizio e fine.
SELECT Descrizione, AnnoInizio, AnnoFine FROM Stagioni ORDER BY AnnoInizio DESC;

-- 2. Selezionare tutti i giocatori con ruolo 'Attaccante' della squadra "Juventus FC".
SELECT G.Nome, G.Cognome, G.NumeroMaglia, G.Nazionalita
FROM Giocatori G
JOIN Squadre S ON G.IDSquadraAttuale = S.IDSquadra
WHERE G.Ruolo = 'Attaccante' AND S.NomeSquadra = 'Juventus FC'
ORDER BY G.Cognome, G.Nome;

-- 3. Trovare le squadre e i rispettivi allenatori per la stagione con IDStagione = @stagione1ID.
SELECT S.NomeSquadra, A.Nome AS NomeAllenatore, A.Cognome AS CognomeAllenatore
FROM SquadrePerStagione SPS
JOIN Squadre S ON SPS.IDSquadra = S.IDSquadra
LEFT JOIN Allenatori A ON SPS.IDAllenatore = A.IDAllenatore
WHERE SPS.IDStagione = @stagione1ID
ORDER BY S.NomeSquadra;

-- 4. Visualizzare tutte le partite non ancora disputate per la stagione corrente (identificata da @stagione2ID).
SELECT P.DataOraPartita,
       SCasa.NomeSquadra AS SquadraCasa,
       SOspite.NomeSquadra AS SquadraOspite,
       P.StadioPartita
FROM Partite P
JOIN Squadre SCasa ON P.IDSquadraCasa = SCasa.IDSquadra
JOIN Squadre SOspite ON P.IDSquadraOspite = SOspite.IDSquadra
WHERE P.IDStagione = @stagione2ID AND P.Disputata = FALSE
ORDER BY P.DataOraPartita;

-- ## Query con JOIN ##
-- 5. Mostrare `NomeSquadra`, `Nome` e `Cognome` dell'allenatore per tutte le squadre partecipanti alla stagione "Stagione 2024/2025".
SELECT S.NomeSquadra, A.Nome AS NomeAllenatore, A.Cognome AS CognomeAllenatore
FROM SquadrePerStagione SPS
JOIN Stagioni ST ON SPS.IDStagione = ST.IDStagione
JOIN Squadre S ON SPS.IDSquadra = S.IDSquadra
LEFT JOIN Allenatori A ON SPS.IDAllenatore = A.IDAllenatore
WHERE ST.Descrizione = 'Stagione 2024/2025'
ORDER BY S.NomeSquadra;

-- 6. Visualizzare i dettagli di una partita specifica (es. `IDPartita` = @partita1_s1): nomi delle squadre, data, stagione.
SELECT P.IDPartita, ST.Descrizione AS Stagione, P.DataOraPartita,
       SCasa.NomeSquadra AS SquadraCasa,
       SOspite.NomeSquadra AS SquadraOspite,
       P.StadioPartita, P.Disputata
FROM Partite P
JOIN Stagioni ST ON P.IDStagione = ST.IDStagione
JOIN Squadre SCasa ON P.IDSquadraCasa = SCasa.IDSquadra
JOIN Squadre SOspite ON P.IDSquadraOspite = SOspite.IDSquadra
WHERE P.IDPartita = @partita1_s1;

-- 7. Per ogni gol segnato nella partita con ID = @partita2_s1, mostrare nome/cognome marcatore, squadra e tipo gol.
SELECT G.Nome, G.Cognome, SQ.NomeSquadra AS SquadraMarcatore, MP.MinutoGol, MP.TipoGol
FROM MarcatoriPartita MP
JOIN Giocatori G ON MP.IDGiocatore = G.IDGiocatore
JOIN Squadre SQ ON G.IDSquadraAttuale = SQ.IDSquadra
WHERE MP.IDPartita = @partita2_s1
ORDER BY MP.MinutoGol;

-- ## Query con Funzioni Aggregate e Raggruppamento ##
-- 8. Calcolare il numero di squadre partecipanti per ogni stagione.
SELECT ST.Descrizione AS Stagione, COUNT(SPS.IDSquadra) AS NumeroSquadre
FROM Stagioni ST
JOIN SquadrePerStagione SPS ON ST.IDStagione = SPS.IDStagione
GROUP BY ST.IDStagione, ST.Descrizione
ORDER BY ST.AnnoInizio DESC;

-- 9. Trovare il numero totale di gol segnati da ciascun giocatore nella stagione @stagione1ID (esclusi autogol).
SELECT G.Nome, G.Cognome, SQ.NomeSquadra, COUNT(MP.IDMarcatorePartita) AS TotaleGolStagione
FROM Giocatori G
JOIN MarcatoriPartita MP ON G.IDGiocatore = MP.IDGiocatore
JOIN Partite P ON MP.IDPartita = P.IDPartita
JOIN Squadre SQ ON G.IDSquadraAttuale = SQ.IDSquadra
WHERE P.IDStagione = @stagione1ID AND MP.TipoGol <> 'Autogol'
GROUP BY G.IDGiocatore, G.Nome, G.Cognome, SQ.NomeSquadra
ORDER BY TotaleGolStagione DESC, G.Cognome;

-- 10. Calcolare il numero medio di gol segnati per partita per la stagione @stagione1ID.
SELECT AVG(GolCasaEffettivi + GolOspiteEffettivi) AS MediaGolPerPartita
FROM (
    SELECT
        SUM(CASE WHEN mp.TipoGol <> 'Autogol' AND g.IDSquadraAttuale = p.IDSquadraCasa THEN 1 WHEN mp.TipoGol = 'Autogol' AND g.IDSquadraAttuale = p.IDSquadraOspite THEN 1 ELSE 0 END) AS GolCasaEffettivi,
        SUM(CASE WHEN mp.TipoGol <> 'Autogol' AND g.IDSquadraAttuale = p.IDSquadraOspite THEN 1 WHEN mp.TipoGol = 'Autogol' AND g.IDSquadraAttuale = p.IDSquadraCasa THEN 1 ELSE 0 END) AS GolOspiteEffettivi
    FROM Partite p
    LEFT JOIN MarcatoriPartita mp ON p.IDPartita = mp.IDPartita
    LEFT JOIN Giocatori g ON mp.IDGiocatore = g.IDGiocatore
    WHERE p.Disputata = TRUE AND p.IDStagione = @stagione1ID
    GROUP BY p.IDPartita
) AS PartiteConGol;

-- ## Subquery e Query Complesse ##
-- 11. Elencare le squadre che, nella stagione @stagione1ID, non hanno subito gol in casa.
SELECT s.NomeSquadra FROM Squadre s WHERE s.IDSquadra NOT IN (
    SELECT p.IDSquadraCasa
    FROM Partite p
    JOIN MarcatoriPartita mp ON p.IDPartita = mp.IDPartita
    JOIN Giocatori g ON mp.IDGiocatore = g.IDGiocatore
    WHERE p.IDStagione = @stagione1ID AND p.Disputata = TRUE
      AND (
        (mp.TipoGol <> 'Autogol' AND g.IDSquadraAttuale = p.IDSquadraOspite) OR
        (mp.TipoGol = 'Autogol' AND g.IDSquadraAttuale = p.IDSquadraCasa)
      )
);

-- 12. Trovare i giocatori che hanno segnato in più di una stagione differente.
SELECT G.Nome, G.Cognome, COUNT(DISTINCT P.IDStagione) AS NumeroStagioniConGol
FROM Giocatori G
JOIN MarcatoriPartita MP ON G.IDGiocatore = MP.IDGiocatore
JOIN Partite P ON MP.IDPartita = P.IDPartita
WHERE MP.TipoGol <> 'Autogol' AND P.Disputata = TRUE
GROUP BY G.IDGiocatore, G.Nome, G.Cognome
HAVING COUNT(DISTINCT P.IDStagione) > 1
ORDER BY NumeroStagioniConGol DESC, G.Cognome;

-- ############################################
-- # 4. Viste (View)                          #
-- ############################################

-- 1. VistaCalendarioCompleto
CREATE OR REPLACE VIEW VistaCalendarioCompleto AS
WITH GolCalcolatiPerPartita AS (
    SELECT
        p.IDPartita,
        SUM(CASE WHEN mp.TipoGol <> 'Autogol' AND g.IDSquadraAttuale = p.IDSquadraCasa THEN 1 WHEN mp.TipoGol = 'Autogol' AND g.IDSquadraAttuale = p.IDSquadraOspite THEN 1 ELSE 0 END) AS GolCasaEffettivi,
        SUM(CASE WHEN mp.TipoGol <> 'Autogol' AND g.IDSquadraAttuale = p.IDSquadraOspite THEN 1 WHEN mp.TipoGol = 'Autogol' AND g.IDSquadraAttuale = p.IDSquadraCasa THEN 1 ELSE 0 END) AS GolOspiteEffettivi
    FROM Partite p
    LEFT JOIN MarcatoriPartita mp ON p.IDPartita = mp.IDPartita
    LEFT JOIN Giocatori g ON mp.IDGiocatore = g.IDGiocatore
    WHERE p.Disputata = TRUE
    GROUP BY p.IDPartita
)
SELECT
    ST.Descrizione AS DescrizioneStagione, ST.AnnoInizio, P.DataOraPartita,
    SCasa.NomeSquadra AS NomeSquadraCasa,
    SOspite.NomeSquadra AS NomeSquadraOspite,
    COALESCE(P.StadioPartita, SCasa.Stadio) AS StadioEffettivoPartita,
    P.Disputata,
    GC.GolCasaEffettivi,
    GC.GolOspiteEffettivi
FROM Partite P
JOIN Stagioni ST ON P.IDStagione = ST.IDStagione
JOIN Squadre SCasa ON P.IDSquadraCasa = SCasa.IDSquadra
JOIN Squadre SOspite ON P.IDSquadraOspite = SOspite.IDSquadra
LEFT JOIN GolCalcolatiPerPartita GC ON P.IDPartita = GC.IDPartita
ORDER BY ST.AnnoInizio, P.DataOraPartita;

-- 2. VistaClassificaStagione
CREATE OR REPLACE VIEW VistaClassificaStagione AS
WITH GolCalcolati AS (
    SELECT
        p.IDPartita,
        p.IDStagione,
        p.IDSquadraCasa,
        p.IDSquadraOspite,
        SUM(CASE WHEN mp.TipoGol <> 'Autogol' AND g.IDSquadraAttuale = p.IDSquadraCasa THEN 1 WHEN mp.TipoGol = 'Autogol' AND g.IDSquadraAttuale = p.IDSquadraOspite THEN 1 ELSE 0 END) AS GolCasaEffettivi,
        SUM(CASE WHEN mp.TipoGol <> 'Autogol' AND g.IDSquadraAttuale = p.IDSquadraOspite THEN 1 WHEN mp.TipoGol = 'Autogol' AND g.IDSquadraAttuale = p.IDSquadraCasa THEN 1 ELSE 0 END) AS GolOspiteEffettivi
    FROM Partite p
    LEFT JOIN MarcatoriPartita mp ON p.IDPartita = mp.IDPartita
    LEFT JOIN Giocatori g ON mp.IDGiocatore = g.IDGiocatore
    WHERE p.Disputata = TRUE
    GROUP BY p.IDPartita, p.IDStagione, p.IDSquadraCasa, p.IDSquadraOspite
),
StatistichePartite AS (
    SELECT gc.IDStagione, gc.IDSquadraCasa AS IDSquadra, COUNT(gc.IDPartita) AS PartiteGiocate, SUM(CASE WHEN gc.GolCasaEffettivi > gc.GolOspiteEffettivi THEN 1 ELSE 0 END) AS Vittorie, SUM(CASE WHEN gc.GolCasaEffettivi = gc.GolOspiteEffettivi THEN 1 ELSE 0 END) AS Pareggi, SUM(CASE WHEN gc.GolCasaEffettivi < gc.GolOspiteEffettivi THEN 1 ELSE 0 END) AS Sconfitte, SUM(gc.GolCasaEffettivi) AS GolFatti, SUM(gc.GolOspiteEffettivi) AS GolSubiti FROM GolCalcolati gc GROUP BY gc.IDStagione, gc.IDSquadraCasa
    UNION ALL
    SELECT gc.IDStagione, gc.IDSquadraOspite AS IDSquadra, COUNT(gc.IDPartita) AS PartiteGiocate, SUM(CASE WHEN gc.GolOspiteEffettivi > gc.GolCasaEffettivi THEN 1 ELSE 0 END) AS Vittorie, SUM(CASE WHEN gc.GolOspiteEffettivi = gc.GolCasaEffettivi THEN 1 ELSE 0 END) AS Pareggi, SUM(CASE WHEN gc.GolOspiteEffettivi < gc.GolCasaEffettivi THEN 1 ELSE 0 END) AS Sconfitte, SUM(gc.GolOspiteEffettivi) AS GolFatti, SUM(gc.GolCasaEffettivi) AS GolSubiti FROM GolCalcolati gc GROUP BY gc.IDStagione, gc.IDSquadraOspite
)
SELECT
    SPS.IDStagione, ST.Descrizione AS DescrizioneStagione, SQ.NomeSquadra,
    COALESCE(SUM(SP.PartiteGiocate), 0) AS PartiteGiocate,
    COALESCE(SUM(SP.Vittorie), 0) AS Vittorie,
    COALESCE(SUM(SP.Pareggi), 0) AS Pareggi,
    COALESCE(SUM(SP.Sconfitte), 0) AS Sconfitte,
    (COALESCE(SUM(SP.Vittorie), 0) * 3) + COALESCE(SUM(SP.Pareggi), 0) AS Punti,
    COALESCE(SUM(SP.GolFatti), 0) AS GolFatti,
    COALESCE(SUM(SP.GolSubiti), 0) AS GolSubiti,
    (COALESCE(SUM(SP.GolFatti), 0) - COALESCE(SUM(SP.GolSubiti), 0)) AS DifferenzaReti
FROM SquadrePerStagione SPS
JOIN Squadre SQ ON SPS.IDSquadra = SQ.IDSquadra
JOIN Stagioni ST ON SPS.IDStagione = ST.IDStagione
LEFT JOIN StatistichePartite SP ON SPS.IDSquadra = SP.IDSquadra AND SPS.IDStagione = SP.IDStagione
GROUP BY SPS.IDStagione, ST.Descrizione, SQ.IDSquadra, SQ.NomeSquadra
ORDER BY SPS.IDStagione, Punti DESC, DifferenzaReti DESC, GolFatti DESC, SQ.NomeSquadra ASC;



-- ############################################
-- # 6. Stored Procedure/Function             #
-- ############################################
DELIMITER //

CREATE PROCEDURE MarcaPartitaComeDisputata(
    IN p_IDPartita INT
)
BEGIN
    DECLARE v_partita_disputata BOOLEAN;
    DECLARE v_ErrorMessage VARCHAR(255);
    SELECT Disputata INTO v_partita_disputata FROM Partite WHERE IDPartita = p_IDPartita;
    IF v_partita_disputata IS NULL THEN
        SET v_ErrorMessage = CONCAT('Errore: Partita con ID ', p_IDPartita, ' non trovata.');
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = v_ErrorMessage;
    ELSEIF v_partita_disputata = TRUE THEN
        SET v_ErrorMessage = CONCAT('Errore: Partita con ID ', p_IDPartita, ' è già stata disputata.');
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = v_ErrorMessage;
    ELSE
        UPDATE Partite SET Disputata = TRUE WHERE IDPartita = p_IDPartita;
        SELECT CONCAT('Partita ID: ', p_IDPartita, ' marcata come disputata. Inserire i marcatori per definire il risultato.') AS Messaggio;
    END IF;
END;
//

CREATE FUNCTION GetAllenatoreSquadraStagione(f_IDSquadra INT, f_IDStagione INT)
RETURNS VARCHAR(101) DETERMINISTIC READS SQL DATA
BEGIN
    DECLARE v_NomeAllenatore VARCHAR(101) DEFAULT 'N/D';
    SELECT CONCAT(A.Nome, ' ', A.Cognome) INTO v_NomeAllenatore
    FROM SquadrePerStagione SPS JOIN Allenatori A ON SPS.IDAllenatore = A.IDAllenatore
    WHERE SPS.IDSquadra = f_IDSquadra AND SPS.IDStagione = f_IDStagione;
    RETURN v_NomeAllenatore;
END;
//

CREATE PROCEDURE MostraClassificaStagione(IN p_IDStagione INT)
BEGIN
    SELECT NomeSquadra, PartiteGiocate, Vittorie, Pareggi, Sconfitte, Punti, GolFatti, GolSubiti, DifferenzaReti
    FROM VistaClassificaStagione
    WHERE IDStagione = p_IDStagione;
END;
//

DELIMITER ;


-- #######################################################
-- # 7. Esempi di Utilizzo di Viste e Procedure (NUOVO)   #
-- #######################################################

-- Esempio 1: Visualizzare il calendario completo, inclusi i risultati calcolati per le partite disputate
SELECT * FROM VistaCalendarioCompleto;

-- Esempio 2: Visualizzare solo le partite disputate della stagione 2023/2024 con il loro risultato finale
SELECT NomeSquadraCasa, GolCasaEffettivi, GolOspiteEffettivi, NomeSquadraOspite
FROM VistaCalendarioCompleto
WHERE DescrizioneStagione = 'Stagione 2023/2024' AND Disputata = TRUE;

-- Esempio 3: Usare la vista della classifica per una stagione specifica (Stagione 1)
SELECT * FROM VistaClassificaStagione WHERE IDStagione = @stagione1ID;

-- Esempio 4: Usare la stored procedure per mostrare la classifica (metodo più pulito per l'utente finale)
CALL MostraClassificaStagione(@stagione1ID);
CALL MostraClassificaStagione(@stagione2ID);

-- Esempio 5: Usare la funzione per ottenere l'allenatore di una squadra in una determinata stagione
SELECT GetAllenatoreSquadraStagione(@milanID, @stagione2ID) AS AllenatoreMilan2024_2025;

-- Esempio 6: Simulare la fine di una partita e visualizzare l'impatto sulla classifica
-- a) Trova una partita non disputata
SET @partitaDaDisputare = (SELECT IDPartita FROM Partite WHERE Disputata = FALSE LIMIT 1);
SELECT * FROM VistaCalendarioCompleto WHERE Disputata = FALSE;

-- b) Marcarla come disputata
CALL MarcaPartitaComeDisputata(@partitaDaDisputare);

-- c) Inserire i marcatori (es. Juve - Roma finisce 2-0)
SET @locatelliID = (SELECT IDGiocatore FROM Giocatori WHERE Cognome = 'Locatelli');
SET @vlahovicID = (SELECT IDGiocatore FROM Giocatori WHERE Cognome = 'Vlahović');
INSERT INTO MarcatoriPartita(IDPartita, IDGiocatore, MinutoGol) VALUES
(@partitaDaDisputare, @locatelliID, 30),
(@partitaDaDisputare, @vlahovicID, 75);

-- d) Rivedere la classifica della stagione 2 per vedere l'impatto della partita appena registrata
CALL MostraClassificaStagione(@stagione2ID);

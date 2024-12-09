-- Create the database
CREATE DATABASE IF NOT EXISTS travel_agency;

-- Use the newly created database
USE travel_agency;

-- Create viaggi table
CREATE TABLE IF NOT EXISTS viaggi (
    Id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    Descrizione TEXT,
    Destinazione VARCHAR(100)
);

-- Create agenzie table
CREATE TABLE IF NOT EXISTS agenzie (
    Id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(100),
    Indirizzo VARCHAR(255)
);

-- Create clienti table
CREATE TABLE IF NOT EXISTS clienti (
    Id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(50),
    Cognome VARCHAR(50),
    Email VARCHAR(100)
);

-- Create offerte table
CREATE TABLE IF NOT EXISTS offerte (
    Id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    fkViaggio INT UNSIGNED,
    fkAgenzia INT UNSIGNED,
    DataScadenza DATE,
    Prezzo DECIMAL(10, 2),
    FOREIGN KEY (fkViaggio) REFERENCES viaggi (Id),
    FOREIGN KEY (fkAgenzia) REFERENCES agenzie (Id)
);

-- Create prenotazioni table
CREATE TABLE IF NOT EXISTS prenotazioni (
    Id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    fkOfferta INT UNSIGNED,
    fkCliente INT UNSIGNED,
    DataPrenotazione DATE,
    FOREIGN KEY (fkOfferta) REFERENCES offerte (Id),
    FOREIGN KEY (fkCliente) REFERENCES clienti (Id)
);

USE travel_agency;
-- Inserimento dati nella tabella viaggi
INSERT INTO viaggi (Descrizione, Destinazione) VALUES
('Tour delle città d''arte', 'Italia'),
('Vacanza al mare', 'Spagna'),
('Avventura nella giungla', 'Costa Rica'),
('Crociera nei Caraibi', 'Caraibi'),
('Safari fotografico', 'Kenya'),
('Tour dei castelli', 'Scozia'),
('Viaggio culturale', 'Giappone'),
('Escursione in montagna', 'Svizzera'),
('Giro delle isole', 'Grecia'),
('Viaggio gastronomico', 'Francia'),
('Vacanza relax', 'Maldive'),
('Tour archeologico', 'Egitto'),
('Avventura in Patagonia', 'Argentina'),
('Esplorazione dei fiordi', 'Norvegia'),
('Tour delle capitali baltiche', 'Baltico');

-- Inserimento dati nella tabella agenzie
INSERT INTO agenzie (Nome, Indirizzo) VALUES
('Viaggi Fantastici', 'Via Roma 123, Milano'),
('Avventure Senza Confini', 'Corso Italia 456, Roma'),
('Sogni in Valigia', 'Piazza Maggiore 789, Bologna'),
('Orizzonti Lontani', 'Via Garibaldi 101, Firenze'),
('Viaggi nel Tempo', 'Corso Vittorio Emanuele 202, Torino');

INSERT INTO clienti (Nome, Cognome, Email) VALUES
('Mario', 'Rossi', 'mario.rossi@email.com'),
('Anna', 'Verdi', 'anna.verdi@email.com'),
('Giuseppe', 'Bianchi', 'giuseppe.bianchi@email.com'),
('Francesca', 'Neri', 'francesca.neri@email.com'),
('Alessandro', 'Gialli', 'alessandro.gialli@email.com'),
('Laura', 'Blu', 'laura.blu@email.com'),
('Roberto', 'Viola', 'roberto.viola@email.com');

-- Inserimento dati nella tabella offerte
INSERT INTO offerte (fkViaggio, fkAgenzia, DataScadenza, Prezzo) VALUES
(1, 1, '2024-12-31', 1500.00),
(2, 2, '2024-11-30', 1200.00),
(3, 3, '2024-10-31', 2000.00),
(4, 4, '2024-09-30', 2500.00),
(5, 5, '2024-08-31', 3000.00),
(6, 1, '2024-07-31', 1800.00),
(7, 2, '2024-06-30', 2200.00),
(8, 3, '2024-05-31', 1600.00),
(9, 4, '2024-04-30', 1900.00),
(10, 5, '2024-03-31', 2100.00);

-- Inserimento dati nella tabella prenotazioni
INSERT INTO prenotazioni (fkOfferta, fkCliente, DataPrenotazione) VALUES
(1, 1, '2024-01-15'), (2, 2, '2024-01-20'), (3, 3, '2024-02-01'),
(4, 4, '2024-02-10'), (5, 5, '2024-02-15'), (6, 6, '2024-02-20'),
(7, 7, '2024-03-01'), (8, 1, '2024-03-05'), (9, 2, '2024-03-10'),
(10, 3, '2024-03-15'), (1, 4, '2024-03-20'), (2, 5, '2024-04-01'),
(3, 6, '2024-04-05'), (4, 7, '2024-04-10'), (5, 1, '2024-04-15'),
(6, 2, '2024-04-20'), (7, 3, '2024-05-01'), (8, 4, '2024-05-05'),
(9, 5, '2024-05-10'), (10, 6, '2024-05-15');

-- Ricercare le prenotazioni fatte dai clienti con data antecedente alla scadenza dell’offerta
SELECT 
    c.Nome, 
    c.Cognome, 
    v.Destinazione, 
    p.DataPrenotazione, 
    o.DataScadenza,
    a.Nome AS NomeAgenzia
FROM 
    prenotazioni p 
JOIN 
    offerte o ON p.fkOfferta = o.Id
JOIN 
    clienti c ON p.fkCliente = c.Id
JOIN 
    viaggi v ON o.fkViaggio = v.Id
JOIN 
    agenzie a ON o.fkAgenzia = a.Id
WHERE 
    p.DataPrenotazione < o.DataScadenza
ORDER BY 
    p.DataPrenotazione;

-- trova i viaggi che hanno prenotazioni (senza riportare il numero di prenotazioni)
SELECT DISTINCT
    v.Id AS IdViaggio,
    v.Descrizione,
    v.Destinazione
FROM
    viaggi v
    JOIN offerte o ON v.Id = o.fkViaggio
    JOIN prenotazioni p ON o.Id = p.fkOfferta;

-- trova i viaggi che hanno prenotazioni (riportare anche il numero di prenotazioni)
-- prima versione
SELECT DISTINCT
    v.Id AS IdViaggio,
    v.Descrizione,
    v.Destinazione,
    (
        SELECT COUNT(*)
        FROM prenotazioni p
            JOIN offerte o ON p.fkOfferta = o.Id
        WHERE
            o.fkViaggio = v.Id
    ) AS NumeroPrenotazioni
FROM
    viaggi v
    JOIN offerte o ON v.Id = o.fkViaggio
    JOIN prenotazioni p ON o.Id = p.fkOfferta
ORDER BY NumeroPrenotazioni DESC;

-- seconda versione
SELECT DISTINCT
    v.Id AS IdViaggio,
    v.Descrizione,
    v.Destinazione,
    COUNT(p.Id) AS NumeroPrenotazioni
FROM
    viaggi v
    JOIN offerte o ON v.Id = o.fkViaggio
    JOIN prenotazioni p ON o.Id = p.fkOfferta
GROUP BY
    v.Id,
    v.Descrizione,
    v.Destinazione
ORDER BY NumeroPrenotazioni DESC;
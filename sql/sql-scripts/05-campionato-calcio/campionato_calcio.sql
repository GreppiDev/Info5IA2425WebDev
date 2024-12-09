-- Create the database
CREATE DATABASE IF NOT EXISTS campionato_calcio;

-- Use the newly created database
USE campionato_calcio;

-- Creazione della tabella giocatori
CREATE TABLE IF NOT EXISTS giocatori (
    codice INT PRIMARY KEY,
    cognome VARCHAR(50),
    nome VARCHAR(50),
    dataNascita DATE
);

-- Creazione della tabella squadre
CREATE TABLE IF NOT EXISTS squadre (
    nome VARCHAR(50) PRIMARY KEY,
    citta VARCHAR(50),
    stadio VARCHAR(50)
);

-- Creazione della tabella formazioni
CREATE TABLE IF NOT EXISTS formazioni (
    giocatore INT,
    squadra VARCHAR(50),
    anno INT,
    contratto VARCHAR(50),
    PRIMARY KEY (giocatore, squadra, anno),
    FOREIGN KEY (giocatore) REFERENCES giocatori (codice),
    FOREIGN KEY (squadra) REFERENCES squadre (nome)
);

-- Creazione della tabella partite
CREATE TABLE IF NOT EXISTS partite (
    squadraCasa VARCHAR(50),
    squadraTrasferta VARCHAR(50),
    data DATE,
    puntiCasa INT,
    puntiTrasferta INT,
    PRIMARY KEY (squadraCasa,squadraTrasferta,data),
    FOREIGN KEY (squadraCasa) REFERENCES squadre (nome),
    FOREIGN KEY (squadraTrasferta) REFERENCES squadre (nome)
);

-- Inserimento dei giocatori
INSERT INTO giocatori (codice, cognome, nome, dataNascita) VALUES
(1, 'Rossi', 'Luca', '1990-01-15'),
(2, 'Bianchi', 'Marco', '1992-05-23'),
(3, 'Verdi', 'Giovanni', '1988-09-12'),
(4, 'Gialli', 'Stefano', '1994-11-30'),
(5, 'Neri', 'Paolo', '1991-03-04'),
(6, 'Marini', 'Giorgio', '1993-07-19'),
(7, 'Esposito', 'Francesco', '1989-08-05'),
(8, 'Bruni', 'Matteo', '1995-02-17'),
(9, 'Fontana', 'Alessandro', '1990-10-20'),
(10, 'Ferrari', 'Davide', '1996-12-25'),
(11, 'Ricci', 'Andrea', '1991-04-15'),
(12, 'Sartori', 'Emanuele', '1992-06-28'),
(13, 'Colombo', 'Lorenzo', '1988-07-01'),
(14, 'Gatti', 'Simone', '1994-03-21'),
(15, 'Martini', 'Fabio', '1990-11-10'),
(16, 'Costa', 'Roberto', '1992-01-02'),
(17, 'Bianchi', 'Luca', '1987-09-15'),
(18, 'Moretti', 'Michele', '1993-05-10'),
(19, 'Amato', 'Luca', '1995-10-01'),
(20, 'De Luca', 'Antonio', '1990-07-14');

-- Inserimento delle squadre
INSERT INTO squadre (nome, citta, stadio) VALUES
('ASD Milano', 'Milano', 'Stadio San Siro'),
('FC Torino', 'Torino', 'Stadio Olimpico'),
('AS Roma', 'Roma', 'Stadio Olimpico di Roma'),
('SS Napoli', 'Napoli', 'Stadio San Paolo'),
('US Palermo', 'Palermo', 'Stadio Renzo Barbera');

-- Inserimento delle formazioni (relazioni tra giocatori e squadre)
INSERT INTO formazioni (giocatore, squadra, anno, contratto) VALUES
(1, 'ASD Milano', 2024, 'Contratto A1'),
(2, 'ASD Milano', 2024, 'Contratto A2'),
(3, 'ASD Milano', 2024, 'Contratto A3'),
(4, 'ASD Milano', 2024, 'Contratto A4'),
(5, 'FC Torino', 2024, 'Contratto B1'),
(6, 'FC Torino', 2024, 'Contratto B2'),
(7, 'FC Torino', 2024, 'Contratto B3'),
(8, 'AS Roma', 2024, 'Contratto C1'),
(9, 'AS Roma', 2024, 'Contratto C2'),
(10, 'AS Roma', 2024, 'Contratto C3'),
(11, 'SS Napoli', 2024, 'Contratto D1'),
(12, 'SS Napoli', 2024, 'Contratto D2'),
(13, 'SS Napoli', 2024, 'Contratto D3'),
(14, 'US Palermo', 2024, 'Contratto E1'),
(15, 'US Palermo', 2024, 'Contratto E2'),
(16, 'US Palermo', 2024, 'Contratto E3'),
(17, 'ASD Milano', 2024, 'Contratto A5'),
(18, 'ASD Milano', 2024, 'Contratto A6'),
(19, 'FC Torino', 2024, 'Contratto B4'),
(20, 'FC Torino', 2024, 'Contratto B5');

-- Inserimento delle partite (alcune partite del campionato)
INSERT INTO partite (squadraCasa, squadraTrasferta, data, puntiCasa, puntiTrasferta) VALUES
('ASD Milano', 'FC Torino', '2024-09-01', 2, 1),
('AS Roma', 'SS Napoli', '2024-09-02', 1, 3),
('US Palermo', 'ASD Milano', '2024-09-03', 0, 1),
('FC Torino', 'SS Napoli', '2024-09-04', 1, 1),
('ASD Milano', 'AS Roma', '2024-09-05', 3, 2),
('SS Napoli', 'US Palermo', '2024-09-06', 2, 0),
('AS Roma', 'FC Torino', '2024-09-07', 0, 1),
('US Palermo', 'AS Roma', '2024-09-08', 1, 1),
('FC Torino', 'ASD Milano', '2024-09-09', 0, 2),
('SS Napoli', 'ASD Milano', '2024-09-10', 1, 2);


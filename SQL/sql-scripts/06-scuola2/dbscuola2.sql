CREATE DATABASE IF NOT EXISTS dbscuola2;
USE dbscuola2;
-- creazione delle tabelle;
-- il Codice è del tipo 3IA, 4IA, 2IB, 5IB, 3SC, etc.
-- l'aula è il codice dell'aula nell'istituto, quindi, ad esempio, A33, A21, 'Est. 1'
CREATE TABLE IF NOT EXISTS classi (
    Codice VARCHAR(4) PRIMARY KEY,
    Aula VARCHAR(10) NOT NULL
);

CREATE TABLE IF NOT EXISTS studenti (
    Matricola MEDIUMINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    Cognome varchar(30) NOT NULL,
    Nome varchar(30) NOT NULL,
    DataNascita date NOT NULL,
    Genere enum('M', 'F') NOT NULL,
    Nazione varchar(30) NOT NULL default 'Italia',
    EMail varchar(50),
    Classe VARCHAR(4),
    FOREIGN KEY (Classe) REFERENCES classi (Codice) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE = InnoDB;

CREATE TABLE IF NOT EXISTS assenze (
    Id MEDIUMINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    Studente MEDIUMINT UNSIGNED NOT NULL,
    Tipo ENUM('AA', 'AG', 'RR', 'RG') DEFAULT 'AA',
    Data DATE NOT NULL,
    FOREIGN KEY (Studente) REFERENCES studenti (Matricola)
) ENGINE = InnoDB;




-- inserimento valori
INSERT INTO classi (Codice, Aula)
VALUES
('3IA', 'A33'),
('4IA', 'A21'),
('2IB', 'A11'),
('5IB', 'A12'),
('3SC', 'Est. 1');

INSERT INTO studenti (Cognome, Nome, DataNascita, Genere, Nazione, EMail, Classe)
VALUES
('Rossi', 'Mario', '2006-05-15', 'M', 'Italia', 'm.rossi@example.com', '3IA'),
('Bianchi', 'Luca', '2005-07-20', 'M', 'Italia', 'l.bianchi@example.com', '4IA'),
('Verdi', 'Anna', '2006-01-22', 'F', 'Italia', 'a.verdi@example.com', '3SC'),
('Neri', 'Marco', '2006-09-30', 'M', 'Italia', 'm.neri@example.com', '2IB'),
('Esposito', 'Giulia', '2005-12-12', 'F', 'Italia', 'g.esposito@example.com', '5IB');

INSERT INTO assenze (Studente, Tipo, Data)
VALUES
-- Assenze per Mario Rossi (3IA)
(1, 'AA', '2023-10-01'),
(1, 'AG', '2023-10-15'),
(1, 'RG', '2023-11-05'),
(1, 'RR', '2023-11-20'),
-- Assenze per Luca Bianchi (4IA)
(2, 'AA', '2023-09-10'),
(2, 'AG', '2023-09-22'),
(2, 'RG', '2023-10-10'),
-- Assenze per Anna Verdi (3SC)
(3, 'AG', '2023-09-13'),
(3, 'RG', '2023-10-18'),
(3, 'AA', '2023-12-02'),
-- Assenze per Marco Neri (2IB)
(4, 'AA', '2023-11-01'),
(4, 'AG', '2023-11-15'),
(4, 'RG', '2023-12-01'),
-- Assenze per Giulia Esposito (5IB)
(5, 'AG', '2023-09-09'),
(5, 'AA', '2023-09-20'),
(5, 'RR', '2023-10-12');


-- aggiungiamo altre assenze
INSERT INTO assenze (Studente, Tipo, Data) VALUES
-- altre assenze per per Mario Rossi (3IA)
(1, 'AA', '2023-11-01'),
(1, 'AA', '2024-02-13');
-- sezione query 
-- Query Q1: Stampare il numero totale di assenze fatte in tutto l'anno scolastico da uno studente (di cui sono noti nome e cognome) di una data classe, classificate per tipo.
SELECT Tipo, COUNT(*) AS TotaleAssenze
FROM assenze a
JOIN studenti s ON a.Studente = s.Matricola
WHERE s.Nome = 'Mario' AND s.Cognome = 'Rossi' AND s.Classe = '3IA'
GROUP BY Tipo;

-- Query Q2: Visualizzare, al termine di un anno scolastico, il numero totale di assenze non giustificate effettuate nelle classi della scuola; 
-- le classi devono essere presentate in ordine alfabetico

SELECT s.Classe, COUNT(*) AS TotaleAssenzeNonGiustificate
FROM assenze a
JOIN studenti s ON a.Studente = s.Matricola
WHERE a.Tipo = 'AA'
GROUP BY s.Classe
ORDER BY s.Classe ASC;

-- Query Q3: Stampare l’andamento delle assenze e dei ritardi non giustificati (classificati per mese) di uno studente di una classe.
SELECT 
    MONTH(a.Data) AS Mese, 
    a.Tipo, 
    COUNT(*) AS Totale
FROM assenze a
JOIN studenti s ON a.Studente = s.Matricola
WHERE s.Nome = 'Mario' AND s.Cognome = 'Rossi' AND s.Classe = '3IA'
AND a.Tipo IN ('AA', 'RR')
GROUP BY Mese, a.Tipo
ORDER BY Mese;



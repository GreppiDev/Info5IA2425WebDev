
/* 
Esercitazione di SQL
Dato lo schema relazionale
SCHEMA RELAZIONALE:
musei (NomeM, Citta)
artisti (NomeA, Nazionalita)
opere (Codice, Titolo, NomeM*, NomeA*)
personaggi (Personaggio, Codice*)
Supponendo di usare il DBMS MariaDb su container Docker, scriver le istruzioni in SQL le istruzioni per:
1.	creare il database museo
2.	creare le tabelle, assumendo che:
NomeM, NomeA, Citta, Nazionalita, Titolo, Personaggio siano di tipo VARCHAR
Codice sia di tipo intero ad auto-incremento non negativo
I campi sottolineati sono chiavi primarie, mentre i campi con * sono chiavi esterne
Nel creare i vincoli di integrità referenziale, impostare la chiave esterna NomeA in opere in modo che, se si cancella l’artista riferito la chiave in opere sia messa a null. Se si modifica il NomeA in Artisti deve modificarsi automaticamente anche la foreign key in opere.
3.	Inserire almeno 5 musei, 10 artisti, 10 opere e 10 personaggi
4.	Modificare la nazionalità di un artista
5.	Creare un account per il database museo che abbia i permessi per effettuare la SELECT, INSERT, DELETE, UPDATE quando si connette da qualunque indirizzo IP.
6.	Effettuare le seguenti query:
1- Il codice ed il titolo delle opere di Tiziano conservate alla 'National Gallery'.
2- Il nome dell'artista ed il titolo delle opere conservate alla 'Galleria degli Uffizi' o alla
'National Gallery'.
3- Le opere dello stesso artista della 'Gioconda' */

-- 1. Creare il database museo
CREATE DATABASE museo;

USE museo;

-- 2. Creare le tabelle
CREATE TABLE musei (
    NomeM VARCHAR(255) PRIMARY KEY,
    Citta VARCHAR(255)
);

CREATE TABLE artisti (
    NomeA VARCHAR(255) PRIMARY KEY,
    Nazionalita VARCHAR(255)
);

CREATE TABLE opere (
    Codice INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    Titolo VARCHAR(255),
    NomeM VARCHAR(255),
    NomeA VARCHAR(255),
    FOREIGN KEY (NomeM) REFERENCES musei (NomeM),
    FOREIGN KEY (NomeA) REFERENCES artisti (NomeA) ON DELETE SET NULL ON UPDATE CASCADE
);

CREATE TABLE personaggi (
    Personaggio VARCHAR(255),
    Codice INT UNSIGNED NOT NULL,
    PRIMARY KEY (Personaggio, Codice),
    FOREIGN KEY (Codice) REFERENCES opere (Codice)
);

-- 3. Inserire dati di esempio
-- Musei
INSERT INTO musei (NomeM, Citta) VALUES
('Louvre', 'Parigi'),
('National Gallery', 'Londra'),
('Galleria degli Uffizi', 'Firenze'),
('Museo del Prado', 'Madrid'),
('Metropolitan Museum of Art', 'New York'),
('Musei Vaticani', 'Roma'),
('Museo Reina Sofia', 'Madrid'),
('Rijksmuseum', 'Amsterdam'),
('Musée de l\'Orangerie', 'Parigi'),
('Galleria Nazionale', 'Roma'),
('Museo Czartoryski', 'Cracovia');


-- Artisti
INSERT INTO artisti (NomeA, Nazionalita) VALUES
('Leonardo da Vinci', 'Italiana'),
('Michelangelo', 'Italiana'),
('Raffaello', 'Italiana'),
('Tiziano', 'Italiana'),
('Rembrandt', 'Olandese'),
('Vincent van Gogh', 'Olandese'),
('Pablo Picasso', 'Spagnola'),
('Claude Monet', 'Francese'),
('Edvard Munch', 'Norvegese'),
('Georgia O\'Keeffe', 'Americana');

-- Opere
INSERT INTO opere (Titolo, NomeM, NomeA) VALUES
('Gioconda', 'Louvre', 'Leonardo da Vinci'),
('La Nascita di Venere', 'Galleria degli Uffizi', 'Michelangelo'),
('La Scuola di Atene', 'Musei Vaticani', 'Raffaello'),
('Venere di Urbino', 'Galleria degli Uffizi', 'Tiziano'),
('La Ronda di Notte', 'Rijksmuseum', 'Rembrandt'),
('Notte Stellata', 'Metropolitan Museum of Art', 'Vincent van Gogh'),
('Guernica', 'Museo Reina Sofia', 'Pablo Picasso'),
('Le Ninfee', 'Musée de l\'Orangerie', 'Claude Monet'),
('L\'Urlo', 'Galleria Nazionale', 'Edvard Munch'),
('Cielo sopra le nuvole IV', 'Metropolitan Museum of Art', 'Georgia O\'Keeffe'),
('Dama con l\'ermellino', 'Museo Czartoryski', 'Leonardo da Vinci');

-- Personaggi
INSERT INTO personaggi (Personaggio, Codice) VALUES
('Monna Lisa', 1),
('Venere', 2),
('Platone', 3),
('Aristotele', 3),
('Cupido', 4),
('Capitano Frans Banninck Cocq', 5),
('Stelle', 6),
('Toro', 7),
('Ninfee', 8),
('Figura urlante', 9),
('Dama con l\'ermellino',11);

-- 4. Modificare la nazionalità di un artista
UPDATE artisti SET Nazionalita = 'Spagnola' WHERE NomeA = 'Pablo Picasso';

-- 5. Creare un account con permessi
CREATE USER 'museo_user'@'%' IDENTIFIED BY 'password_sicura';
GRANT SELECT, INSERT, DELETE, UPDATE ON museo.* TO 'museo_user'@'%';
FLUSH PRIVILEGES;

-- 6. Query richieste
-- 1- Il codice ed il titolo delle opere di Tiziano conservate alla 'Galleria degli Uffizi'.
SELECT Codice, Titolo
FROM opere
WHERE NomeA = 'Tiziano' AND NomeM = 'Galleria degli Uffizi';

-- 2- Il nome dell'artista ed il titolo delle opere conservate alla 'Galleria degli Uffizi' o alla 'National Gallery'.
SELECT o.NomeA, o.Titolo
FROM opere o
WHERE o.NomeM IN ('Galleria degli Uffizi', 'National Gallery');

-- 3- Le opere dello stesso artista della 'Gioconda'
SELECT Titolo
FROM opere
WHERE NomeA = (SELECT NomeA FROM opere WHERE Titolo = 'Gioconda');
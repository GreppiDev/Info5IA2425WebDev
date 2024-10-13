CREATE DATABASE IF NOT EXISTS musei;
USE musei;

-- creazione delle tabelle
/* 
MUSEI (NomeM, Città)
ARTISTI (NomeA, Nazionalità)
OPERE (Codice, Titolo, NomeM*, NomeA*)
PERSONAGGI (Personaggio, Codice*) 
*/

-- bisogna creare prima le tabelle che non si riferiscono ad altre tabelle
CREATE TABLE IF NOT EXISTS musei(
    NomeM VARCHAR(120) PRIMARY KEY,
    Citta VARCHAR(120) NOT NULL
) ENGINE = InnoDB;

CREATE TABLE IF NOT EXISTS artisti(
    NomeA VARCHAR(120) PRIMARY KEY,
    Nazionalita VARCHAR(120) NOT NULL
)ENGINE = InnoDB;

CREATE TABLE IF NOT EXISTS opere(
    Codice INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    Titolo VARCHAR(120) NOT NULL,
    NomeM
)

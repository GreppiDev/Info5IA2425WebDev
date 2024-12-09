
/* 
supponendo di avere il seguente modello relazionale:
classi(Codice, Aula)
studenti(Codice,Cognome, Nome, DataNascita, Genere,EMail, Classe)
stages(Id, Azienda, Studente, AnnoScolastico, DataInizio, DurataComplessiva)
aziende(Codice, Denominazione, Indirizzo, Sede, Telefono, EMail) 
Si supponga che:
classi.Codice sia una stringa che rappresenta la classi frequentata, ad esempio '1IA', '3IB', '4SC', etc.
classi.Aula sia una stringa corrispondente alla collocazione nell'istituto, ad esempio 'A02', 'A31', 'Est.1', etc.
studenti.Codice sia un alfanumerico a lunghezza fissa di 8 caratteri che identifica ogni studente
studenti.Genere sia un enumerativo che può assumere i valori 'M', 'F'
studenti.Classe è la chiave esterna che punta al codice della classi
stages.Id è un intero non negativo ad auto incremento chiave priamria
stages.Azienda è la chiave esterna che punta a aziende.Codice
stages.Studente è la chiave esterna che punta a studenti.Codice
stages.DataInizio è la data di inizio dello stage
stages.DurataComplessiva è la durata in ore dello stage
aziende.Codice è un codice che corrisponde alla partita iva dell'azienda oppure al codice fiscale del rappresentante legale, nel caso di azienda sprovvista di partita iva
aziende.Denominazione è la ragione sociale dell'azienda
aziende.Indirizzo è l'indirizzo completo della sede legale dell'azienda (si supponga in Italia)
azienda.Sede è l'indirizzo della sede dell'azienda dove è stato svolto lo stage
azienda.Telefono e azienda.EMail sono rispettivamente il numero di telefono e la e-mail del tutor aziendale

Scrivere nell'SQL di MariaDB il codice per creare il database e le tabelle.
Popolare il database con almeno 5 classi, 10 studenti, 20 stages, 15 aziende

*/
CREATE DATABASE IF NOT EXISTS studenti_stages_aziende;
USE studenti_stages_aziende;

-- Creazione della tabella classi
CREATE TABLE IF NOT EXISTS classi (
    Codice VARCHAR(4) PRIMARY KEY,
    Aula VARCHAR(10) NOT NULL
);
show TABLES;
-- Creazione della tabella aziende
CREATE TABLE IF NOT EXISTS aziende (
    Codice VARCHAR(16) PRIMARY KEY, -- Partita IVA o CF
    Denominazione VARCHAR(100) NOT NULL,
    Indirizzo VARCHAR(255) NOT NULL,
    Sede VARCHAR(255) NOT NULL,
    Telefono VARCHAR(15),
    EMail VARCHAR(100)
);

-- Creazione della tabella studenti
CREATE TABLE IF NOT EXISTS studenti (
    Codice CHAR(8) PRIMARY KEY,
    Cognome VARCHAR(50) NOT NULL,
    Nome VARCHAR(50) NOT NULL,
    DataNascita DATE NOT NULL,
    Genere ENUM('M', 'F') NOT NULL,
    EMail VARCHAR(100) NOT NULL,
    Classe VARCHAR(4),
    FOREIGN KEY (Classe) REFERENCES classi (Codice) ON DELETE SET NULL ON UPDATE CASCADE
);

-- Creazione della tabella stages
CREATE TABLE IF NOT EXISTS stages (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Azienda VARCHAR(16),
    Studente CHAR(8),
    AnnoScolastico VARCHAR(9) NOT NULL,
    DataInizio DATE NOT NULL,
    DurataComplessiva INT NOT NULL,
    FOREIGN KEY (Azienda) REFERENCES aziende (Codice) ON DELETE SET NULL ON UPDATE CASCADE,
    FOREIGN KEY (Studente) REFERENCES studenti (Codice) ON DELETE CASCADE ON UPDATE CASCADE
);

-- dati per la tabella classi

INSERT INTO
    classi (Codice, Aula)
VALUES ('1IA', 'A01'),
    ('2IB', 'A02'),
    ('3SC', 'B10'),
    ('4SC', 'C21'),
    ('5IA', 'D12');

-- dati per la tabella aziende
INSERT INTO aziende (Codice, Denominazione, Indirizzo, Sede, Telefono, EMail) VALUES
('12345678901', 'Tech Solutions', 'Via Roma 10, Milano', 'Via Milano 15, Milano', '025678901', 'info@techsolutions.com'),
('98765432100', 'Eco Green', 'Via Verdi 22, Torino', 'Via Torino 12, Torino', '011234567', 'contact@ecogreen.com'),
('55566677788', 'Fast Logistics', 'Via Napoli 5, Napoli', 'Via Napoli 10, Napoli', '081987654', 'logistics@fast.com'),
('11223344556', 'EduSoft', 'Via Garibaldi 33, Firenze', 'Via Firenze 45, Firenze', '055678123', 'info@edusoft.com'),
('66778899001', 'BioFarm', 'Via Campagna 8, Parma', 'Via Emilia 20, Parma', '052198765', 'farm@biofarm.com'),
('11122233344', 'StartUp Inc.', 'Via Startup 1, Bologna', 'Via Innovazione 3, Bologna', '051456789', 'hello@startup.com'),
('33344455566', 'Future Energy', 'Via Sole 14, Roma', 'Via Roma 19, Roma', '066543210', 'info@futureenergy.com'),
('88899900011', 'Travel Co.', 'Via Mare 88, Genova', 'Via Porto 20, Genova', '010123678', 'support@travelco.com'),
('12332145665', 'SoundWave', 'Via Musica 7, Venezia', 'Via San Marco 9, Venezia', '041987321', 'info@soundwave.com'),
('23456789012', 'MediCare', 'Via Salute 40, Bari', 'Via Bari 50, Bari', '080654123', 'contact@medicare.com'),
('98765123458', 'Digital Dreams', 'Via Fantasia 23, Roma', 'Via Colosseo 11, Roma', '067890123', 'info@digitaldreams.com'),
('99988877766', 'Innovatech', 'Via Scienza 5, Pisa', 'Via Galileo 10, Pisa', '050789456', 'innovatech@info.com'),
('55443322110', 'Foodies', 'Via Cucina 30, Torino', 'Via Sapori 3, Torino', '011765432', 'info@foodies.com'),
('78945612300', 'AutoMaster', 'Via Motori 12, Modena', 'Via Ferrari 21, Modena', '059987654', 'service@automaster.com'),
('66778899912', 'OceanLife', 'Via Mare 55, Palermo', 'Via Porto 9, Palermo', '091765123', 'info@oceanlife.com');

-- dati per la tabella studenti
INSERT INTO studenti (Codice, Cognome, Nome, DataNascita, Genere, EMail, Classe) VALUES
('S1234567', 'Rossi', 'Mario', '2005-03-15', 'M', 'm.rossi@example.com', '1IA'),
('S2345678', 'Bianchi', 'Luca', '2006-07-20', 'M', 'l.bianchi@example.com', '2IB'),
('S3456789', 'Verdi', 'Sara', '2005-11-30', 'F', 's.verdi@example.com', '3SC'),
('S4567890', 'Neri', 'Anna', '2004-02-05', 'F', 'a.neri@example.com', '4SC'),
('S5678901', 'Gialli', 'Paolo', '2006-06-10', 'M', 'p.gialli@example.com', '5IA'),
('S6789012', 'Esposito', 'Chiara', '2005-09-25', 'F', 'c.esposito@example.com', '1IA'),
('S7890123', 'Russo', 'Giovanni', '2007-12-17', 'M', 'g.russo@example.com', '2IB'),
('S8901234', 'Colombo', 'Martina', '2006-04-12', 'F', 'm.colombo@example.com', '3SC'),
('S9012345', 'Ferrari', 'Luca', '2005-05-19', 'M', 'l.ferrari@example.com', '4SC'),
('S0123456', 'Fontana', 'Giulia', '2004-10-08', 'F', 'g.fontana@example.com', '5IA');

-- dati per la tabella stages
INSERT INTO stages (Azienda, Studente, AnnoScolastico, DataInizio, DurataComplessiva) VALUES
('12345678901', 'S1234567', '2023/2024', '2024-06-01', 80),
('98765432100', 'S2345678', '2023/2024', '2024-07-15', 100),
('55566677788', 'S3456789', '2022/2023', '2023-05-20', 90),
('11223344556', 'S4567890', '2022/2023', '2023-06-05', 120),
('66778899001', 'S5678901', '2023/2024', '2024-06-15', 60),
('11122233344', 'S6789012', '2023/2024', '2024-07-01', 70),
('33344455566', 'S7890123', '2023/2024', '2024-07-20', 80),
('88899900011', 'S8901234', '2022/2023', '2023-06-10', 50),
('12332145665', 'S9012345', '2023/2024', '2024-08-01', 100),
('23456789012', 'S0123456', '2022/2023', '2023-07-05', 110),
('98765123458', 'S1234567', '2023/2024', '2024-07-01', 60),
('99988877766', 'S2345678', '2023/2024', '2024-08-10', 90),
('55443322110', 'S3456789', '2022/2023', '2023-06-01', 50),
('78945612300', 'S4567890', '2023/2024', '2024-06-20', 80),
('66778899912', 'S5678901', '2022/2023', '2023-07-15', 100),
('12345678901', 'S6789012', '2023/2024', '2024-08-05', 75),
('98765432100', 'S7890123', '2023/2024', '2024-07-25', 60),
('55566677788', 'S8901234', '2022/2023', '2023-08-01', 90),
('11223344556', 'S9012345', '2023/2024', '2024-06-15', 80),
('66778899001', 'S0123456', '2022/2023', '2023-07-10', 85);

-- Ricercare i cognomi ed i nomi degli allievi a cui è stato assegnato uno stage in un certo anno scolastico
SELECT DISTINCT s.Codice, s.Cognome, s.Nome
FROM studenti s
JOIN stages st ON s.Codice = st.Studente
WHERE st.AnnoScolastico = '2023/2024';

-- Ricercare la denominazione delle aziende che hanno offerto stage in un certo periodo di tempo (tra una data iniziale ed una data finale
SELECT DISTINCT a.Denominazione
FROM aziende a
JOIN stages st ON a.Codice = st.Azienda
WHERE st.DataInizio BETWEEN '2024-06-01' AND '2024-08-31';

-- Ricercare codice, cognome e nome degli allievi a cui è stato assegnato uno stage in un certo anno scolastico
SELECT DISTINCT s.Codice, s.Cognome, s.Nome
FROM studenti s
JOIN stages st ON s.Codice = st.Studente
WHERE st.AnnoScolastico = '2023/2024';


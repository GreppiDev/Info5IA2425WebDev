-- ##############################################################################
-- # Esercitazione SQL: Gestione E-commerce con Pagamenti - Svolgimento #
-- ##############################################################################

-- ############################################
-- # 1. Creazione dello Schema Fisico (DDL)   #
-- ############################################

-- Creazione del database
CREATE DATABASE IF NOT EXISTS EcommerceDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE EcommerceDB;



-- Tabella: Clienti
CREATE TABLE IF NOT EXISTS Clienti (
    IDCliente INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(50) NOT NULL,
    Cognome VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    IndirizzoSpedizione VARCHAR(200),
    CittaSpedizione VARCHAR(50),
    CAPSpedizione VARCHAR(10),
    DataRegistrazione DATE DEFAULT (CURDATE())
);

-- Tabella: Categorie
CREATE TABLE IF NOT EXISTS Categorie (
    IDCategoria INT AUTO_INCREMENT PRIMARY KEY,
    NomeCategoria VARCHAR(100) NOT NULL UNIQUE,
    DescrizioneCategoria TEXT
);

-- Tabella: Prodotti
CREATE TABLE IF NOT EXISTS Prodotti (
    IDProdotto INT AUTO_INCREMENT PRIMARY KEY,
    NomeProdotto VARCHAR(150) NOT NULL,
    DescrizioneProdotto TEXT,
    PrezzoUnitario DECIMAL(10,2) NOT NULL,
    IDCategoria INT NOT NULL,
    Giacenza INT DEFAULT 0,
    DataInserimento DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT CHK_PrezzoPositivo CHECK (PrezzoUnitario > 0),
    CONSTRAINT CHK_GiacenzaNonNegativa CHECK (Giacenza >= 0),
    FOREIGN KEY (IDCategoria) REFERENCES Categorie(IDCategoria) ON DELETE RESTRICT ON UPDATE CASCADE
);

-- Tabella: Ordini
CREATE TABLE IF NOT EXISTS Ordini (
    IDOrdine INT AUTO_INCREMENT PRIMARY KEY,
    IDCliente INT NOT NULL,
    DataOrdine DATETIME DEFAULT CURRENT_TIMESTAMP,
    StatoOrdine ENUM('In Attesa di Pagamento', 'Pagamento Fallito', 'In Lavorazione', 'Spedito', 'Consegnato', 'Annullato') DEFAULT 'In Attesa di Pagamento',
    IndirizzoSpedizioneOrdine VARCHAR(200),
    CittaSpedizioneOrdine VARCHAR(50),
    CAPSpedizioneOrdine VARCHAR(10),
    TotaleOrdine DECIMAL(10,2) DEFAULT 0.00,
    FOREIGN KEY (IDCliente) REFERENCES Clienti(IDCliente) ON DELETE RESTRICT ON UPDATE CASCADE
);

-- Tabella: DettagliOrdine
CREATE TABLE IF NOT EXISTS DettagliOrdine (
    IDDettaglioOrdine INT AUTO_INCREMENT PRIMARY KEY,
    IDOrdine INT NOT NULL,
    IDProdotto INT NOT NULL,
    Quantita INT NOT NULL,
    PrezzoUnitarioAlMomentoOrdine DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (IDOrdine) REFERENCES Ordini(IDOrdine) ON DELETE CASCADE ON UPDATE CASCADE,
    FOREIGN KEY (IDProdotto) REFERENCES Prodotti(IDProdotto) ON DELETE RESTRICT ON UPDATE CASCADE,
    UNIQUE (IDOrdine, IDProdotto),
    CONSTRAINT CHK_QuantitaPositiva CHECK (Quantita > 0)
);

-- Tabella: Pagamenti
CREATE TABLE IF NOT EXISTS Pagamenti (
    IDPagamento INT AUTO_INCREMENT PRIMARY KEY,
    IDOrdine INT NOT NULL UNIQUE, -- Assumiamo un pagamento per ordine per semplicità
    DataOraPagamento DATETIME DEFAULT CURRENT_TIMESTAMP,
    ImportoPagato DECIMAL(10,2) NOT NULL,
    MetodoPagamento ENUM('CartaCredito', 'PayPal', 'Stripe', 'Bonifico') NOT NULL,
    IDTransazioneGateway VARCHAR(100) NULL UNIQUE,
    StatoPagamento ENUM('In Attesa', 'Completato', 'Fallito', 'Rimborsato', 'Annullato') NOT NULL DEFAULT 'In Attesa',
    DettagliRispostaGateway JSON NULL, -- Modificato da TEXT a JSON
    FOREIGN KEY (IDOrdine) REFERENCES Ordini(IDOrdine) ON DELETE CASCADE ON UPDATE CASCADE
);
-- NOTA: Il campo DettagliRispostaGateway è stato modificato in JSON.
-- Questo tipo di dato è più appropriato per memorizzare risposte strutturate da gateway di pagamento
-- e offre vantaggi in termini di validazione, query e manipolazione dei dati nelle versioni recenti di MariaDB (10.2+).

-- ####################################################################################
-- # DEFINIZIONE DEI TRIGGER                                                        #
-- ####################################################################################
DELIMITER //

-- Trigger esistente: AggiornaGiacenzaDopoInserimentoDettaglio
CREATE TRIGGER AggiornaGiacenzaDopoInserimentoDettaglio
AFTER INSERT ON DettagliOrdine
FOR EACH ROW
BEGIN
    DECLARE giacenzaAttuale INT;
    -- Seleziona la giacenza attuale. È importante che questo SELECT non causi problemi
    -- se la tabella Prodotti è già in uso. In questo caso specifico, l'INSERT INTO DettagliOrdine
    -- non dovrebbe più usare "SELECT FROM Prodotti", quindi questo trigger dovrebbe funzionare.
    SELECT Giacenza INTO giacenzaAttuale FROM Prodotti WHERE IDProdotto = NEW.IDProdotto;

    IF giacenzaAttuale < NEW.Quantita THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Errore: Giacenza non sufficiente per il prodotto.';
    ELSE
        UPDATE Prodotti
        SET Giacenza = Giacenza - NEW.Quantita
        WHERE IDProdotto = NEW.IDProdotto;
    END IF;
END;
//

-- Trigger esistente: AggiornaTotaleOrdineDopoInserimentoDettaglio
CREATE TRIGGER AggiornaTotaleOrdineDopoInserimentoDettaglio
AFTER INSERT ON DettagliOrdine
FOR EACH ROW
BEGIN
    UPDATE Ordini
    SET TotaleOrdine = TotaleOrdine + (NEW.Quantita * NEW.PrezzoUnitarioAlMomentoOrdine)
    WHERE IDOrdine = NEW.IDOrdine;
END;
//

-- Trigger NUOVO: DopoAggiornamentoPagamento
CREATE TRIGGER DopoAggiornamentoPagamento
AFTER UPDATE ON Pagamenti
FOR EACH ROW
BEGIN
    IF OLD.StatoPagamento <> NEW.StatoPagamento THEN -- Esegui solo se lo stato del pagamento è cambiato
        IF NEW.StatoPagamento = 'Completato' THEN
            UPDATE Ordini
            SET StatoOrdine = 'In Lavorazione'
            WHERE IDOrdine = NEW.IDOrdine AND StatoOrdine IN ('In Attesa di Pagamento', 'Pagamento Fallito');
        ELSEIF NEW.StatoPagamento = 'Fallito' THEN
            UPDATE Ordini
            SET StatoOrdine = 'Pagamento Fallito'
            WHERE IDOrdine = NEW.IDOrdine AND StatoOrdine = 'In Attesa di Pagamento';
        ELSEIF NEW.StatoPagamento = 'Rimborsato' THEN
             UPDATE Ordini
            SET StatoOrdine = 'Annullato' -- O uno stato 'Rimborsato' se esistesse in Ordini
            WHERE IDOrdine = NEW.IDOrdine;
        END IF;
    END IF;
END;
//

-- Trigger opzionali (gestione aggiornamento e cancellazione DettagliOrdine) - MANTENUTI
CREATE TRIGGER AggiornaDatiDopoUpdateDettaglio
AFTER UPDATE ON DettagliOrdine
FOR EACH ROW
BEGIN
    DECLARE differenzaQuantita INT;
    DECLARE differenzaImporto DECIMAL(10,2);
    SET differenzaQuantita = NEW.Quantita - OLD.Quantita;
    UPDATE Prodotti SET Giacenza = Giacenza - differenzaQuantita WHERE IDProdotto = NEW.IDProdotto;
    SET differenzaImporto = (NEW.Quantita * NEW.PrezzoUnitarioAlMomentoOrdine) - (OLD.Quantita * OLD.PrezzoUnitarioAlMomentoOrdine);
    UPDATE Ordini SET TotaleOrdine = TotaleOrdine + differenzaImporto WHERE IDOrdine = NEW.IDOrdine;
END;
//

CREATE TRIGGER RipristinaGiacenzaDopoDeleteDettaglio
AFTER DELETE ON DettagliOrdine
FOR EACH ROW
BEGIN
    UPDATE Prodotti SET Giacenza = Giacenza + OLD.Quantita WHERE IDProdotto = OLD.IDProdotto;
    UPDATE Ordini SET TotaleOrdine = TotaleOrdine - (OLD.Quantita * OLD.PrezzoUnitarioAlMomentoOrdine) WHERE IDOrdine = OLD.IDOrdine;
END;
//

DELIMITER ;


-- ############################################
-- # 2. Popolamento del Database (DML)        #
-- ############################################

-- Clienti, Categorie, Prodotti (come prima)
INSERT INTO Clienti (Nome, Cognome, Email, IndirizzoSpedizione, CittaSpedizione, CAPSpedizione, DataRegistrazione) VALUES
('Mario', 'Rossi', 'mario.rossi@example.com', 'Via Roma 1', 'Milano', '20100', '2023-01-15'),
('Laura', 'Bianchi', 'laura.bianchi@example.com', 'Corso Europa 20', 'Roma', '00100', '2023-03-22'),
('Paolo', 'Verdi', 'paolo.verdi@example.com', 'Piazza Dante 5', 'Napoli', '80100', '2024-02-10'),
('Anna', 'Gialli', 'anna.gialli@example.com', 'Via Garibaldi 7', 'Torino', '10100', CURDATE() - INTERVAL 6 MONTH),
('Luca', 'Neri', 'luca.neri@example.com', 'Viale Italia 30', 'Firenze', '50100', CURDATE() - INTERVAL 1 YEAR);

INSERT INTO Categorie (NomeCategoria, DescrizioneCategoria) VALUES
('Elettronica', 'Dispositivi elettronici, componenti e accessori.'),
('Libri', 'Libri di vario genere, romanzi, saggi, manuali.'),
('Abbigliamento', 'Vestiti, scarpe e accessori moda.');

INSERT INTO Prodotti (NomeProdotto, DescrizioneProdotto, PrezzoUnitario, IDCategoria, Giacenza) VALUES
('Smartphone XYZ', 'Ultimo modello con fotocamera 20MP', 799.99, (SELECT IDCategoria FROM Categorie WHERE NomeCategoria = 'Elettronica'), 50),
('Laptop Pro 15"', 'Portatile potente per professionisti', 1499.50, (SELECT IDCategoria FROM Categorie WHERE NomeCategoria = 'Elettronica'), 30),
('Cuffie Wireless Pro', 'Cuffie con cancellazione del rumore', 199.00, (SELECT IDCategoria FROM Categorie WHERE NomeCategoria = 'Elettronica'), 100),
('Il Signore degli Anelli', 'Edizione completa della trilogia', 35.50, (SELECT IDCategoria FROM Categorie WHERE NomeCategoria = 'Libri'), 75),
('Introduzione a SQL', 'Manuale pratico per database', 25.00, (SELECT IDCategoria FROM Categorie WHERE NomeCategoria = 'Libri'), 120);
INSERT INTO Prodotti (NomeProdotto, DescrizioneProdotto, PrezzoUnitario, IDCategoria, Giacenza) VALUES
('T-shirt Cotone Bio', 'Maglietta in cotone biologico, vari colori', 19.99, (SELECT IDCategoria FROM Categorie WHERE NomeCategoria = 'Abbigliamento'), 200),
('Jeans Slim Fit', 'Jeans elasticizzati modello slim', 79.90, (SELECT IDCategoria FROM Categorie WHERE NomeCategoria = 'Abbigliamento'), 150),
('Smartwatch Active 2', 'Orologio intelligente per sportivi', 249.00, (SELECT IDCategoria FROM Categorie WHERE NomeCategoria = 'Elettronica'), 60),
('Il Nome della Rosa', 'Romanzo storico di Umberto Eco', 15.75, (SELECT IDCategoria FROM Categorie WHERE NomeCategoria = 'Libri'), 90),
('Felpa con Cappuccio', 'Felpa sportiva con logo', 45.00, (SELECT IDCategoria FROM Categorie WHERE NomeCategoria = 'Abbigliamento'), 110);


-- Inserimento Ordini
-- Ordine 1: Mario Rossi
INSERT INTO Ordini (IDCliente, IndirizzoSpedizioneOrdine, CittaSpedizioneOrdine, CAPSpedizioneOrdine)
SELECT IDCliente, IndirizzoSpedizione, CittaSpedizione, CAPSpedizione FROM Clienti WHERE Email = 'mario.rossi@example.com';
SET @IDOrdine1 = LAST_INSERT_ID();

-- Ordine 2: Laura Bianchi
INSERT INTO Ordini (IDCliente, IndirizzoSpedizioneOrdine, CittaSpedizioneOrdine, CAPSpedizioneOrdine)
SELECT IDCliente, IndirizzoSpedizione, CittaSpedizione, CAPSpedizione FROM Clienti WHERE Email = 'laura.bianchi@example.com';
SET @IDOrdine2 = LAST_INSERT_ID();

-- Ordine 3: Paolo Verdi
INSERT INTO Ordini (IDCliente, IndirizzoSpedizioneOrdine, CittaSpedizioneOrdine, CAPSpedizioneOrdine)
SELECT IDCliente, IndirizzoSpedizione, CittaSpedizione, CAPSpedizione FROM Clienti WHERE Email = 'paolo.verdi@example.com';
SET @IDOrdine3 = LAST_INSERT_ID();

-- Ordine 4: Anna Gialli
INSERT INTO Ordini (IDCliente, IndirizzoSpedizioneOrdine, CittaSpedizioneOrdine, CAPSpedizioneOrdine)
SELECT IDCliente, IndirizzoSpedizione, CittaSpedizione, CAPSpedizione FROM Clienti WHERE Email = 'anna.gialli@example.com';
SET @IDOrdine4 = LAST_INSERT_ID();

-- Ordine 5: Mario Rossi (secondo ordine)
INSERT INTO Ordini (IDCliente, IndirizzoSpedizioneOrdine, CittaSpedizioneOrdine, CAPSpedizioneOrdine)
SELECT IDCliente, 'Via Nuova 123', 'Milano', '20123' FROM Clienti WHERE Email = 'mario.rossi@example.com';
SET @IDOrdine5 = LAST_INSERT_ID();

-- Ordine 6: Luca Neri
INSERT INTO Ordini (IDCliente, IndirizzoSpedizioneOrdine, CittaSpedizioneOrdine, CAPSpedizioneOrdine)
SELECT IDCliente, IndirizzoSpedizione, CittaSpedizione, CAPSpedizione FROM Clienti WHERE Email = 'luca.neri@example.com';
SET @IDOrdine6 = LAST_INSERT_ID();

-- Ordine 7: Laura Bianchi (secondo ordine)
INSERT INTO Ordini (IDCliente, IndirizzoSpedizioneOrdine, CittaSpedizioneOrdine, CAPSpedizioneOrdine)
SELECT IDCliente, IndirizzoSpedizione, CittaSpedizione, CAPSpedizione FROM Clienti WHERE Email = 'laura.bianchi@example.com';
SET @IDOrdine7 = LAST_INSERT_ID();

-- Ordine 8: Anna Gialli (secondo ordine)
INSERT INTO Ordini (IDCliente, IndirizzoSpedizioneOrdine, CittaSpedizioneOrdine, CAPSpedizioneOrdine)
SELECT IDCliente, IndirizzoSpedizione, CittaSpedizione, CAPSpedizione FROM Clienti WHERE Email = 'anna.gialli@example.com';
SET @IDOrdine8 = LAST_INSERT_ID();

-- Inserimento DettagliOrdine
-- Dettaglio Ordine 1
SET @IDProdSmartphone = (SELECT IDProdotto FROM Prodotti WHERE NomeProdotto = 'Smartphone XYZ');
SET @PrezzoSmartphone = (SELECT PrezzoUnitario FROM Prodotti WHERE IDProdotto = @IDProdSmartphone);
INSERT INTO DettagliOrdine (IDOrdine, IDProdotto, Quantita, PrezzoUnitarioAlMomentoOrdine)
VALUES (@IDOrdine1, @IDProdSmartphone, 1, @PrezzoSmartphone);

SET @IDProdTshirt = (SELECT IDProdotto FROM Prodotti WHERE NomeProdotto = 'T-shirt Cotone Bio');
SET @PrezzoTshirt = (SELECT PrezzoUnitario FROM Prodotti WHERE IDProdotto = @IDProdTshirt);
INSERT INTO DettagliOrdine (IDOrdine, IDProdotto, Quantita, PrezzoUnitarioAlMomentoOrdine)
VALUES (@IDOrdine1, @IDProdTshirt, 2, @PrezzoTshirt);

-- Dettaglio Ordine 2
SET @IDProdLaptop = (SELECT IDProdotto FROM Prodotti WHERE NomeProdotto = 'Laptop Pro 15"');
SET @PrezzoLaptop = (SELECT PrezzoUnitario FROM Prodotti WHERE IDProdotto = @IDProdLaptop);
INSERT INTO DettagliOrdine (IDOrdine, IDProdotto, Quantita, PrezzoUnitarioAlMomentoOrdine)
VALUES (@IDOrdine2, @IDProdLaptop, 1, @PrezzoLaptop);

-- Dettaglio Ordine 3
SET @IDProdSQLBook = (SELECT IDProdotto FROM Prodotti WHERE NomeProdotto = 'Introduzione a SQL');
SET @PrezzoSQLBook = (SELECT PrezzoUnitario FROM Prodotti WHERE IDProdotto = @IDProdSQLBook);
INSERT INTO DettagliOrdine (IDOrdine, IDProdotto, Quantita, PrezzoUnitarioAlMomentoOrdine)
VALUES (@IDOrdine3, @IDProdSQLBook, 1, @PrezzoSQLBook);

-- Dettaglio Ordine 4
SET @IDProdCuffie = (SELECT IDProdotto FROM Prodotti WHERE NomeProdotto = 'Cuffie Wireless Pro');
SET @PrezzoCuffie = (SELECT PrezzoUnitario FROM Prodotti WHERE IDProdotto = @IDProdCuffie);
INSERT INTO DettagliOrdine (IDOrdine, IDProdotto, Quantita, PrezzoUnitarioAlMomentoOrdine)
VALUES (@IDOrdine4, @IDProdCuffie, 1, @PrezzoCuffie);

-- Dettaglio Ordine 5
SET @IDProdLOTR = (SELECT IDProdotto FROM Prodotti WHERE NomeProdotto = 'Il Signore degli Anelli');
SET @PrezzoLOTR = (SELECT PrezzoUnitario FROM Prodotti WHERE IDProdotto = @IDProdLOTR);
INSERT INTO DettagliOrdine (IDOrdine, IDProdotto, Quantita, PrezzoUnitarioAlMomentoOrdine)
VALUES (@IDOrdine5, @IDProdLOTR, 3, @PrezzoLOTR);

-- Dettaglio Ordine 6
SET @IDProdJeans = (SELECT IDProdotto FROM Prodotti WHERE NomeProdotto = 'Jeans Slim Fit');
SET @PrezzoJeans = (SELECT PrezzoUnitario FROM Prodotti WHERE IDProdotto = @IDProdJeans);
INSERT INTO DettagliOrdine (IDOrdine, IDProdotto, Quantita, PrezzoUnitarioAlMomentoOrdine)
VALUES (@IDOrdine6, @IDProdJeans, 1, @PrezzoJeans);

-- Dettaglio Ordine 7
SET @IDProdSmartwatch = (SELECT IDProdotto FROM Prodotti WHERE NomeProdotto = 'Smartwatch Active 2');
SET @PrezzoSmartwatch = (SELECT PrezzoUnitario FROM Prodotti WHERE IDProdotto = @IDProdSmartwatch);
INSERT INTO DettagliOrdine (IDOrdine, IDProdotto, Quantita, PrezzoUnitarioAlMomentoOrdine)
VALUES (@IDOrdine7, @IDProdSmartwatch, 1, @PrezzoSmartwatch);

-- Dettaglio Ordine 8
SET @IDProdFelpa = (SELECT IDProdotto FROM Prodotti WHERE NomeProdotto = 'Felpa con Cappuccio');
SET @PrezzoFelpa = (SELECT PrezzoUnitario FROM Prodotti WHERE IDProdotto = @IDProdFelpa);
INSERT INTO DettagliOrdine (IDOrdine, IDProdotto, Quantita, PrezzoUnitarioAlMomentoOrdine)
VALUES (@IDOrdine8, @IDProdFelpa, 1, @PrezzoFelpa);


-- Inserimento Pagamenti (DettagliRispostaGateway come stringa JSON valida)
-- Pagamento per Ordine 1 (Completato)
SET @TotaleOrdine1 = (SELECT TotaleOrdine FROM Ordini WHERE IDOrdine = @IDOrdine1);
INSERT INTO Pagamenti (IDOrdine, ImportoPagato, MetodoPagamento, IDTransazioneGateway, StatoPagamento, DettagliRispostaGateway)
VALUES (@IDOrdine1, @TotaleOrdine1, 'Stripe', CONCAT('stripe_tx_', UUID_SHORT()), 'In Attesa', JSON_OBJECT("initial_status", "pending"));
UPDATE Pagamenti SET StatoPagamento = 'Completato', DataOraPagamento = NOW() - INTERVAL 2 DAY, DettagliRispostaGateway = JSON_OBJECT("final_status", "success", "message", "Pagamento approvato Stripe") WHERE IDOrdine = @IDOrdine1;

-- Pagamento per Ordine 2 (Completato)
SET @TotaleOrdine2 = (SELECT TotaleOrdine FROM Ordini WHERE IDOrdine = @IDOrdine2);
INSERT INTO Pagamenti (IDOrdine, ImportoPagato, MetodoPagamento, IDTransazioneGateway, StatoPagamento, DettagliRispostaGateway)
VALUES (@IDOrdine2, @TotaleOrdine2, 'PayPal', CONCAT('paypal_tx_', UUID_SHORT()), 'In Attesa', '{"gateway": "PayPal", "step": "initiated"}');
UPDATE Pagamenti SET StatoPagamento = 'Completato', DataOraPagamento = NOW() - INTERVAL 1 DAY, DettagliRispostaGateway = '{"gateway": "PayPal", "step": "completed", "paypal_id": "PYPL123ABC"}' WHERE IDOrdine = @IDOrdine2;

-- Pagamento per Ordine 3 (Fallito)
SET @TotaleOrdine3 = (SELECT TotaleOrdine FROM Ordini WHERE IDOrdine = @IDOrdine3);
INSERT INTO Pagamenti (IDOrdine, ImportoPagato, MetodoPagamento, StatoPagamento, DettagliRispostaGateway)
VALUES (@IDOrdine3, @TotaleOrdine3, 'CartaCredito', 'In Attesa', NULL);
UPDATE Pagamenti SET StatoPagamento = 'Fallito', DataOraPagamento = NOW() - INTERVAL 5 HOUR, DettagliRispostaGateway = JSON_OBJECT("error_code", "CC_DECLINED", "reason", "Fondi insufficienti") WHERE IDOrdine = @IDOrdine3;

-- Pagamento per Ordine 4 (In Attesa)
SET @TotaleOrdine4 = (SELECT TotaleOrdine FROM Ordini WHERE IDOrdine = @IDOrdine4);
INSERT INTO Pagamenti (IDOrdine, ImportoPagato, MetodoPagamento, StatoPagamento, DettagliRispostaGateway)
VALUES (@IDOrdine4, @TotaleOrdine4, 'Stripe', 'In Attesa', JSON_OBJECT("status", "created", "next_action_url", "https://stripe.com/pay/some_id"));

-- Pagamento per Ordine 5 (Completato)
SET @TotaleOrdine5 = (SELECT TotaleOrdine FROM Ordini WHERE IDOrdine = @IDOrdine5);
INSERT INTO Pagamenti (IDOrdine, ImportoPagato, MetodoPagamento, IDTransazioneGateway, StatoPagamento, DettagliRispostaGateway)
VALUES (@IDOrdine5, @TotaleOrdine5, 'CartaCredito', CONCAT('cc_tx_', UUID_SHORT()), 'In Attesa', NULL);
UPDATE Pagamenti SET StatoPagamento = 'Completato', DataOraPagamento = NOW() - INTERVAL 3 DAY, DettagliRispostaGateway = '{"auth_code": "AUTH789", "card_type": "Visa"}' WHERE IDOrdine = @IDOrdine5;

-- Pagamento per Ordine 6 (Completato)
SET @TotaleOrdine6 = (SELECT TotaleOrdine FROM Ordini WHERE IDOrdine = @IDOrdine6);
INSERT INTO Pagamenti (IDOrdine, ImportoPagato, MetodoPagamento, IDTransazioneGateway, StatoPagamento, DettagliRispostaGateway)
VALUES (@IDOrdine6, @TotaleOrdine6, 'PayPal', CONCAT('paypal_tx_', UUID_SHORT()), 'In Attesa', NULL);
UPDATE Pagamenti SET StatoPagamento = 'Completato', DataOraPagamento = NOW() - INTERVAL 6 HOUR, DettagliRispostaGateway = JSON_OBJECT("status", "COMPLETED", "payer_email", "payer@example.com") WHERE IDOrdine = @IDOrdine6;

-- Pagamento per Ordine 7 (In Attesa)
SET @TotaleOrdine7 = (SELECT TotaleOrdine FROM Ordini WHERE IDOrdine = @IDOrdine7);
INSERT INTO Pagamenti (IDOrdine, ImportoPagato, MetodoPagamento, StatoPagamento, DettagliRispostaGateway)
VALUES (@IDOrdine7, @TotaleOrdine7, 'Stripe', 'In Attesa', NULL);

-- Pagamento per Ordine 8 (Rimborsato)
SET @TotaleOrdine8 = (SELECT TotaleOrdine FROM Ordini WHERE IDOrdine = @IDOrdine8);
INSERT INTO Pagamenti (IDOrdine, ImportoPagato, MetodoPagamento, IDTransazioneGateway, StatoPagamento, DataOraPagamento, DettagliRispostaGateway)
VALUES (@IDOrdine8, @TotaleOrdine8, 'Stripe', CONCAT('stripe_tx_refund_', UUID_SHORT()), 'Completato', NOW() - INTERVAL 7 DAY, '{"original_tx": "prev_stripe_id", "status": "paid"}');
UPDATE Pagamenti SET StatoPagamento = 'Rimborsato', DataOraPagamento = NOW() - INTERVAL 1 HOUR, DettagliRispostaGateway = '{"refund_id": "re_123xyz", "reason": "requested_by_customer"}' WHERE IDOrdine = @IDOrdine8;


-- ############################################
-- # 3. Interrogazioni SQL                    #
-- ############################################

-- ## Query di Base ##
-- 1. Selezionare tutti i prodotti con un `PrezzoUnitario` superiore a 50.00 €, ordinati per prezzo.
SELECT NomeProdotto, PrezzoUnitario, Giacenza FROM Prodotti WHERE PrezzoUnitario > 50.00 ORDER BY PrezzoUnitario DESC;

-- 2. Trovare i clienti registrati nell'ultimo anno.
SELECT Nome, Cognome, Email, DataRegistrazione FROM Clienti WHERE DataRegistrazione >= DATE_SUB(CURDATE(), INTERVAL 1 YEAR);

-- 3. Elencare tutti gli ordini (`IDOrdine`, `DataOrdine`, `TotaleOrdine`) con `StatoOrdine` = 'In Lavorazione'.
SELECT IDOrdine, DataOrdine, TotaleOrdine, StatoOrdine FROM Ordini WHERE StatoOrdine = 'In Lavorazione';

-- 4. Visualizzare tutti i pagamenti effettuati tramite 'PayPal' con `StatoPagamento` = 'Completato'.
SELECT P.IDPagamento, P.IDOrdine, O.DataOrdine, P.ImportoPagato, P.DataOraPagamento, P.DettagliRispostaGateway
FROM Pagamenti P
JOIN Ordini O ON P.IDOrdine = O.IDOrdine
WHERE P.MetodoPagamento = 'PayPal' AND P.StatoPagamento = 'Completato';

-- 5. Trovare gli ordini per i quali il pagamento è 'Fallito'.
SELECT O.IDOrdine, O.DataOrdine, O.TotaleOrdine, C.Email AS EmailCliente, P.DataOraPagamento AS DataFallimentoPagamento,
       JSON_UNQUOTE(JSON_EXTRACT(P.DettagliRispostaGateway, '$.reason')) AS MotivoFallimento -- Esempio uso JSON_EXTRACT
FROM Ordini O
JOIN Pagamenti P ON O.IDOrdine = P.IDOrdine
JOIN Clienti C ON O.IDCliente = C.IDCliente
WHERE P.StatoPagamento = 'Fallito';

-- ## Query con JOIN ##
-- 6. Mostrare `NomeProdotto` e `NomeCategoria` per ogni prodotto.
SELECT P.NomeProdotto, C.NomeCategoria, P.PrezzoUnitario FROM Prodotti P JOIN Categorie C ON P.IDCategoria = C.IDCategoria ORDER BY C.NomeCategoria, P.NomeProdotto;

-- 7. Per un `IDOrdine` specifico (es. @IDOrdine1), elencare `NomeProdotto`, `Quantita` e `PrezzoUnitarioAlMomentoOrdine`,
--    e anche i dettagli del pagamento associato (`MetodoPagamento`, `StatoPagamento`, `DataOraPagamento`).
SELECT O.IDOrdine, PR.NomeProdotto, DO.Quantita, DO.PrezzoUnitarioAlMomentoOrdine,
       PG.MetodoPagamento, PG.StatoPagamento, PG.DataOraPagamento, PG.ImportoPagato, PG.IDTransazioneGateway, PG.DettagliRispostaGateway
FROM Ordini O
JOIN DettagliOrdine DO ON O.IDOrdine = DO.IDOrdine
JOIN Prodotti PR ON DO.IDProdotto = PR.IDProdotto
LEFT JOIN Pagamenti PG ON O.IDOrdine = PG.IDOrdine
WHERE O.IDOrdine = @IDOrdine1;

-- 8. Visualizzare `Nome` e `Cognome` del cliente, `IDOrdine`, `DataOrdine` e `StatoPagamento`
--    per tutti gli ordini con `StatoPagamento` = 'Completato'.
SELECT C.Nome, C.Cognome, O.IDOrdine, O.DataOrdine, O.TotaleOrdine, PG.StatoPagamento, PG.MetodoPagamento
FROM Clienti C
JOIN Ordini O ON C.IDCliente = O.IDCliente
JOIN Pagamenti PG ON O.IDOrdine = PG.IDOrdine
WHERE PG.StatoPagamento = 'Completato'
ORDER BY O.DataOrdine DESC;

-- ## Query con Funzioni Aggregate e Raggruppamento ##
-- 9. Calcolare il numero di prodotti per categoria.
SELECT C.NomeCategoria, COUNT(P.IDProdotto) AS NumeroProdotti
FROM Categorie C LEFT JOIN Prodotti P ON C.IDCategoria = P.IDCategoria
GROUP BY C.IDCategoria, C.NomeCategoria ORDER BY NumeroProdotti DESC;

-- 10. Calcolare l'importo totale incassato per ciascun `MetodoPagamento` (considerando solo i pagamenti 'Completato').
SELECT MetodoPagamento, SUM(ImportoPagato) AS TotaleIncassato, COUNT(*) AS NumeroPagamentiCompletati
FROM Pagamenti
WHERE StatoPagamento = 'Completato'
GROUP BY MetodoPagamento
ORDER BY TotaleIncassato DESC;

-- 11. Trovare l'importo medio degli ordini il cui pagamento è stato 'Completato'.
SELECT AVG(O.TotaleOrdine) AS ImportoMedioOrdinePagato
FROM Ordini O
JOIN Pagamenti P ON O.IDOrdine = P.IDOrdine
WHERE P.StatoPagamento = 'Completato';

-- ## Subquery e Query Complesse ##
-- 12. Elencare i clienti che hanno effettuato ordini con pagamento 'Completato'
--     per un `TotaleOrdine` aggregato (somma dei TotaleOrdine per i pagamenti completati) superiore a 200.00 €.
SELECT C.IDCliente, C.Nome, C.Cognome, C.Email, SUM(O.TotaleOrdine) AS TotaleSpesoConPagamentiCompletati
FROM Clienti C
JOIN Ordini O ON C.IDCliente = O.IDCliente
JOIN Pagamenti P ON O.IDOrdine = P.IDOrdine
WHERE P.StatoPagamento = 'Completato'
GROUP BY C.IDCliente, C.Nome, C.Cognome, C.Email
HAVING SUM(O.TotaleOrdine) > 200.00
ORDER BY TotaleSpesoConPagamentiCompletati DESC;

-- 13. Trovare i prodotti che sono stati inclusi in ordini con pagamento 'Fallito'.
SELECT DISTINCT P.IDProdotto, P.NomeProdotto, P.PrezzoUnitario
FROM Prodotti P
JOIN DettagliOrdine DO ON P.IDProdotto = DO.IDProdotto
JOIN Ordini O ON DO.IDOrdine = O.IDOrdine
JOIN Pagamenti PG ON O.IDOrdine = PG.IDOrdine
WHERE PG.StatoPagamento = 'Fallito';


-- ############################################
-- # 4. Viste (View)                          #
-- ############################################

-- 1. Vista `VistaOrdiniConDettagliPagamento`
CREATE OR REPLACE VIEW VistaOrdiniConDettagliPagamento AS
SELECT O.IDOrdine, O.DataOrdine, O.TotaleOrdine, O.StatoOrdine,
       CL.Nome AS NomeCliente, CL.Cognome AS CognomeCliente, CL.Email AS EmailCliente,
       PG.MetodoPagamento, PG.StatoPagamento, PG.ImportoPagato, PG.IDTransazioneGateway, PG.DataOraPagamento, PG.DettagliRispostaGateway
FROM Ordini O
JOIN Clienti CL ON O.IDCliente = CL.IDCliente
LEFT JOIN Pagamenti PG ON O.IDOrdine = PG.IDOrdine;

-- Per interrogare la vista:
SELECT * FROM VistaOrdiniConDettagliPagamento ORDER BY DataOrdine DESC;

-- 2. Vista `VistaRiepilogoIncassiGiornalieri`
CREATE OR REPLACE VIEW VistaRiepilogoIncassiGiornalieri AS
SELECT DATE(DataOraPagamento) AS Giorno,
       COUNT(*) AS NumeroPagamentiCompletati,
       SUM(ImportoPagato) AS TotaleIncassatoGiorno
FROM Pagamenti
WHERE StatoPagamento = 'Completato'
GROUP BY DATE(DataOraPagamento)
ORDER BY Giorno DESC;

-- Per interrogare la vista:
SELECT * FROM VistaRiepilogoIncassiGiornalieri;

-- ############################################
-- # 6. Stored Procedure/Function             #
-- ############################################
DELIMITER //

-- 1. Modifica Stored Procedure `CreaNuovoOrdine`
CREATE PROCEDURE CreaNuovoOrdine(
    IN p_IDCliente INT,
    IN p_IndirizzoSpedizione VARCHAR(200),
    IN p_CittaSpedizione VARCHAR(50),
    IN p_CAPSpedizione VARCHAR(10),
    IN p_IDProdotto INT,
    IN p_Quantita INT,
    IN p_MetodoPagamentoScelto ENUM('CartaCredito', 'PayPal', 'Stripe', 'Bonifico'),
    OUT p_IDNuovoOrdine INT
)
BEGIN
    DECLARE v_GiacenzaAttuale INT;
    DECLARE v_PrezzoAttuale DECIMAL(10,2);
    DECLARE v_IDOrdineCreato INT;
    DECLARE v_TotaleCalcolatoOrdine DECIMAL(10,2) DEFAULT 0.00;
    DECLARE v_ErrorMessage VARCHAR(255);

    IF NOT EXISTS (SELECT 1 FROM Clienti WHERE IDCliente = p_IDCliente) THEN
        SET v_ErrorMessage = CONCAT('Errore: Cliente con ID ', p_IDCliente, ' non trovato.');
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = v_ErrorMessage;
    END IF;

    SELECT PrezzoUnitario, Giacenza INTO v_PrezzoAttuale, v_GiacenzaAttuale FROM Prodotti WHERE IDProdotto = p_IDProdotto;
    IF v_PrezzoAttuale IS NULL THEN
        SET v_ErrorMessage = CONCAT('Errore: Prodotto con ID ', p_IDProdotto, ' non trovato.');
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = v_ErrorMessage;
    END IF;

    IF v_GiacenzaAttuale < p_Quantita THEN
        SET v_ErrorMessage = CONCAT('Errore: Giacenza insufficiente per il prodotto ID ', p_IDProdotto);
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = v_ErrorMessage;
    END IF;
    
    IF p_Quantita <= 0 THEN
        SET v_ErrorMessage = 'Errore: La quantità deve essere maggiore di zero.';
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = v_ErrorMessage;
    END IF;

    -- SET v_TotaleCalcolatoOrdine = p_Quantita * v_PrezzoAttuale; -- Calcolato dopo l'insert in DettagliOrdine per usare il trigger

    INSERT INTO Ordini (IDCliente, IndirizzoSpedizioneOrdine, CittaSpedizioneOrdine, CAPSpedizioneOrdine, StatoOrdine, TotaleOrdine)
    VALUES (p_IDCliente, p_IndirizzoSpedizione, p_CittaSpedizione, p_CAPSpedizione, 'In Attesa di Pagamento', 0.00);
    SET v_IDOrdineCreato = LAST_INSERT_ID();

    INSERT INTO DettagliOrdine (IDOrdine, IDProdotto, Quantita, PrezzoUnitarioAlMomentoOrdine)
    VALUES (v_IDOrdineCreato, p_IDProdotto, p_Quantita, v_PrezzoAttuale);
    
    -- Recupera il TotaleOrdine aggiornato dal trigger AggiornaTotaleOrdineDopoInserimentoDettaglio
    SELECT TotaleOrdine INTO v_TotaleCalcolatoOrdine FROM Ordini WHERE IDOrdine = v_IDOrdineCreato;

    INSERT INTO Pagamenti (IDOrdine, ImportoPagato, MetodoPagamento, StatoPagamento, DettagliRispostaGateway)
    VALUES (v_IDOrdineCreato, v_TotaleCalcolatoOrdine, p_MetodoPagamentoScelto, 'In Attesa', JSON_OBJECT("creation_source", "CreaNuovoOrdine_SP"));

    SET p_IDNuovoOrdine = v_IDOrdineCreato;
END;
//

-- 2. Stored Procedure `RegistraEsitoPagamentoGateway` (p_DettagliRisposta si aspetta una stringa JSON)
CREATE PROCEDURE RegistraEsitoPagamentoGateway(
    IN p_IDOrdine INT,
    IN p_IDTransazioneGateway VARCHAR(100),
    IN p_EsitoGateway ENUM('Completato', 'Fallito'),
    IN p_ImportoConfermatoGateway DECIMAL(10,2),
    IN p_DettagliRispostaJSON VARCHAR(1000) -- Accetta una stringa che DOVREBBE essere JSON.
)
BEGIN
    DECLARE v_pagamentoEsiste INT;
    DECLARE v_ordineEsiste INT;
    DECLARE v_errorMessage VARCHAR(255);
    DECLARE v_jsonValido BOOLEAN DEFAULT TRUE; -- Non strettamente necessario se MariaDB valida all'UPDATE
    
    SET v_jsonValido = JSON_VALID(p_DettagliRispostaJSON);
    IF NOT v_jsonValido AND p_DettagliRispostaJSON IS NOT NULL THEN
        SET v_ErrorMessage = 'Errore: DettagliRispostaGateway non è un JSON valido.';
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = v_ErrorMessage;
    END IF;

    SELECT COUNT(*) INTO v_ordineEsiste FROM Ordini WHERE IDOrdine = p_IDOrdine;
    IF v_ordineEsiste = 0 THEN
        SET v_ErrorMessage = CONCAT('Errore: Ordine con ID ', p_IDOrdine, ' non trovato.');
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = v_ErrorMessage;
    END IF;

    SELECT COUNT(*) INTO v_pagamentoEsiste FROM Pagamenti WHERE IDOrdine = p_IDOrdine AND StatoPagamento = 'In Attesa';
    IF v_pagamentoEsiste = 0 THEN
        SET v_ErrorMessage = CONCAT('Errore: Pagamento in attesa per l''ordine ID ', p_IDOrdine, ' non trovato o già processato.');
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = v_ErrorMessage;
    END IF;

    UPDATE Pagamenti
    SET IDTransazioneGateway = p_IDTransazioneGateway,
        StatoPagamento = p_EsitoGateway,
        ImportoPagato = p_ImportoConfermatoGateway,
        DataOraPagamento = CURRENT_TIMESTAMP,
        DettagliRispostaGateway = p_DettagliRispostaJSON -- MariaDB validerà che sia JSON
    WHERE IDOrdine = p_IDOrdine AND StatoPagamento = 'In Attesa';

    SELECT CONCAT('Esito pagamento per ordine ID: ', p_IDOrdine, ' registrato come: ', p_EsitoGateway) AS Messaggio;
END;
//

-- 3. Funzione `GetTotaleVendutoProdotto`
CREATE FUNCTION GetTotaleVendutoProdotto(
    f_IDProdotto INT
)
RETURNS DECIMAL(12,2)
DETERMINISTIC
READS SQL DATA
BEGIN
    DECLARE v_TotaleVenduto DECIMAL(12,2);

    SELECT SUM(DO.Quantita * DO.PrezzoUnitarioAlMomentoOrdine) INTO v_TotaleVenduto
    FROM DettagliOrdine DO
    JOIN Ordini O ON DO.IDOrdine = O.IDOrdine
    JOIN Pagamenti P ON O.IDOrdine = P.IDOrdine
    WHERE DO.IDProdotto = f_IDProdotto AND P.StatoPagamento = 'Completato';

    IF v_TotaleVenduto IS NULL THEN
        SET v_TotaleVenduto = 0.00;
    END IF;
    RETURN v_TotaleVenduto;
END;
//

DELIMITER ;
-- Test Stored Procedure CreaNuovoOrdine e RegistraEsitoPagamentoGateway:
SET @newOrderID = 0;
CALL CreaNuovoOrdine(1, 'Via Prova SP', 'TestCity SP', '00000', 1, 1, 'Stripe', @newOrderID);
SELECT @newOrderID;
SELECT * FROM Ordini WHERE IDOrdine = @newOrderID;
SELECT * FROM Pagamenti WHERE IDOrdine = @newOrderID;

CALL RegistraEsitoPagamentoGateway(@newOrderID, CONCAT('stripe_final_tx_', UUID_SHORT()), 'Completato', (SELECT TotaleOrdine FROM Ordini WHERE IDOrdine=@newOrderID), '{"gateway_status": "succeeded", "charge_id": "ch_123"}');
SELECT * FROM Ordini WHERE IDOrdine = @newOrderID;
SELECT * FROM Pagamenti WHERE IDOrdine = @newOrderID;
SELECT JSON_EXTRACT(DettagliRispostaGateway, '$.gateway_status') FROM Pagamenti WHERE IDOrdine = @newOrderID;

SET @IDOrdine4 = 4; -- Supponiamo che l'IDOrdine 4 sia quello dell'ordine di Anna Gialli
CALL RegistraEsitoPagamentoGateway(@IDOrdine4, CONCAT('stripe_fail_tx_', UUID_SHORT()), 'Fallito', 0.00, '{"error": {"code": "card_declined", "message": "Your card was declined."}}');
SELECT * FROM Ordini WHERE IDOrdine = @IDOrdine4;
SELECT * FROM Pagamenti WHERE IDOrdine = @IDOrdine4;
SELECT JSON_EXTRACT(DettagliRispostaGateway, '$.error.message') FROM Pagamenti WHERE IDOrdine = @IDOrdine4;

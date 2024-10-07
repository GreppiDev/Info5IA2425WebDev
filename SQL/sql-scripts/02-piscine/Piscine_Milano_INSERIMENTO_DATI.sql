
# inserisco dati nel database

#inserisco gli insegnanti
# primo insegnante
INSERT INTO Insegnanti (CodiceFiscale, Cognome, Nome, Cellulare) 
	VALUES ('SALMAT74C24F129F', 'Salvati', 'Matteo', '3471824234');
# secondo insegnante di cui non conosco il numero di telefono
INSERT INTO Insegnanti (CodiceFiscale, Cognome, Nome) 
	VALUES ('ALBGIN77B31C133C', 'Alberti', 'Giovanni');
#inserisco le qualifiche del primo insegnante
INSERT INTO Qualifiche (QualificaIn, Insegnante) VALUES ('Bagnino di Salvataggio', 'SALMAT74C24F129F');
INSERT INTO Qualifiche (QualificaIn, Insegnante) VALUES ('Brevetto di Sub di Primo Grado', 'SALMAT74C24F129F');
INSERT INTO Qualifiche (QualificaIn, Insegnante) VALUES ('Brevetto di Sub di Secondo Grado', 'SALMAT74C24F129F');
#inserisco le qualifiche per il secondo insegnante
INSERT INTO Qualifiche (QualificaIn, Insegnante) VALUES 
('Bagnino di Salvataggio', 'ALBGIN77B31C133C'),
('Brevetto di Sub di Terzo Grado', 'ALBGIN77B31C133C');
#inserisco i dati delle piscine
INSERT INTO Piscine (NomeP, Indirizzo, Telefono, Responsabile, Tipo, Da, A) 
	VALUES ('Cozzi', 'Viale Tunisia, 35 - Milano', '026599703', 'Marcello Piretti',0, NULL, NULL);
INSERT INTO Piscine (NomeP, Indirizzo, Telefono, Responsabile) 
	VALUES ('Arioli Venegoni', 'Via Arioli Venegoni, 9 - Milano', '024566316','Aristide Paolini');
INSERT INTO Piscine (NomeP, Indirizzo, Telefono, Responsabile, Tipo, Da, A) 
	VALUES ('Lido', 'P.le Lotto, 15 - Milano', '02392791','Tommaso Bianchi',1, '2024-06-01','2024-09-30');
#inserisco i dati delle rotazioni
#rotazioni dello insegnate con CF SALMAT74C24F129F
INSERT INTO Rotazioni(Insegnante, Piscina, DataIniziale, DataFinale) 
	VALUES ('SALMAT74C24F129F', 'Cozzi', '2024-02-01', '2024-06-30');
INSERT INTO Rotazioni(Insegnante, Piscina, DataIniziale, DataFinale) 
	VALUES ('SALMAT74C24F129F', 'Arioli Venegoni', '2024-07-01', '2024-10-31');
#rotazioni dello insegnate con CF 'ALBGIN77B31C133C'
INSERT INTO Rotazioni(Insegnante, Piscina, DataIniziale, DataFinale) 
	VALUES ('ALBGIN77B31C133C', 'Lido', '2024-05-01', '2024-09-30');
INSERT INTO Rotazioni(Insegnante, Piscina, DataIniziale, DataFinale) 
	VALUES ('ALBGIN77B31C133C', 'Arioli Venegoni', '2024-10-01', '2024-12-31');	
#inserisco i dati di alcuni frequentatori di piscine registrati nel database
INSERT INTO Persone(CodiceFiscale, Nome, Cognome) VALUES ('DGNMTT74B24C127R', 'Mattia','Di Giovanni');
INSERT INTO Persone(CodiceFiscale, Nome, Cognome) VALUES ('ALBGNC80B30C123F', 'Giacomo','Albertini');
INSERT INTO Persone(CodiceFiscale, Nome, Cognome) VALUES ('CRSALS90B24B347R', 'Alessio','Caruso');
#inserisco i dati relativi ad alcuni ingressi singoli
INSERT INTO Ingressi_Singoli(Persona, DataUltimoIng, Piscina) VALUES ('DGNMTT74B24C127R',CURRENT_DATE, 'Cozzi');
#inserisco i dati relativi ad alcuni ingressi singoli
INSERT INTO Ingressi_Singoli(Persona, DataUltimoIng, Piscina) VALUES ('ALBGNC80B30C123F','2024-09-09', 'Lido');
#inserisco i dati di alcune persone iscritte ai corsi
INSERT INTO Iscritti_Corsi(Persona, DataNascita, NomeMedico, DataCertificato) 
	VALUES('DGNMTT74B24C127R', '1990-02-21', 'De Benedittis Carlo', '2022-08-03');
INSERT INTO Iscritti_Corsi(Persona, DataNascita, NomeMedico, DataCertificato) 
	VALUES('CRSALS90B24B347R', '2006-04-01', 'Altobelli Giovanni', '2024-07-03');
#inserisco alcuni corsi di qualche piscina
INSERT INTO Corsi(Piscina, NomeC, MaxP, MinP, Costo) VALUES ('Cozzi', 'Acquagym', 26, 8, 126.00);
INSERT INTO Corsi(Piscina, NomeC, MaxP, MinP, Costo) VALUES ('Cozzi', 'Nuoto Libero', 30, 0, 80.00); 
INSERT INTO Corsi(Piscina, NomeC, MaxP, MinP, Costo) VALUES ('Cozzi', 'Corso di Salvamento', 10, 4, 500.00);
INSERT INTO Corsi(Piscina, NomeC, MaxP, MinP, Costo) VALUES ('Lido', 'Acquagym', 34, 8, 190.00);
INSERT INTO Corsi(Piscina, NomeC, MaxP, MinP, Costo) VALUES ('Lido', 'Nuoto Libero', 38, 0, 100.00); 
INSERT INTO Corsi(Piscina, NomeC, MaxP, MinP, Costo) VALUES ('Lido', 'Corso di Salvamento', 10, 4, 700.00);
# inserisco i dettagli dei corsi delle persone che frequentano corsi
INSERT INTO Frequenta_Corsi(Persona, NomeC, Piscina) VALUES ('DGNMTT74B24C127R', 'Nuoto Libero', 'Cozzi');
INSERT INTO Frequenta_Corsi(Persona, NomeC, Piscina) VALUES ('DGNMTT74B24C127R', 'Corso di Salvamento', 'Cozzi'); 
INSERT INTO Frequenta_Corsi(Persona, NomeC, Piscina) VALUES ('CRSALS90B24B347R', 'Acquagym', 'Lido'); 
#inserisco i dati di alcune lezioni
INSERT INTO Lezioni(Piscina, NomeC, Giorno, Ora) VALUES ('Cozzi', 'Acquagym', CURRENT_DATE, '22:30');
INSERT INTO Lezioni(Piscina, NomeC, Giorno, Ora) VALUES ('Cozzi', 'Acquagym', CURRENT_DATE + INTERVAL 1 WEEK, '22:30');
INSERT INTO Lezioni(Piscina, NomeC, Giorno, Ora) VALUES ('Cozzi', 'Acquagym', CURRENT_DATE + INTERVAL 2 WEEK, '22:30');
INSERT INTO Lezioni(Piscina, NomeC, Giorno, Ora) VALUES ('Cozzi', 'Acquagym', CURRENT_DATE + INTERVAL 3 WEEK, '22:30');
INSERT INTO Lezioni(Piscina, NomeC, Giorno, Ora) VALUES ('Cozzi', 'Acquagym', CURRENT_DATE + INTERVAL 4 WEEK, '22:30');

INSERT INTO Lezioni(Piscina, NomeC, Giorno, Ora) VALUES ('Lido', 'Acquagym', CURRENT_DATE, '20:30');
INSERT INTO Lezioni(Piscina, NomeC, Giorno, Ora) VALUES ('Lido', 'Acquagym', CURRENT_DATE + INTERVAL 1 WEEK, '20:30');
INSERT INTO Lezioni(Piscina, NomeC, Giorno, Ora) VALUES ('Lido', 'Acquagym', CURRENT_DATE + INTERVAL 2 WEEK, '20:30');
INSERT INTO Lezioni(Piscina, NomeC, Giorno, Ora) VALUES ('Lido', 'Acquagym', CURRENT_DATE + INTERVAL 3 WEEK, '20:30');
INSERT INTO Lezioni(Piscina, NomeC, Giorno, Ora) VALUES ('Lido', 'Acquagym', CURRENT_DATE + INTERVAL 4 WEEK, '20:30');
	
INSERT INTO Lezioni(Piscina, NomeC, Giorno, Ora) VALUES ('Lido', 'Corso di Salvamento', CURRENT_DATE, '17:30');
INSERT INTO Lezioni(Piscina, NomeC, Giorno, Ora) VALUES ('Lido', 'Corso di Salvamento', CURRENT_DATE + INTERVAL 1 WEEK, '17:30');
INSERT INTO Lezioni(Piscina, NomeC, Giorno, Ora) VALUES ('Lido', 'Corso di Salvamento', CURRENT_DATE + INTERVAL 2 WEEK, '17:30');
INSERT INTO Lezioni(Piscina, NomeC, Giorno, Ora) VALUES ('Lido', 'Corso di Salvamento', CURRENT_DATE + INTERVAL 3 WEEK, '17:30');
INSERT INTO Lezioni(Piscina, NomeC, Giorno, Ora) VALUES ('Lido', 'Corso di Salvamento', CURRENT_DATE + INTERVAL 4 WEEK, '17:30');


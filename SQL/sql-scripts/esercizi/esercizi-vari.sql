
-- esempi di codice sul database piscine_milano
USE piscine_milano;
SELECT p.Cognome, p.Nome
FROM persone p JOIN iscritti_corsi ic
ON p.CodiceFiscale = ic.Persona;

-- riportare i cognomi e i nomi delle persone maggiorenni iscritte ad almeno un corso delle piscine di Milano ;
SELECT p.Cognome, p.Nome, ic.DataNascita
FROM persone  p INNER JOIN iscritti_corsi ic
ON p.CodiceFiscale = ic.Persona
WHERE ((YEAR(CURDATE()) - YEAR(ic.DataNascita) ) - (RIGHT(CURDATE(),5) < RIGHT(ic.DataNascita,5)))>= 18;

SELECT p.Cognome, p.Nome, ic.DataNascita
FROM persone  p JOIN iscritti_corsi ic
ON p.CodiceFiscale = ic.Persona
WHERE ((YEAR(CURDATE()) - YEAR(ic.DataNascita) ) - (RIGHT(CURDATE(),5) < RIGHT(ic.DataNascita,5)))>= 18;

SELECT l.*, c.MinP 'N. min di iscritti', c.MaxP 'N. max di iscritti'
FROM lezioni l NATURAL JOIN corsi c;

SELECT l.*, c.MinP 'N. min di iscritti', c.MaxP 'N. max di iscritti'
FROM lezioni l JOIN corsi c ON l.Piscina=c.Piscina AND l.NomeC=c.NomeC;

SELECT i.*, q.QualificaIn
FROM insegnanti i INNER JOIN qualifiche q ON i.CodiceFiscale = q.Insegnante;

SELECT DISTINCT a.Codice, a.Denominazione
FROM aziende a
JOIN stages st ON a.Codice = st.Azienda
WHERE st.DataInizio BETWEEN '2024-06-01' AND '2024-08-31';

SELECT s.*, p.PuntiCasa, s2.Nome, s2.Citta, p.PuntiTrasferta
FROM partite p, squadre s, squadre s2
WHERE
    p.SquadraCasa = s.Nome
    AND p.SquadraTrasferta = s2.Nome
    AND p.Data = '2024-09-05';


SELECT s.*, p.PuntiCasa, s2.Nome, s2.Citta, p.PuntiTrasferta
  FROM 
  partite p 
  JOIN 
  squadre s 
    ON p.SquadraCasa = s.Nome
  JOIN squadre s2 
    ON  p.SquadraTrasferta = s2.Nome 
  WHERE p.Data = '2024-09-05';

use travel_agency;
-- Ricercare le prenotazioni fatte dai clienti con data antecedente alla scadenza dell’offerta

-- dove sono i dati richiesti?
-- prima versione 

SELECT p.Id 'Id prenotazione', p.DataPrenotazione, c.Id, c.Nome, c.Cognome, o.Id 'Id offerta', o.DataScadenza, v.Destinazione 'Viaggio'
FROM 
prenotazioni p 
JOIN 
offerte o 
ON p.fkOfferta = o.Id 
JOIN cliente c 
ON c.Id = p.fkCliente
JOIN 
viaggio v 
ON v.Id = o.fkViaggio
WHERE p.DataPrenotazione < o.DataScadenza;

SELECT
    c.Nome, 
    c.Cognome, 
    v.Destinazione, 
    p.DataPrenotazione, 
    o.DataScadenza
FROM 
prenotazioni p 
JOIN 
offerte o 
ON p.fkOfferta = o.Id 
JOIN cliente c 
ON c.Id = p.fkCliente
JOIN 
viaggio v 
ON v.Id = o.fkViaggio
WHERE p.DataPrenotazione < o.DataScadenza;

-- Trovare i viaggi che hanno prenotazioni (senza riportare il numero di prenotazioni)
-- occorre fare la JOIN tra viaggio, offerte, prenotazioni 
-- e prendere solo i viaggi
SELECT DISTINCT v.* , (
    SELECT COUNT(*)
    FROM prenotazioni p
        JOIN offerte o ON p.fkOfferta = o.Id
    WHERE
        o.fkViaggio = v.Id
) AS NumeroPrenotazioni
FROM  viaggio v 
JOIN offerte o 
ON o.fkViaggio = v.Id 
JOIN prenotazioni p 
ON p.fkOfferta = o.Id;

/* ALTER TABLE cliente RENAME TO clienti;
ALTER TABLE viaggio RENAME TO viaggi;
ALTER TABLE agenzia RENAME TO agenzie; */

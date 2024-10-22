
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

# questo è un commento
-- questo è un commento
/*
commento multilinea
commento multilinea
*/
-- si veda anche https://www.mysqltutorial.org/mysql-basics/mysql-comment/ 

-- creazione del database;
-- In questo caso si utilizzano le impostazioni di CHARACTER e COLLATE di default
CREATE DATABASE IF NOT EXISTS dbscuola;

# Se volessimo specificare COLLATE e COLLATE potremmo scrivere un'istruzione come la seguente (per MariaDB):
CREATE DATABASE IF NOT EXISTS dbscuola 
CHARACTER SET 'utf8mb4' 
COLLATE 'uca1400_ai_ci'; 
# questa è la default per MariaDb 10.11 e superiori

# Nel caso di MySQL
CREATE DATABASE IF NOT EXISTS dbscuola 
CHARACTER SET 'utf8mb4' 
COLLATE 'utf8mb4_0900_ai_ci';

# Per approfondimenti:  https://www.coderedcorp.com/blog/guide-to-mysql-charsets-collations/ 

-- uso il database dbscuola;
USE dbscuola; 

/* 
creazione delle tabelle;

*/


CREATE TABLE IF NOT EXISTS studenti (
  matricola mediumint unsigned ZEROFILL NOT NULL  auto_increment,
  cognome varchar(30) NOT NULL,
  nome varchar(30) NOT NULL,
  data_nascita date NOT NULL,
  genere enum('M','F') NOT NULL,
  nazione varchar(30) NOT NULL default 'Italia',
  e_mail varchar(50),
  PRIMARY KEY  (matricola),
  UNIQUE KEY cognomenomeDataN (cognome,nome, data_nascita)
) ENGINE=InnoDB; 

#popolo il database
INSERT INTO studenti (cognome, nome, data_nascita, genere, nazione, e_mail) VALUES
						( 'Verdi', 'Alessandro', '1988-02-24','M', 'argentina', 'you@ymail.com');

INSERT INTO studenti (cognome, nome, data_nascita, genere, nazione, e_mail) VALUES
						( 'Rossi', 'Alberto', '1990-03-12', 'M', 'Argentina',NULL);
						
INSERT INTO studenti (cognome, nome, data_nascita, genere, nazione, e_mail) VALUES						
							('Bianchi', 'Chiara', '1970-10-07', 'F', default, 'chiarab@ymail.com');
							
INSERT INTO studenti (matricola, cognome, nome, data_nascita, genere, nazione, e_mail) VALUES						
							(231, 'Alberti', 'Simone', '1987-03-23', 'M', default, 'a.simone@libero.it');
						
INSERT INTO studenti (matricola, cognome, nome, data_nascita, genere, nazione, e_mail) VALUES							
						(232, 'Dell\'Acqua', 'Mattia','1988-03-23',default, default,default);

INSERT INTO studenti (cognome, nome, data_nascita, genere, nazione, e_mail) VALUES
						( 'Falco', 'Alessandro', '1978-02-24','M', 'venezuela', 'me@ymail.com');

INSERT INTO studenti (matricola, cognome, nome, data_nascita, genere, nazione, e_mail) VALUES
						(123, 'Giovanni', 'Rossi', '1979-02-24','M', 'Costa Rica', 'medo@ymail.com');

SELECT * FROM studenti;

-- La seguente query non funziona, perché?
INSERT INTO studenti ( cognome, nome, data_nascita, genere, nazione, e_mail) VALUES
    -> ('De Chirico', 'Giorgio','1888-07-10',default, default,default);
/* 
quale valore del genere è stato assegnato?
quale valore della mail è stato assegnato? 


Il prossimo esempio genera errore perché il genere non corrisponde a uno di quelli previsti dal tipo enum
*/

INSERT INTO studenti ( cognome, nome, data_nascita, genere, nazione, e_mail) VALUES							
						('De Chirico 2', 'Giorgio 2','1888-07-10','X', default,default);

/* 
Un server SQL può essere più o meno aderente agli standard dell'SQL e ha dei modi di funzionamento che si possono impostare:
 https://mariadb.com/kb/en/sql-mode/
 Nel caso di MariaDB dalla versione 10.2.4 la modalità operativa di default è:
 STRICT_TRANS_TABLES, ERROR_FOR_DIVISION_BY_ZERO , NO_AUTO_CREATE_USER, NO_ENGINE_SUBSTITUTION
*/

# proviamo a modificare la modalità operativa di MariaDB
# salvo la modalità corrente
SET @OLD__SESSION_SQL_MODE = (SELECT @@SESSION.sql_mode);
# rimuovo la modalità STRICT
SET SQL_MODE ='';

-- verifico la modalità operativa
SELECT @@SESSION.sql_mode;

# provo a fare qualcosa di non molto corretto:
INSERT INTO studenti (matricola, cognome, nome, data_nascita, genere, nazione, e_mail) VALUES
						(124, 'GiovanniX', 'RossiX', '1979-02-24','X', 'Costa Rica', 'medoX@ymail.com');

-- provo a inserire un utente 'strano'
INSERT INTO studenti (matricola, cognome, nome, data_nascita, genere, nazione, e_mail) VALUES							
						(234, 'Dell\'Acqua', 'Giorgio',default,default, default,default);
						
INSERT INTO studenti (matricola, cognome, nome, data_nascita, genere, nazione, e_mail) VALUES							
						(235, 'Dell\'Acqua', 'Giorgio','1980-12-01',default, default,default);
						-- > query OK
-- posso ritornare alla modalità di default con 
SET SQL_MODE = (SELECT @OLD__SESSION_SQL_MODE);

# provo a fare qualcosa di non molto corretto:
INSERT INTO studenti ( cognome, nome, data_nascita, genere, nazione, e_mail) VALUES
						( 'GiovanniX2', 'RossiX2', '1979-02-24','X', 'Costa Rica', 'medoX@ymail.com');

-- MORALE: E' SEMPRE CONSIGLIABILE utilizzare la modalità di dafault "SQL STRICT".

-- altri inserimenti

INSERT INTO studenti (cognome, nome, data_nascita, genere, nazione, e_mail) VALUES							
						('Dell\'Acqua', 'Antonella','1970-12-23',default, default,default); 
-- non inserisco la matricola
-- quale valore assume la matricola?

INSERT INTO studenti ( cognome, nome, data_nascita) VALUES							
						('De Benedictis', 'Mattia','1993-02-23'); 
-- non inserisco la matricola, il genere, la nazione, la e-mail
-- cosa è inserito nel DB?

SELECT * FROM studenti;		
						
-- eliminare righe da una tabella, con l'uso di LIMIT
-- https://www.mysqltutorial.org/mysql-basics/mysql-limit/ 
DELETE FROM studenti WHERE matricola = 233 LIMIT 1;

-- eliminare un gruppo di righe, in questo caso non ci sono studenti con matricola compresa nei limiti imposti
 DELETE FROM studenti 
	WHERE matricola >=50 AND matricola <= 60;

# esempio di modifica di uno studente
UPDATE studenti SET nazione='Argentina' 
WHERE matricola=232 LIMIT 1;


# esempio di eliminazione di un alunno
DELETE FROM studenti 
WHERE matricola=232 LIMIT 1;

-- INTEGRITA' REFERENZIALE
-- aggiungiamo al database scuola la seguente tabella che riporta le assenze fatte da uno studente
-- RITORNIAMO AL DATABASE scuola;

USE dbscuola;
#ATTENZIONE: ricordarsi che MySQL/MariaDB su Linux è Case Sensitive, su Windows è Case Insensitive di default
# http://dev.mysql.com/doc/refman/5.7/en/identifier-case-sensitivity.html 
# http://dba.stackexchange.com/questions/16198/mysql-case-sensitive-table-names-on-linux 

CREATE TABLE IF NOT EXISTS assenze (
ID MEDIUMINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
Studente MEDIUMINT unsigned ZEROFILL NOT NULL,
Tipo ENUM('AA','AG', 'RR','RG') DEFAULT 'AA',
Data DATE NOT NULL ,
FOREIGN KEY (Studente) REFERENCES studenti(matricola)
) ENGINE = InnoDB CHARSET = latin1;

-- per verificare la struttura di una tabella si possono usare diversi comandi SQL:
-- 1 SHOW CREATE TABLE: permette di vedere la struttura della tabella così come descritta nella CREATE TABLE
SHOW CREATE TABLE assenze \G
-- 2 DESCRIBE: permette di vedere la stuttura dei campi della tabella, con indicazioni parziali sulle chiavi
DESCRIBE assenze \G

-- 3 SHOW COLUMNS FROM: fornisce lo stesso risultato di DESCRIBE
SHOW COLUMNS FROM assenze \G
use dbscuola;
-- inseriamo qualche assenza nel database
INSERT INTO assenze (Studente, Tipo, Data) VALUES (123, 'AA', CURRENT_DATE);
INSERT INTO assenze (Studente, Tipo, Data) VALUES (234, 'AG', CURRENT_DATE);
INSERT INTO assenze (Studente, Tipo, Data) VALUES (123, 'AA', CURRENT_DATE - interval 3 day);
-- notare come in MySQl si possa effettuare la differenza tra una data e un intervallo,
--  ma non la differenze tra due date che va fatta con la DATEDIFF che vedremo...

-- domanda: cosa succede se proviamo a inserire?
INSERT INTO assenze (Studente, Tipo, Data) VALUES (500, 'AG', CURRENT_DATE);
-- >otteniamo un messaggio d'errore! perché? 
-- MySQl supporta l'integrità referenziale con tabelle di tipo InnoDB:
-- per ogni valore della colonna (o colonne) in comune nella parte a molti (tabella esterna: assenze) 
-- sia sempre presente un valore uguale nella parte a uno (tabella interna: Studenti). 

-- proviamo a eliminare lo studente con codice 123 e vediamo cosa succede
DELETE FROM studenti WHERE matricola = 123;
-- > otteniamo un messaggio d'errore! perché?
-- cosa succederebbe se potessimo eliminare uno studente di cui sono ancora memorizzate le assenze nel DB?
-- gurdare le slide a questo punto
-- modifichiamo la struttura della chiave esterna
-- prima rimuoviamo il vincolo di chiave esterna e poi lo ricreiamo nuovamente
-- per vedere la chiave esterna faccio una SHOW CREATE TABLE
SHOW CREATE TABLE assenze \G
ALTER TABLE assenze DROP FOREIGN KEY assenze_ibfk_1; -- rimuovo la chiave esterna precedente
-- ricostruisco la chiave esterna con la clausola CASCADE
ALTER TABLE assenze ADD CONSTRAINT assenze_fk1 
	FOREIGN KEY (Studente) REFERENCES studenti(matricola) 
	ON DELETE CASCADE 
	ON UPDATE CASCADE;
-- cosa succede se proviamo ora a eliminare lo studente con matricola 123?
DELETE FROM studenti WHERE matricola = 123;
-- > Query OK --> ho eliminato lo studente, e le assenze?
-- anche quelle eliminate!
-- cosa succede se cambiamo il numero di matricola allo studente con matricola 234 (Dell'Acqua Giorgio)?
-- prima vediamo nella tabella assenze il contenuto del record relativo all'assenza dello studente 234
-- facciamo il cambio di matricola
UPDATE studenti SET matricola = 255 
WHERE matricola = 234;

--vediamo il risultato
SELECT * FROM studenti;
SELECT * FROM assenze;
-- notiamo che la chiave esterna nella tabella assenze è stata modificata!

-- ************************************************	
-- DA QUI IN POI MATERIALE DI SUPPORTO: SLIDE SQL P3
-- *************************************************
-- modificare la struttura di una tabella
-- 12.1.7. ALTER TABLE Syntax
	-- aggiungere una colonna
	ALTER TABLE studenti 
		ADD COLUMN indirizzo VARCHAR(60) AFTER nazione;
		
	-- eliminare una colonna di una tabella
	ALTER TABLE studenti
		DROP COLUMN indirizzo;
		
	-- modificare un campo di una tabella (uso di CHANGE non standard)
	ALTER TABLE studenti
		CHANGE COLUMN nazione nazionalita varchar(30) NOT NULL default 'Italiana';
	-- domanda: il valore della nazionalità è cambiato nelle tuple di studenti?
	
	-- per modificare il valore di un campo di una tabella si usa l'istruzione UPDATE
	-- 12.2.11. UPDATE Syntax
	UPDATE studenti
		SET nazionalita = 'Cilena'
		WHERE nazionalita = 'Argentina';
		-- notare che la condizione con il nome 'Argentina' modifica anche la tupla con 'argentina' perché?
				-- > per rispondere andare a vedere le caratterisciche delle tabelle 
				-- 	 con il comando SHOW TABLE STATUS FROM dbscuola \G
				-- > Collation?
	-- verificare lo stato di una tabella 
	SHOW TABLE STATUS FROM dbscuola LIKE 'studenti' \G
	

-- come si cambia il tipo di engine di una tabella nel caso fosse necessario?
-- il tipo di engine si visualizza con il comando SHOW TABLE STATUS ...
SHOW TABLE STATUS FROM dbscuola LIKE 'studenti' \G
-- per cambiare engine si procede con l'istruzione 
ALTER TABLE studenti engine = MyISAM;
-- controllo l'engine 
SHOW TABLE STATUS FROM dbscuola LIKE 'studenti' \G
-- passo nuovamente a InnoDB
ALTER TABLE studenti engine = InnoDB;

-- per vedere quali engine sono supportarti dal nostro server basta digitare 
SHOW ENGINES;

-- Qual è l'engine di default in MySQL?
		-- > dipende dall'installazione 
		-- 		per versioni < 5.5.5 --> default = MyISAM
		--		per versioni > 5.5.5 --> default = InnoDB

-- INDICI SU TABELLE
	-- la tabella studenti presenta già degli indici?
	-- 	> PRIMARY KEY
	-- 	> UNIQUE KEY
	-- creiamo un indice generico per velocizzare la ricerca per cognome, utilizzando solo i primi 10 caratteri del cognome
	CREATE INDEX per_cognome ON studenti (cognome(10));
	-- come fare a visualizzare gli indici presenti su una tabella?
	SHOW INDEX FROM studenti \G
	-- cancelliamo l'indice creato
	DROP INDEX per_cognome ON studenti;
	
	-- possiamo creare indici di tipo UNIQUE (già visti) oppure un indice di tipo FULLTEXT
	-- gli indici di tipo FULLTEXT richiedono tabelle con engine = MyISAM (dall versione 5.6 di MySQL è supportato anche in tabelle Innodb)
	-- prendiamo l'esempio del manuale di MySQL: 11.9.1. Natural Language Full-Text Searches
	-- per la teoria e gli esempi sugli indici di tipo FULLTEXT si poò vedere i seguenti riferimenti:
	-- https://www.mysqltutorial.org/introduction-to-mysql-full-text-search.aspx
	-- https://www.mysqltutorial.org/activating-full-text-searching.aspx
	-- https://www.mysqltutorial.org/mysql-natural-language-search.aspx
	-- https://www.mysqltutorial.org/mysql-boolean-text-searches.aspx
	-- CAMBIAMO TEMPORANEAMENTE DATABASE: prendiamo il database test e creiamo due tabelle:
	-- articles e clienti
	-- cambio database:
	CREATE DATABASE IF NOT EXISTS test;
	USE test;
	CREATE TABLE IF NOT EXISTS clienti (
	codice MEDIUMINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
	denominazione VARCHAR(40) NOT NULL UNIQUE,
	telefono CHAR(15),
	cellulare CHAR(15),
	UNIQUE(telefono, cellulare)
	);
	
	CREATE TABLE IF NOT EXISTS articles (
		id INT UNSIGNED AUTO_INCREMENT NOT NULL PRIMARY KEY,
		title VARCHAR(200),
		body TEXT,
		FULLTEXT (title,body)
	) ENGINE=InnoDB;

INSERT INTO articles (title,body) VALUES
 ('MySQL Tutorial','DBMS stands for DataBase ...'),
 ('How To Use MySQL Well','After you went through a ...'),
 ('Optimizing MySQL','In this tutorial we will show ...'),
 ('1001 MySQL Tricks','1. Never run mysqld as root. 2. ...'),
 ('MySQL vs. YourSQL','In the following database comparison ...'),
 ('MySQL Security','When configured properly, MySQL ...');	
 
 INSERT INTO articles (title,body) VALUES 
 ('MySQL MySQL MySQL', 'DBMS stands for MySQL, MySQL,MySQL,MySQL ...');

 INSERT INTO articles (title,body) VALUES 
 ('MySQL is a database', 'DBMS stands for database management system ...');

 INSERT INTO articles (title,body) VALUES 
 ('Most important database', 'MySQL is very important database management system ...');
-- verifichiamo il contenuto della tabella articles
SELECT * FROM articles;
-- trovare gli articoli che hanno nel titolo o nel corpo del testo la parola database
SELECT * FROM articles
	WHERE MATCH(title, body) 
	AGAINST ('database' IN NATURAL LANGUAGE MODE) > 0;

SELECT * FROM articles
	WHERE title LIKE '%database%' OR body LIKE '%database%';
	
SELECT id, MATCH(title, body) AGAINST ('database' IN NATURAL LANGUAGE MODE) AS score 
FROM articles;


SELECT id, MATCH(title, body) 
	AGAINST ('database') AS score FROM articles;	
	
-- nella query precedente abbiamo usato la funzione MATCH(): 11.9. Full-Text Search Functions
-- attenzione: in linguaggio naturale bisogna mettere nella MATCH le stesse colonne inserite nell'indice FULLTEXT creato sulla tabella
-- se volessimo fare una ricerca solo con titolo o solo con body dovremmo creare indici appositi

-- la stesso risultato si poteva ottenere anche con una query del tipo seguente (più lenta)
SELECT * FROM articles
	WHERE title LIKE '%database%' OR body LIKE '%database%';
-- ma la vera differenza sta nel fatto che la funzione MATCH restituisce lo score vale a dire la rilevanza 
-- di una frase in un testo
SELECT id, MATCH(title, body) AGAINST ('database' IN NATURAL LANGUAGE MODE) AS punteggio
FROM articles;

SELECT id, MATCH(body) AGAINST ('database' IN NATURAL LANGUAGE MODE) AS punteggio
FROM articles;

SELECT id, body, MATCH(title, body) AGAINST ('database' IN NATURAL LANGUAGE MODE) AS punteggio
FROM articles;

SELECT id, body, MATCH(title, body) AGAINST ('database' IN NATURAL LANGUAGE MODE) AS punteggio
FROM articles
WHERE MATCH(title, body) AGAINST ('database' IN NATURAL LANGUAGE MODE) > 0;


-- si noti che il risultato è automaticamente ordinato in ordine di rilevanza decrescente 
-- dal più rilevante al meno rilevante.

-- cosa succede se proviamo a cercare la parola MySQL?
SELECT id, body, MATCH(title, body) AGAINST ('MySQL' IN NATURAL LANGUAGE MODE) AS punteggio
FROM articles
WHERE MATCH(title, body) AGAINST ('MySQL' IN NATURAL LANGUAGE MODE) > 0;

SELECT id, body, MATCH(title, body) AGAINST ('database,MySQL' IN NATURAL LANGUAGE MODE) AS punteggio
FROM articles
WHERE MATCH(title, body) AGAINST ('database,MySQL' IN NATURAL LANGUAGE MODE) > 0;
-- quando la ricerca è fatta in modalità NATURAL LANGUAGE le parole che sono presenti in più del 50% 
-- delle righe sono scartate!
-- infatti sono considerate irrilevanti nel documento 
--"they may have a low semantic value for the particular data set in which they occur"

-- inoltre in configurazione di default le parole con meno di 4 caratteri sono ignorate 
-- e le parole che appartengono a una lista 
-- di stop words sono ignorate (parole come 'alone', 'along', etc.. --> queste impostazioni possono essere modificate --> MANUALE
-- https://dev.mysql.com/doc/refman/8.0/en/fulltext-stopwords.html
-- https://dev.mysql.com/doc/refman/8.0/en/fulltext-natural-language.html 
-- http://dev.mysql.com/doc/refman/8.0/en/fulltext-fine-tuning.html

-- vediamo come ultimo esempio la ricerca IN BOOLEAN MODE
SELECT * FROM articles 
WHERE MATCH (title,body)
	AGAINST ('+MySQL -Database' IN BOOLEAN MODE) > 0;
-- cosa otteniamo?
-- tutte le righe dove è presente la parola 'MySQL' ma non è presente la parola 'Database'
/*
• + stands for AND
• - stands for NOT
• [no operator] implies OR
*/


SELECT id, title, body, MATCH (title,body) AGAINST ('+MySQL -Database' IN BOOLEAN MODE) AS Punteggio
FROM articles 
WHERE MATCH (title,body) AGAINST ('+MySQL -Database' IN BOOLEAN MODE) > 0;

-- quanto vale il punteggio?
-- proviamo a inserire la tupla con body ' MySQL e MySQL e ancora MySQL'
INSERT INTO articles (title,body) VALUES
 ('Crazy words','MySQL e MySQL e ancora MySQL');
 -- proviamo a rifare la query con la ricerca booleana di prima
 SELECT id, title, body, MATCH (title,body) AGAINST ('+MySQL -Database' IN BOOLEAN MODE) AS Punteggio
 FROM articles 
 WHERE MATCH (title,body) AGAINST ('+MySQL -Database' IN BOOLEAN MODE) > 0;

 -- il punteggio dell'ultimo articolo non cambia, infatti in questa modalità 
-- non è riportata la rilevanza, ma solo la presenza o meno
-- non si applica la soglia del 50%
-- la ricerca in modo booleano si può applicare anche se non è stato creato un indice FULLTEXT (più lentamente)
-- si applica comunque la lista di stop words.

SELECT cognome, nome, (YEAR(CURDATE()) - YEAR(data_nascita)) 
FROM studenti;

SELECT RIGHT(CURDATE(),5);

SELECT cognome, nome, RIGHT(CURDATE(),5) as data_corrente, RIGHT(data_nascita,5) as data_nascita, (RIGHT(CURDATE(),5)<RIGHT(data_nascita,5))
FROM studenti;

-- esercizio con le date: usiamo il database studenti
-- trovare l'età degli studenti
SELECT cognome, nome, (YEAR(CURDATE()) - YEAR(data_nascita)) - (RIGHT(CURDATE(),5)<RIGHT(data_nascita,5))  Eta
FROM studenti;

SELECT cognome, nome, (YEAR(CURDATE()) - YEAR(data_nascita)) - (RIGHT(CURDATE(),5)<RIGHT(data_nascita,5))  AS eta
FROM studenti
ORDER BY eta ASC;
SELECT cognome, nome, (YEAR(CURDATE()) - YEAR(data_nascita)) - (RIGHT(CURDATE(),5)<RIGHT(data_nascita,5))  AS eta
FROM studenti
ORDER BY eta ASC;

-- selezionare gli studenti con età superiore a un certo valore
SELECT cognome, nome, (YEAR(CURDATE()) - YEAR(data_nascita)) - (RIGHT(CURDATE(),5)<RIGHT(data_nascita,5))  AS eta
FROM studenti
WHERE (YEAR(CURDATE()) - YEAR(data_nascita)) - (RIGHT(CURDATE(),5)<RIGHT(data_nascita,5)) > 100;


SELECT cognome, nome
FROM studenti
WHERE (YEAR(CURDATE()) - YEAR(data_nascita)) - (RIGHT(CURDATE(),5)<RIGHT(data_nascita,5)) > 100;

-- ordinati per età
SELECT cognome, nome, (YEAR(CURDATE()) - YEAR(data_nascita)) - (RIGHT(CURDATE(),5)<RIGHT(data_nascita,5))  AS eta
FROM studenti
WHERE (YEAR(CURDATE()) - YEAR(data_nascita)) - (RIGHT(CURDATE(),5)<RIGHT(data_nascita,5)) > 20
ORDER BY eta DESC, cognome, nome;

SELECT DATEDIFF('2007-12-31 23:59:59','2007-12-30'); 

-- ************************************************	
-- DA QUI IN POI MATERIALE DI SUPPORTO: SLIDE SQL P4
-- *************************************************
-- GESTIONE DEI UTENTI E DEI PERMESSI IN UN DATABASE
-- vediamo gli utenti presenti sul DBMS
-- use mysql;
select * from mysql.user \G
-- visualizzo solo i parametri più significativi degli utenti;
SELECT User, Host, authentication_string FROM mysql.user;
-- creiamo un utente con privilegi limitati sul DBMS
CREATE USER 'dbscuola_user'@'localhost' IDENTIFIED BY 'lapassword';

-- creiamo un utente con permessi limitati sul database scuola
GRANT SELECT, INSERT, UPDATE ON dbscuola.* TO 'dbscuola_user'@'localhost';

ALTER USER 'dbscuola_user'@'localhost' 
WITH MAX_QUERIES_PER_HOUR 100 MAX_CONNECTIONS_PER_HOUR 10;


-- rivediamo la lista degli utenti con la relativa password
SELECT User, Host, authentication_string FROM mysql.user;

-- cosa notiamo a proposito della password?
-- si provi ora a vedere il risultato della select 
-- 11.13. Encryption and Compression Functions


-- NOTA: 5.4.2.3. Password Hashing in MySQL -->As of MySQL 4.1, the PASSWORD() function has been modified to produce a 41-byte hash value
-- come sono memorizzati i dati sensibili, come una password in un database?
-- MD5('stringa') --> 32 hex code che occupa 32 caratteri --> This is the “RSA Data Security, Inc. MD5 Message-Digest Algorithm.” 
SELECT md5('Lapassword');

-- SHA2('stringa', hash_length) --> the SHA-2 family of hash functions (SHA-224, SHA-256, SHA-384, and SHA-512)
-- lo spazio occupato dal risultato dipende dal parametro hash_length - ad esempio se hash_length=256 si ottiene un 
-- risultato di 256 bit rappresentato da 64 cifre ottali che possono essere memorizzate in una stringa di 64 caratteri
SELECT SHA2('Lapassword', 256); 
-- modifichiamo la password di 'dbscuola_user'@'localhost'
-- per le vecchie versioni di MySQL  v<5.7.6
UPDATE mysql.user SET Password=PASSWORD('pippo')
  WHERE User='dbscuola_user' AND Host='localhost';
FLUSH PRIVILEGES;
-- altro metodo
-- metodo preferito per cambiare la password di un account
-- mariaDB v >=10.2 e MySQL v>5.7.6
ALTER USER 'dbscuola_user'@'localhost' IDENTIFIED BY 'paperino'; 
FLUSH PRIVILEGES;
-- inizializzare la password di root https://dev.mysql.com/doc/refman/8.0/en/default-privileges.html

-- impostiamo l'account root con la password root
ALTER USER 'root'@'localhost' IDENTIFIED BY 'root';
ALTER USER 'root'@'127.0.0.1' IDENTIFIED BY 'root';
ALTER USER 'root'@'::1' IDENTIFIED BY 'root';
FLUSH PRIVILEGES;

-- resetting root password https://dev.mysql.com/doc/refman/8.0/en/resetting-permissions.html
-- change root password (if you know it and you want to change) https://dev.mysql.com/doc/refman/8.0/en/alter-user.html
-- altro metodo
-- metodo preferito per cambiare la password di un account
-- mariaDB v<10.2 e MySQL v<5.7.6
SET PASSWORD FOR 'dbscuola_user'@'localhost' = PASSWORD('paperino');
FLUSH PRIVILEGES;
-- per revocare tutti i diritti di un utente
REVOKE ALL PRIVILEGES, GRANT OPTION 
FROM 'dbscuola_user'@'localhost';
-- notare chde l'utente esiste ancora e può ancora connetteri al database, anche se non può fare praticamente nulla;
-- per eliminare del tutto un utente si può eseguire il comando:
DROP USER 'dbscuola_user'@'localhost';

-- il seguente esempio crea un utente al massimo livello di privilegio che può connettersi da qualsiasi dominio/indirizzo di rete
CREATE USER 'superman'@'%' IDENTIFIED BY 'Kryptonite';
GRANT ALL PRIVILEGES ON *.* TO 'superman'@'%'  WITH GRANT OPTION;
ALTER USER 'superman'@'%' WITH MAX_QUERIES_PER_HOUR 0 MAX_CONNECTIONS_PER_HOUR 0 MAX_UPDATES_PER_HOUR 0 MAX_USER_CONNECTIONS 0; 
-- se si vuole usare il plugin native_password per alcuni plugin che si connettono a MySQL
-- https://stackoverflow.com/questions/50373427/node-js-cant-authenticate-to-mysql-8-0?rq=1
-- https://mariadb.com/kb/en/alter-user/
-- https://mariadb.com/kb/en/authentication-plugins/
ALTER USER 'superman'@'%' IDENTIFIED WITH mysql_native_password BY 'Kryptonite'; -- MySQL
ALTER USER 'superman'@'%' IDENTIFIED VIA mysql_native_password USING PASSWORD("Kryptonite"); -- mariaDB
ALTER USER 'superman'@'%' IDENTIFIED  BY 'Kryptonite';
use dbscuola;
SELECT * FROM studenti;
select * from artisti;

SELECT
  table_name,
  table_type,
  engine
FROM
  information_schema.tables
WHERE
  table_schema = 'dbscuola'
ORDER BY
  table_name;

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

# Se volessimo specificare CHARACTER e COLLATE potremmo scrivere un'istruzione come la seguente (per MariaDB):
CREATE DATABASE IF NOT EXISTS dbscuola 
CHARACTER SET 'utf8mb4' 
COLLATE 'uca1400_ai_ci'; 
# questa è la default per MariaDb 10.11 e superiori

# Nel caso di MySQL
CREATE DATABASE IF NOT EXISTS dbscuola 
CHARACTER SET 'utf8mb4' 
COLLATE 'utf8mb4_0900_ai_ci';
# questa è la default da MySQL 8 in poi

# Per approfondimenti:  https://www.coderedcorp.com/blog/guide-to-mysql-charsets-collations/ 

-- selezioniamo il database dbscuola;
USE dbscuola; 

--
-- creazione delle tabelle;
--

CREATE TABLE IF NOT EXISTS studenti (
  Matricola MEDIUMINT UNSIGNED NOT NULL AUTO_INCREMENT,
  Cognome varchar(30) NOT NULL,
  Nome varchar(30) NOT NULL,
  DataNascita date NOT NULL,
  Genere enum('M','F') NOT NULL,
  Nazione varchar(30) NOT NULL default 'Italia',
  EMail varchar(50),
  PRIMARY KEY  (Matricola),
  UNIQUE KEY CognomeNomeDataN(Cognome,Nome, DataNascita)
) ENGINE=InnoDB; 

#popolo il database
INSERT INTO studenti (Cognome, Nome, DataNascita, Genere, Nazione, EMail) VALUES
						( 'Verdi', 'Alessandro', '2010-02-24','M', 'argentina', 'you@ymail.com');

INSERT INTO studenti (Cognome, Nome, DataNascita, Genere, Nazione, EMail) VALUES
						( 'Rossi', 'Alberto', '2011-03-12', 'M', 'Argentina',NULL);
						
INSERT INTO studenti (Cognome, Nome, DataNascita, Genere, Nazione, EMail) VALUES						
							('Bianchi', 'Chiara', '2009-10-07', 'F', default, 'chiarab@ymail.com');
							
INSERT INTO studenti (Matricola, Cognome, Nome, DataNascita, Genere, Nazione, EMail) VALUES						
							(231, 'Alberti', 'Simone', '2008-03-23', 'M', default, 'a.simone@libero.it');
						
INSERT INTO studenti (Matricola, Cognome, Nome, DataNascita, Genere, Nazione, EMail) VALUES							
						(232, 'Dell\'Acqua', 'Mattia','2000-03-23',default, default,default);

INSERT INTO studenti (Cognome, Nome, DataNascita, Genere, Nazione, EMail) VALUES
						( 'Falco', 'Alessandro', '2011-02-24','M', 'venezuela', 'me@ymail.com');

INSERT INTO studenti (Matricola, Cognome, Nome, DataNascita, Genere, Nazione, EMail) VALUES
						(123, 'Giovanni', 'Rossi', '2007-02-24','M', 'Costa Rica', 'medo@ymail.com');

SELECT * FROM studenti;

-- La seguente query non funziona, perché?
INSERT INTO studenti ( Cognome, Nome, DataNascita, Genere, Nazione, EMail) VALUES
    -> ('De Chirico', 'Giorgio','1888-07-10',default, default,default);
/* 
quale valore del Genere è stato assegnato?
quale valore della EMail è stato assegnato? 
Il prossimo esempio genera errore perché il Genere non corrisponde a uno di quelli previsti dal tipo enum
*/
INSERT INTO studenti ( Cognome, Nome, DataNascita, Genere, Nazione, EMail) VALUES							
						('De Chirico 2', 'Giorgio 2','1888-07-10','X', default,default);

/* 
Un server SQL può essere più o meno aderente agli standard dell'SQL e ha dei modi di funzionamento che si possono impostare:
 https://mariadb.com/kb/en/sql-mode/
 Nel caso di MariaDB dalla versione 10.2.4 la modalità operativa di default è:
 STRICT_TRANS_TABLES, ERROR_FOR_DIVISION_BY_ZERO , NO_AUTO_CREATE_USER, NO_ENGINE_SUBSTITUTION
*/

# proviamo a modificare la modalità operativa di MariaDB
# salviamo la modalità corrente
SET @OLD__SESSION_SQL_MODE = (SELECT @@SESSION.sql_mode);
# rimuoviamo la modalità STRICT
SET SQL_MODE ='';

-- verifichiamo la modalità operativa
SELECT @@SESSION.sql_mode;

# proviamo a fare qualcosa di non molto corretto:
INSERT INTO studenti (Matricola, Cognome, Nome, DataNascita, Genere, Nazione, EMail) VALUES
						(124, 'GiovanniX', 'RossiX', '2000-02-24','X', 'Costa Rica', 'medoX@ymail.com');

-- proviamo a inserire un utente 'strano'
INSERT INTO studenti (Matricola, Cognome, Nome, DataNascita, Genere, Nazione, EMail) VALUES							
						(234, 'Dell\'Acqua', 'Giorgio',default,default, default,default);
						
INSERT INTO studenti (Matricola, Cognome, Nome, DataNascita, Genere, Nazione, EMail) VALUES							
						(235, 'Dell\'Acqua', 'Giorgio','2001-12-01',default, default,default);
						-- > query OK
-- possiamo ritornare alla modalità di default con 
SET SQL_MODE = (SELECT @OLD__SESSION_SQL_MODE);

# proviamo a fare qualcosa di non molto corretto:
INSERT INTO studenti ( Cognome, Nome, DataNascita, Genere, Nazione, EMail) VALUES
						( 'GiovanniX2', 'RossiX2', '2002-02-24','X', 'Costa Rica', 'medoX@ymail.com');

-- MORALE: E' SEMPRE CONSIGLIABILE utilizzare la modalità di dafault "SQL STRICT".

-- altri inserimenti

INSERT INTO studenti (Cognome, Nome, DataNascita, Genere, Nazione, EMail) VALUES							
						('Dell\'Acqua', 'Antonella','2010-12-23',default, default,default); 
-- non inseriamo la Matricola
-- quale valore assume la Matricola?

INSERT INTO studenti ( Cognome, Nome, DataNascita) VALUES							
						('De Benedictis', 'Mattia','2011-02-23'); 
-- non inseriamo la Matricola, il Genere, la Nazione, la EMail
-- cosa è inserito nel DB?

SELECT * FROM studenti;		
						
-- eliminare righe da una tabella, con l'uso di LIMIT
-- https://www.mysqltutorial.org/mysql-basics/mysql-limit/ 
DELETE FROM studenti WHERE Matricola = 233 LIMIT 1;

-- eliminare un gruppo di righe, in questo caso non ci sono studenti con Matricola compresa nei limiti imposti
 DELETE FROM studenti 
	WHERE Matricola >=50 AND Matricola <= 60;

# esempio di modifica di uno studente
UPDATE studenti SET Nazione='Argentina'
WHERE Matricola=232 LIMIT 1;


# esempio di eliminazione di un alunno
DELETE FROM studenti 
WHERE Matricola=234 LIMIT 1;

-- INTEGRITA' REFERENZIALE
-- aggiungiamo al database dbscuola la seguente tabella che riporta le assenze fatte da uno studente

USE dbscuola;
#ATTENZIONE: ricordarsi che MySQL/MariaDB su Linux è Case Sensitive, su Windows è Case Insensitive di default
# http://dev.mysql.com/doc/refman/5.7/en/identifier-case-sensitivity.html 
# http://dba.stackexchange.com/questions/16198/mysql-case-sensitive-table-names-on-linux 

CREATE TABLE IF NOT EXISTS assenze (
	Id MEDIUMINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
	Studente MEDIUMINT UNSIGNED NOT NULL,
	Tipo ENUM('AA','AG', 'RR','RG') DEFAULT 'AA',
	Data DATE NOT NULL ,
	FOREIGN KEY (Studente) REFERENCES studenti(Matricola)
) ENGINE = InnoDB;

-- nel caso in cui la chiave esterna sia composta da più campi, bisogna specificare tutti i campi collegati
-- supponendo di creare la chiave esterna con Nome e Cognome e che quindi la chiave primaria di studenti sia 
-- composta da Nome e Cognome, avremmo:

/* CREATE TABLE IF NOT EXISTS assenze (
    Id MEDIUMINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    CognomeStudente VARCHAR(30) NOT NULL,
	NomeStudente VARCHAR(30) NOT NULL,
    Tipo ENUM('AA', 'AG', 'RR', 'RG') DEFAULT 'AA',
    Data DATE NOT NULL,
    FOREIGN KEY (CognomeStudente,NomeStudente) REFERENCES studenti (Cognome,Nome)
) ENGINE = InnoDB; */

-- per verificare la struttura di una tabella si possono usare diversi comandi SQL:
-- 1 SHOW CREATE TABLE: permette di vedere la struttura della tabella così come descritta nella CREATE TABLE
SHOW CREATE TABLE assenze \G
-- 2 DESCRIBE: permette di vedere la stuttura dei campi della tabella, con indicazioni parziali sulle chiavi
DESCRIBE assenze \G

-- 3 SHOW COLUMNS FROM: fornisce lo stesso risultato di DESCRIBE
SHOW COLUMNS FROM assenze \G
USE dbscuola;
-- inseriamo qualche assenza nel database
INSERT INTO assenze (Studente, Tipo, Data) VALUES (123, 'AA', CURRENT_DATE);
INSERT INTO assenze (Studente, Tipo, Data) VALUES (235, 'AG', CURRENT_DATE);
INSERT INTO assenze (Studente, Tipo, Data) VALUES (123, 'AA', CURRENT_DATE - interval 3 day);
-- notare come in MySQL si possa effettuare la differenza tra una data e un intervallo,
--  ma non la differenze tra due date che va fatta con la DATEDIFF che vedremo...

-- domanda: cosa succede se provassimo a fare il seguente inserimento?
INSERT INTO assenze (Studente, Tipo, Data) VALUES (500, 'AG', CURRENT_DATE);
--> otterremmo un messaggio d'errore! perché? 
-- MySQL e mariaDB supportano l'integrità referenziale con tabelle di tipo InnoDB:
-- per ogni valore della colonna (o colonne) in comune nella parte a molti (tabella esterna: assenze) 
-- sia sempre presente un valore uguale nella parte a uno (tabella interna: Studenti). 

-- proviamo a eliminare lo studente con codice 123 e vediamo cosa succede
DELETE FROM studenti WHERE Matricola = 123;
-- > otteniamo un messaggio d'errore! perché?
-- cosa succederebbe se potessimo eliminare uno studente di cui sono ancora memorizzate le assenze nel DB?

-- modifichiamo la struttura della chiave esterna
-- prima rimuoviamo il vincolo di chiave esterna e poi lo ricreiamo nuovamente
-- per vedere la chiave esterna faccio una SHOW CREATE TABLE
SHOW CREATE TABLE assenze \G
-- rimuovo la chiave esterna precedente
ALTER TABLE assenze DROP FOREIGN KEY assenze_ibfk_1;
-- ricostruiamo la chiave esterna con la clausola CASCADE
ALTER TABLE assenze ADD CONSTRAINT assenze_fk1 
	FOREIGN KEY (Studente) REFERENCES studenti(Matricola) 
	ON DELETE CASCADE 
	ON UPDATE CASCADE;
-- cosa succederebbe se provassimo ora a eliminare lo studente con Matricola 123?
DELETE FROM studenti WHERE Matricola = 123;
-- > Query OK --> abbiamo eliminato lo studente, e le assenze?
-- anche quelle sono state eliminate!
-- cosa succederebbe se cambiassimo il numero di Matricola allo studente con Matricola 234 (Dell'Acqua Giorgio)?
-- prima vediamo nella tabella assenze il contenuto del record relativo all'assenza dello studente 234
-- facciamo il cambio di Matricola
UPDATE studenti SET Matricola = 255 
WHERE Matricola = 235;

--vediamo il risultato
SELECT * FROM studenti;
SELECT * FROM assenze;
-- notiamo che la chiave esterna nella tabella assenze è stata modificata!

-- modificare la struttura di una tabella ALTER TABLE
	-- aggiungere una colonna
	ALTER TABLE studenti 
		ADD COLUMN indirizzo VARCHAR(60) AFTER Nazione;
		
	-- eliminare una colonna di una tabella
	ALTER TABLE studenti
		DROP COLUMN indirizzo;
		
	-- modificare un campo di una tabella (uso di CHANGE non standard)
	ALTER TABLE studenti
		CHANGE COLUMN Nazione nazionalita varchar(30) NOT NULL default 'Italiana';
	-- domanda: il valore della nazionalità è cambiato nelle tuple di studenti?
	
	-- per modificare il valore di un campo di una tabella si usa l'istruzione UPDATE
		UPDATE studenti
		SET nazionalita = 'Cilena'
		WHERE nazionalita = 'Argentina';
		-- notare che la condizione con il Nome 'Argentina' modifica anche la tupla con 'argentina' perché?
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

-- INDICI SU TABELLE
	-- la tabella studenti presenta già degli indici?
	-- 	> PRIMARY KEY
	-- 	> UNIQUE KEY
	-- creiamo un indice generico per velocizzare la ricerca per Cognome, utilizzando solo i primi 10 caratteri del Cognome
	CREATE INDEX perCognome ON studenti (Cognome(10));
	-- come fare a visualizzare gli indici presenti su una tabella?
	SHOW INDEX FROM studenti \G
	-- cancelliamo l'indice creato
	DROP INDEX perCognome ON studenti;
	
SELECT Cognome, Nome, (YEAR(CURDATE()) - YEAR(DataNascita)) 
	FROM studenti;

SELECT RIGHT(CURDATE(),5);

SELECT Cognome, Nome, RIGHT(CURDATE(),5) as data_corrente, RIGHT(DataNascita,5) as DataNascita, (RIGHT(CURDATE(),5)<RIGHT(DataNascita,5))
	FROM studenti;

-- trovare l'età degli studenti
SELECT Cognome, Nome, (YEAR(CURDATE()) - YEAR(DataNascita)) - (RIGHT(CURDATE(),5)<RIGHT(DataNascita,5))  Eta
FROM studenti;

SELECT Cognome, Nome, (YEAR(CURDATE()) - YEAR(DataNascita)) - (RIGHT(CURDATE(),5)<RIGHT(DataNascita,5))  AS Eta
FROM studenti
ORDER BY Eta;

SELECT Cognome, Nome, (YEAR(CURDATE()) - YEAR(DataNascita)) - (RIGHT(CURDATE(),5)<RIGHT(DataNascita,5))  AS Eta
FROM studenti
ORDER BY Eta DESC;

-- selezionare gli studenti con età superiore a un certo valore
SELECT Cognome, Nome, (YEAR(CURDATE()) - YEAR(DataNascita)) - (RIGHT(CURDATE(),5)<RIGHT(DataNascita,5))  AS Eta
FROM studenti
WHERE (YEAR(CURDATE()) - YEAR(DataNascita)) - (RIGHT(CURDATE(),5)<RIGHT(DataNascita,5)) > 22;


SELECT Cognome, Nome
FROM studenti
WHERE (YEAR(CURDATE()) - YEAR(DataNascita)) - (RIGHT(CURDATE(),5)<RIGHT(DataNascita,5)) > 22;

-- ordinati per età
SELECT Cognome, Nome, (YEAR(CURDATE()) - YEAR(DataNascita)) - (RIGHT(CURDATE(),5)<RIGHT(DataNascita,5))  AS Eta
FROM studenti
WHERE (YEAR(CURDATE()) - YEAR(DataNascita)) - (RIGHT(CURDATE(),5)<RIGHT(DataNascita,5)) > 15
ORDER BY Eta DESC, Cognome, Nome;

SELECT DATEDIFF('2007-12-31 23:59:59','2007-12-30'); 


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

-- Osservazione importante --
--
-- Se si utilizza un container Docker per far eseguire il server di MySQL o di MariaDB l'account dbscuola_user@localhost
-- non funzionerà per connessioni che non avvengano direttamente dall'interno del container. In altri termini,
-- per connettersi al DBMS Server si dovrebbe effettuare una connessione direttamente dal container con un comando del tipo:
-- docker exec -it nome-server /bin/bash e poi dall'interno del container eseguire il comando:
-- mariadb -u dbscuola_user -h localhost -p 
-- oppure
-- mariadb -u dbscuola_user -p
-- nel caso di MySQL si dovrebbe usare:
-- mysql -u dbscuola_user -h localhost -p
-- oppure 
-- mysql -u dbscuola_user -p

-- Per creare un account che si possa connettere al server di MariaDB o di MySQL dall'esterno del container si può procedere come segue:
-- Opzione 1) si crea un account che si possa connettere da qualunque host (come accade alla configurazione di default di root nel
-- container di MariaDB o di MySQL)
-- Opzione 2) si crea un account che si possa connettere da un host specifico di cui si conosce l'indirizzo IP, oppure il nome di 
-- dominio DNS.

-- Opzione 1: creiamo l'utente per il database dbscuola in modo che si possa connettere da qualunque host:
CREATE USER 'dbscuola_user_omni'@'%' IDENTIFIED BY 'lapassword';
GRANT SELECT, INSERT, UPDATE ON dbscuola.* TO 'dbscuola_user_omni'@'%';
FLUSH PRIVILEGES;

-- Dopo aver creato l'utente dbscuola_user_omni sarà possibile connettersi dal docker host con il comando (WSL Ubuntu)
-- mariadb -u dbscuola_user_omni -h 127.0.0.1 -p  

-- Opzione 2: creiamo un utente che si possa connettere solo da un host specifico:
-- In questo caso, siccome utilizziamo una rete di tipo bridge, la connessione dal Docker host verso i container del Docker engine
-- avviene attraverso il NAT/Firewall di Docker: i pacchetti IP che dal nostro host (WSL Ubuntu) vengono spediti verso i container
-- subiscono un'operazione di NAT (Network Address Translation) ad opera del bridge. Infatti, in una rete di tipo **bridge** in Docker,
--  ogni container ottiene un indirizzo IP all'interno di quella rete privata. 
-- Quando un pacchetto arriva al container da una macchina esterna al **bridge** (ad esempio dall' host o da un'altra macchina 
-- sulla rete esterna), Docker usa il **Network Address Translation (NAT)** per tradurre l'indirizzo IP di origine.

-- Quindi, nel caso in cui ci si connetta dall'esterno (ad esempio dall'host) al container che esegue MySQL, il server MySQL 
-- vedrà l'indirizzo IP dell'**host Docker** (di solito l'indirizzo del gateway della rete bridge) come indirizzo sorgente
--  dei pacchetti, non l'indirizzo della macchina esterna.
--
-- per ottenere l'indirizzo ip del Gateway della rete a cui sono connessi i container è possibile usare il comando docker inspect in 
-- combinazione con il comando jp (nella WSL Ubuntu)
-- supponendo che la bridge network di Docker si chiami my-net, è possibile ottenere l'indirizzo ip del Gateway con il comando:
--
-- gateway_ip=$(docker network inspect my-net | jq -r '.[0].IPAM.Config[0].Gateway')
-- echo $gateway_ip
-- 172.18.0.1
--
-- con l'indirizzo ip del Gateway è possibile creare l'account di mysql/mariadb come segue:


-- Creiamo l'utente dbscuola_user_docker_internal come segue:
CREATE USER 'dbscuola_user_gateway'@'172.18.0.1' IDENTIFIED BY 'lapassword';
GRANT SELECT, INSERT, UPDATE ON dbscuola.* TO 'dbscuola_user_gateway' @'172.18.0.1';
FLUSH PRIVILEGES;

-- con l'untente appena creato è possibile connettersi dal Docker host (WSL Ubuntu) con il comando:
-- mariadb -u dbscuola_user_gateway -h 127.0.0.1 -p 

-- altre impostazioni che si possono configurare su un account
-- l'istruzione seguente imposta i limiti sul numero di query e sul numero di connessioni per ora
-- questa opzione non sempre è utilizzata. In questo caso è riportata per mostrare come fanno i fornitori di
-- servizio a limitare l'accesso alle risorse di un DBMS
ALTER USER 'dbscuola_user'@'localhost' 
WITH MAX_QUERIES_PER_HOUR 100 MAX_CONNECTIONS_PER_HOUR 10;


-- rivediamo la lista degli utenti con la relativa password
SELECT User, Host, authentication_string FROM mysql.user;

-- cosa notiamo a proposito della password?
-- si provi ora a vedere il risultato della select 

-- come sono memorizzati i dati sensibili, come una password in un database?
-- MD5('stringa') --> 32 hex code che occupa 32 caratteri --> This is the “RSA Data Security, Inc. MD5 Message-Digest Algorithm.” 
SELECT md5('lapassword');

-- SHA2('stringa', hash_length) --> the SHA-2 family of hash functions (SHA-224, SHA-256, SHA-384, and SHA-512)
-- lo spazio occupato dal risultato dipende dal parametro hash_length 
-- ad esempio se hash_length=256 si ottiene un risultato di 256 bit rappresentato da 64 cifre ottali,
--  che possono essere memorizzate in una stringa di 64 caratteri
SELECT SHA2('lapassword', 256); 
-- la password generata con SHA2 non coincide con quella creata dal MySQL/MariaDB perché l'algoritmo di hashing usato dal DBMS Server 
-- utilizza anche un salt per generare l'hash

-- modifichiamo la password di 'dbscuola_user'@'localhost'

-- metodo per cambiare la password di un account
-- per mariaDB version >=10.2 e MySQL version >5.7.6
ALTER USER 'dbscuola_user'@'localhost' IDENTIFIED BY 'paperino'; 
FLUSH PRIVILEGES;
-- inizializzare la password di root https://dev.mysql.com/doc/refman/8.0/en/default-privileges.html

-- impostiamo l'account root con la password root
ALTER USER 'root'@'localhost' IDENTIFIED BY 'root';
ALTER USER 'root'@'127.0.0.1' IDENTIFIED BY 'root';
ALTER USER 'root'@'::1' IDENTIFIED BY 'root';
FLUSH PRIVILEGES;

-- resetting root password https://dev.mysql.com/doc/refman/9.0/en/resetting-permissions.html
-- change root password (if you know it and you want to change) https://dev.mysql.com/doc/refman/9.0/en/alter-user.html

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

-- uso del plugin native_password
-- https://mariadb.com/kb/en/authentication-plugin-mysql_native_password/
-- Nel caso di MariaDB "The easiest way to create a user account with the mysql_native_password authentication plugin is to make sure that old_passwords=0 is set, 
-- and then create a user account via CREATE USER that does not specify an authentication plugin, but does specify a password 
-- via the IDENTIFIED BY clause. For example:"

-- https://mariadb.com/kb/en/authentication-plugin-mysql_native_password/#creating-users

 SET old_passwords = 0;
 CREATE USER username @hostname IDENTIFIED BY 'mariadb';
-- If SQL_MODE does not have NO_AUTO_CREATE_USER set, then you can also create the user account via GRANT. For example:
-- https://mariadb.com/kb/en/sql-mode/ 
SET old_passwords = 0;
GRANT SELECT ON db.* TO username @hostname IDENTIFIED BY 'mariadb';
-- se si vuole usare il plugin native_password per alcuni client che si connettono a MySQL
-- https://stackoverflow.com/questions/50373427/node-js-cant-authenticate-to-mysql-8-0?rq=1
-- https://mariadb.com/kb/en/alter-user/
-- https://mariadb.com/kb/en/authentication-plugins/

ALTER USER 'superman'@'%' IDENTIFIED WITH mysql_native_password BY 'Kryptonite'; -- MySQL
ALTER USER 'superman'@'%' IDENTIFIED VIA mysql_native_password USING PASSWORD("Kryptonite"); -- mariaDB
ALTER USER 'superman'@'%' IDENTIFIED  BY 'Kryptonite';


-- un esempio di utilizzo del database information_schema
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
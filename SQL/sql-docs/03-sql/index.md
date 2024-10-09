# SQL in MySQL/MariaDB - Parte 3

## Sommario

- Vincoli di integrità
- Primary key
- Unique
- Creazioni di indici
- Tipi di tabelle (storage engine)
- Creazione di indici
- Manipolazione di date e tempo
  
## Vincoli d'integrità

I vincoli d'integrità sono di tre tipi:

1. Integrità sulle colonne :arrow_right: NOT NULL, DEFAULT, CHECK[^1]
2. Integrità sulle tabelle :arrow_right: PRIMARY KEY, UNIQUE, CHECK
3. Integrità referenziale (tra le colonne in comune delle tabelle in relazione)  :arrow_right: FOREIGN KEY, REFERENCES

## Vincoli d'integrità sulle tabelle: chiavi

Un insieme di attributi è detto chiave candidata o superchiave per una tabella (relazione) quando permette di individuare univocamente ogni tupla di una tabella

- a valori distinti della chiave corrispondono tuple distinte
- le colonne di una superchiave possono essere nulli (NULL)

### Chiave primaria

Un insieme di attributi è detto chiave primaria quando è chiave candidata ed inoltre ha il numero minimo di attributi

- Nessuno degli attributi di una chiave primaria può assumere il valore NULL

> :memo: Una chiave primaria non può avere valori nulli nei suoi attributi, per cui il NOT NULL qui non serve, anche se non è sbagliato

La chiave primaria può essere composta da un solo attributo, oppure da più attributi.

- Nel caso di chiave primaria composta da un solo attributo la sintassi per definire la chiave primaria è:

    ```sql
    CREATE TABLE table_name(
    column1 datatype PRIMARY KEY,
    column2 datatype, 
    ...
    );
    ```

- Se la chiave primaria è composta da più attributi,la sintassi per dichiarare la chiave primaria è:
  
  ```sql
  CREATE TABLE table_name(
   column1 datatype,
   column2 datatype,
   column3 datatype,
   ...,
   PRIMARY KEY(column1, column2)
    );
    ```

### Superchiave - UNIQUE Constraint

In una tabella possono esistere una o più colonne, i cui valori devono essere necessariamente distinti, e che non formano una chiave primaria.

SQL prevede il vincolo UNIQUE

- Se la superchiave è composta da più attributi (colonne) il vincolo `UNIQUE` viene espresso secondo la sintassi:
  
  ```sql
  CREATE TABLE table_name(
   ...
   column1 datatype,
   column2 datatype,
   ...,
   UNIQUE(column1, column2)
   );
  ```

  - Ad esempio il `Nome`, il `Cognome` e la `DataNascita` potrebbero essere una superchiave per la tabella `studenti` se ipotizziamo che non possano esistere due studenti con lo stesso nome, cognome e data di nascita ( è un vincolo)

      ```sql
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
      ```

- Se la superchiave è semplice, è possibile specificare il vincolo UNIQUE come vincolo di colonna

    ```sql
    CREATE TABLE table_name(
        ...,
        column1 datatype UNIQUE,
        ...
    );
    ```

### Tipi di tabelle - Storage ENGINE

Il tipo di tabella definisce il modo in cui i dati sono organizzati e memorizzati all’interno del database fisico (su file). MySQL e MariaDB permettono di specificare per ciascuna tabella il tipo di memorizzazione tramite l’opzione ENGINE, oppure tramite il suo sinonimo TYPE.

Ad esempio:

```sql
CREATE TABLE table_name(
    column_list
) ENGINE = engine_name;
```

MySQL e MariaDB supportano diversi tipi di engine per le tabelle. Per ottenere l'elenco degli engine supportati dal server si può eseguire la query:

```sql
SELECT 
  engine, 
  support 
FROM 
  information_schema.engines 
ORDER BY 
  engine;
```

Nella stragrande maggioranza dei casi l'`ENGINE` che dovrà essere utilizzato è `InnoDB`, che è anche quello supportato di default.

Come vedremo nelle sezioni successive, InnoDB ha un pieno  supporto alle transazioni ACID e ai vincoli di integrità referenziale.

Per vedere il tipo di `ENGINE` e altre informazioni delle singole tabelle di un database è possibile utilizzare l'istruzione SQL:

```sql
SHOW TABLE STATUS FROM database_name \G
```

## Indici su tabelle

Si definisce **indice** un archivio che consente un accesso più  rapido alle informazioni contenute in una tabella, mediante tecniche di ottimizzazione dell’accesso.

Un caso nel quale potrebbe essere utile avere un indice potrebbe essere, ad esempio, quello in cui bisogna fare molte ricerche di tuple in base al cognome e al nome. In questo caso conviene definire un indice su questi attributi in modo tale che la ricerca avvenga più velocemente.

**Come funziona un indice?**

- Una tabella con indice è sostanzialmente struttura dati di supporto (ad esempio B-tree, oppure hash table) che rende l'accesso ai dati più veloce.

**Qual è il prezzo da pagare per l’incremento di prestazione durante la ricerca di una tupla?**

- Quando si effettua un inserimento, una cancellazione o una modifica di una tupla di una tabella su cui è definito un indice il DBMS deve aggiornare automaticamente la struttura dati di supporto all'indice associato ai dati.

### Creazione di indici su tabelle

- Una `PRIMARY KEY` crea automaticamente un indice associato alla chiave - primaria
  
- Una `UNIQUE KEY`  crea automaticamente un indice associato alla - superchiave

- Quando si crea una tabella è possibile specificare un indice con la clausola `INDEX`. Ad esempio, il codice SQL seguente crea un indice sui campi `c2` e `c3`:

    ```sql
    CREATE TABLE t(
    c1 INT PRIMARY KEY,
    c2 INT NOT NULL,
    c3 INT NOT NULL,
    c4 VARCHAR(10),
    INDEX (c2,c3) 
    );
    ```

- Per creare un indice su una tabella già esistente è possibile usare la sintassi:

  ```sql
  CREATE INDEX index_name 
  ON table_name (column_list)
  ```
  
  Più in generale, la sintassi prevede la possibilità di creare indici di diverso tipo:

  ```sql
    CREATE [UNIQUE | FULLTEXT | SPATIAL] INDEX index_name
        [index_type]
        ON tbl_name (key_part,...)
        [index_option]
        [algorithm_option | lock_option] ...

    key_part: {col_name [(length)] | (expr)} [ASC | DESC]

    index_option: {
        KEY_BLOCK_SIZE [=] value
    | index_type
    | WITH PARSER parser_name
    | COMMENT 'string'
    | {VISIBLE | INVISIBLE}
    | ENGINE_ATTRIBUTE [=] 'string'
    | SECONDARY_ENGINE_ATTRIBUTE [=] 'string'
    }

    index_type:
        USING {BTREE | HASH}

    algorithm_option:
        ALGORITHM [=] {DEFAULT | INPLACE | COPY}

    lock_option:
        LOCK [=] {DEFAULT | NONE | SHARED | EXCLUSIVE}
  ```

- Supponendo di avere una tabella `clienti` e di voler creare un indice sui numeri di telefono, si potrebbe creare l'indice con l'istruzione SQL seguente:
  
  ```sql
  -- creiamo un indice sulle prime 10 cifre del campo telefono
  CREATE INDEX ricercaPerTelefono ON clienti (Telefono(10));
  ```

> :memo: **Nota**: in MySQL/MariaDB gli indici sono, per impostazione predefinita, ASC, ossia ascendenti

- Per visualizzare gli indici su una tabella è possibile utilizzare il comando `DESCRIBE table_name`, oppure il comando `SHOW INDEX FROM table_name`

### Eliminazione di un indice da una tabella

Per eliminare un indice su una tabella si può utilizzare l'istruzione:

```sql
DROP INDEX index_name ON table_name;
```

## Il tempo e le date

I tipi di dato temporali sono già stati descritti nella [sezione `date e tempo` della parte 1](../01-sql/index.md#date-e-tempo). In questa sezione esaminiamo più in dettaglio alcuni aspetti di questi tipi e le operazioni che è possibile effettuare su di essi.

### The DATETIME, DATE, and TIMESTAMP Types

I tipi `DATETIME`, `DATE`, and `TIMESTAMP` sono collegati tra loro e ben descritti nelle [pagine della documentazione di MySQL](https://dev.mysql.com/doc/refman/9.0/en/datetime.html).

Il tipo `TIME` è utilizzato per rappresentare il tempo ed è descritto nelle [pagine della documentazione di MySQL](https://dev.mysql.com/doc/refman/9.0/en/time.html)

#### Funzioni di MYSQL/MariaDB sul tempo e le date

WEEKDAY(date) 
Returns the weekday index for date (0 = Monday, 1 = Tuesday, … 6 = Sunday). 
mysql> SELECT WEEKDAY('2008-02-03 22:23:00'); -> 6 
mysql> SELECT WEEKDAY('2007-11-06'); -> 1 
WEEKOFYEAR(date) 
Returns the calendar week of the date as a number in the range from 1 to 53. WEEKOFYEAR() is a compatibility function that is equivalent to WEEK(date,3). 
mysql> SELECT WEEKOFYEAR('2008-02-20'); -> 8 
YEAR(date) 
Returns the year for date, in the range 1000 to 9999, or 0 for the “zero” date. 
mysql> SELECT YEAR('1987-01-01'); -> 1987 
YEARWEEK(date), YEARWEEK(date,mode) 
Returns year and week for a date. The mode argument works exactly like the mode argument to WEEK(). The year in the result may be different from the year in the date argument for the first and the last week of the year. 
mysql> SELECT YEARWEEK('1987-01-01'); -> 198653 

MONTH(date) 
Returns the month for date, in the range 1 to 12 for January to December, or 0 for dates such as '0000-00-00' or '2008-00-00' that have a zero month part. 
mysql> SELECT MONTH('2008-02-03'); -> 2 
MONTHNAME(date) 
Returns the full name of the month for date. As of MySQL 5.0.25, the language used for the name is controlled by the value of the lc_time_names system variable (Section 9.8, “MySQL Server Locale Support”). 
mysql> SELECT MONTHNAME('2008-02-03'); -> 'February' 
NOW() 
Returns the current date and time as a value in 'YYYY-MM-DD HH:MM:SS' or YYYYMMDDHHMMSS.uuuuuu format, depending on whether the function is used in a string or numeric context. The value is expressed in the current time zone. 
mysql> SELECT NOW(); -> '2007-12-15 23:50:26' 
mysql> SELECT NOW() + 0; -> 20071215235026.000000 
NOW() returns a constant time that indicates the time at which the statement began to execute

Esempi
mysql> SELECT name, birth, CURDATE(), 
 (YEAR(CURDATE())-YEAR(birth)) 
 - (RIGHT(CURDATE(),5)<RIGHT(birth,5)) 
 AS age  FROM pet ORDER BY age; 

Nota: 
RIGHT(str,len) 
Returns the rightmost len characters from the string str, or NULL if any argument is NULL. 
mysql> SELECT RIGHT('foobarbar', 4); -> 'rbar' 
This function is multi-byte safe. 

#### Elaborazione sulle date

DATEDIFF(expr1,expr2) 
DATEDIFF() returns expr1 – expr2 expressed as a value in days from one date to the other. expr1 and expr2 are date or date-and-time expressions. Only the date parts of the values are used in the calculation. 
mysql> SELECT DATEDIFF('2007-12-31 23:59:59','2007-12-30'); -> 1 mysql> SELECT DATEDIFF('2010-11-30 23:59:59','2010-12-31'); -> -31 
DATE_ADD(date,INTERVAL expr unit), DATE_SUB(date,INTERVAL expr unit) 
These functions perform date arithmetic. The date argument specifies the starting date or datetime value. expr is an expression specifying the interval value to be added or subtracted from the starting date. expr is a string; it may start with a “-” for negative intervals. unit is a keyword indicating the units in which the expression should be interpreted. 
The INTERVAL keyword and the unit specifier are not case sensitive. 
Date arithmetic also can be performed using INTERVAL together with the + or - operator:  
date + INTERVAL expr unit 
date - INTERVAL expr unit

#### Unità di misura del tempo

[^1]: I vincoli d’integrità sulle colonne sono stati analizzati parti precedenti sull'SQL.
Esempi d’uso della clausola `CHECK`:

    ```sql
    CHECK (Studente LIKE ‘S-____’) oppure
    CHECK (Giorno BETWEEN 1 AND 31)
    ```

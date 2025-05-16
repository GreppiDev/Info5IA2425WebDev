# Algebra relazionale

- [Algebra relazionale](#algebra-relazionale)
  - [Operatori dell'Algebra Relazionale](#operatori-dellalgebra-relazionale)
    - [Operatori Unari](#operatori-unari)
    - [Operatori Binari (basati sulla teoria degli insiemi)](#operatori-binari-basati-sulla-teoria-degli-insiemi)
    - [Operatori Binari (Specifici del Modello Relazionale)](#operatori-binari-specifici-del-modello-relazionale)
    - [Operatore Derivato (o Ausiliario)](#operatore-derivato-o-ausiliario)
  - [Confronto tra Algebra Relazionale e SQL](#confronto-tra-algebra-relazionale-e-sql)
  - [Esercizi Finali](#esercizi-finali)
    - [Soluzione degli esercizi](#soluzione-degli-esercizi)
    - [Esercizio 1: Espressioni dell'Algebra Relazionale](#esercizio-1-espressioni-dellalgebra-relazionale)
    - [Esercizio 2: Traduzione da SQL ad Algebra Relazionale](#esercizio-2-traduzione-da-sql-ad-algebra-relazionale)
    - [Esercizio 3: Riflessione](#esercizio-3-riflessione)

L'algebra relazionale rappresenta un linguaggio formale, di natura procedurale, utilizzato per interrogare i database relazionali. Essa fornisce un insieme di operazioni che agiscono su relazioni (tabelle) per produrre nuove relazioni. Comprendere l'algebra relazionale è fondamentale, poiché costituisce la base teorica su cui si fonda il linguaggio SQL (Structured Query Language) e aiuta a comprendere come i sistemi di gestione di database (DBMS) elaborano e ottimizzano le interrogazioni. Dato che il modello relazionale (relazioni intese come sottoinsiemi di prodotti cartesiani di n domini) e l'SQL (specificamente con MySQL/MariaDB) sono già noti, questa spiegazione si concentrerà sul collegare questi concetti.

Nell'algebra relazionale, ogni operazione prende una o due relazioni come input e produce una nuova relazione come output. Questa proprietà, nota come **chiusura**, permette di annidare le operazioni, costruendo espressioni complesse per recuperare i dati desiderati.

## Operatori dell'Algebra Relazionale

Gli operatori dell'algebra relazionale si possono classificare in diverse categorie.

### Operatori Unari

Questi operatori agiscono su una singola relazione.

1. **Selezione (**σ**)**

    - **Definizione e Scopo**: L'operatore di selezione restituisce un sottoinsieme delle tuple (righe) di una relazione che soddisfano una determinata condizione (predicato). La selezione opera orizzontalmente sulla tabella.

    - **Sintassi**: σ<sub>predicato</sub>​(R)

        - R è la relazione di input.

        - predicato è una condizione booleana sugli attributi di R. I predicati possono includere operatori di confronto (=,!=,<,≤,>,≥) e operatori logici (AND,OR,NOT).

    - **Esempio**: Data la relazione **Studenti**:

        Studenti

        | Matricola | Nome  | Cognome | AnnoNascita | Città  |
        |-----------|-------|---------|-------------|--------|
        | 101       | Mario | Rossi   | 2004        | Roma   |
        | 102       | Luigi | Verdi   | 2003        | Milano |
        | 103       | Anna  | Bianchi | 2004        | Roma   |
        | 104       | Sara  | Neri    | 2005        | Napoli |

        Per selezionare gli studenti nati nel 2004: σ<sub>AnnoNascita=2004​</sub>(Studenti)

        Il risultato sarà:

        | Matricola | Nome  | Cognome | AnnoNascita | Città |
        |-----------|-------|---------|-------------|-------|
        | 101       | Mario | Rossi   | 2004        | Roma  |
        | 103       | Anna  | Bianchi | 2004        | Roma  |

    - **Corrispondenza SQL**: La clausola `WHERE` di SQL.

        ```sql
        SELECT *
        FROM Studenti
        WHERE AnnoNascita = 2004;
        ```

2. **Proiezione (**π**)**

    - **Definizione e Scopo**: L'operatore di proiezione seleziona determinati attributi (colonne) di una relazione, eliminando le altre. La proiezione opera verticalmente sulla tabella e, nella sua definizione formale basata sulla teoria degli insiemi, elimina eventuali tuple duplicate risultanti.

    - **Sintassi**: π<sub>A1​,A2​,...,Ak​</sub>​(R)

        - R è la relazione di input.

        - A1​,A2​,...,Ak​ è la lista degli attributi da mantenere.

    - Esempio: Dalla relazione Studenti, per visualizzare solo Nome e Città: π<sub>Nome,Città</sub>​(Studenti)

        Il risultato sarà:

        | Nome  | Città  |
        |-------|--------|
        | Mario | Roma   |
        | Luigi | Milano |
        | Anna  | Roma   |
        | Sara  | Napoli |

        (Si noti che se ci fossero state tuple come (Mario, Roma) e (Mario, Roma), solo una sarebbe apparsa nel risultato formale dell'algebra relazionale. SQL, per default, non elimina i duplicati con SELECT a meno che non si usi DISTINCT).

    - **Corrispondenza SQL**: La lista degli attributi nella clausola `SELECT`. Per ottenere l'eliminazione dei duplicati come nell'algebra relazionale pura, si usa `SELECT DISTINCT`.

        ```sql
        SELECT DISTINCT Nome, Città
        FROM Studenti;
        ```

        Senza `DISTINCT`, SQL restituirebbe tutte le coppie (Nome, Città) presenti, inclusi i duplicati:

        ```sql
        SELECT Nome, Città
        FROM Studenti;
        ```

3. **Ridenominazione (**ρ**)**

    - **Definizione e Scopo**: L'operatore di ridenominazione permette di cambiare il nome di una relazione e/o dei suoi attributi. È utile per evitare ambiguità quando si combinano più relazioni (ad esempio, in un prodotto cartesiano o join dove le relazioni di input hanno attributi con lo stesso nome) o per rendere più leggibili i risultati intermedi o finali.

    - **Sintassi**:

        - Per ridenominare una relazione: ρ<sub>S</sub>​(R) (la relazione R viene ridenominata S).

        - Per ridenominare gli attributi di una relazione: ρ<sub>B1​,B2​,...,Bk</sub>​​(R) (gli attributi di R vengono ridenominati B1​,...,Bk​ nell'ordine in cui appaiono).

        - Per ridenominare sia la relazione che i suoi attributi: ρ<sub>S(B1​,B2​,...,Bk​)</sub>​(R).

    - Esempio: Data la relazione Studenti, si vuole ridenominare l'attributo AnnoNascita in AnnoDiNascita e la relazione in AnagraficaStudenti.

        Consideriamo prima la proiezione per selezionare solo alcuni attributi e poi ridenominarli:

        R<sub>temp</sub>​←π<sub>Matricola,Nome,Cognome,AnnoNascita,Città</sub>​(Studenti)

        ρ<sub>AnagraficaStudenti(Mat,Nom,Cogn,AnnoDiNascita,Residenza)</sub>​(Rtemp​)

        Il risultato dell'operazione di proiezione e ridenominazione sarebbe una relazione chiamata AnagraficaStudenti con gli attributi Mat, Nom, Cogn, AnnoDiNascita, Residenza.

        L'operazione complessiva è:

        ρ<sub>AnagraficaStudenti(Mat,Nom,Cogn,AnnoDiNascita,Residenza)</sub>
        (π<sub>Matricola,Nome,Cognome,AnnoNascita,Città</sub>(Studenti))

    - **Corrispondenza SQL**: La clausola `AS` per rinominare tabelle (alias di tabella) o colonne (alias di colonna).

        ```sql
        SELECT
            Matricola AS Mat,
            Nome AS Nom,
            Cognome AS Cogn,
            AnnoNascita AS AnnoDiNascita,
            Città AS Residenza
        FROM Studenti AS AnagraficaStudenti; -- L'alias per la tabella si usa nella clausola FROM
        ```

### Operatori Binari (basati sulla teoria degli insiemi)

Questi operatori derivano dalla teoria degli insiemi e richiedono che le due relazioni su cui operano siano **compatibili per l'unione** (o union-compatibili). Due relazioni R e S sono compatibili per l'unione se:

1. Hanno lo stesso numero di attributi (stessa arità).[^1]

2. I domini degli attributi corrispondenti (cioè, il primo attributo di R con il primo di S, il secondo con il secondo, e così via) sono gli stessi o compatibili. I nomi degli attributi non devono necessariamente essere identici, anche se spesso lo sono per chiarezza.

3. **Unione (**∪**)**

    - **Definizione e Scopo**: L'unione di due relazioni R e S compatibili per l'unione è una relazione contenente tutte le tuple che sono in R, o in S, o in entrambe, eliminando i duplicati (come per le operazioni insiemistiche).

    - **Sintassi**: R∪S

    - Esempio: Siano due relazioni che elencano studenti di diverse città, con la stessa struttura (Matricola, Nome, Cognome).

        StudentiDaRoma (ipotetica, risultato di π<sub>Matricola,Nome,Cognome</sub>​(σ<sub>Città='Roma'</sub>​(Studenti)))

        | Matricola | Nome  | Cognome |
        |-----------|-------|---------|
        | 101       | Mario | Rossi   |
        | 103       | Anna  | Bianchi |

        StudentiDaMilano (ipotetica, risultato di π<sub>Matricola,Nome,Cognome</sub>​(σ<sub>Città='Milano'</sub>​(Studenti)))

        | Matricola | Nome  | Cognome |
        |-----------|-------|---------|
        | 102       | Luigi | Verdi   |

        StudentiDaRoma∪StudentiDaMilano produrrebbe:

        | Matricola | Nome  | Cognome |
        |-----------|-------|---------|
        | 101       | Mario | Rossi   |
        | 103       | Anna  | Bianchi |
        | 102       | Luigi | Verdi   |

    - **Corrispondenza SQL**: `UNION`. L'operatore `UNION` di SQL elimina automaticamente i duplicati. Per mantenere i duplicati, SQL fornisce `UNION ALL`.

        ```sql
        SELECT Matricola, Nome, Cognome FROM Studenti WHERE Città = 'Roma'
        UNION
        SELECT Matricola, Nome, Cognome FROM Studenti WHERE Città = 'Milano';
        ```

4. **Intersezione (**∩**)**

    - **Definizione e Scopo**: L'intersezione di due relazioni R e S compatibili per l'unione è una relazione contenente solo le tuple che sono presenti sia in R sia in S.

    - **Sintassi**: R∩S

    - Esempio: Si considerino due relazioni che elencano gli ID degli studenti iscritti a due corsi diversi.

        IscrittiCorsoA (es. π<sub>Matricola</sub>​(σ<sub>IDCorso='C001'</sub>​(Iscrizioni)))

        | Matricola |
        |-----------|
        | 101       |
        | 102       |

        IscrittiCorsoB (es. π<sub>Matricola</sub>​(σ<sub>IDCorso='C002'</sub>​(Iscrizioni)))

        | Matricola |
        |-----------|
        | 101       |
        | 103       |

        IscrittiCorsoA∩IscrittiCorsoB (studenti iscritti ad entrambi i corsi):

        | Matricola |
        |-----------|
        | 101       |

    - **Corrispondenza SQL**: `INTERSECT`. MariaDB supporta `INTERSECT` a partire dalla versione 10.3.0.

        ```sql
        SELECT Matricola FROM Iscrizioni WHERE IDCorso = 'C001'
        INTERSECT
        SELECT Matricola FROM Iscrizioni WHERE IDCorso = 'C002';
        ```

        Se INTERSECT non fosse supportato o per compatibilità con versioni precedenti, si potrebbe emulare usando IN o EXISTS:

        ```sql
        SELECT Matricola FROM Iscrizioni I1 WHERE IDCorso = 'C001' AND I1.Matricola IN (SELECT Matricola FROM Iscrizioni I2 WHERE I2.IDCorso = 'C002');
        ```

5. **Differenza (**- **o `\`)**

    - **Definizione e Scopo**: La differenza tra due relazioni R e S compatibili per l'unione (R-S) è una relazione contenente tutte le tuple che sono in R ma non sono in S.

    - **Sintassi**: R-S

    - Esempio: Usando le relazioni IscrittiCorsoA e IscrittiCorsoB dell'esempio precedente:

        IscrittiCorsoA-IscrittiCorsoB (studenti iscritti al Corso A ma non al Corso B):

        | Matricola |
        |-----------|
        | 102       |

    - **Corrispondenza SQL**: `EXCEPT` (standard SQL, supportato da MariaDB dalla versione 10.3.0). Alcuni DBMS usano `MINUS` (es. Oracle).

        ```sql
        SELECT Matricola FROM Iscrizioni WHERE IDCorso = 'C001'
        EXCEPT
        SELECT Matricola FROM Iscrizioni WHERE IDCorso = 'C002';
        ```

        Se EXCEPT non fosse supportato, si potrebbe emulare usando NOT IN o NOT EXISTS:

        ```sql
        SELECT Matricola FROM Iscrizioni I1 WHERE IDCorso = 'C001' AND I1.Matricola NOT IN (SELECT Matricola FROM Iscrizioni I2 WHERE I2.IDCorso = 'C002');
        ```

        :memo: :warning: Attenzione con `NOT IN` se la subquery può restituire NULL. `NOT EXISTS` è generalmente più sicuro[^2]

        ```sql
        SELECT Matricola FROM Iscrizioni I1 WHERE IDCorso = 'C001' AND NOT EXISTS (SELECT 1 FROM Iscrizioni I2 WHERE I2.IDCorso = 'C002' AND I2.Matricola = I1.Matricola);
        ```

### Operatori Binari (Specifici del Modello Relazionale)

1. **Prodotto Cartesiano (**×**)**

    - **Definizione e Scopo**: Il prodotto cartesiano di due relazioni R e S è una relazione che contiene tutte le possibili combinazioni di tuple, dove ogni tupla è formata concatenando una tupla di R con una tupla di S. Se R ha n attributi e S ha m attributi, R×S avrà n+m attributi. Se R ha ∣R∣ tuple e S ha ∣S∣ tuple, R×S avrà ∣R∣⋅∣S∣ tuple. Se R e S hanno attributi con lo stesso nome, è necessario ridenominarli per evitare ambiguità (es. usando ρ o qualificando i nomi come R.NomeAttributo).

    - **Sintassi**: R×S

    - Esempio: Sia StudentiRidotta (π<sub>Nome</sub>​(Studenti)) e CorsiRidotta (π<sub>NomeCorso​</sub>(Corsi)).

        StudentiRidotta

        | Nome  |
        |-------|
        | Mario |
        | Luigi |

        CorsiRidotta

        | NomeCorso      |
        |----------------|
        | Basi di Dati   |
        | Programmazione |

        StudentiRidotta×CorsiRidotta produrrebbe:

        | StudentiRidotta.Nome | CorsiRidotta.NomeCorso |
        |----------------------|------------------------|
        | Mario                | Basi di Dati           |
        | Mario                | Programmazione         |
        | Luigi                | Basi di Dati           |
        | Luigi                | Programmazione         |

        (*I nomi degli attributi sono stati qualificati per chiarezza. Se gli attributi originali avessero nomi diversi, la qualifica non sarebbe strettamente necessaria nel risultato, ma è buona pratica per tracciare l'origine*).

    - **Corrispondenza SQL**: Specificando più tabelle nella clausola `FROM` separate da virgola (sintassi più datata, ma ancora supportata) o usando `CROSS JOIN` (sintassi SQL standard più moderna). Generalmente, il prodotto cartesiano da solo non è molto utile e viene seguito da una selezione per filtrare le combinazioni significative (formando un join).

        ```sql
        SELECT StudentiRidotta.Nome, CorsiRidotta.NomeCorso
        FROM StudentiRidotta, CorsiRidotta; -- Sintassi datata
        -- Oppure
        SELECT SR.Nome, CR.NomeCorso -- Uso di alias per brevità
        FROM StudentiRidotta AS SR
        CROSS JOIN CorsiRidotta AS CR; -- Sintassi moderna
        ```

2. Join

    Il join è una delle operazioni più importanti e comuni. Combina tuple da due relazioni basandosi su una condizione relativa ai loro attributi. È concettualmente un prodotto cartesiano seguito da una selezione, ma **i DBMS lo implementano con algoritmi più efficienti**.

    - **Theta Join (**R⋈<sub>θ</sub>​S**)**

        - **Definizione e Scopo**: Il Theta Join (o θ-join) combina tuple da due relazioni R e S che soddisfano una condizione P (predicato θ). Il predicato può essere qualsiasi condizione booleana che coinvolge attributi di R e S, usando operatori di confronto (=,!=,<,≤,>,≥).

        - **Sintassi**: R⋈<sub>P</sub>​S (equivale a σ<sub>P</sub>​(R×S))

        - **Esempio**: Trovare gli studenti e gli esami da loro sostenuti con voto maggiore di 28. Relazioni: **Studenti** e **Esami** Studenti⋈<sub>Studenti.Matricola=Esami.IDStudente AND Esami.Voto>28</sub>​Esami (Assumendo che `Esami.IDStudente` sia la chiave esterna che referenzia `Studenti.Matricola`) Risultato (parziale, mostrando le colonne rilevanti e alcune tuple):
  
        | Matricola | Nome  | Cognome | ... | IDStudente | IDCorso | Data       | Voto |
        |-----------|-------|---------|-----|------------|---------|------------|------|
        | 101       | Mario | Rossi   | ... | 101        | C02     | 2024-07-20 | 30   |
        | 103       | Anna  | Bianchi | ... | 103        | C03     | 2024-09-10 | 29   |

- **Corrispondenza SQL**: Utilizzando `JOIN ... ON condizione` oppure specificando le tabelle nella clausola `FROM` e la condizione di join nella clausola `WHERE`.SQL

    ```sql
    SELECT *
    FROM Studenti
    JOIN Esami ON Studenti.Matricola = Esami.IDStudente AND Esami.Voto > 28;
    -- Oppure (vecchia sintassi)
    SELECT *
    FROM Studenti, Esami
    WHERE Studenti.Matricola = Esami.IDStudente AND Esami.Voto > 28;
    ```

    - **Equi-Join**

        - **Definizione e Scopo**: È un tipo di Theta Join dove il predicato P contiene solo congiunzioni (AND) di uguaglianze (=) tra attributi.

        - Esempio: Associare studenti e iscrizioni sulla base della matricola.

            Studenti⋈Studenti.Matricola=Iscrizioni.MatricolaStud​Iscrizioni

            Il risultato conterrà le colonne di entrambe le tabelle, incluse le due colonne di join (es. Studenti.Matricola e Iscrizioni.MatricolaStud) che avranno valori identici per ogni tupla risultante.

        - **Corrispondenza SQL**: Come il Theta Join, ma la condizione usa solo uguaglianze.

            ```sql
            SELECT *
            FROM Studenti S
            JOIN Iscrizioni I ON S.Matricola = I.MatricolaStud;

            ```

    - **Join Naturale (**R⋈S**)**

        - **Definizione e Scopo**: Il Natural Join è un Equi-Join che opera implicitamente su tutti gli attributi che hanno lo stesso nome nelle due relazioni. Le colonne di join appaiono una sola volta nel risultato.

        - **Sintassi**: R⋈S

        - **Prerequisiti**: Le due relazioni devono avere uno o più attributi con lo stesso nome e dominio compatibile.

        - Esempio: Se la colonna MatricolaStud in Iscrizioni si chiamasse semplicemente Matricola (come in Studenti).

            Studenti

            | Matricola | Nome  | AnnoNascita |
            |-----------|-------|-------------|
            | 101       | Mario | 2004        |
            | 102       | Luigi | 2003        |

            IscrizioniConMatricola (ipotetica, dove Iscrizioni.MatricolaStud è rinominato Matricola)

            | Matricola | IDCorso | AnnoIscrizione |
            |-----------|---------|----------------|
            | 101       | C001    | 2023           |
            | 101       | C002    | 2023           |
            | 103       | C001    | 2024           |

            Studenti⋈IscrizioniConMatricola produrrebbe (solo per Matricola 101, poiché 102 non ha iscrizioni e 103 non è in Studenti):

            | Matricola | Nome  | AnnoNascita | IDCorso | AnnoIscrizione |
            |-----------|-------|-------------|---------|----------------|
            | 101       | Mario | 2004        | C001    | 2023           |
            | 101       | Mario | 2004        | C002    | 2023           |

            (La colonna Matricola appare una sola volta).

        - **Corrispondenza SQL**: `NATURAL JOIN`.

            ```sql
            -- Assumendo che Iscrizioni abbia una colonna Matricola invece di MatricolaStud
            SELECT *
            FROM Studenti
            NATURAL JOIN IscrizioniConMatricola;
            ```

            Il Natural Join può essere conveniente ma anche rischioso se le tabelle hanno attributi con lo stesso nome per coincidenza ma con significato diverso, portando a join errati o inattesi. È spesso preferibile usare `JOIN ... USING(colonna_comune)` o `JOIN ... ON` per specificare esplicitamente le colonne di join.

    - **Outer Join (Left, Right, Full)**

        - **Definizione e Scopo**: Gli Outer Join sono estensioni del join (tipicamente dell'equi-join o natural join) che includono anche le tuple di una o entrambe le relazioni che non trovano corrispondenza nell'altra relazione, riempiendo gli attributi mancanti della relazione non corrispondente con valori `NULL`.

            - **Left Outer Join (**R⋈<sub>Lθ</sub> ​S **o** R ←θ​ S**)**: Mantiene tutte le tuple di R (la relazione a sinistra). Se una tupla di R non ha corrispondenze in S (secondo la condizione θ), viene inclusa nel risultato con gli attributi provenienti da S impostati a `NULL`.

            - **Right Outer Join (**R ⋈<sub>Rθ</sub>​ S** o** R →θ​ S **)**: Mantiene tutte le tuple di S (la relazione a destra). Se una tupla di S non ha corrispondenze in R, viene inclusa con gli attributi provenienti da R impostati a `NULL`.

            - **Full Outer Join (**R ⋈<sub>Fθ</sub>​ S** o** R ↔θ​ S **)**: Mantiene tutte le tuple di entrambe le relazioni. Le tuple senza corrispondenza da una parte o dall'altra vengono estese con `NULL` per gli attributi dell'altra relazione.

        - **Esempio (Left Outer Join)**: Trovare tutti gli studenti e gli esami da loro sostenuti, includendo gli studenti che non hanno ancora sostenuto esami.
          - Studenti ⋈<sub>L(Studenti.Matricola=Esami.IDStudente)</sub>​ Esami
          - Se lo studente con `Matricola = 105`, "Pippo Pluto", non ha esami:
  
            | Matricola | Nome  | Cognome | ... | IDStudente | IDCorso | Data       | Voto |
            |-----------|-------|---------|-----|------------|---------|------------|------|
            | 101       | Mario | Rossi   | ... | 101        | C01     | 2024-06-15 | 28   |
            | ...     | ... | ...   | ... | ...      | ...   | ...      | ... |
            | 105       | Pippo | Pluto   | ... | NULL       | NULL    | NULL       | NULL |

- **Corrispondenza SQL**: `LEFT OUTER JOIN`, `RIGHT OUTER JOIN`, `FULL OUTER JOIN`.SQL

    ```sql
    SELECT *
    FROM Studenti
    LEFT OUTER JOIN Esami ON Studenti.Matricola = Esami.IDStudente;
    ```

    MariaDB supporta `LEFT JOIN`, `RIGHT JOIN` e `FULL OUTER JOIN` (quest'ultimo a partire da versioni più recenti, es. 10.2.1 per la sintassi completa, ma l'emulazione era possibile prima).

### Operatore Derivato (o Ausiliario)

1. **Divisione (**÷**)**

    - **Definizione e Scopo**: L'operatore di divisione è utile per query che contengono la frase "per tutti" o "ognuno". Date due relazioni R<sub>(A1​,...,An​,B1​,...,Bm​)</sub> e S<sub>(B1​,...,Bm​)</sub> (dove gli attributi Bj​ sono quelli su cui si divide, e S non deve essere vuota), R÷S restituisce un sottoinsieme degli attributi Ai​ di R. Precisamente, R÷S restituisce le tuple (a1​,...,an​) tali che per *ogni* tupla (b1​,...,bm​) presente in S, la tupla concatenata (a1​,...,an​,b1​,...,bm​) sia presente in R.

    - **Sintassi**: R÷S

    - Esempio: Trovare gli studenti che si sono iscritti a tutti i corsi presenti in una data lista di corsi obbligatori.

        Sia StudentiIscrizioni (π<sub>Matricola,IDCorso​</sub>(IscrizioniConMatricola)):

        | Matricola | IDCorso |
        |-----------|---------|
        | 101       | C001    |
        | 101       | C002    |
        | 102       | C001    |
        | 103       | C002    |
        | 103       | C003    |
        | 101       | C003    |

        Sia CorsiObbligatori (π<sub>IDCorso</sub>​(σ<sub>Tipo='Obbligatorio'</sub>​(Corsi))):

        | IDCorso |
        |---------|
        | C001    |
        | C002    |

        StudentiIscrizioni÷CorsiObbligatori restituirebbe:

        | Matricola |
        |-----------|
        | 101       |

        (Perché lo studente 101 si è iscritto sia a C001 che a C002. Lo studente 102 si è iscritto solo a C001. Lo studente 103 si è iscritto a C002 ma non a C001).

    - **Corrispondenza SQL**: Non esiste un operatore di divisione diretto in SQL. Si implementa tipicamente usando una doppia negazione con NOT EXISTS (la "tecnica della doppia negazione") o tramite conteggi e GROUP BY con una clausola HAVING.

        Esempio con doppia negazione per trovare gli studenti iscritti a tutti i corsi in CorsiObbligatori:

        ```sql
        SELECT DISTINCT S.Matricola
        FROM Studenti S -- Assumiamo di avere una tabella Studenti da cui partire
        WHERE NOT EXISTS ( -- Non esiste un corso obbligatorio...
            SELECT CO.IDCorso
            FROM CorsiObbligatori CO
            WHERE NOT EXISTS ( -- ...tale che lo studente NON vi sia iscritto.
                SELECT I.Matricola
                FROM StudentiIscrizioni I -- o direttamente la tabella Iscrizioni
                WHERE I.Matricola = S.Matricola AND I.IDCorso = CO.IDCorso
            )
        );
        ```

    - L'operatore di divisione può essere espresso usando altri operatori fondamentali:
      - La formula è: R÷S=π<sub>A</sub>​(R)-π<sub>A</sub>​((π<sub>A</sub>​(R)×S)-R)

        Dove:

        - R è la relazione "dividendo".
        - S è la relazione "divisore".
        - A rappresenta l'insieme degli attributi di R che **non** sono presenti in S. Il risultato della divisione R÷S conterrà solo questi attributi A.
        - Assumiamo che l'insieme degli attributi di S sia un sottoinsieme degli attributi di R. Se chiamiamo Attr(R) l'insieme degli attributi di R e Attr(S) l'insieme degli attributi di S, allora A=Attr(R)-Attr(S).

        L'obiettivo della divisione R÷S è trovare quelle tuple sugli attributi A (chiamiamole tA​) tali che per **ogni** tupla tS​ in S, la tupla combinata (tA​,tS​) esista in R.

        Analizziamo la formula pezzo per pezzo, seguendo la logica "dall'interno verso l'esterno":

        1. **π<sub>A</sub>​(R)**

            - **Significato**: Proietta la relazione R sugli attributi A.
            - **Risultato**: Otteniamo un insieme di tutte le unique combinazioni di valori per gli attributi A che esistono in R. Queste sono le "candidate" per il risultato finale della divisione. Se una tupla tA​ non appare qui, non può certamente essere nel risultato di R÷S.
            - *Esempio intuitivo*: Se R sono le iscrizioni (Studente, Corso) e S è una lista di corsi obbligatori (Corso), allora πStudente​(R) ci dà tutti gli studenti che sono iscritti ad almeno un corso.
        2. **π<sub>A</sub>​(R)×S**

            - **Significato**: È il prodotto cartesiano tra l'insieme dei candidati t<sub>A</sub>​ (dal passo 1) e la relazione S (il divisore).
            - **Risultato**: Una relazione che contiene tutte le possibili combinazioni di (t<sub>A</sub>​,t<sub>S</sub>​), dove t<sub>A</sub>​ è una tupla candidata dagli attributi A di R, e t<sub>S</sub>​ è una tupla dalla relazione S.
            - *Esempio intuitivo*: Se R<sub>A</sub>A sono gli studenti candidati e S sono i corsi obbligatori, questo prodotto cartesiano genera tutte le coppie (Studente, CorsoObbligatorio) che *dovrebbero* esistere se uno studente fosse iscritto a *tutti* i corsi obbligatori. È l'insieme di "tutte le cose che dovrebbero essere vere" per ogni candidato.

        3. **(π<sub>A</sub>​(R)×S)-R**

            - **Significato**: È la differenza insiemistica. Stiamo sottraendo la relazione originale R dall'insieme di "tutte le cose che dovrebbero essere vere" calcolato al passo 2.
            - **Risultato**: Questa operazione ci dà le tuple (t<sub>A</sub>A,t<sub>S</sub>​) che *avrebbero dovuto* essere in R (perché t<sub>A</sub>​ è un candidato e t<sub>S</sub>​ è in S) ma che in realtà **non ci sono**. Queste sono le "mancanze" o le "violazioni" della condizione "per tutti". Se una tupla (t<sub>A</sub>​,t<sub>S</sub>​) è in questo risultato, significa che al candidato t<sub>A</sub>​ manca l'associazione con t<sub>S</sub>​ (che invece è richiesta da S).
            - *Esempio intuitivo*: Se (Pippo, Matematica) è una coppia che *dovrebbe* esistere (Pippo è uno studente, Matematica è un corso obbligatorio) ma non esiste nella tabella delle iscrizioni R, allora (Pippo, Matematica) sarà in questo risultato.

        4. **π<sub>A</sub>​((π<sub>A</sub>​(R)×S)-R)**

            - **Significato**: Proiettiamo il risultato del passo 3 (le "mancanze") sugli attributi A.
            - **Risultato**: Otteniamo l'insieme delle tuple t<sub>A</sub>​ (cioè, i valori per gli attributi A) per le quali esiste *almeno una* tupla t<sub>S</sub>​ in S tale che la combinazione (t<sub>A</sub>​,t<sub>S</sub>​) non è presente in R. In altre parole, queste sono tutte le t<sub>A</sub>​ che **non** soddisfano la condizione della divisione (non sono associate a *tutte* le tuple di S). Sono i candidati "squalificati".
            - *Esempio intuitivo*: Se Pippo non è iscritto a Matematica (che è obbligatoria), allora "Pippo" sarà in questo insieme. Se Pluto non è iscritto a Fisica (che è obbligatoria), allora "Pluto" sarà in questo insieme.

        5. **π<sub>A</sub>A(R)-π<sub>A</sub>​((π<sub>A</sub>A(R)×S)-R)**

            - **Significato**: È di nuovo una differenza insiemistica. Stiamo prendendo l'insieme di tutti i candidati t<sub>A</sub>​ (dal passo 1) e stiamo sottraendo l'insieme dei candidati t<sub>A</sub>​ "squalificati" (dal passo 4).
            - **Risultato**: Ciò che rimane sono esattamente quelle tuple t<sub>A</sub>​ che erano candidate e per le quali non è stata trovata nessuna "mancanza". Queste sono le tuple t<sub>A</sub>​ che sono associate in R con **ogni** tupla t<sub>S</sub>​ della relazione S. Questo è, per definizione, il risultato di R÷S.
            - *Esempio intuitivo*: Prendiamo tutti gli studenti (passo 1). Togliamo quelli a cui manca almeno un corso obbligatorio (passo 4). Quelli che restano sono gli studenti iscritti a tutti i corsi obbligatori.

    **In parole povere, la logica è:**

    1. Identifica tutti i possibili "protagonisti" A dalla tabella R.
    2. Per ogni protagonista A e per ogni "requisito" S, genera la lista di tutte le coppie (A,S) che *dovrebbero* esistere.
    3. Trova quali di queste coppie (A,S) "ideali" in realtà *non* esistono in R. Queste sono le "mancanze".
    4. Identifica i protagonisti A che hanno almeno una "mancanza". Questi sono i protagonisti che non soddisfano tutti i requisiti.
    5. Il risultato della divisione sono tutti i protagonisti iniziali meno quelli che hanno fallito almeno un requisito.

    Questa scomposizione mostra come una query apparentemente complessa come la divisione (che implica una quantificazione universale "per tutti") possa essere costruita utilizzando operatori più semplici come proiezione, prodotto cartesiano e differenza insiemistica. È un esempio della potenza espressiva dell'algebra relazionale.

## Confronto tra Algebra Relazionale e SQL

| **Caratteristica** | **Algebra Relazionale** | **SQL** |
| --- |  --- |  --- |
| **Natura** | Procedurale: specifica "come" ottenere il risultato attraverso una sequenza di operazioni definite. | Prevalentemente Dichiarativa: specifica "cosa" si vuole ottenere, lasciando al DBMS il compito di determinare il "come" (il piano di esecuzione). |
| **Formalismo** | Linguaggio formale con solide basi matematiche (teoria degli insiemi). | Linguaggio standardizzato per database, più pratico, verboso e con molte funzionalità aggiuntive (DML, DDL, DCL). |
| **Ottimizzazione** | Le espressioni algebriche possono essere trasformate algebricamente in espressioni equivalenti ma più efficienti. Questa è la base per l'ottimizzazione delle query nei DBMS. | Il DBMS traduce la query SQL in un piano di esecuzione (spesso una forma di albero di operatori relazionali) e lo ottimizza internamente, spesso applicando regole di riscrittura derivate dall'algebra relazionale. |
| **Gestione Duplicati** | Le relazioni sono insiemi, quindi per definizione non contengono tuple duplicate. La proiezione, l'unione, ecc., eliminano i duplicati. | Le tabelle e i risultati delle query sono multinsiemi (bag) per default; i duplicati sono ammessi e mantenuti a meno che non si usi `DISTINCT` o ci siano vincoli di unicità (es. `PRIMARY KEY`, `UNIQUE`). `UNION ALL` mantiene i duplicati, `UNION` li elimina. |
| **Valori NULL** | L'algebra relazionale classica non tratta i valori `NULL`. Esistono estensioni per gestire i `NULL`, ma la logica diventa più complessa (es. logica a tre valori). | SQL supporta pienamente i valori `NULL`, con una logica a tre valori (TRUE, FALSE, UNKNOWN) per le condizioni booleane. Questo influenza il comportamento dei join e delle selezioni. |
| **Ordine Tuple** | Le relazioni sono insiemi non ordinati di tuple. Non c'è concetto di "prima" o "ultima" tupla. | Le tuple in una tabella o nel risultato di una query non hanno un ordine intrinseco, a meno che non sia esplicitamente specificato con la clausola `ORDER BY` per la presentazione del risultato finale. |
| **Aggregazioni e Raggruppamento** | L'algebra relazionale di base non include operatori di aggregazione (es. `SUM`, `AVG`, `COUNT`) o raggruppamento. Questi sono considerati estensioni dell'algebra (operatore G o `gamma`). | SQL ha funzioni di aggregazione native (`SUM`, `AVG`, `COUNT`, `MIN`, `MAX`) usate in congiunzione con la clausola `GROUP BY` per calcolare valori su gruppi di tuple. |
| **Modifiche ai Dati** | L'algebra relazionale è un linguaggio di interrogazione; non include operatori per modificare i dati (inserimento, aggiornamento, cancellazione). | SQL include comandi DML (Data Manipulation Language) come `INSERT`, `UPDATE`, `DELETE` per modificare i dati. |

L'algebra relazionale è cruciale perché:

1. **Fornisce una base formale**: Sostiene il modello relazionale e i linguaggi di interrogazione come SQL.

2. **Permette l'ottimizzazione delle query**: I DBMS usano principi dell'algebra relazionale per trasformare una query SQL in un piano di esecuzione efficiente. Ad esempio, una query può essere riscritta in molte forme algebriche equivalenti, e l'ottimizzatore sceglie quella con il costo stimato minore.

3. **Aiuta la comprensione**: Comprendere l'algebra relazionale aiuta a scrivere query SQL più efficienti e a capire meglio il funzionamento interno di un DBMS, specialmente come vengono processati i join e le selezioni complesse.

## Esercizi Finali

Si considerino i seguenti schemi di relazione per una semplice base di dati aziendale:

- **Impiegato(IDImpiegato, Nome, Cognome, Stipendio, IDDipartimento)** (IDDipartimento è una chiave esterna verso Dipartimento)

- **Dipartimento(IDDipartimento, NomeDipartimento, CittàSede)**

- **Progetto(IDProgetto, NomeProgetto, Budget, IDResponsabile)** (IDResponsabile è una chiave esterna verso Impiegato.IDImpiegato)

- **LavoraSu(IDImpiegato, IDProgetto, OreSettimanali)** (IDImpiegato e IDProgetto sono chiavi esterne)

Esercizio 1:

Scrivere le espressioni dell'algebra relazionale per trovare:

1. Il nome e cognome di tutti gli impiegati che lavorano nel dipartimento con `NomeDipartimento = 'Ricerca'`.

    - *Suggerimento: potrebbe essere necessario un join o una sequenza di operazioni.*

2. Gli `IDImpiegato` degli impiegati che lavorano sia al progetto `P100` che al progetto `P200`.

    - *Suggerimento: pensare all'intersezione o a come ottenerla.*

3. I nomi dei progetti il cui budget è superiore a 50000€ e il cui responsabile è l'impiegato con `IDImpiegato = 'E001'`.

4. Gli `IDImpiegato` di tutti gli impiegati che *non* lavorano al progetto `P100`. (Considerare solo gli impiegati che lavorano ad almeno un progetto).

    - *Suggerimento: pensare alla differenza.*

5. Il nome e cognome degli impiegati e il nome del dipartimento in cui lavorano, per tutti gli impiegati.

    - *Suggerimento: join naturale o equi-join.*

6. Gli `IDImpiegato` che lavorano a *tutti* i progetti gestiti dal dipartimento 'Vendite'.

    - *Assumere di poter prima ottenere una relazione **ProgettiVendite(IDProgetto)** che elenca i progetti del dipartimento Vendite (es. tramite un join tra Progetto, Impiegato (per il responsabile) e Dipartimento).*

    - *Suggerimento: operatore di divisione.*

Esercizio 2:

Tradurre le seguenti query SQL in espressioni dell'algebra relazionale (usare i nomi delle tabelle e degli attributi degli schemi sopra):

1. query:

    ```sql
    SELECT Nome, Cognome
    FROM Impiegato
    WHERE Stipendio > 30000 AND IDDipartimento = 'D002';
    ```

2. query:

    ```sql
    SELECT I.Nome, I.Cognome, P.NomeProgetto
    FROM Impiegato I
    JOIN LavoraSu L ON I.IDImpiegato = L.IDImpiegato
    JOIN Progetto P ON L.IDProgetto = P.IDProgetto
    WHERE P.Budget < 20000;

    ```

3. query: (Assumendo che `INTERSECT` sia disponibile)

    ```sql
    SELECT IDImpiegato FROM LavoraSu WHERE IDProgetto = 'P300'
    INTERSECT
    SELECT IDImpiegato FROM LavoraSu WHERE IDProgetto = 'P400';
    ```

Esercizio 3 (Riflessione):

Si supponga che la tabella Impiegato abbia un attributo DataAssunzione e la tabella Progetto abbia un attributo DataInizio.

1. Se si eseguisse un `NATURAL JOIN` tra `Impiegato` e `Progetto` e, per errore, entrambe le tabelle avessero anche un attributo chiamato `Descrizione` (es. `Impiegato.Descrizione` per una nota sull'impiegato, `Progetto.Descrizione` per la descrizione del progetto), cosa succederebbe? Il join avverrebbe anche su questo attributo `Descrizione`?

2. Spiegare perché, in generale, è più sicuro usare `JOIN ... ON` o `JOIN ... USING` piuttosto che `NATURAL JOIN` in query complesse o in sistemi dove gli schemi delle tabelle potrebbero evolvere.

Questi esercizi mirano a consolidare la comprensione degli operatori dell'algebra relazionale e della loro relazione con SQL, preparando lo studente ad affrontare problemi di interrogazione di database con maggiore consapevolezza teorica.

### Soluzione degli esercizi

Schemi di relazione considerati:

- **Impiegato(IDImpiegato, Nome, Cognome, Stipendio, IDDipartimento)**

- **Dipartimento(IDDipartimento, NomeDipartimento, CittàSede)**

- **Progetto(IDProgetto, NomeProgetto, Budget, IDResponsabile)**

- **LavoraSu(IDImpiegato, IDProgetto, OreSettimanali)**

### Esercizio 1: Espressioni dell'Algebra Relazionale

1. **Il nome e cognome di tutti gli impiegati che lavorano nel dipartimento con `NomeDipartimento = 'Ricerca'`.**

    - Si seleziona prima il dipartimento 'Ricerca'.

    - Poi si fa un join con la tabella `Impiegato` usando `IDDipartimento`.

    - Infine, si proiettano gli attributi `Nome` e `Cognome`.

    π<sub>Nome,Cognome</sub>​(σ<sub>NomeDipartimento='Ricerca'</sub>​(Dipartimento)⋈<sub>Dipartimento.IDDipartimento=Impiegato.IDDipartimento</sub>​Impiegato)

    *Alternativamente, unendo prima e selezionando poi (se si rinominano gli attributi per evitare ambiguità o si usa un natural join assumendo che IDDipartimento sia l'unico attributo comune con quel nome):* π<sub>Impiegato.Nome,Impiegato.Cognome</sub>​(σ<sub>NomeDipartimento='Ricerca'</sub>​(Impiegato⋈Dipartimento))

2. **Gli `IDImpiegato` degli impiegati che lavorano sia al progetto `P100` che al progetto `P200`.**

    - Si trovano gli impiegati che lavorano al progetto 'P100'.

    - Si trovano gli impiegati che lavorano al progetto 'P200'.

    - Si fa l'intersezione dei due insiemi di `IDImpiegato`.

    R1​←π<sub>IDImpiegato</sub>​(σ<sub>IDProgetto='P100'​</sub>(LavoraSu)) R2​←π<sub>IDImpiegato</sub>​(σ<sub>IDProgetto=′P200′</sub>​(LavoraSu)) Risultato←R1​∩R2​

    *Forma compatta:* (π<sub>IDImpiegato</sub>​(σ<sub>IDProgetto=′P100′</sub>​(LavoraSu)))∩(π<sub>IDImpiegato</sub>​(σ<sub>IDProgetto=′P200′</sub>​(LavoraSu)))

3. **I nomi dei progetti il cui budget è superiore a 50000€ e il cui responsabile è l'impiegato con `IDImpiegato = 'E001'`.**

    - Si selezionano i progetti che soddisfano entrambe le condizioni.

    - Si proietta l'attributo `NomeProgetto`.

    π<sub>NomeProgetto</sub>​(σ<sub>Budget>50000∧IDResponsabile='E001'</sub>​(Progetto))

4. **Gli `IDImpiegato` di tutti gli impiegati che** ***non*** **lavorano al progetto `P100`. (Considerare solo gli impiegati che lavorano ad almeno un progetto).**

    - Si trovano tutti gli `IDImpiegato` presenti nella tabella `LavoraSu` (quelli che lavorano ad almeno un progetto).

    - Si trovano gli `IDImpiegato` che lavorano al progetto 'P100'.

    - Si fa la differenza tra il primo insieme e il secondo.

    R<sub>TuttiLavoratori</sub>​←π<sub>IDImpiegato</sub>​(LavoraSu) R<sub>LavoratoriP100</sub>​←π<sub>IDImpiegato</sub>​(σ<sub>IDProgetto='P100'</sub>​(LavoraSu)) Risultato←R<sub>TuttiLavoratori</sub>​-R<sub>LavoratoriP100​</sub>

    *Forma compatta:* π<sub>IDImpiegato</sub>​(LavoraSu)-π<sub>IDImpiegato</sub>​(σ<sub>IDProgetto='P100'</sub>​(LavoraSu))

5. **Il nome e cognome degli impiegati e il nome del dipartimento in cui lavorano, per tutti gli impiegati.**

    - Si fa un join tra `Impiegato` e `Dipartimento` sulla colonna `IDDipartimento`.

    - Si proiettano gli attributi desiderati. È buona pratica qualificare gli attributi se i nomi potrebbero essere ambigui (es. `Impiegato.Nome`).

    π<sub>Impiegato.Nome,Impiegato.Cognome,Dipartimento.NomeDipartimento</sub>​(Impiegato⋈<sub>Impiegato.IDDipartimento=Dipartimento.IDDipartimento</sub>​Dipartimento)

6. **Gli `IDImpiegato` che lavorano a** ***tutti*** **i progetti gestiti dal dipartimento 'Vendite'.** Questo richiede un'operazione di divisione. Prima, dobbiamo identificare i progetti gestiti dal dipartimento 'Vendite'.

    - **Passo 1: Trovare `ProgettiVendite(IDProgetto)`** Assumiamo che "progetti gestiti dal dipartimento Vendite" significhi che il *responsabile* del progetto appartiene al dipartimento Vendite.

        1. Trovare l'`IDDipartimento` del dipartimento 'Vendite': DipVenditeID←π<sub>IDDipartimento</sub>​(σ<sub>NomeDipartimento='Vendite'</sub>​(Dipartimento))

        2. Trovare gli `IDImpiegato` dei responsabili che appartengono al dipartimento 'Vendite': ResponsabiliVendite←π<sub>IDImpiegato</sub>​(Impiegato⋈<sub>Impiegato.IDDipartimento=DipVenditeID.IDDipartimento</sub>​DipVenditeID)

        3. Trovare gli `IDProgetto` dei progetti il cui `IDResponsabile` è in `ResponsabiliVendite`: ProgettiVendite←π<sub>IDProgetto</sub>​(Progetto⋈<sub>Progetto.IDResponsabile=ResponsabiliVendite.IDImpiegato</sub>​ResponsabiliVendite) *Alternativamente, in un unico passaggio più complesso:* ProgettiVendite←π<sub>Progetto.IDProgetto</sub>​(Progetto⋈<sub>Progetto.IDResponsabile=Impiegato.IDImpiegato</sub>​(Impiegato⋈<sub>Impiegato.IDDipartimento=Dipartimento.IDDipartimento</sub>​(σ<sub>NomeDipartimento='Vendite'</sub>​(Dipartimento))))

    - **Passo 2: Divisione** Ora dividiamo la relazione che lega gli impiegati ai progetti a cui lavorano (`LavoraSu`, proiettata sugli attributi rilevanti) per la relazione `ProgettiVendite`. ImpiegatiProgetti←π<sub>IDImpiegato,IDProgetto</sub>​(LavoraSu) Risultato←ImpiegatiProgetti÷ProgettiVendite

### Esercizio 2: Traduzione da SQL ad Algebra Relazionale

1. **SQL:**

    ```sql
    SELECT Nome, Cognome
    FROM Impiegato
    WHERE Stipendio > 30000 AND IDDipartimento = 'D002';
    ```

    **Algebra Relazionale:** π<sub>Nome,Cognome​</sub>(σ<sub>Stipendio>30000∧IDDipartimento='D002'</sub>​(Impiegato))

2. **SQL:**

    ```sql
    SELECT I.Nome, I.Cognome, P.NomeProgetto
    FROM Impiegato I
    JOIN LavoraSu L ON I.IDImpiegato = L.IDImpiegato
    JOIN Progetto P ON L.IDProgetto = P.IDProgetto
    WHERE P.Budget < 20000;
    ```

    **Algebra Relazionale:** Si usano alias per chiarezza, corrispondenti a quelli SQL. ρ<sub>I</sub>​(Impiegato) ρ<sub>L</sub>​(LavoraSu) ρ<sub>P</sub>​(Progetto)

    Join1←I⋈<sub>I.IDImpiegato=L.IDImpiegato</sub>​L Join2←Join1⋈<sub>L.IDProgetto=P.IDProgetto​</sub>P SelezioneBudget←σ<sub>P.Budget<20000</sub>​(Join2) Risultato←π<sub>I.Nome,I.Cognome,P.NomeProgetto​</sub>(SelezioneBudget)

    *Forma compatta (le ridenominazioni sono implicite nell'uso di I, L, P per qualificare gli attributi):* π<sub>I.Nome,I.Cognome,P.NomeProgetto</sub>​(σ<sub>P.Budget<20000</sub>​((ρ<sub>I​</sub>(Impiegato)⋈<sub>I.IDImpiegato=L.IDImpiegato</sub>​ρ<sub>L</sub>​(LavoraSu))⋈<sub>L.IDProgetto=P.IDProgetto​ρ</sub>​P(Progetto)))

3. **SQL:**

    ```sql
    SELECT IDImpiegato FROM LavoraSu WHERE IDProgetto = 'P300'
    INTERSECT
    SELECT IDImpiegato FROM LavoraSu WHERE IDProgetto = 'P400';
    ```

    **Algebra Relazionale:** Questa è una traduzione diretta dell'operatore `INTERSECT`. (π<sub>IDImpiegato</sub>​(σ<sub>IDProgetto='P300'</sub>​(LavoraSu)))∩(π<sub>IDImpiegato​</sub>(σ<sub>IDProgetto='P400'</sub>​(LavoraSu)))

### Esercizio 3: Riflessione

1. **Se si eseguisse un `NATURAL JOIN` tra `Impiegato` e `Progetto` e, per errore, entrambe le tabelle avessero anche un attributo chiamato `Descrizione` (es. `Impiegato.Descrizione` per una nota sull'impiegato, `Progetto.Descrizione` per la descrizione del progetto), cosa succederebbe? Il join avverrebbe anche su questo attributo `Descrizione`?**

    Sì, il `NATURAL JOIN` esegue un equi-join su **tutti** gli attributi che hanno lo stesso nome in entrambe le tabelle. Quindi, se sia `Impiegato` che `Progetto` avessero un attributo chiamato `Descrizione`, la condizione di join includerebbe implicitamente `Impiegato.Descrizione = Progetto.Descrizione`, oltre a qualsiasi altra colonna con nome comune (ad esempio, se `IDResponsabile` in `Progetto` si chiamasse `IDImpiegato` e fosse l'unico altro attributo comune).

    Questo comporterebbe che verrebbero unite solo le tuple (Impiegato, Progetto) per le quali il valore dell'attributo `Impiegato.Descrizione` è identico al valore di `Progetto.Descrizione`. È altamente improbabile che una nota generica su un impiegato coincida con la descrizione di un progetto, a meno di una casualità o di una specifica (e insolita) convenzione di dati. Di conseguenza, il risultato del join sarebbe probabilmente vuoto o conterrebbe pochissime tuple, quasi certamente non quelle desiderate se l'intento era, ad esempio, unire un progetto al suo impiegato responsabile.

2. **Spiegare perché, in generale, è più sicuro usare `JOIN ... ON` o `JOIN ... USING` piuttosto che `NATURAL JOIN` in query complesse o in sistemi dove gli schemi delle tabelle potrebbero evolvere.**

    In generale, `JOIN ... ON` o `JOIN ... USING` sono considerati più sicuri e manutenibili di `NATURAL JOIN` per i seguenti motivi:

    - **Notazione esplicita e Chiarezza**:

        - `JOIN ... ON` permette di specificare l'esatta condizione di join, incluse uguaglianze tra colonne con nomi diversi o condizioni più complesse (es. `A.col1 > B.col2`). Questo rende la query auto-documentante e l'intenzione del programmatore inequivocabile.

        - `JOIN ... USING(col1, col2, ...)` specifica esplicitamente le colonne (che devono avere lo stesso nome in entrambe le tabelle) da usare per il join. Anche questo è chiaro.

        - `NATURAL JOIN`, invece, nasconde le colonne di join. Chi legge la query deve ispezionare gli schemi di entrambe le tabelle per capire su quali colonne avverrà il join.

    - **Robustezza alle Modifiche dello Schema**:

        - Se in futuro venisse aggiunto un nuovo attributo a una delle tabelle coinvolte in un `NATURAL JOIN`, e questo nuovo attributo avesse casualmente lo stesso nome di un attributo dell'altra tabella, il `NATURAL JOIN` cambierebbe silenziosamente il suo comportamento includendo questa nuova colonna nella condizione di join. Questo potrebbe portare a risultati errati o inattesi, e il bug potrebbe essere difficile da individuare.

        - Con `JOIN ... ON` o `JOIN ... USING`, la query continuerebbe a funzionare come previsto, poiché le colonne di join sono specificate esplicitamente e non verrebbero influenzate dall'aggiunta di altre colonne.

    - **Prevenzione di Join Errati**:

        - Come discusso nel punto precedente, `NATURAL JOIN` può portare a join su colonne che non dovrebbero essere semanticamente collegate, solo perché condividono lo stesso nome (es. `Descrizione` in `Impiegato` e `Progetto`).

        - `JOIN ... ON` e `JOIN ... USING` obbligano il programmatore a pensare e specificare attivamente le colonne corrette per la relazione logica che si vuole stabilire.

    - **Maggiore Controllo**:

        - `JOIN ... ON` offre il massimo controllo, permettendo join non basati sull'uguaglianza o join che coinvolgono espressioni.

    Sebbene `NATURAL JOIN` possa sembrare più conciso per join semplici su chiavi primarie/esterne con nomi identici, il rischio di errori sottili e la minore leggibilità a lungo termine, specialmente in sistemi complessi o in evoluzione, rendono `JOIN ... ON` (o `JOIN ... USING` quando appropriato) le scelte preferibili per la maggior parte delle situazioni.

[^1]: L'arità di una relazione, in questo contesto, si riferisce al numero di attributi (o colonne) che essa possiede.Nel contesto dell'informatica, della matematica e della logica, **arità** (in inglese "arity") si riferisce al **numero di argomenti o operandi** che una funzione, un operatore o una relazione accetta. Ecco alcuni esempi di arità:

    - **Funzioni/Operazioni:**

        - Una funzione **unaria** ha arità 1 (prende un solo argomento). Esempio: la negazione logica `NOT p`, o la funzione trigonometrica `sin(x)`.
        - Una funzione **binaria** ha arità 2 (prende due argomenti). Esempio: l'addizione `a + b`, o l'operatore di confronto `x > y`.
        - Una funzione **ternaria** ha arità 3 (prende tre argomenti). Esempio: l'operatore condizionale `a ? b : c` in alcuni linguaggi di programmazione.
        - Una funzione **nullaria** (o di arità zero) non prende argomenti e di solito rappresenta una costante. Esempio: `PI` (la costante pi greco).
    - **Relazioni (nel contesto dei database):**

        - L'arità di una relazione (o tabella) corrisponde al **numero dei suoi attributi (o colonne)**.
        - Ad esempio, se hai una relazione `Studenti(Matricola, Nome, Cognome, DataNascita)`, la sua arità è 4, perché ha quattro attributi.
    - **Predicati Logici:**

        - Un predicato logico ha un'arità che indica quanti termini collega. Esempio: `P(x)` è un predicato unario, `Ama(x, y)` ("x ama y") è un predicato binario.

    In sintesi, l'arità definisce "quante cose" sono coinvolte in un'operazione, funzione o relazione.

[^2]: **Logica a Tre Valori di SQL (TRUE, FALSE, UNKNOWN)**: SQL non utilizza solo la logica booleana classica (vero/falso). Quando sono coinvolti i valori `NULL`, entra in gioco un terzo valore logico: `UNKNOWN` (sconosciuto).

    - Qualsiasi confronto aritmetico o di stringa con `NULL` restituisce `UNKNOWN`. Ad esempio, `5 = NULL` è `UNKNOWN`, `NULL = NULL` è `UNKNOWN` (questo sorprende molti!).
    - Una clausola `WHERE` in SQL considera una condizione soddisfatta (e quindi include la riga) solo se la condizione valuta a `TRUE`. Se valuta a `FALSE` o `UNKNOWN`, la riga viene scartata.

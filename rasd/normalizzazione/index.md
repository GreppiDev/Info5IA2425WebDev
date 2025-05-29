
# Normalizzazione delle Basi di Dati Relazionali

- [Normalizzazione delle Basi di Dati Relazionali](#normalizzazione-delle-basi-di-dati-relazionali)
  - [Introduzione](#introduzione)
  - [Perché Normalizzare? Le Anomalie Operative](#perché-normalizzare-le-anomalie-operative)
  - [Analisi di Qualità tramite Normalizzazione](#analisi-di-qualità-tramite-normalizzazione)
  - [Normalizzazione e Mapping da E/R a Logico](#normalizzazione-e-mapping-da-er-a-logico)
  - [Dipendenze Funzionali (Concetto Chiave)](#dipendenze-funzionali-concetto-chiave)
    - [🔹 **Dipendenza funzionale banale**](#-dipendenza-funzionale-banale)
    - [🔹 **Dipendenza funzionale non banale**](#-dipendenza-funzionale-non-banale)
  - [Chiavi Candidate e Chiave Primaria](#chiavi-candidate-e-chiave-primaria)
  - [Le Forme Normali](#le-forme-normali)
    - [Prima Forma Normale (1NF)](#prima-forma-normale-1nf)
    - [Seconda Forma Normale (2NF)](#seconda-forma-normale-2nf)
    - [Terza Forma Normale (3NF)](#terza-forma-normale-3nf)
    - [Forma Normale di Boyce-Codd (BCNF)](#forma-normale-di-boyce-codd-bcnf)
    - [Guida alla Comprensione della Forma Normale di Boyce-Codd (BCNF)](#guida-alla-comprensione-della-forma-normale-di-boyce-codd-bcnf)
      - [Un esempio da analizzare](#un-esempio-da-analizzare)
      - [Verifica della Terza Forma Normale (3NF)](#verifica-della-terza-forma-normale-3nf)
      - [L'Analisi per la BCNF: Dove sorge il problema?](#lanalisi-per-la-bcnf-dove-sorge-il-problema)
      - [Trasformazione in BCNF: La Decomposizione](#trasformazione-in-bcnf-la-decomposizione)
      - [Risultato Finale: Tabelle in BCNF](#risultato-finale-tabelle-in-bcnf)
    - [Riepilogo sulle forme normali](#riepilogo-sulle-forme-normali)
    - [Esercizi Proposti](#esercizi-proposti)
    - [Conclusione](#conclusione)
    - [Soluzioni degli Esercizi Proposti](#soluzioni-degli-esercizi-proposti)

**Obiettivi della Lezione:**

* Comprendere il concetto di ridondanza dei dati e le anomalie (inserimento, aggiornamento, cancellazione) che essa provoca.
* Apprendere lo scopo e i benefici della normalizzazione nel processo di progettazione di database.
* Definire e identificare le dipendenze funzionali tra attributi.
* Definire i concetti di Chiave Candidata e Chiave Primaria.
* Comprendere e applicare le regole della Prima Forma Normale (1NF), Seconda Forma Normale (2NF) e Terza Forma Normale (3NF).
* Comprendere e applicare le regole della Forma Normale di Boyce-Codd (BCNF).
* Saper analizzare la qualità di uno schema relazionale utilizzando le forme normali.
* Comprendere la relazione tra il mapping da Modello Entità/Relazione (E/R) a modello logico relazionale e il processo di normalizzazione.
* Saper decomporre relazioni che violano una forma normale in relazioni equivalenti che la soddisfano.

## Introduzione

Nel contesto della progettazione di database relazionali, la normalizzazione è un processo sistematico utilizzato per organizzare gli attributi e le tabelle (o relazioni) al fine di minimizzare la ridondanza dei dati e migliorare l'integrità dei dati. La ridondanza non solo spreca spazio di archiviazione, ma soprattutto può portare a incoerenze e problemi durante l'inserimento, l'aggiornamento o la cancellazione dei dati, noti come anomalie. Questa lezione guiderà gli studenti attraverso le principali forme normali, fornendo le basi teoriche ed esempi pratici per progettare database robusti ed efficienti.

## Perché Normalizzare? Le Anomalie Operative

Una base di dati non normalizzata, o normalizzata in modo inadeguato, può soffrire di diversi problemi:

1. **Anomalia di Inserimento:** Difficoltà o impossibilità di inserire dati relativi a un'entità se mancano dati relativi a un'altra entità correlata nella stessa tabella. Ad esempio, non poter inserire i dati di un nuovo dipartimento finché non gli viene assegnato almeno un impiegato, se le informazioni sul dipartimento e sull'impiegato sono nella stessa tabella.
2. **Anomalia di Cancellazione:** La cancellazione di una tupla (riga) può comportare la perdita involontaria di informazioni relative a entità diverse. Ad esempio, cancellando l'ultimo impiegato di un dipartimento, si potrebbero perdere anche le informazioni sul dipartimento stesso.
3. **Anomalia di Aggiornamento:** La modifica di un valore di attributo richiede l'aggiornamento di più tuple, aumentando il rischio di incoerenze se non tutte le tuple vengono aggiornate correttamente. Ad esempio, se l'indirizzo di un dipartimento è ripetuto per ogni impiegato che vi lavora, un cambio di indirizzo richiederebbe la modifica di molte righe.

La normalizzazione mira a eliminare queste anomalie strutturando le tabelle in modo che ogni "fatto" sia memorizzato una sola volta.

## Analisi di Qualità tramite Normalizzazione

La normalizzazione non è solo un processo meccanico, ma uno strumento fondamentale per l'analisi della qualità dello schema logico di un database. Raggiungere forme normali elevate (come 3NF o BCNF) è generalmente indice di un buon design, caratterizzato da:

* **Minima Ridondanza:** I dati sono memorizzati in modo conciso.
* **Maggiore Integrità:** Si riduce il rischio di dati incoerenti.
* **Flessibilità:** Il database è più facile da modificare ed estendere in futuro.
* **Chiarezza Semantica:** Ogni tabella rappresenta un concetto ben definito (un'entità o una relazione specifica).

## Normalizzazione e Mapping da E/R a Logico

Il processo di progettazione di un database spesso inizia con un modello concettuale, come il Modello Entità/Relazione (E/R). Questo modello viene poi tradotto (mappato) in uno schema logico relazionale (un insieme di tabelle).

* Un modello E/R ben progettato, seguendo le regole standard di mapping, tende a produrre tabelle che sono già in buona parte normalizzate (spesso almeno in 3NF). Ad esempio, entità forti diventano tabelle con la chiave primaria dell'entità, e relazioni molti-a-molti diventano tabelle associative con chiavi esterne.
* Tuttavia, il mapping diretto non garantisce sempre il raggiungimento delle forme normali più elevate o potrebbe non gestire casi complessi in modo ottimale.
* La normalizzazione agisce quindi come un processo di **verifica e raffinamento** dello schema logico ottenuto dal mapping. Si analizzano le dipendenze funzionali all'interno delle tabelle generate e, se necessario, si decompongono ulteriormente per raggiungere la forma normale desiderata (tipicamente 3NF o BCNF).

## Dipendenze Funzionali (Concetto Chiave)

Prima di affrontare le forme normali, è essenziale comprendere il concetto di dipendenza funzionale (DF). Si dice che un attributo Y è funzionalmente dipendente da un attributo (o insieme di attributi) X, e si scrive X → Y, se per ogni valore valido di X esiste un solo valore di Y associato. X è chiamato *determinante* e Y è chiamato *dipendente*.

* **Esempio:** In una tabella `Studenti(Matricola, Nome, Cognome, DataNascita, CodiceCorso, NomeCorso)`, si ha:
    * `Matricola → Nome` (Ogni matricola identifica un solo nome)
    * `Matricola → Cognome`
    * `Matricola → DataNascita`
    * `CodiceCorso → NomeCorso` (Ogni codice corso identifica un solo nome di corso)
    * `(Matricola, CodiceCorso)` potrebbe essere la chiave primaria se uno studente può seguire più corsi.

È importante distinguere tra dipendenze funzionali non banali e dipendenze funzionali banali.

### 🔹 **Dipendenza funzionale banale**

Una dipendenza funzionale X -> Y è detta **banale** se Y⊆X.Cioè: l'insieme degli attributi sul lato destro è **contenuto** (o uguale) a quello sul lato sinistro.

✅ Esempi di dipendenze **banali**:

- {A,B}→A

- {A,B}→{A,B}

Sono **ovvie** e **sempre vere**, per definizione.

### 🔹 **Dipendenza funzionale non banale**

Una dipendenza funzionale X→Y è detta **non banale** se Y⊈X. In altre parole: il lato destro **contiene almeno un attributo** che **non è incluso** nel lato sinistro.
✅ Esempi di dipendenze **non banali**:

- A→B se B∉{A}

- {Corso}→Docente se `Docente` non è parte di `Corso`

## Chiavi Candidate e Chiave Primaria

Prima di procedere con le forme normali, è fondamentale definire i tipi di chiavi basate sulle dipendenze funzionali:

- **Chiave Candidata (Candidate Key):** È un attributo, o un insieme *minimo* di attributi, che identifica univocamente ogni tupla (riga) in una relazione. "Minimo" significa che nessun sottoinsieme proprio della chiave candidata possiede anch'esso la proprietà di unicità. Una relazione può avere più chiavi candidate. Ad esempio, in una tabella `Persone`, sia `CodiceFiscale` che `(Nome, Cognome, DataNascita)` potrebbero identificare univocamente una persona e sarebbero entrambe chiavi candidate.
- **Chiave Primaria (Primary Key - PK):** È una delle chiavi candidate che viene *scelta* dal progettista del database come identificatore *principale* della relazione. La chiave primaria deve essere univoca per ogni tupla e non può contenere valori nulli (vincolo di `NOT NULL`). Viene comunemente utilizzata per stabilire collegamenti tra tabelle attraverso le chiavi esterne (Foreign Keys). Nell'esempio precedente, si potrebbe scegliere `CodiceFiscale` come chiave primaria.
- **Superchiave (Superkey):** Qualsiasi insieme di attributi che identifica univocamente una tupla. Include tutte le chiavi candidate e anche insiemi di attributi che le contengono (quindi non necessariamente minimali).
- **Attributo Chiave (Prime Attribute):** Un attributo che fa parte di *almeno una* chiave candidata.
- **Attributo Non-Chiave (Non-Prime Attribute):** Un attributo che *non* fa parte di *nessuna* chiave candidata.

## Le Forme Normali

### Prima Forma Normale (1NF)

* **Definizione:** Una relazione è in 1NF se tutti i suoi attributi sono *atomici*, ovvero contengono un solo valore per tupla e non insiemi di valori o tabelle annidate. Per essere in 1NF una relazione:
  * deve avere una chiave primaria
  * deve avere solo attributi semplici (non deve avere attributi composti o multi-valore)
  * In presenza di attributi composti, in base all'uso che si farà dell'attributo, si potrà considerare l'attributo come atomico oppure no. Ad esempio, un attributo come l'`indirizzo` potrebbe essere considerato atomico se, in base all'uso che se ne deve fare, non è necessario scomporlo nelle sue parti semanticamente rilevanti. Questo aspetto relativo agli attributi composti va sempre dichiarato in fase di analisi.
* **Scopo:** Eliminare gruppi ripetuti e attributi multi-valore, rendendo la struttura della tabella uniforme.
* **Esempio (Non in 1NF):**
    Tabella `ORDINI`

    | IDOrdine | Cliente | Prodotti (Codice, Qta)          |
    | :------- | :------ | :------------------------------ |
    | 1        | Rossi   | (P10, 2), (P25, 1)              |
    | 2        | Bianchi | (P15, 5)                        |
    | 3        | Verdi   | (P10, 1), (P15, 3), (P30, 1)    |

    *Violazione:* L'attributo `Prodotti` non è atomico, contiene una lista/gruppo ripetuto.

* **Trasformazione in 1NF:** Si creano righe separate per ogni elemento del gruppo ripetuto.

    Tabella `ORDINI_PRODOTTI` (ora in 1NF)

    | IDOrdine | Cliente | CodiceProdotto | Qta |
    | :------- | :------ | :------------- | :-: |
    | 1        | Rossi   | P10            | 2   |
    | 1        | Rossi   | P25            | 1   |
    | 2        | Bianchi | P15            | 5   |
    | 3        | Verdi   | P10            | 1   |
    | 3        | Verdi   | P15            | 3   |
    | 3        | Verdi   | P30            | 1   |

    *Nota:* Questa tabella è in 1NF, ma presenta ancora ridondanza (il nome del cliente è ripetuto) e potenziali anomalie. La chiave primaria qui sarebbe `(IDOrdine, CodiceProdotto)`.

### Seconda Forma Normale (2NF)

* **Prerequisito:** La relazione deve essere in 1NF.
* **Definizione:** Una relazione è in 2NF se è in 1NF e *ogni attributo non-chiave* (attributo che non fa parte di *alcuna* chiave candidata) è *pienamente dipendente funzionalmente* dalla chiave primaria. Ciò significa che l'attributo non-chiave deve dipendere dall'intera chiave primaria, non solo da una parte di essa (nel caso di chiavi primarie composite).
* **Scopo:** Eliminare le dipendenze parziali degli attributi non-chiave dalla chiave primaria.
* **Esempio (Non in 2NF):** Si consideri la tabella `ORDINI_PRODOTTI` precedente (in 1NF).
    * Chiave Primaria: `(IDOrdine, CodiceProdotto)`
    * Attributi non-chiave: `Cliente`, `Qta`
    * Dipendenze Funzionali:
        * `(IDOrdine, CodiceProdotto) → Qta` (Piena dipendenza: la quantità dipende dall'ordine *e* dal prodotto specifico in quell'ordine)
        * `IDOrdine → Cliente` (Dipendenza parziale: il cliente dipende solo da `IDOrdine`, una parte della chiave primaria)

    *Violazione:* L'attributo non-chiave `Cliente` dipende solo da una parte della chiave primaria (`IDOrdine`).

* **Trasformazione in 2NF:** Si decompone la tabella per eliminare la dipendenza parziale.

    Tabella `ORDINI` (in 2NF e oltre)

    | IDOrdine | Cliente |
    | :------- | :------ |
    | 1        | Rossi   |
    | 2        | Bianchi |
    | 3        | Verdi   |

    *Chiave Primaria: `IDOrdine`*

    Tabella `DETTAGLI_ORDINE` (in 2NF e oltre)

    | IDOrdine | CodiceProdotto | Qta |
    | :------- | :------------- | :-: |
    | 1        | P10            | 2   |
    | 1        | P25            | 1   |
    | 2        | P15            | 5   |
    | 3        | P10            | 1   |
    | 3        | P15            | 3   |
    | 3        | P30            | 1   |

    *Chiave Primaria: `(IDOrdine, CodiceProdotto)`*
    *Chiave Esterna: `IDOrdine` referenzia `ORDINI(IDOrdine)`*

    Ora, in `DETTAGLI_ORDINE`, l'unico attributo non-chiave `Qta` dipende pienamente dall'intera chiave primaria `(IDOrdine, CodiceProdotto)`.

### Terza Forma Normale (3NF)

* **Prerequisito:** La relazione deve essere in 2NF.
* **Definizione:** Una relazione è in 3NF se è in 2NF e *nessun attributo non-chiave è transitivamente dipendente dalla chiave primaria*. Una dipendenza transitiva si verifica quando X → Y e Y → Z, dove X è la chiave primaria, e Y non è una chiave candidata (Y è un attributo non-chiave), e Z è un attributo non-chiave. In sostanza, **un attributo non-chiave non deve dipendere da un altro attributo non-chiave.**
* In maniera alternativa si può dire che (**Regola della 3NF:**):

    **Una relazione è in 3NF se, per ogni dipendenza funzionale X → Y**:

    1. **X è una superchiave, OPPURE**
    2. **Y è un attributo primo (cioè fa parte di ALMENO UNA chiave candidata)**.
* **Scopo:** Eliminare le dipendenze transitive tra attributi non-chiave.
* **Esempio (Non in 3NF):**
    Tabella `IMPIEGATI_DIPARTIMENTO` (supponiamo sia già in 2NF)

    | IDImpiegato | NomeImpiegato | IDDipartimento | NomeDipartimento | SedeDipartimento |
    | :----------- | :------------ | :------------- | :--------------- | :--------------- |
    | E01          | Mario Rossi   | D10            | Vendite          | Milano           |
    | E02          | Luca Bianchi  | D20            | IT               | Roma             |
    | E03          | Anna Verdi    | D10            | Vendite          | Milano           |
    | E04          | Paolo Neri    | D30            | Marketing        | Milano           |

    *Chiave Primaria: `IDImpiegato`*
    *Attributi non-chiave: `NomeImpiegato`, `IDDipartimento`, `NomeDipartimento`, `SedeDipartimento`*
    *Dipendenze Funzionali:*
    * `IDImpiegato → NomeImpiegato`
    * `IDImpiegato → IDDipartimento`
    * `IDDipartimento → NomeDipartimento` (Un ID dipartimento determina il nome del dipartimento)
    * `IDDipartimento → SedeDipartimento` (Un ID dipartimento determina la sede)

    *Violazione:* Esiste una dipendenza transitiva. `IDImpiegato` (PK) determina `IDDipartimento` (non-chiave), e `IDDipartimento` (non-chiave) determina `NomeDipartimento` e `SedeDipartimento` (altri attributi non-chiave). Quindi, `IDImpiegato → NomeDipartimento` e `IDImpiegato → SedeDipartimento` sono dipendenze transitive. La ridondanza è evidente (es. "Vendite", "Milano" ripetuti).

* **Trasformazione in 3NF:** Si decompone la tabella per eliminare la dipendenza transitiva.

    Tabella `IMPIEGATI` (in 3NF)

    | IDImpiegato | NomeImpiegato | IDDipartimento |
    | :----------- | :------------ | :------------- |
    | E01          | Mario Rossi   | D10            |
    | E02          | Luca Bianchi  | D20            |
    | E03          | Anna Verdi    | D10            |
    | E04          | Paolo Neri    | D30            |

    *Chiave Primaria: `IDImpiegato`*
    *Chiave Esterna: `IDDipartimento` referenzia `DIPARTIMENTI(IDDipartimento)`*

    Tabella `DIPARTIMENTI` (in 3NF)

    | IDDipartimento | NomeDipartimento | SedeDipartimento |
    | :------------- | :--------------- | :--------------- |
    | D10            | Vendite          | Milano           |
    | D20            | IT               | Roma             |
    | D30            | Marketing        | Milano           |

    *Chiave Primaria: `IDDipartimento`*

    Ora ogni attributo non-chiave dipende direttamente dalla chiave primaria della propria tabella, eliminando la dipendenza transitiva e la ridondanza associata.

### Forma Normale di Boyce-Codd (BCNF)

* **Prerequisito:** La relazione deve essere in 3NF.
* **Definizione:** **Una relazione è in BCNF se *per ogni dipendenza funzionale non banale X → Y, X è una superchiave***. Una superchiave è un insieme di attributi che identifica univocamente una tupla (include tutte le chiavi candidate e anche insiemi che le contengono). In termini più semplici, ogni determinante di una dipendenza funzionale deve essere una chiave candidata (o contenerne una).
* **Scopo:** Risolvere alcune rare anomalie non gestite dalla 3NF, specialmente in presenza di chiavi candidate multiple e sovrapposte. La BCNF è una forma normale più stringente della 3NF.
* **Differenza chiave dalla 3NF:** La 3NF permetteva dipendenze X → Y dove Y è un attributo chiave (parte di una chiave candidata) anche se X non è una superchiave. La BCNF non lo permette.
* **Si può dire che**:
  * **Regola della BCNF:** **Una relazione è in BCNF se, per ogni dipendenza funzionale non banale X → Y**:

    1. **X deve essere una superchiave. (Non c'è la seconda opzione della 3NF)**.

### Guida alla Comprensione della Forma Normale di Boyce-Codd (BCNF)

Nel mondo della progettazione di database, l'obiettivo è organizzare i dati in modo efficiente, evitando ridondanze e potenziali problemi (anomalie) quando i dati vengono inseriti, modificati o cancellati. Le forme normali forniscono una serie di regole per raggiungere questo obiettivo. La Forma Normale di Boyce-Codd (BCNF) è una forma normale particolarmente stringente.

**Definizione di BCNF:** Una relazione (o tabella) si dice in BCNF se, per ogni dipendenza funzionale non banale (del tipo X → Y, dove Y non è un sottoinsieme di X) presente nella relazione, X è una **superchiave**. Una superchiave è un insieme di attributi che identifica univocamente ogni riga della tabella.

**In termini più semplici: ogni volta che un insieme di attributi X determina un altro attributo Y, quell'insieme X deve essere in grado, da solo, di identificare univocamente un'intera riga della tabella.**

#### Un esempio da analizzare

Si consideri la seguente tabella, chiamata `ISCRIZIONI_TUTOR`, che tiene traccia degli studenti, dei corsi che seguono, dei professori che tengono i corsi e dei tutor assegnati agli studenti per specifici corsi.

| Matricola | CodiceCorso | Professore | Tutor |
| :-- |  :-- |  :-- |  :-- |
| S100 | C01 | Prof. Rossi | T01 |
| S100 | C02 | Prof. Neri | T02 |
| S200 | C01 | Prof. Rossi | T01 |
| S200 | C03 | Prof. Verdi | T03 |
| S300 | C02 | Prof. Neri | T02 |

Esporta in Fogli

**Ipotesi sulle Dipendenze Funzionali (DF):** Queste sono le "regole di business" che i dati devono rispettare:

1. `(Matricola, CodiceCorso) → Professore`: Uno studente specifico in un corso specifico ha un solo professore per quel corso.
2. `(Matricola, CodiceCorso) → Tutor`: Uno studente specifico in un corso specifico ha un solo tutor per quel corso.
3. `CodiceCorso → Professore`: Ogni corso è tenuto da un solo professore.
4. **`Tutor → CodiceCorso`**: Questa è un'ipotesi cruciale per l'esempio. Significa che ogni tutor è specializzato e segue studenti *esclusivamente per un determinato corso*. Se si conosce il tutor, si conosce il corso.
5. `Tutor → Professore`: Questa è una conseguenza delle DF 3 e 4 (se un tutor determina un corso, e un corso determina un professore, allora il tutor determina il professore).

**Identificazione delle Chiavi Candidate:** Una chiave candidata è un insieme minimo di attributi che identifica univocamente una riga.

- **CK1: `(Matricola, CodiceCorso)`**: Questa coppia determina univocamente `Professore` (per DF1) e `Tutor` (per DF2), e quindi l'intera riga.
- **CK2: `(Matricola, Tutor)`**: Poiché `Tutor → CodiceCorso` (DF4), conoscendo `Matricola` e `Tutor`, si può derivare `CodiceCorso`. Avendo `Matricola` e `CodiceCorso`, si possono determinare tutti gli altri attributi (come per CK1). Quindi, `(Matricola, Tutor)` è anch'essa una chiave candidata.

#### Verifica della Terza Forma Normale (3NF)

Prima di passare alla BCNF, è utile notare che questa tabella è in 3NF. La 3NF permette una dipendenza X → Y dove X non è una superchiave, a patto che Y sia un "attributo primo" (cioè, parte di almeno una chiave candidata). Nell'esempio, si analizza la dipendenza `Tutor → CodiceCorso`.

- `Tutor` non è una superchiave (es. T01 appare per S100 e S200).
- Tuttavia, `CodiceCorso` è parte della chiave candidata CK1 `(Matricola, CodiceCorso)`. Dato che `CodiceCorso` è un attributo primo, la 3NF "tollera" questa dipendenza.

#### L'Analisi per la BCNF: Dove sorge il problema?

La BCNF è più esigente: **ogni determinante X in una dipendenza X → Y deve essere una superchiave.**

Si riesaminino le dipendenze:

1. `(Matricola, CodiceCorso) → Professore`: Il determinante `(Matricola, CodiceCorso)` è CK1, quindi è una superchiave. **Conforme a BCNF.**
2. `(Matricola, CodiceCorso) → Tutor`: Il determinante `(Matricola, CodiceCorso)` è CK1 (superchiave). **Conforme a BCNF.**
3. `CodiceCorso → Professore`:
    - Il determinante è `CodiceCorso`.
    - `CodiceCorso` da solo **non è una superchiave** (es. C01 appare in più righe, associato a S100 e S200). Non identifica univocamente una riga in `ISCRIZIONI_TUTOR`.
    - **Violazione BCNF!**
4. `Tutor → CodiceCorso`:
    - Il determinante è `Tutor`.
    - `Tutor` da solo **non è una superchiave** (es. T01 appare in più righe).
    - **Violazione BCNF!** (Questa è la violazione su cui l'esempio originale si concentra per la decomposizione).

**Perché queste violazioni sono un problema? (Anomalie)** Prendiamo la dipendenza `Tutor → CodiceCorso` che viola la BCNF.

- **Ridondanza dei dati:** L'informazione che "T01 segue il corso C01" è ripetuta per ogni studente seguito da T01 nel corso C01.
- **Anomalia di aggiornamento:** Se il Tutor T01 dovesse cambiare corso (ad es., passare da C01 a un ipotetico C05), bisognerebbe aggiornare `CodiceCorso` (e di conseguenza `Professore`) in *tutte* le righe dove appare T01. Dimenticare un aggiornamento porterebbe a dati incoerenti.
- **Anomalia di inserimento:** Non è possibile inserire l'informazione che un nuovo Tutor T04 è assegnato al corso C04 (tenuto da Prof. Gialli) finché non c'è almeno uno studente (`Matricola`) che segue quel corso con quel tutor.
- **Anomalia di cancellazione:** Se S200, l'unico studente seguito da T03 per il corso C03, venisse cancellato, si perderebbe l'informazione che il Tutor T03 è associato al CodiceCorso C03.

#### Trasformazione in BCNF: La Decomposizione

Per raggiungere la BCNF, si scompone la tabella originale basandosi sulle dipendenze che causano la violazione. L'obiettivo è creare tabelle più piccole in cui ogni determinante sia una superchiave.

**Passo 1: Decomposizione basata su `Tutor → CodiceCorso` (e la sua conseguenza `Tutor → Professore`)**

Si isola la dipendenza `Tutor → CodiceCorso` (e `Tutor → Professore`) in una nuova tabella.

- **Nuova Tabella 1: `TUTOR_DETTAGLI`** (nome ipotetico per questa fase)
    - Contiene: `Tutor, CodiceCorso, Professore`
    - Chiave Primaria: `Tutor` (poiché `Tutor` determina gli altri due attributi in questa nuova tabella).
    - Dati:
  
    | Tutor | CodiceCorso | Professore  |
    | :---- | :---------- | :---------- |
    | T01   | C01         | Prof. Rossi |
    | T02   | C02         | Prof. Neri  |
    | T03   | C03         | Prof. Verdi |

La tabella originale viene privata degli attributi ora in `TUTOR_DETTAGLI` (eccetto `Tutor`, che serve per il collegamento).

- **Nuova Tabella 2: `STUDENTE_TUTOR`**
    - Contiene: `Matricola, Tutor`
    - Chiave Primaria: `(Matricola, Tutor)`
    - Dati:
  
        | Matricola | Tutor |
        | :-------- | :---- |
        | S100      | T01   |
        | S100      | T02   |
        | S200      | T01   |
        | S200      | T03   |
        | S300      | T02   |
  
  Questa tabella `STUDENTE_TUTOR` è ora in BCNF, poiché l'unica dipendenza significativa è quella della chiave primaria che determina gli attributi stessi.

**Passo 2: Analisi della tabella `TUTOR_DETTAGLI` per BCNF**

Si esamini `TUTOR_DETTAGLI(Tutor, CodiceCorso, Professore)` con Chiave Primaria `(Tutor)`. Al suo interno, si nota la dipendenza `CodiceCorso → Professore`.

- Il determinante è `CodiceCorso`.
- `CodiceCorso` **non è una superchiave** di `TUTOR_DETTAGLI` (la chiave è `Tutor`).
- Quindi, anche `TUTOR_DETTAGLI` viola la BCNF. Si procede a un'ulteriore decomposizione.

**Passo 2a: Decomposizione di `TUTOR_DETTAGLI` basata su `CodiceCorso → Professore`**

- **Nuova Tabella 3: `CORSI`**

    - Contiene: `CodiceCorso, Professore`
    - Chiave Primaria: `CodiceCorso`
    - Dati:

        | CodiceCorso | Professore  |
        | :---------- | :---------- |
        | C01         | Prof. Rossi |
        | C02         | Prof. Neri  |
        | C03         | Prof. Verdi |
  
  Questa tabella è in BCNF (il determinante `CodiceCorso` è la sua chiave).
- **Nuova Tabella 4: `TUTOR_CORSO`** (prende il posto di `TUTOR_DETTAGLI` per la parte rimanente)

    - Contiene: `Tutor, CodiceCorso`
    - Chiave Primaria: `Tutor` (mantenendo la DF originale `Tutor → CodiceCorso`)
    - Chiave Esterna: `CodiceCorso` referenzia `CORSI(CodiceCorso)`
    - Dati:
  
        | Tutor | CodiceCorso |
        | :---- | :---------- |
        | T01   | C01         |
        | T02   | C02         |
        | T03   | C03         |
  
  Questa tabella è in BCNF (il determinante `Tutor` è la sua chiave).

#### Risultato Finale: Tabelle in BCNF

Dopo la decomposizione, si ottiene il seguente schema di database, con tutte le tabelle in BCNF:

1. **`STUDENTE_TUTOR`**
    - `(Matricola, Tutor)` (Chiave Primaria)
2. **`TUTOR_CORSO`** (chiamata `TUTOR_CORSO_INFO` nell'esempio originale)
    - `(Tutor, CodiceCorso)` (Chiave Primaria: `Tutor`; `CodiceCorso` è chiave esterna verso `CORSI`)
3. **`CORSI`**
    - `(CodiceCorso, Professore)` (Chiave Primaria: `CodiceCorso`)

**Vantaggi della Decomposizione in BCNF:**

- **Eliminazione della ridondanza:** Ogni "fatto" è memorizzato una sola volta (es. l'associazione tra T01 e C01 è solo in `TUTOR_CORSO`).
- **Semplificazione degli aggiornamenti:** Per cambiare il corso di un tutor, si modifica una sola riga in `TUTOR_CORSO`.
- **Maggiore integrità dei dati:** Le anomalie di inserimento e cancellazione sono state risolte. È possibile aggiungere un nuovo corso o un nuovo tutor senza dover avere già uno studente associato, e cancellare uno studente non comporta la perdita di informazioni su tutor o corsi.

In conclusione, il processo di normalizzazione fino alla BCNF porta a un database meglio strutturato, più robusto e più facile da mantenere, assicurando che ogni informazione sia determinata logicamente dalla chiave appropriata.

### Riepilogo sulle forme normali

* **1NF:** Attributi atomici.
* **2NF:** 1NF + Nessuna dipendenza parziale di attributi non-chiave dalla chiave primaria.
* **3NF:** 2NF + Nessuna dipendenza transitiva di attributi non-chiave dalla chiave primaria, ossia nessuno attributo non-chiave che dipenda da un altro attributo non-chiave.
* **BCNF:** 3NF + Ogni determinante di una dipendenza funzionale è una superchiave.

Generalmente, l'obiettivo nella progettazione di database relazionali è raggiungere almeno la 3NF, e preferibilmente la BCNF, a meno che non ci siano specifiche ragioni di performance (denormalizzazione controllata) per fermarsi prima o fare eccezioni.

### Esercizi Proposti

1. **Identificazione Forma Normale:**
    Data la seguente relazione `PROGETTI(CodProgetto, NomeProgetto, CodImpiegato, NomeImpiegato, OreLavorate, SedeImpiegato)` con le dipendenze:
    * `CodProgetto → NomeProgetto`
    * `CodImpiegato → NomeImpiegato, SedeImpiegato`
    * `(CodProgetto, CodImpiegato) → OreLavorate`
    Assumendo `(CodProgetto, CodImpiegato)` come chiave primaria, determinare la forma normale più alta soddisfatta dalla relazione e giustificare la risposta.

2. **Normalizzazione:**
    Normalizzare la relazione `PROGETTI` dell'esercizio 1 fino alla BCNF, mostrando i passaggi intermedi e le relazioni risultanti con le rispettive chiavi primarie ed esterne.

3. **Analisi Dipendenze:**
    Data la relazione `VENDITE(CodCliente, NomeCliente, IndirizzoCliente, CodArticolo, DescrizioneArticolo, PrezzoUnitario, QtaVenduta, DataVendita)` e assumendo che `(CodCliente, CodArticolo, DataVendita)` sia la chiave primaria:
    * Identificare tutte le dipendenze funzionali plausibili.
    * Determinare se la relazione è in 1NF, 2NF, 3NF.
    * Se non è in 3NF, normalizzarla fino a 3NF.

4. **Caso BCNF:**
    Considerare la relazione `ESAMI(Matricola, CodiceMateria, Voto, Docente, DipartimentoDocente)` dove:
    * Uno studente (`Matricola`) può dare più materie (`CodiceMateria`).
    * Una materia può essere data da più studenti.
    * Per una data materia e un dato studente, c'è un solo voto.
    * Ogni materia è insegnata da un solo docente (`CodiceMateria → Docente`).
    * Ogni docente appartiene a un solo dipartimento (`Docente → DipartimentoDocente`).
    * Chiave primaria: `(Matricola, CodiceMateria)`.
    Determinare se questa relazione è in BCNF. Se non lo è, spiegare perché e decomporla in relazioni BCNF.

### Conclusione

La normalizzazione è una tecnica essenziale nella cassetta degli attrezzi di chi progetta database. Comprendere e applicare correttamente le forme normali permette di creare database più robusti, manutenibili e privi delle problematiche legate alla ridondanza dei dati. Sebbene il processo possa sembrare teorico, i suoi benefici pratici nella gestione quotidiana e nell'evoluzione dei sistemi informativi sono immensi.

### Soluzioni degli Esercizi Proposti

1. **Identificazione Forma Normale (PROGETTI):**
     Data la seguente relazione `PROGETTI(CodProgetto, NomeProgetto, CodImpiegato, NomeImpiegato, OreLavorate, SedeImpiegato)` con le dipendenze:
    * `CodProgetto → NomeProgetto`
    * `CodImpiegato → NomeImpiegato, SedeImpiegato`
    * `(CodProgetto, CodImpiegato) → OreLavorate`
    Assumendo `(CodProgetto, CodImpiegato)` come chiave primaria, determinare la forma normale più alta soddisfatta dalla relazione e giustificare la risposta.

    **Svolgimento:**
    - **Relazione:** `PROGETTI(CodProgetto, NomeProgetto, CodImpiegato, NomeImpiegato, OreLavorate, SedeImpiegato)`
    - **PK:** `(CodProgetto, CodImpiegato)`
    - **Attributi Non-Chiave:** `NomeProgetto`, `NomeImpiegato`, `OreLavorate`, `SedeImpiegato`
    - **FD:**
        - `CodProgetto → NomeProgetto`
        - `CodImpiegato → NomeImpiegato, SedeImpiegato`
        - `(CodProgetto, CodImpiegato) → OreLavorate`
    - **Analisi:**
        - **1NF:** Assunta soddisfatta (attributi atomici).
        - **2NF:** Violata. Esistono dipendenze parziali:
            - `NomeProgetto` (non-chiave) dipende da `CodProgetto` (parte della PK).
            - `NomeImpiegato` e `SedeImpiegato` (non-chiave) dipendono da `CodImpiegato` (parte della PK).
    - **Conclusione:** La forma normale più alta soddisfatta è la **1NF**.

2. **Normalizzazione (PROGETTI):**
    Normalizzare la relazione `PROGETTI` dell'esercizio 1 fino alla BCNF, mostrando i passaggi intermedi e le relazioni risultanti con le rispettive chiavi primarie ed esterne.

    **Svolgimento:**
    - **Passo 1 (da 1NF a 2NF):** Separare le dipendenze parziali.
        - `PROGETTI_INFO(CodProgetto, NomeProgetto)`
            - PK: `CodProgetto`
        - `IMPIEGATI_INFO(CodImpiegato, NomeImpiegato, SedeImpiegato)`
            - PK: `CodImpiegato`
        - `ASSEGNAMENTI(CodProgetto, CodImpiegato, OreLavorate)`
            - PK: `(CodProgetto, CodImpiegato)`
            - FK1: `CodProgetto` referenzia `PROGETTI_INFO(CodProgetto)`
            - FK2: `CodImpiegato` referenzia `IMPIEGATI_INFO(CodImpiegato)`
    - **Passo 2 (Verifica 3NF):** Analizzare le relazioni ottenute per dipendenze transitive.
       - `PROGETTI_INFO`: È in 3NF (e BCNF). Nessun attributo non-chiave dipende da altri non-chiave.
       - `IMPIEGATI_INFO`: È in 3NF (e BCNF). Nessun attributo non-chiave dipende da altri non-chiave.
       - `ASSEGNAMENTI`: È in 3NF (e BCNF). L'unico attributo non-chiave (`OreLavorate`) dipende direttamente dalla PK.
    - **Passo 3 (Verifica BCNF):** Analizzare le relazioni per determinanti che non siano superchiavi.
        - In tutte e tre le relazioni (`PROGETTI_INFO`, `IMPIEGATI_INFO`, `ASSEGNAMENTI`), gli unici determinanti delle dipendenze funzionali sono le rispettive chiavi primarie (che sono superchiavi).
    - **Conclusione:** La decomposizione in `PROGETTI_INFO`, `IMPIEGATI_INFO`, e `ASSEGNAMENTI` porta lo schema in **BCNF**.
3. **Analisi Dipendenze e Normalizzazione (VENDITE):**
    Data la relazione `VENDITE(CodCliente, NomeCliente, IndirizzoCliente, CodArticolo, DescrizioneArticolo, PrezzoUnitario, QtaVenduta, DataVendita)` e assumendo che `(CodCliente, CodArticolo, DataVendita)` sia la chiave primaria:
    * Identificare tutte le dipendenze funzionali plausibili.
    * Determinare se la relazione è in 1NF, 2NF, 3NF.
    * Se non è in 3NF, normalizzarla fino a 3NF.
  
    **Svolgimento:**
    - **Relazione:** `VENDITE(CodCliente, NomeCliente, IndirizzoCliente, CodArticolo, DescrizioneArticolo, PrezzoUnitario, QtaVenduta, DataVendita)`
    - **PK:** `(CodCliente, CodArticolo, DataVendita)`
    - **FD Plausibili:**
        - `CodCliente → NomeCliente, IndirizzoCliente`
        - `CodArticolo → DescrizioneArticolo, PrezzoUnitario`
        - `(CodCliente, CodArticolo, DataVendita) → QtaVenduta`
    - **Analisi Forma Normale:**
        - **1NF:** Assunta soddisfatta.
        - **2NF:** Violata. Ci sono dipendenze parziali:
            - `NomeCliente`, `IndirizzoCliente` dipendono solo da `CodCliente` (parte della PK).
            - `DescrizioneArticolo`, `PrezzoUnitario` dipendono solo da `CodArticolo` (parte della PK).
    - **Normalizzazione a 3NF (e BCNF):**
        - Si separano le dipendenze parziali:
            - `CLIENTI(CodCliente, NomeCliente, IndirizzoCliente)`
                - PK: `CodCliente`
            - `ARTICOLI(CodArticolo, DescrizioneArticolo, PrezzoUnitario)`
                - PK: `CodArticolo`
            - `DETTAGLIO_VENDITA(CodCliente, CodArticolo, DataVendita, QtaVenduta)`
                - PK: `(CodCliente, CodArticolo, DataVendita)`
                - FK1: `CodCliente` referenzia `CLIENTI(CodCliente)`
                - FK2: `CodArticolo` referenzia `ARTICOLI(CodArticolo)`
        - **Verifica:** Le tabelle risultanti (`CLIENTI`, `ARTICOLI`, `DETTAGLIO_VENDITA`) sono tutte in 3NF e anche in BCNF, poiché i determinanti delle dipendenze sono le chiavi primarie.
4. **Caso BCNF (ESAMI):**
    Considerare la relazione `ESAMI(Matricola, CodiceMateria, Voto, Docente, DipartimentoDocente)` dove:
    * Uno studente (`Matricola`) può dare più materie (`CodiceMateria`).
    * Una materia può essere data da più studenti.
    * Per una data materia e un dato studente, c'è un solo voto.
    * Ogni materia è insegnata da un solo docente (`CodiceMateria → Docente`).
    * Ogni docente appartiene a un solo dipartimento (`Docente → DipartimentoDocente`).
    * Chiave primaria: `(Matricola, CodiceMateria)`.
    Determinare se questa relazione è in BCNF. Se non lo è, spiegare perché e decomporla in relazioni BCNF.

    **Svolgimento:**
    - **Relazione:** `ESAMI(Matricola, CodiceMateria, Voto, Docente, DipartimentoDocente)`
    - **PK:** `(Matricola, CodiceMateria)`
    - **FD:**
        - `(Matricola, CodiceMateria) → Voto`
        - `CodiceMateria → Docente`
        - `Docente → DipartimentoDocente`
        - Derivata: `CodiceMateria → DipartimentoDocente`
    - **Analisi BCNF:**
        - La definizione di BCNF richiede che per ogni FD `X → Y`, `X` sia una superchiave.
        - Consideriamo `CodiceMateria → Docente`. Il determinante è `CodiceMateria`. `CodiceMateria` da solo *non* è una superchiave della relazione `ESAMI` (non identifica univocamente una tupla).
        - Consideriamo `Docente → DipartimentoDocente`. Il determinante è `Docente`. `Docente` da solo *non* è una superchiave.
    - **Conclusione:** La relazione `ESAMI` **non è in BCNF**. (Non è nemmeno in 2NF a causa delle dipendenze parziali `CodiceMateria → Docente` e `CodiceMateria → DipartimentoDocente`, e di conseguenza non è neanche in 3NF).
    - **Decomposizione in BCNF:** Si decompone basandosi sulle dipendenze che violano la BCNF.
        - Dalla violazione `CodiceMateria → Docente` e la transitiva `Docente → DipartimentoDocente`:
            - `STUDENTE_VOTI(Matricola, CodiceMateria, Voto)`
                - PK: `(Matricola, CodiceMateria)`
            - `MATERIA_DOCENTE(CodiceMateria, Docente)`
                - PK: `CodiceMateria`
            - `DOCENTE_DIPARTIMENTO(Docente, DipartimentoDocente)`
                - PK: `Docente`
        - **Chiavi Esterne:**
            - `STUDENTE_VOTI.CodiceMateria` referenzia `MATERIA_DOCENTE(CodiceMateria)`
            - `MATERIA_DOCENTE.Docente` referenzia `DOCENTE_DIPARTIMENTO(Docente)`
        - **Verifica:** Tutte e tre le relazioni risultanti sono in BCNF, poiché in ciascuna, l'unico determinante è la chiave primaria.

# Sessione ordinaria 2017 - seconda prova scritta - Indirizzo ITIA - INFORMATICA E TELECOMUNICAZIONI ARTICOLAZIONE "INFORMATICA" - Disciplina: INFORMATICA

- [Sessione ordinaria 2017 - seconda prova scritta - Indirizzo ITIA - INFORMATICA E TELECOMUNICAZIONI ARTICOLAZIONE "INFORMATICA" - Disciplina: INFORMATICA](#sessione-ordinaria-2017---seconda-prova-scritta---indirizzo-itia---informatica-e-telecomunicazioni-articolazione-informatica---disciplina-informatica)
  - [Traccia della prova](#traccia-della-prova)
  - [Svolgimento della prima parte](#svolgimento-della-prima-parte)
    - [Prima Parte](#prima-parte)
      - [1. Analisi della Realtà di Riferimento e Schema Concettuale (E/R)](#1-analisi-della-realtà-di-riferimento-e-schema-concettuale-er)
      - [2. Schema Logico Relazionale](#2-schema-logico-relazionale)
        - [Script completo per la creazione del database (non richiesto dalla traccia)](#script-completo-per-la-creazione-del-database-non-richiesto-dalla-traccia)
      - [3. Interrogazioni SQL (MariaDB)](#3-interrogazioni-sql-mariadb)
      - [4. Progetto di Massima dell'Applicazione Web](#4-progetto-di-massima-dellapplicazione-web)
        - [Discussione Architettura e Deployment](#discussione-architettura-e-deployment)
    - [Seconda Parte](#seconda-parte)
      - [Quesito I: Gestione Automatica Posti Disponibili](#quesito-i-gestione-automatica-posti-disponibili)
      - [Quesito II: Relazione Tecnica sulla Piattaforma Car Pooling](#quesito-ii-relazione-tecnica-sulla-piattaforma-car-pooling)
      - [Struttura della Relazione Tecnica](#struttura-della-relazione-tecnica)
        - [Esempio Significativo di Contenuto (Estratto dal Capitolo 3.2)](#esempio-significativo-di-contenuto-estratto-dal-capitolo-32)
        - [Utilizzo di Diagrammi UML](#utilizzo-di-diagrammi-uml)
      - [Quesito III: Schema Relazionale Film/Attore/Recita](#quesito-iii-schema-relazionale-filmattorerecita)
      - [Quesito IV: Applicazione Web Responsive per Eventi Culturali](#quesito-iv-applicazione-web-responsive-per-eventi-culturali)

## Traccia della prova

[La traccia della prova](https://www.istruzione.it/esame_di_stato/201617/Istituti%20tecnici/Ordinaria/I044_ORD17.pdf) è disponibile sul sito del Ministero dell'Istruzione e del Merito.

## Svolgimento della prima parte

### Prima Parte

#### 1\. Analisi della Realtà di Riferimento e Schema Concettuale (E/R)

Seguendo l'approccio RASD (Requisiti, Analisi, Specifica, Design) procediamo con l'analisi.

**A. Dizionario dei Dati e Requisiti:**

- **Utenti:** Entità generica che rappresenta chi interagisce con la piattaforma. Può specializzarsi in Autista o Passeggero. Ogni utente ha recapito telefonico ed email.
- **Autista:** Utente che offre passaggi. Deve registrarsi fornendo generalità, numero e scadenza patente, dati automobile, recapito telefonico, email, fotografia.
- **Passeggero:** Utente che cerca passaggi. Deve registrarsi fornendo cognome, nome, documento di identità, recapito telefonico, email.
- **Automobile:** Veicolo utilizzato dall'autista. Sono richiesti i "dati dell'automobile" (ipotizziamo marca, modello, targa, colore, anno immatricolazione). Un autista può avere una o più auto, ma per un viaggio ne usa una specifica.
- **Viaggio:** Tragitto offerto da un autista. Caratterizzato da città di partenza, città di destinazione, data e ora di partenza, contributo economico per passeggero, tempi di percorrenza stimati. Può avere dettagli aggiuntivi come soste previste, possibilità di caricare bagaglio o animali. Un viaggio ha uno stato (es: Aperto, Chiuso alle prenotazioni).
- **Prenotazione:** Richiesta di un passeggero per un determinato viaggio. Ha uno stato (es: Richiesta, Accettata, Rifiutata). Contiene un riferimento al passeggero e al viaggio.
- **Feedback:** Valutazione lasciata da un utente (passeggero o autista) su un altro utente al termine di un viaggio. Include un voto numerico e un giudizio discorsivo.
- **Funzionalità Principali:**
    - Registrazione Utenti (Autisti e Passeggeri).
    - Inserimento Viaggi da parte degli Autisti.
    - Ricerca Viaggi da parte dei Passeggeri (per città e data).
    - Visualizzazione Dettagli Viaggio e Autista (inclusi feedback).
    - Prenotazione Viaggio da parte dei Passeggeri.
    - Gestione Prenotazioni da parte degli Autisti (Accetta/Rifiuta, visualizzazione feedback passeggero).
    - Notifiche Email (prenotazione a autista, accettazione/rifiuto a passeggero).
    - Inserimento Feedback (Passeggero -> Autista, Autista -> Passeggero).
    - Chiusura Prenotazioni Viaggio da parte dell'Autista.

**B. Attori e Casi d'Uso (Use Case Diagram):**

:memo: Nota: Per il compito in classe basta elencarli e dettagliarne qualcuno, poi si scrive il diagramma dei casi d'uso principali come mostrato sotto.

**1\. Registrazione Utente**

- **Attori:** Utente non registrato, Sistema.
- **Descrizione/Flusso Base:**
    1. L'Utente accede alla pagina/sezione di registrazione della piattaforma.
    2. Il Sistema presenta un form per inserire i dati comuni (es. Email, Telefono, Nome, Cognome - basato sullo schema logico derivato).
    3. L'Utente sceglie se registrarsi principalmente come Autista o Passeggero (o il sistema potrebbe gestire i ruoli separatamente dopo la registrazione base).
    4. **Se Autista:** Il Sistema richiede dati aggiuntivi: generalità (già inserite), numero e scadenza patente, dati dell'automobile (marca, modello, targa, ecc.), fotografia.  
    5. **Se Passeggero:** Il Sistema richiede dati aggiuntivi: cognome e nome (già inseriti), documento di identità.  
    6. L'Utente inserisce i dati richiesti.
    7. Il Sistema valida i dati (es. formato email, unicità email/telefono/patente/documento).
    8. Il Sistema crea l'account utente e memorizza i dati specifici del ruolo (Autista/Passeggero).
    9. Il Sistema conferma all'utente l'avvenuta registrazione.
- **Precondizioni:** L'utente non deve avere già un account con la stessa email/telefono.
- **Postcondizioni (Successo):** L'utente ha un account sulla piattaforma con il ruolo specificato (Autista o Passeggero o entrambi) ed è in grado di effettuare il login.

**2\. Inserimento Viaggio**

- **Attori:** Autista, Sistema.
- **Descrizione/Flusso Base:**
    1. L'Autista effettua il login sulla piattaforma.
    2. L'Autista accede alla funzione per inserire un nuovo viaggio.
    3. Il Sistema presenta un form per inserire i dettagli del viaggio.
    4. L'Autista inserisce: città di partenza, città di destinazione, data ed ora di partenza, contributo economico richiesto per passeggero, tempi di percorrenza stimati.  
    5. (Opzionale, ma menzionato) L'Autista può inserire dettagli aggiuntivi: soste previste, possibilità di caricare bagaglio o animali.  
    6. (Aggiunta dal Quesito I) L'Autista inserisce il numero massimo di posti disponibili per quel viaggio.  
    7. L'Autista seleziona l'automobile che utilizzerà per il viaggio tra quelle associate al suo profilo.  
    8. L'Autista conferma l'inserimento.
    9. Il Sistema valida i dati.
    10. Il Sistema memorizza il nuovo viaggio associandolo all'autista e all'auto selezionata, impostando lo stato iniziale su 'Aperto'.  
    11. Il Sistema conferma all'autista l'avvenuto inserimento.
- **Precondizioni:** L'Autista deve essere registrato e loggato. L'Autista deve avere almeno un'automobile registrata sul proprio profilo.
- **Postcondizioni (Successo):** Un nuovo viaggio è stato creato, è visibile nelle ricerche (se soddisfa i criteri) ed è disponibile per le prenotazioni.

**3\. Ricerca Viaggi**

- **Attori:** Utente (registrato o non), Passeggero, Sistema.
- **Descrizione/Flusso Base:**
    1. L'Utente/Passeggero accede alla funzione di ricerca viaggi.
    2. Il Sistema presenta un form per inserire i criteri di ricerca.
    3. L'Utente/Passeggero inserisce almeno città di partenza, città di destinazione e data desiderata.  
    4. L'Utente/Passeggero avvia la ricerca.
    5. Il Sistema interroga il database cercando i viaggi che corrispondono ai criteri, che abbiano prenotazioni non ancora chiuse e (dal Quesito I) che abbiano posti disponibili.  
    6. Il Sistema presenta all'utente un elenco dei viaggi trovati. Per ogni viaggio mostra informazioni essenziali (orario, costo), le caratteristiche dell'autista (nome, foto?, voto medio ), e i dettagli del viaggio inseriti dall'autista (soste, bagaglio, animali ). (Dal Quesito I) Mostra anche il numero di posti disponibili e il numero di prenotazioni non ancora accettate.  
- **Precondizioni:** Nessuna particolare (accessibile anche a utenti non loggati).
- **Postcondizioni (Successo):** L'utente visualizza un elenco di viaggi che corrispondono ai suoi criteri di ricerca, oppure un messaggio che indica che non sono stati trovati viaggi.

**4\. Prenotazione Viaggio**

- **Attori:** Passeggero, Sistema.
- **Descrizione/Flusso Base:**
    1. Il Passeggero, dopo aver effettuato una ricerca (UC3) o navigato tra i viaggi disponibili, seleziona un viaggio di suo interesse.  
    2. Il Passeggero esamina i dettagli del viaggio, dell'autista e i relativi feedback.  
    3. Il Passeggero decide di prenotare e attiva l'apposita funzione (es. click su un bottone "Prenota").
    4. Il Sistema registra la richiesta di prenotazione, associandola al passeggero e al viaggio scelto, impostando lo stato iniziale su 'Richiesta'.
    5. Il Sistema invia una notifica (email) all'autista del viaggio scelto, informandolo della nuova prenotazione e fornendo le informazioni sul passeggero.  
    6. Il Sistema conferma al Passeggero che la richiesta di prenotazione è stata inviata all'autista.
- **Precondizioni:** Il Passeggero deve essere registrato e loggato. Il viaggio selezionato deve avere posti disponibili (vedi UC3) e stato 'Aperto'. Il Passeggero non deve essere l'autista del viaggio stesso. Il Passeggero non deve avere già una prenotazione attiva per lo stesso viaggio.
- **Postcondizioni (Successo):** Una nuova prenotazione è stata creata con stato 'Richiesta'. L'autista è stato notificato. Il passeggero è in attesa di una risposta.

**5\. Gestione Prenotazioni (Accetta/Rifiuta)**

- **Attori:** Autista, Sistema.
- **Descrizione/Flusso Base:**
    1. L'Autista riceve la notifica di una nuova richiesta di prenotazione (vedi UC4) o accede alla sezione "Le mie prenotazioni ricevute" sulla piattaforma.
    2. L'Autista visualizza i dettagli della prenotazione e del passeggero che l'ha effettuata.
    3. L'Autista può consultare sul portale il voto medio e i giudizi dei feedback ricevuti da quel passeggero da parte di altri autisti precedenti.  
    4. L'Autista decide se accettare o rifiutare la prenotazione.  
    5. **Se Accetta:** a. (Controllo dal Quesito I) Il Sistema verifica se ci sono ancora posti disponibili nel viaggio (considerando le prenotazioni già accettate). Se non ci sono posti, l'accettazione fallisce (flusso alternativo). b. Il Sistema aggiorna lo stato della prenotazione a 'Accettata'. c. (Gestione dal Quesito I) Il Sistema decrementa (logicamente) il numero di posti disponibili per quel viaggio. d. Il Sistema invia una notifica (email) di accettazione al passeggero, contenente un promemoria del viaggio (città, data, orario, dati autista e auto).  
    6. **Se Rifiuta:** a. Il Sistema aggiorna lo stato della prenotazione a 'Rifiutata'. b. Il Sistema invia una notifica (email) di rifiuto al passeggero.  
    7. Il Sistema aggiorna la visualizzazione delle prenotazioni per l'autista.
- **Precondizioni:** Esiste una prenotazione con stato 'Richiesta' per un viaggio offerto dall'autista. L'Autista è loggato.
- **Postcondizioni (Successo):** Lo stato della prenotazione è stato aggiornato ('Accettata' o 'Rifiutata'). Il passeggero è stato notificato della decisione. Se accettata, un posto nel viaggio è considerato occupato.

**6\. Inserimento Feedback**

- **Attori:** Passeggero, Autista, Sistema.
- **Descrizione/Flusso Base:**
    1. Dopo che un viaggio si è concluso (la prenotazione è stata completata o il sistema determina la conclusione del viaggio).
    2. Il Passeggero accede alla piattaforma e alla sezione per lasciare feedback.
    3. Il Sistema mostra le prenotazioni relative a viaggi conclusi per cui non è stato ancora lasciato feedback sull'autista.
    4. Il Passeggero seleziona la prenotazione/viaggio e inserisce un feedback sull'autista: voto numerico e giudizio discorsivo.  
    5. Il Sistema salva il feedback del passeggero associandolo alla prenotazione (e quindi implicitamente all'autista e al viaggio).
    6. Analogamente, l'Autista accede alla piattaforma per lasciare feedback.
    7. Il Sistema mostra le prenotazioni relative a viaggi conclusi per cui non è stato ancora lasciato feedback sul passeggero.
    8. L'Autista seleziona la prenotazione/passeggero e inserisce un feedback sul passeggero: voto numerico e giudizio discorsivo.  
    9. Il Sistema salva il feedback dell'autista associandolo alla prenotazione (e quindi implicitamente al passeggero e al viaggio).
    10. Il Sistema aggiorna i dati aggregati (voto medio) per l'autista e per il passeggero che hanno ricevuto il feedback.  
- **Precondizioni:** Il viaggio associato alla prenotazione deve essersi concluso. L'utente (Passeggero o Autista) deve essere loggato. Non deve essere già stato inserito un feedback per quella specifica interazione (Passeggero->Autista o Autista->Passeggero) relativa a quella prenotazione.
- **Postcondizioni (Successo):** Il feedback è stato salvato. Il voto medio dell'utente recensito è stato aggiornato. Il feedback è visibile ad altri utenti secondo le regole.  

**7\. Chiusura Prenotazioni Viaggio**

- **Attori:** Autista, Sistema.
- **Descrizione/Flusso Base:**
    1. L'Autista accede alla gestione di un viaggio che ha inserito precedentemente e le cui prenotazioni sono ancora 'Aperte'.
    2. L'Autista decide che non vuole più accettare nuove prenotazioni per quel viaggio (ad esempio perché ha raggiunto il numero desiderato di passeggeri accettati, o per altri motivi).  
    3. L'Autista attiva l'apposita funzione sul portale per "chiudere le prenotazioni".  
    4. Il Sistema aggiorna lo stato del viaggio a 'Chiuso'.
    5. Il Sistema conferma all'autista l'avvenuta modifica.
- **Precondizioni:** L'Autista è loggato ed è il proprietario del viaggio. Lo stato del viaggio è 'Aperto'.
- **Postcondizioni (Successo):** Lo stato del viaggio è 'Chiuso'. Il viaggio non apparirà più nelle ricerche di nuovi passeggeri (o verrà indicato come non prenotabile). Le prenotazioni esistenti con stato 'Richiesta' potrebbero rimanere gestibili dall'autista o essere automaticamente rifiutate (ipotesi aggiuntiva da definire). *Nota: Questo caso d'uso diventa meno rilevante o cambia natura con l'introduzione della gestione automatica dei posti del Quesito I, dove la chiusura avviene implicitamente quando i posti accettati raggiungono il massimo.*
- **Diagramma dei Casi d'Uso Principali:**

![Diagramma dei casi d'uso in svg](piattaforma-car-pooling-use-cases.svg)

Immagine dei casi d'uso generata con il codice per [PlantUML](https://www.plantuml.com/plantuml/uml/):

```text
@startuml "Piattaforma Car Pooling - Casi d'Uso"

left to right direction

' Attori
actor "Utente" as utente
actor "Passeggero" as passeggero
actor "Autista" as autista
actor "Sistema" as sistema <<system>>

' Ereditarietà tra attori
passeggero --|> utente
autista --|> utente

rectangle "Piattaforma Car Pooling" {
  ' Casi d'uso dell'Utente generico
  usecase "Ricerca Viaggi" as RicercaViaggi
  usecase "Registrazione Utente" as Registrazione
  
  ' Casi d'uso del Passeggero
  usecase "Visualizza Feedback su Autista" as VisualizzaFeedbackAutista
  usecase "Prenotazione Viaggio" as PrenotaViaggio
  usecase "Visualizza Stato Prenotazione" as VisualizzaStato
  usecase "Inserimento Feedback\n(Passeggero su Autista)" as InserisciFeedbackP
  
  ' Casi d'uso dell'Autista
  usecase "Inserimento Viaggio" as InserisciViaggio
  usecase "Gestione Prenotazioni Ricevute\n(Accetta/Rifiuta)" as GestisciPrenotazioni
  usecase "Visualizza Feedback su Passeggero" as VisualizzaFeedbackPasseggero
  usecase "Inserimento Feedback\n(Autista su Passeggero)" as InserisciFeedbackA
  usecase "Chiusura Prenotazioni Viaggio\n(Manuale)" as ChiudiPrenotazioni
}

' Relazioni tra attori e casi d'uso
utente --> RicercaViaggi
utente --> Registrazione

passeggero --> VisualizzaFeedbackAutista
passeggero --> PrenotaViaggio
passeggero --> VisualizzaStato
passeggero --> InserisciFeedbackP

autista --> InserisciViaggio
autista --> GestisciPrenotazioni
autista --> VisualizzaFeedbackPasseggero
autista --> InserisciFeedbackA
autista --> ChiudiPrenotazioni

' Relazioni con il sistema (notifiche)
PrenotaViaggio ..> sistema : Notifica Autista
GestisciPrenotazioni ..> sistema : Notifica Passeggero

' Note aggiuntive
note bottom of passeggero
  Il Passeggero eredita tutte le 
  funzionalità dell'Utente generico
end note

note bottom of autista
  L'Autista eredita tutte le
  funzionalità dell'Utente generico
end note

note bottom of ChiudiPrenotazioni
  Diventa meno rilevante con la
  gestione automatica dei posti
end note

@enduml
```

**C. Schema Concettuale E/R (Entity-Relationship):**

- **Entità:**

    - `UTENTE` (IdUtente PK, Email, Telefono, Nome, Cognome, TipoUtente{'Autista', 'Passeggero'}) - *Nota: Generalizzazione/Specializzazione.*
    - `AUTISTA` (-> `UTENTE`) (NumPatente, ScadenzaPatente, Fotografia)
    - `PASSEGGERO` (-> `UTENTE`) (DocumentoIdentita)
    - `AUTOMOBILE` (IdAuto PK, Targa, Marca, Modello, Colore, AnnoImm) - *Ipotesi aggiuntiva sui dati.*
    - `VIAGGIO` (IdViaggio PK, CittaPartenza, CittaDestinazione, DataOraPartenza, ContributoEconomico, TempoStimato, DescrizioneAggiuntiva, StatoViaggio{'Aperto', 'Chiuso'})
    - `PRENOTAZIONE` (IdPrenotazione PK, DataOraPrenotazione, StatoPrenotazione{'Richiesta', 'Accettata', 'Rifiutata'})
    - `FEEDBACK` (IdFeedback PK, VotoNumerico, GiudizioDiscorsivo, DataOraFeedback)
- **Relazioni:**

    - `Possiede` (N:M tra `AUTISTA` e `AUTOMOBILE`, con attributo DataAssociazione) - *Ipotesi: un autista può avere più auto e un'auto può essere (teoricamente) condivisa, anche se improbabile. Semplificazione: 1:N tra Autista e Auto se un'auto appartiene a un solo autista.* Assumiamo 1:N per semplicità: `AUTISTA` (1) --- `Possiede` --- (N) `AUTOMOBILE`.
    - `Offre` (1:N tra `AUTISTA` e `VIAGGIO`)
    - `Utilizza` (1:1 tra `VIAGGIO` e `AUTOMOBILE`) - *Un viaggio specifico usa una sola auto*.
    - `Richiede` (1:N tra `PASSEGGERO` e `PRENOTAZIONE`)
    - `Riguarda` (1:N tra `VIAGGIO` e `PRENOTAZIONE`)
    - `LasciaFeedbackAutista` (1:N tra `PASSEGGERO` e `FEEDBACK`, riferito a un viaggio specifico) \- Ternaria tra PASSEGGERO, AUTISTA (ricevente), VIAGGIO. Semplificabile se il feedback è legato alla prenotazione.
    - `LasciaFeedbackPasseggero` (1:N tra `AUTISTA` e `FEEDBACK`, riferito a un viaggio specifico) \- Ternaria tra AUTISTA, PASSEGGERO (ricevente), VIAGGIO. Semplificabile.
    - `RiceveFeedbackAutista` (1:N tra `AUTISTA` e `FEEDBACK`)
    - `RiceveFeedbackPasseggero` (1:N tra `PASSEGGERO` e `FEEDBACK`)
- **Vincoli e Ipotesi Aggiuntive:**

    - Email e Telefono sono unici per `UTENTE`.
    - NumPatente, DocumentoIdentita, Targa sono unici.
    - Un utente non può essere contemporaneamente Autista e Passeggero (o meglio, si registra come utente e poi può agire in entrambi i ruoli, ma la specializzazione nello schema E/R potrebbe essere gestita diversamente nel logico, ad esempio con campi specifici nella tabella Utente). Assumiamo che un Utente possa avere entrambi i ruoli, quindi la specializzazione diventa attributi nullable nella tabella Utenti o tabelle separate collegate 1:1 a Utente. Scegliamo tabelle separate per chiarezza.
    - Il contributo economico è per passeggero.
    - Il voto numerico del feedback è in una scala definita (es. 1-5).
    - La fotografia dell'autista è memorizzata come BLOB o come path a un file storage. Scegliamo path per semplicità.
    - `StatoViaggio`: 'Aperto', 'Chiuso'.
    - `StatoPrenotazione`: 'Richiesta', 'Accettata', 'Rifiutata', 'Completata' (post-viaggio).
    - I feedback possono essere inseriti solo dopo che il viaggio è avvenuto (StatoPrenotazione = 'Completata').
    - Le città potrebbero essere normalizzate in una tabella `CITTA` a parte. Per semplicità le lasciamo come stringhe.

- **Schema E/R Iniziale (Semplificato in versione Mermaid):**

```mermaid
erDiagram
    UTENTE {
        string IdUtente PK
        string Email
        string Telefono
        string Nome
        string Cognome
        string TipoUtente
    }
    AUTISTA {
        string IdUtente PK
        string NumPatente
        date ScadenzaPatente
        string Fotografia
    }
    PASSEGGERO {
        string IdUtente PK
        string DocumentoIdentita
    }
    AUTOMOBILE {
        string IdAuto PK
        string Targa
        string Marca
        string Modello
        string Colore
        int AnnoImm
    }
    VIAGGIO {
        string IdViaggio PK
        string CittaPartenza
        string CittaDestinazione
        date DataOraPartenza
        float ContributoEconomico
        int TempoStimato
        string DescrizioneAggiuntiva
        string StatoViaggio
    }
    PRENOTAZIONE {
        string IdPrenotazione PK
        date DataOraPrenotazione
        string StatoPrenotazione
    }
    FEEDBACK {
        string IdFeedback PK
        int VotoNumerico
        string GiudizioDiscorsivo
        date DataOraFeedback
    }
    
    UTENTE ||--|| AUTISTA : "generalizza"
    UTENTE ||--|| PASSEGGERO : "generalizza"
    
    AUTISTA ||--o{ AUTOMOBILE : "Possiede"
    AUTISTA ||--o{ VIAGGIO : "Offre"
    AUTOMOBILE ||--o| VIAGGIO : "Utilizza"
    PASSEGGERO ||--o{ PRENOTAZIONE : "Richiede"
    VIAGGIO ||--o{ PRENOTAZIONE : "Riguarda"
    
    PASSEGGERO ||--o{ FEEDBACK : "LasciaFeedback"
    AUTISTA ||--o{ FEEDBACK : "RiceveFeedback"
    AUTISTA ||--o{ FEEDBACK : "LasciaFeedback"
    PASSEGGERO ||--o{ FEEDBACK : "RiceveFeedback"
    VIAGGIO ||--o{ FEEDBACK : "Riguarda"
```

- **Schema E/R ristrutturato (semplificato in versione Mermaid)**

    - Per semplificare i feedback sono collegati alla PRENOTAZIONE (che lega implicitamente passeggero, autista e viaggio).
    - Per distinguere i feedback ricevuti dall'autista dai feedback ricevuti dai passeggeri decidiamo di sdoppiare l'entità feedback prevedendo un'entità `FeedbackAutista` che rappresenta il feedback ricevuto dall'autista e un'entità `FeedbackPasseggero` che rappresenta il feedback ricevuto dal passeggero.
    - La gerarchia ISA tra `Utente` e le entità figlie `Autista` e `Passeggero` viene ristrutturata lasciando sia l'entità genitore che le entità figlie poiché i ruoli completamente diversi delle entità figlie fanno in modo che queste siano collegate a parti completamente diverse del modello dei dati, pur conservando diversi attributi in comune (lasciati nell'entità genitore).
    - Assumiamo che:
      - StatoViaggio "ENUM('Aperto', 'Chiuso') DEFAULT 'Aperto'"
      - StatoPrenotazione "ENUM('Richiesta', 'Accettata', 'Rifiutata', 'Completata') DEFAULT 'Richiesta'"
      - VotoNumerico "CHECK(VotoNumerico BETWEEN 1 AND 5)"

```mermaid
erDiagram
    Utenti {
        int IdUtente PK
        string Email
        string Telefono
        string Nome
        string Cognome
    }
    
    Autisti {
        int IdAutista PK
        string NumPatente
        date ScadenzaPatente
        string PathFotografia
    }
    
    Passeggeri {
        int IdPasseggero
        string DocumentoIdentita
    }
    
    Automobili {
        int IdAuto PK
        string Targa
        string Marca
        string Modello
        string Colore
        int AnnoImm
    }
    
    Viaggi {
        int IdViaggio PK
        string CittaPartenza
        string CittaDestinazione
        datetime DataOraPartenza
        decimal ContributoEconomico
        int TempoStimatoMinuti
        string DescrizioneAggiuntiva
        enum StatoViaggio
    }
    
    Prenotazioni {
        int IdPrenotazione PK
        datetime DataOraPrenotazione
        enum StatoPrenotazione
    }
    
    FeedbackAutisti {
        int IdFeedbackA PK
        int VotoNumerico
        text GiudizioDiscorsivo
        datetime DataOraFeedback
    }
    
    FeedbackPasseggeri {
        int IdFeedbackP PK
        int VotoNumerico
        text GiudizioDiscorsivo
        datetime DataOraFeedback
    }
    
    Utenti ||--|| Autisti : Ha
    Utenti ||--|| Passeggeri : Ha
    Autisti ||--o{ Automobili : Possiede
    Autisti ||--o{ Viaggi : Offre
    Automobili ||--o{ Viaggi : UsataIn
    Passeggeri ||--o{ Prenotazioni : Effettua
    Viaggi ||--o{ Prenotazioni : Riceve
    Prenotazioni ||--o| FeedbackAutisti : "il guidatore lascia"
    Prenotazioni ||--o| FeedbackPasseggeri : "il passeggero lascia"
```

#### 2\. Schema Logico Relazionale

Mapping dello schema E/R ristrutturato in relazioni (tabelle) per un database relazionale (MariaDB).

- **Utenti** (`IdUtente` PK, `Email` UK, `Telefono` UK, `Nome`, `Cognome`)
    - *Nota:* Il TipoUtente non è necessario se usiamo tabelle separate per i ruoli.
- **Autisti** (`IdAutista` PK FK -> Utenti.IdUtente, `NumPatente` UK, `ScadenzaPatente`, `PathFotografia`)
- **Passeggeri** (`IdPasseggero` PK FK -> Utenti.IdUtente, `DocumentoIdentita` UK)
- **Automobili** (`IdAuto` PK, `Targa` UK, `Marca`, `Modello`, `Colore`, `AnnoImm`, `IdAutista` FK -> Autisti.IdAutista)
- **Viaggi** (`IdViaggio` PK, `CittaPartenza`, `CittaDestinazione`, `DataOraPartenza`, `ContributoEconomico`, `TempoStimatoMinuti`, `DescrizioneAggiuntiva`, `StatoViaggio` ENUM('Aperto', 'Chiuso') DEFAULT 'Aperto', `IdAutista` FK -> Autisti.IdAutista, `IdAuto` FK -> Automobili.IdAuto)
- **Prenotazioni** (`IdPrenotazione` PK, `DataOraPrenotazione`, `StatoPrenotazione` ENUM('Richiesta', 'Accettata', 'Rifiutata', 'Completata') DEFAULT 'Richiesta', `IdPasseggero` FK -> Passeggeri.IdPasseggero, `IdViaggio` FK -> Viaggi.IdViaggio)
- **FeedbackAutisti** (`IdFeedbackA` PK, `VotoNumerico` INT CHECK(VotoNumerico BETWEEN 1 AND 5), `GiudizioDiscorsivo` TEXT, `DataOraFeedback`, `IdPrenotazione` UK FK -> Prenotazioni.IdPrenotazione) - *Feedback lasciato dall'autista sul passeggero*
- **FeedbackPasseggeri** (`IdFeedbackP` PK, `VotoNumerico` INT CHECK(VotoNumerico BETWEEN 1 AND 5), `GiudizioDiscorsivo` TEXT, `DataOraFeedback`, `IdPrenotazione` UK FK -> Prenotazioni.IdPrenotazione) - *Feedback lasciato dal passeggero sull'autista*

```mermaid
erDiagram
    Utenti {
        int IdUtente PK
        string Email UK
        string Telefono UK
        string Nome
        string Cognome
    }
    
    Autisti {
        int IdAutista PK "FK -> Utenti.IdUtente"
        string NumPatente UK
        date ScadenzaPatente
        string PathFotografia
    }
    
    Passeggeri {
        int IdPasseggero PK "FK -> Utenti.IdUtente"
        string DocumentoIdentita UK
    }
    
    Automobili {
        int IdAuto PK
        string Targa UK
        string Marca
        string Modello
        string Colore
        int AnnoImm
        int IdAutista FK "-> Autisti.IdAutista"
    }
    
    Viaggi {
        int IdViaggio PK
        string CittaPartenza
        string CittaDestinazione
        datetime DataOraPartenza
        decimal ContributoEconomico
        int TempoStimatoMinuti
        string DescrizioneAggiuntiva
        enum StatoViaggio "ENUM('Aperto', 'Chiuso') DEFAULT 'Aperto'"
        int IdAutista FK "-> Autisti.IdAutista"
        int IdAuto FK "-> Automobili.IdAuto"
    }
    
    Prenotazioni {
        int IdPrenotazione PK
        datetime DataOraPrenotazione
        enum StatoPrenotazione "ENUM('Richiesta', 'Accettata', 'Rifiutata', 'Completata') DEFAULT 'Richiesta'"
        int IdPasseggero FK "-> Passeggeri.IdPasseggero"
        int IdViaggio FK "-> Viaggi.IdViaggio"
    }
    
    FeedbackAutisti {
        int IdFeedbackA PK
        int VotoNumerico "CHECK(VotoNumerico BETWEEN 1 AND 5)"
        text GiudizioDiscorsivo
        datetime DataOraFeedback
        int IdPrenotazione UK "FK -> Prenotazioni.IdPrenotazione"
    }
    
    FeedbackPasseggeri {
        int IdFeedbackP PK
        int VotoNumerico "CHECK(VotoNumerico BETWEEN 1 AND 5)"
        text GiudizioDiscorsivo
        datetime DataOraFeedback
        int IdPrenotazione UK "FK -> Prenotazioni.IdPrenotazione"
    }
    
    Utenti ||--|| Autisti : has
    Utenti ||--|| Passeggeri : has
    Autisti ||--o{ Automobili : owns
    Autisti ||--o{ Viaggi : offers
    Automobili ||--o{ Viaggi : is_used_in
    Passeggeri ||--o{ Prenotazioni : makes
    Viaggi ||--o{ Prenotazioni : receives
    Prenotazioni ||--o| FeedbackAutisti : "driver leaves"
    Prenotazioni ||--o| FeedbackPasseggeri : "passenger leaves"
```

Normalizzazione:

Lo schema proposto è ragionevolmente normalizzato (3NF). Le dipendenze funzionali sembrano rispettare le chiavi primarie. Ad esempio, in Viaggi, tutti gli attributi dipendono da IdViaggio. Non ci sono evidenti dipendenze transitive problematiche. La possibile denormalizzazione delle città è stata considerata ma scartata per semplicità. L'uso di ENUM per gli stati è una scelta implementativa valida in MariaDB/MySQL.

##### Script completo per la creazione del database (non richiesto dalla traccia)

```sql
-- PASSO 1: Creazione del Database (Schema)
CREATE DATABASE IF NOT EXISTS carpooling_db;
   -- CHARACTER SET utf8mb4 -- è il default
   -- COLLATE utf8mb4_unicode_ci; -- è il default

-- PASSO 2: Selezione del Database per le operazioni successive
USE carpooling_db;

-- PASSO 3: Creazione delle Tabelle 

-- Tabella Utenti
CREATE TABLE Utenti (
    IdUtente INT AUTO_INCREMENT PRIMARY KEY,
    Email VARCHAR(255) NOT NULL UNIQUE,
    Telefono VARCHAR(20) NOT NULL UNIQUE,
    Nome VARCHAR(100) NOT NULL,
    Cognome VARCHAR(100) NOT NULL,
    INDEX idx_utenti_cognome_nome (Cognome, Nome)
) ENGINE=InnoDB;

-- Tabella Autisti
CREATE TABLE Autisti (
    IdAutista INT PRIMARY KEY,
    NumPatente VARCHAR(50) NOT NULL UNIQUE,
    ScadenzaPatente DATE NOT NULL,
    PathFotografia VARCHAR(512),
    CONSTRAINT fk_autisti_utenti FOREIGN KEY (IdAutista) REFERENCES Utenti(IdUtente) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB;

-- Tabella Passeggeri
CREATE TABLE Passeggeri (
    IdPasseggero INT PRIMARY KEY,
    DocumentoIdentita VARCHAR(100) NOT NULL UNIQUE,
    CONSTRAINT fk_passeggeri_utenti FOREIGN KEY (IdPasseggero) REFERENCES Utenti(IdUtente) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB;

-- Tabella Automobili
CREATE TABLE Automobili (
    IdAuto INT AUTO_INCREMENT PRIMARY KEY,
    Targa VARCHAR(10) NOT NULL UNIQUE,
    Marca VARCHAR(50) NOT NULL,
    Modello VARCHAR(50) NOT NULL,
    Colore VARCHAR(30),
    AnnoImm INT,
    IdAutista INT NOT NULL,
    CONSTRAINT fk_automobili_autisti FOREIGN KEY (IdAutista) REFERENCES Autisti(IdAutista) ON DELETE RESTRICT ON UPDATE CASCADE,
    INDEX idx_automobili_autista (IdAutista)
) ENGINE=InnoDB;

-- Tabella Viaggi
CREATE TABLE Viaggi (
    IdViaggio INT AUTO_INCREMENT PRIMARY KEY,
    CittaPartenza VARCHAR(100) NOT NULL,
    CittaDestinazione VARCHAR(100) NOT NULL,
    DataOraPartenza DATETIME NOT NULL,
    ContributoEconomico DECIMAL(6, 2) NOT NULL,
    TempoStimatoMinuti INT,
    DescrizioneAggiuntiva TEXT,
    StatoViaggio ENUM('Aperto', 'Chiuso') NOT NULL DEFAULT 'Aperto',
    PostiDisponibiliIniziali INT NOT NULL DEFAULT 1,
    IdAutista INT NOT NULL,
    IdAuto INT NOT NULL,
    CONSTRAINT fk_viaggi_autisti FOREIGN KEY (IdAutista) REFERENCES Autisti(IdAutista) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_viaggi_automobili FOREIGN KEY (IdAuto) REFERENCES Automobili(IdAuto) ON DELETE RESTRICT ON UPDATE CASCADE,
    INDEX idx_viaggi_partenza_dest_data (CittaPartenza, CittaDestinazione, DataOraPartenza),
    INDEX idx_viaggi_autista (IdAutista),
    INDEX idx_viaggi_auto (IdAuto)
) ENGINE=InnoDB;

-- Tabella Prenotazioni
CREATE TABLE Prenotazioni (
    IdPrenotazione INT AUTO_INCREMENT PRIMARY KEY,
    DataOraPrenotazione DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    StatoPrenotazione ENUM('Richiesta', 'Accettata', 'Rifiutata', 'Completata') NOT NULL DEFAULT 'Richiesta',
    IdPasseggero INT NOT NULL,
    IdViaggio INT NOT NULL,
    CONSTRAINT fk_prenotazioni_passeggeri FOREIGN KEY (IdPasseggero) REFERENCES Passeggeri(IdPasseggero) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_prenotazioni_viaggi FOREIGN KEY (IdViaggio) REFERENCES Viaggi(IdViaggio) ON DELETE CASCADE ON UPDATE CASCADE,
    UNIQUE INDEX uk_prenotazioni_passeggero_viaggio (IdPasseggero, IdViaggio),
    INDEX idx_prenotazioni_viaggio (IdViaggio),
    INDEX idx_prenotazioni_stato (StatoPrenotazione)
) ENGINE=InnoDB;

-- Tabella FeedbackAutisti
CREATE TABLE FeedbackAutisti (
    IdFeedbackA INT AUTO_INCREMENT PRIMARY KEY,
    VotoNumerico INT NOT NULL,
    GiudizioDiscorsivo TEXT,
    DataOraFeedback DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IdPrenotazione INT NOT NULL UNIQUE,
    CONSTRAINT fk_feedback_autisti_prenotazioni FOREIGN KEY (IdPrenotazione) REFERENCES Prenotazioni(IdPrenotazione) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT chk_voto_autista CHECK (VotoNumerico BETWEEN 1 AND 5)
) ENGINE=InnoDB;

-- Tabella FeedbackPasseggeri
CREATE TABLE FeedbackPasseggeri (
    IdFeedbackP INT AUTO_INCREMENT PRIMARY KEY,
    VotoNumerico INT NOT NULL,
    GiudizioDiscorsivo TEXT,
    DataOraFeedback DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IdPrenotazione INT NOT NULL UNIQUE,
    CONSTRAINT fk_feedback_passeggeri_prenotazioni FOREIGN KEY (IdPrenotazione) REFERENCES Prenotazioni(IdPrenotazione) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT chk_voto_passeggero CHECK (VotoNumerico BETWEEN 1 AND 5)
) ENGINE=InnoDB;


-- PASSO 4: Creazione dell'Utente per l'Applicazione Web
-- IMPORTANTE: Sostituisci 'SUA_PASSWORD_SICURA_QUI' con una password robusta e casuale!
CREATE USER IF NOT EXISTS 'carpooling_app_user'@'localhost'
    IDENTIFIED BY 'SUA_PASSWORD_SICURA_QUI';

-- PASSO 5: Concessione dei Privilegi all'Utente sul Database specifico
-- Concede solo i permessi necessari per le operazioni CRUD (Create, Read, Update, Delete)
GRANT SELECT, INSERT, UPDATE, DELETE ON carpooling_db.* TO 'carpooling_app_user'@'localhost';

-- Nota: Se l'applicazione gestisce le migrazioni dello schema (es. con EF Core migrations),
-- potrebbe aver bisogno anche di privilegi ALTER, CREATE, DROP, INDEX, REFERENCES.
-- È più sicuro eseguire le migrazioni con un utente diverso con privilegi più alti
-- o concedere temporaneamente questi privilegi all'utente dell'app durante il deployment.
-- Per il funzionamento base CRUD, i permessi sopra sono sufficienti.

-- PASSO 6: Applicare le modifiche ai privilegi
FLUSH PRIVILEGES;
```

#### 3\. Interrogazioni SQL (MariaDB)

a) Elencare i viaggi disponibili per tratta e data:

```sql
SELECT
    -- Dati autista
    A.IdAutista,
    U.Nome AS NomeAutista,
    U.Cognome AS CognomeAutista,
    -- Dati auto
    AU.Marca AS MarcaAuto,
    AU.Modello AS ModelloAuto,
    AU.Targa AS TargaAuto,
    -- Dati viaggio
    V.DataOraPartenza,
    V.ContributoEconomico
FROM
    Viaggi AS V
JOIN
    Autisti AS A ON V.IdAutista = A.IdAutista
JOIN
    Utenti AS U ON A.IdAutista = U.IdUtente -- Join Utenti per nome/cognome autista
JOIN
    Automobili AS AU ON V.IdAuto = AU.IdAuto -- Join Automobili per dettagli auto
WHERE
    V.CittaPartenza = ?       -- Parametro: città di partenza fornita
    AND V.CittaDestinazione = ? -- Parametro: città di arrivo fornita
    AND DATE(V.DataOraPartenza) = ? -- Parametro: data fornita (es: '2024-12-25')
    AND V.StatoViaggio = 'Aperto'   -- Solo viaggi con prenotazioni non chiuse 
ORDER BY
    V.DataOraPartenza ASC;          -- Ordine crescente di orario 
```

b) Dati per promemoria prenotazione accettata:

```sql
SELECT
    -- Dati Viaggio
    V.CittaPartenza,
    V.CittaDestinazione,
    V.DataOraPartenza,
    -- Dati Autista (tramite Utenti alias U)
    U.Nome AS NomeAutista,
    U.Cognome AS CognomeAutista,
    U.Telefono AS TelefonoAutista,
    -- Dati Automobile
    AU.Marca AS MarcaAuto,
    AU.Modello AS ModelloAuto,
    AU.Targa AS TargaAuto,
    AU.Colore AS ColoreAuto
FROM
    Prenotazioni AS P
JOIN
    Viaggi AS V ON P.IdViaggio = V.IdViaggio -- Lega prenotazione al viaggio
JOIN
    Autisti AS A ON V.IdAutista = A.IdAutista -- Lega viaggio all'autista
JOIN
    Utenti AS U ON A.IdAutista = U.IdUtente -- Prende dati anagrafici autista (ALIAS RICHIESTO: U)
JOIN
    Automobili AS AU ON V.IdAuto = AU.IdAuto -- Prende dati auto usata per il viaggio
WHERE
    P.IdPrenotazione = ?          -- Parametro: ID della prenotazione specifica
    AND P.StatoPrenotazione = 'Accettata'; -- Assicura che sia stata accettata
```

c) Elenco passeggeri prenotati per un viaggio con voto medio superiore a X:

- **Soluzione con tabella temporanea (non modifica lo schema)**
  
    ```sql
    -- PASSO 1: Creare una tabella temporanea per memorizzare il voto medio di ciascun passeggero
    CREATE TEMPORARY TABLE TempFeedbackMedio (
        IdPasseggero INT PRIMARY KEY,
        AvgVoto DECIMAL(3, 2) -- Assumendo voti con max 1 decimale, es. 4.5
    );

    -- PASSO 2: Popolare la tabella temporanea calcolando il voto medio
    INSERT INTO TempFeedbackMedio (IdPasseggero, AvgVoto)
    SELECT
        Pre.IdPasseggero,
        AVG(FB.VotoNumerico) AS AvgVoto
    FROM
        Prenotazioni AS Pre -- Prenotazioni storiche (Alias: Pre)
    JOIN
        FeedbackAutisti AS FB ON Pre.IdPrenotazione = FB.IdPrenotazione -- Feedback ricevuti (Alias: FB)
    -- WHERE Pre.StatoPrenotazione = 'Completata' -- Opzionale: considera solo viaggi completati
    GROUP BY
        Pre.IdPasseggero;

    -- PASSO 3: Eseguire la query principale, unendo la tabella temporanea
    SELECT
        Pass.IdPasseggero,
        U.Nome AS NomePasseggero,
        U.Cognome AS CognomePasseggero,
        -- Prendo il voto medio dalla tabella temporanea (può essere NULL se il passeggero non è nella tabella temp)
        TFM.AvgVoto AS VotoMedioRicevutoDalPasseggero
    FROM
        Prenotazioni AS P -- Prenotazioni per il viaggio corrente (Alias: P)
    JOIN
        Passeggeri AS Pass ON P.IdPasseggero = Pass.IdPasseggero
    JOIN
        Utenti AS U ON Pass.IdPasseggero = U.IdUtente -- Dati anagrafici passeggero (Alias: U)
    LEFT JOIN
        TempFeedbackMedio AS TFM ON Pass.IdPasseggero = TFM.IdPasseggero -- Join con la tabella temporanea
    WHERE
        P.IdViaggio = ? -- Parametro: ID del viaggio che l'autista sta esaminando
        AND P.StatoPrenotazione = 'Richiesta' -- Solo prenotazioni in attesa
        AND (TFM.AvgVoto > ? OR TFM.AvgVoto IS NULL) -- Filtro: Voto medio > X OPPURE passeggero non trovato nella tabella temp (AvgVoto è NULL)
    ORDER BY
        VotoMedioRicevutoDalPasseggero DESC NULLS LAST, -- Opzionale: ordina per voto
        U.Cognome ASC,
        U.Nome ASC;

    -- PASSO 4: (Importante!) Rimuovere la tabella temporanea quando non serve più
    DROP TEMPORARY TABLE IF EXISTS TempFeedbackMedio;
    ```

    **Spiegazione dell'approccio con Tabella Temporanea:**

    1. **`CREATE TEMPORARY TABLE TempFeedbackMedio`**: Si crea una tabella che esisterà solo per la durata della sessione corrente. Conterrà l'ID del passeggero e il suo voto medio calcolato.
    2. **`INSERT INTO TempFeedbackMedio ... SELECT ...`**: Si esegue la query che calcola il voto medio per ogni passeggero (unendo `Prenotazioni` storiche `Pre` e `FeedbackAutisti` `FB` e raggruppando per passeggero) e si inseriscono i risultati nella tabella temporanea appena creata.
    3. **`SELECT ... FROM Prenotazioni P ... LEFT JOIN TempFeedbackMedio TFM ...`**: Si esegue la query finale. Questa seleziona i passeggeri dalla prenotazione corrente (`P`) e usa un `LEFT JOIN` con la tabella temporanea `TempFeedbackMedio` (`TFM`) per recuperare il voto medio precedentemente calcolato. Il `LEFT JOIN` è fondamentale per includere anche i passeggeri che non hanno una media (cioè, non sono presenti nella tabella temporanea). La clausola `WHERE` filtra poi in base al voto medio (o alla sua assenza).
    4. **`DROP TEMPORARY TABLE IF EXISTS TempFeedbackMedio`**: Alla fine, è buona norma eliminare esplicitamente la tabella temporanea per liberare risorse. `IF EXISTS` previene errori se, per qualche motivo, la tabella non dovesse esistere.

- **Soluzione con View (modifica lo schema)**

    **Passo 1 - Creazione della Vista:**

    Questa istruzione `CREATE VIEW` va eseguita una sola volta nel database per definire la vista.

    ```sql
    CREATE OR REPLACE VIEW VistaFeedbackMedioPasseggeri AS
    SELECT
        Pre.IdPasseggero,
        AVG(FB.VotoNumerico) AS AvgVoto
    FROM
        Prenotazioni AS Pre -- Prenotazioni storiche (Alias: Pre)
    JOIN
        FeedbackAutisti AS FB ON Pre.IdPrenotazione = FB.IdPrenotazione -- Feedback ricevuti (Alias: FB)
    -- WHERE Pre.StatoPrenotazione = 'Completata' -- Opzionale: considera solo viaggi completati
    GROUP BY
        Pre.IdPasseggero;
    ```

    **Spiegazione della Vista:**

    - `CREATE OR REPLACE VIEW VistaFeedbackMedioPasseggeri AS ...`: Crea una nuova vista chiamata `VistaFeedbackMedioPasseggeri` o la sostituisce se esiste già.
    - La query `SELECT` all'interno della vista è la stessa usata per popolare la tabella temporanea o nella CTE: calcola l'`IdPasseggero` e il suo voto medio (`AvgVoto`) basandosi sui feedback ricevuti nelle prenotazioni storiche.

    **Passo 2 - Utilizzo della Vista nella Query Finale:**

    Una volta che la vista è stata creata nel database, la query per ottenere l'elenco dei passeggeri diventa:

    ```sql
    SELECT
        Pass.IdPasseggero,
        U.Nome AS NomePasseggero,
        U.Cognome AS CognomePasseggero,
        -- Prendo il voto medio dalla vista (può essere NULL se il passeggero non è nella vista)
        VFMP.AvgVoto AS VotoMedioRicevutoDalPasseggero
    FROM
        Prenotazioni AS P -- Prenotazioni per il viaggio corrente (Alias: P)
    JOIN
        Passeggeri AS Pass ON P.IdPasseggero = Pass.IdPasseggero
    JOIN
        Utenti AS U ON Pass.IdPasseggero = U.IdUtente -- Dati anagrafici passeggero (Alias: U)
    LEFT JOIN
        VistaFeedbackMedioPasseggeri AS VFMP ON Pass.IdPasseggero = VFMP.IdPasseggero -- Join con la VISTA
    WHERE
        P.IdViaggio = ? -- Parametro: ID del viaggio che l'autista sta esaminando
        AND P.StatoPrenotazione = 'Richiesta' -- Solo prenotazioni in attesa
        AND (VFMP.AvgVoto > ? OR VFMP.AvgVoto IS NULL) -- Filtro: Voto medio > X OPPURE passeggero non trovato nella vista (AvgVoto è NULL)
    ORDER BY
        VotoMedioRicevutoDalPasseggero DESC NULLS LAST, -- Opzionale: ordina per voto
        U.Cognome ASC,
        U.Nome ASC;
    ```

**Spiegazione della Query Finale con Vista:**

- La struttura è molto simile a quella che usava la tabella temporanea .
- La differenza chiave è che invece di fare un `LEFT JOIN` con `TempFeedbackMedio`, facciamo un `LEFT JOIN` direttamente con `VistaFeedbackMedioPasseggeri` (alias `VFMP`).
- Il database tratterà `VistaFeedbackMedioPasseggeri` come se fosse una tabella, eseguendo la query definita nella vista per fornire i dati necessari al join.
- La logica del `WHERE` per filtrare in base al voto medio rimane la stessa.

Questo approccio incapsula la logica di calcolo della media in un oggetto riutilizzabile (la vista) e semplifica la query finale che deve solo concentrarsi sulla selezione dei passeggeri per il viaggio corrente e sull'applicazione del filtro usando i dati pre-aggregati dalla vista.

#### 4\. Progetto di Massima dell'Applicazione Web

**Architettura:** ASP.NET Core Minimal API con Frontend statico (HTML, CSS, JavaScript) servito da `wwwroot`.

**Struttura Funzionale:**

- **Backend (ASP.NET Core Minimal API):**

    - **Endpoints API (.NET Minimal API):**
        - `/api/auth/register`: Registrazione nuovi utenti.
        - `/api/auth/login`: Login utenti (Autisti/Passeggeri) - imposta un cookie di autenticazione.
        - `/api/auth/logout`: Logout.
        - `/api/auth/me`: Restituisce i dati dell'utente loggato.
        - `/api/viaggi`:
            - `POST /`: Inserimento nuovo viaggio (solo Autisti autorizzati).
            - `GET /`: Ricerca viaggi (parametri: `cittaPartenza`, `cittaDestinazione`, `data`).
            - `GET /{id}`: Dettagli di un viaggio specifico (include dati autista e auto).
            - `PUT /{id}/chiudi`: Chiusura prenotazioni per un viaggio (solo Autista proprietario).
        - `/api/prenotazioni`:
            - `POST /`: Creazione nuova prenotazione (solo Passeggeri autorizzati).
            - `GET /mie`: Elenco prenotazioni dell'utente loggato (passeggero).
            - `GET /viaggio/{idViaggio}`: Elenco prenotazioni per un viaggio (solo Autista proprietario, per accettare/rifiutare).
            - `PUT /{id}/accetta`: Accettazione prenotazione (solo Autista proprietario).
            - `PUT /{id}/rifiuta`: Rifiuto prenotazione (solo Autista proprietario).
        - `/api/feedback`:
            - `POST /autista`: Inserimento feedback su autista (solo Passeggero dopo viaggio completato).
            - `POST /passeggero`: Inserimento feedback su passeggero (solo Autista dopo viaggio completato).
            - `GET /autista/{idAutista}`: Visualizza feedback ricevuti da un autista.
            - `GET /passeggero/{idPasseggero}`: Visualizza feedback ricevuti da un passeggero.
        - `/api/utenti/{id}/profilo`: Dati profilo utente (per visualizzazione feedback, foto autista etc).
    - **Servizi Applicativi (C#):** Logica di business (validazioni, coordinamento operazioni).
    - **Data Access Layer (EF Core):** Interazione con il database MariaDB.
        - `DbContext`: Rappresentazione del database.
        - Repositories o uso diretto del DbContext negli endpoints (tipico delle Minimal API).
    - **Autenticazione/Autorizzazione:**
        - **Autenticazione:** Basata su Cookie. Dopo il login, viene creato un cookie sicuro (HttpOnly, Secure, SameSite=Strict) contenente i claims dell'utente (ID, ruoli - es. "Autista", "Passeggero").
        - **Autorizzazione:** Basata su Ruoli e Policy. Gli endpoint API sono protetti con attributi `[Authorize]` e policy specifiche (es. `[Authorize(Roles = "Autista")]`, `[Authorize(Policy = "IsViaggioOwner")]`). Le policy verificano se l'utente loggato ha il permesso di eseguire l'azione (es. se è l'autista che ha creato quel viaggio).
- **Frontend (HTML, CSS, JavaScript in `wwwroot`):**

    - **Pagine HTML:** Struttura delle viste (registrazione, login, ricerca viaggi, dettaglio viaggio, dashboard autista, dashboard passeggero, inserimento feedback).
    - **CSS:** Styling (possibilmente usando un framework come Bootstrap o Tailwind CSS).
    - **JavaScript (Vanilla JS o un framework leggero):**
        - Interazione con l'utente (gestione form, validazione client-side).
        - Chiamate API al backend tramite `Fetch API`.
        - Aggiornamento dinamico dell'interfaccia utente (DOM manipulation).
        - Gestione stato lato client (es. informazioni utente loggato).

**Segmento Significativo dell'Applicazione (Esempio: Ricerca Viaggi e Prenotazione):**

- **Diagramma di sequenza di Ricerca Viaggi**

```mermaid
sequenceDiagram
    actor Utente
    participant Sistema as Piattaforma Web
    participant Database

    Utente->>Piattaforma Web: Accede alla funzione/pagina di ricerca viaggi (es. GET /cerca)
    activate Piattaforma Web
    Piattaforma Web-->>Utente: Restituisce pagina con form di ricerca

    Note right of Utente: Utente inserisce criteri: Città Partenza,<br/>Città Destinazione, Data Desiderata.

    Utente->>Piattaforma Web: Invia criteri e avvia ricerca (es. GET /api/viaggi?partenza=...&destinazione=...&data=...)
    Piattaforma Web->>Piattaforma Web: Riceve e valida i criteri di ricerca

    %% --- Fase di Interrogazione Database ---
    Note over Piattaforma Web, Database: La query include filtri per tratta, data, stato 'Aperto',<br/>calcolo posti disponibili e recupero dati aggiuntivi (autista, voto, etc.)
    Piattaforma Web->>Database: Esegue query per trovare viaggi compatibili
    activate Database
    %% Internamente, il DB esegue JOIN tra Viaggi, Autisti, Utenti, Automobili;
    %% potrebbe eseguire subquery o calcoli per verificare i posti disponibili
    %% (Viaggi.PostiDisponibiliIniziali - COUNT(Prenotazioni.Accettate)) > 0
    %% e per calcolare il voto medio dell'autista e le prenotazioni in attesa.
    Database-->>Piattaforma Web: Restituisce elenco di viaggi trovati (o elenco vuoto) con dati richiesti
    deactivate Database

    %% --- Fase di Presentazione Risultati ---
    alt Elenco viaggi è vuoto
        Piattaforma Web-->>Utente: Mostra messaggio "Nessun viaggio trovato per i criteri specificati."
    else Elenco viaggi non è vuoto
        Piattaforma Web->>Piattaforma Web: Prepara/Formatta i dati dei viaggi per la visualizzazione
        Piattaforma Web-->>Utente: Mostra elenco dei viaggi trovati,<br/>ciascuno con: orario, costo, dettagli autista (nome, foto?, voto medio),<br/>dettagli viaggio (soste?), posti disponibili, prenotazioni in attesa.
    end
    deactivate Piattaforma Web
```

- **Frontend (`wwwroot/index.html`, `wwwroot/js/app.js`):**

    - `index.html`: Contiene un form per inserire città di partenza, destinazione e data. Un'area per visualizzare i risultati.
    - `app.js`:
        - Al submit del form, previene il comportamento di default.
        - Recupera i valori dal form.
        - Esegue una chiamata `Fetch` a `GET /api/viaggi?cittaPartenza=...&cittaDestinazione=...&data=...`.
        - Popola l'area dei risultati con i viaggi trovati, mostrando dettagli base e un bottone "Vedi Dettagli/Prenota".
        - Al click su "Vedi Dettagli/Prenota", esegue una `Fetch` a `GET /api/viaggi/{id}` e mostra i dettagli completi (autista, auto, feedback medio autista, descrizione) in una modale o nuova sezione.
        - Se l'utente è loggato come Passeggero, mostra un bottone "Prenota Questo Viaggio".
        - Al click su "Prenota", esegue una chiamata `Fetch` a `POST /api/prenotazioni` con `{ idViaggio: ... }` nel body.
        - Gestisce la risposta (successo o errore) mostrando un messaggio all'utente.
- **Backend (ASP.NET Core Minimal API - `Program.cs` o file separati):**

    - **Endpoint Ricerca Viaggi:**

        ```cs
        // In Program.cs o file dedicato agli endpoints dei viaggi
        app.MapGet("/api/viaggi", async (string cittaPartenza, string cittaDestinazione, DateOnly data, AppDbContext db) => {
            // Logica per query SQL/LINQ simile alla query 3a
            var viaggi = await db.Viaggi
                .Include(v => v.Autista).ThenInclude(a => a.Utente) // Include dati Utente dell'Autista
                .Include(v => v.Auto)
                .Where(v => v.CittaPartenza == cittaPartenza &&
                             v.CittaDestinazione == cittaDestinazione &&
                             v.DataOraPartenza.Date == data.ToDateTime(TimeOnly.MinValue) && // Confronto solo data
                             v.StatoViaggio == StatoViaggio.Aperto)
                .OrderBy(v => v.DataOraPartenza)
                .Select(v => new { // Proiezione per non esporre tutto il modello
                    v.IdViaggio,
                    v.DataOraPartenza,
                    v.ContributoEconomico,
                    Autista = new { v.Autista.Utente.Nome, v.Autista.Utente.Cognome },
                    Auto = new { v.Auto.Marca, v.Auto.Modello }
                    // Aggiungere Voto Medio Autista se necessario qui
                })
                .ToListAsync();
            return Results.Ok(viaggi);
        }).Produces<IEnumerable<object>>(); // Definire un DTO appropriato

        ```

    - **Endpoint Creazione Prenotazione:**

        ```cs
        app.MapPost("/api/prenotazioni", async (PrenotazioneRequestDto request, AppDbContext db, HttpContext httpContext) => {
            // 1. Verifica Autenticazione/Autorizzazione (Assume middleware già configurato)
            if (!httpContext.User.Identity.IsAuthenticated || !httpContext.User.IsInRole("Passeggero")) {
                return Results.Forbid();
            }
            var idPasseggero = int.Parse(httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)); // Ottiene ID dal cookie

            // 2. Validazione (esiste viaggio? è aperto? utente non è l'autista? etc.)
            var viaggio = await db.Viaggi.FindAsync(request.IdViaggio);
            if (viaggio == null || viaggio.StatoViaggio != StatoViaggio.Aperto || viaggio.IdAutista == idPasseggero) {
                return Results.BadRequest("Viaggio non valido o non prenotabile.");
            }
             // Verifica se ha già prenotato questo viaggio
            var esisteGia = await db.Prenotazioni.AnyAsync(p => p.IdViaggio == request.IdViaggio && p.IdPasseggero == idPasseggero && p.StatoPrenotazione != StatoPrenotazione.Rifiutata);
            if(esisteGia) {
                 return Results.Conflict("Hai già una prenotazione attiva per questo viaggio.");
            }

            // 3. Creazione Prenotazione
            var prenotazione = new Prenotazione {
                IdViaggio = request.IdViaggio,
                IdPasseggero = idPasseggero,
                DataOraPrenotazione = DateTime.UtcNow,
                StatoPrenotazione = StatoPrenotazione.Richiesta
            };
            db.Prenotazioni.Add(prenotazione);
            await db.SaveChangesAsync();

            // 4. (Opzionale) Inviare notifica email all'autista - Logica da implementare separatamente

            return Results.Created($"/api/prenotazioni/{prenotazione.IdPrenotazione}", prenotazione); // Ritornare un DTO

        }).RequireAuthorization(policy => policy.RequireRole("Passeggero")) // Policy specifica per passeggeri
          .Produces<Prenotazione>(StatusCodes.Status201Created)
          .Produces(StatusCodes.Status400BadRequest)
          .Produces(StatusCodes.Status401Unauthorized)
          .Produces(StatusCodes.Status403Forbidden)
          .Produces(StatusCodes.Status409Conflict);

        // Definire PrenotazioneRequestDto
        public record PrenotazioneRequestDto(int IdViaggio);

        ```

- **Database Access (EF Core Examples):**

    - **LINQ (come nell'endpoint sopra):**

        ```cs
        var viaggi = await db.Viaggi
            .Where(v => v.CittaPartenza == "Milano" && v.StatoViaggio == StatoViaggio.Aperto)
            .Include(v => v.Autista.Utente) // Usa Include per Eager Loading
            .ToListAsync();

        ```

    - **SQL Puro (FromSql - per la query 3a):**

        ```cs
        // Assumiamo che 'db' sia l'istanza del DbContext
        // e che 'cittaPartenza', 'cittaDestinazione', 'data' siano le variabili C#
        // contenenti i valori dei parametri. 'data' dovrebbe essere DateOnly o DateTime.

        // SQL con interpolazione di stringhe C# ($"")
        // EF Core convertirà automaticamente le variabili C# in parametri SQL (@p0, @p1, @p2)
        var viaggiDto = await db.Database
            .SqlQuery<ViaggioDisponibileDto>(
                $@"SELECT
                    A.IdAutista,
                    U.Nome,          
                    U.Cognome,       
                    AU.Marca,        
                    AU.Modello,      
                    AU.Targa,        
                    V.DataOraPartenza,
                    V.ContributoEconomico
                FROM Viaggi AS V
                JOIN Autisti AS A ON V.IdAutista = A.IdAutista
                JOIN Utenti AS U ON A.IdAutista = U.IdUtente
                JOIN Automobili AS AU ON V.IdAuto = AU.IdAuto
                WHERE V.CittaPartenza = {cittaPartenza} 
                AND V.CittaDestinazione = {cittaDestinazione} 
                AND DATE(V.DataOraPartenza) = {data} 
                AND V.StatoViaggio = 'Aperto'
                ORDER BY V.DataOraPartenza ASC")
            .ToListAsync();

        // Il DTO ViaggioDisponibileDto:
        public class ViaggioDisponibileDto {
            public int IdAutista { get; set; }
            public string Nome { get; set; } // Corrisponde a U.Nome
            public string Cognome { get; set; } // Corrisponde a U.Cognome
            public string Marca { get; set; } // Corrisponde a AU.Marca
            public string Modello { get; set; } // Corrisponde a AU.Modello
            public string Targa { get; set; } // Corrisponde a AU.Targa
            public DateTime DataOraPartenza { get; set; }
            public decimal ContributoEconomico { get; set; }
        }

        // 'viaggiDto' contiene la lista di oggetti ViaggioDisponibileDto.
        // EF Core ha mappato automaticamente le colonne restituite dalla SELECT
        // alle proprietà del DTO con lo stesso nome (case-insensitive).
        ```

##### Discussione Architettura e Deployment

- **Architettura Implementativa:** L'approccio ASP.NET Core Minimal API unificata con frontend statico è semplice ed efficace per questo tipo di applicazione. EF Core facilita l'accesso ai dati. L'autenticazione cookie è standard e adatta.
- **Deployment (Semplificato):**
    - **Opzione 1: Azure App Service + Azure Database for MariaDB:**
        - **App:** L'applicazione ASP.NET Core viene pubblicata su un Azure App Service (piano Linux o Windows). Il codice viene installato tramite Git, zip deploy, o container.
        - **Database:** Si utilizza il servizio PaaS Azure Database for MariaDB. La connection string nell'app ASP.NET Core punterà a questo database gestito.
        - **Vantaggi:** Gestione semplificata dell'infrastruttura (scalabilità, patch, backup gestiti da Azure per entrambi i servizi).
        - **Costi:** Basati sul consumo dei servizi (tier scelti).
    - **Opzione 2: Container (Azure Container Instances / Azure Kubernetes Service / App Service for Containers):**
        - **App Container:** Si crea un'immagine Docker per l'app ASP.NET Core. L'immagine viene inviata su un registro (es. Azure Container Registry). Il container viene eseguito su ACI (semplice), AKS (orchestrato), o App Service (Web App for Containers).
        - **Database Container:** Si può creare un container Docker per MariaDB (meno consigliato per produzione per via della gestione della persistenza e backup) oppure usare Azure Database for MariaDB (come Opzione 1).
        - **Vantaggi:** Portabilità, consistenza tra ambienti (sviluppo, test, produzione). AKS offre scalabilità avanzata e resilienza.
        - **Complessità:** Leggermente maggiore rispetto all'App Service standard, specialmente con AKS. Gestire la persistenza del database in container richiede attenzione (volumi persistenti).
    - **Configurazione:** Le connection string e altre configurazioni sensibili vanno gestite tramite le impostazioni dell'applicazione nel servizio di hosting (es. Application Settings in App Service, Secrets in Kubernetes) e non hardcoded nel codice.

### Seconda Parte

#### Quesito I: Gestione Automatica Posti Disponibili

**1\. Integrazione del Modello:**

- **Schema Logico (Modifiche):**
    - Aggiungere alla tabella `Viaggi` il campo `PostiDisponibiliIniziali` (INT NOT NULL).
- **Considerazioni:**
    - Il numero di posti *effettivamente* disponibili in un dato momento non viene memorizzato direttamente, ma calcolato dinamicamente.
    - Una prenotazione impegna un posto solo quando è nello stato 'Accettata'.
    - Bisogna visualizzare sia i posti ancora liberi sia il numero di prenotazioni in attesa ('Richiesta').

**2\. Calcolo Posti Disponibili e Prenotazioni in Attesa (Logica/SQL):**

Per un dato `IdViaggio`:

- **Posti Occupati:** `SELECT COUNT(*) FROM Prenotazioni WHERE IdViaggio = ? AND StatoPrenotazione = 'Accettata'`
- **Posti Disponibili Attuali:** `Viaggi.PostiDisponibiliIniziali` - Posti Occupati
- **Prenotazioni in Attesa:** `SELECT COUNT(*) FROM Prenotazioni WHERE IdViaggio = ? AND StatoPrenotazione = 'Richiesta'`

**3\. Modifica Logica Applicativa:**

- **Inserimento/Modifica Viaggio (Autista):** Deve specificare `PostiDisponibiliIniziali`.
- **Ricerca Viaggi (Passeggero):** La query di ricerca (punto 3a) deve essere modificata per:
    - Calcolare i posti disponibili attuali.
    - Filtrare mostrando solo viaggi con `PostiDisponibiliAttuali > 0`.
    - Includere nel risultato il numero di posti disponibili attuali e il numero di prenotazioni in attesa.
- **Accettazione Prenotazione (Autista):** Prima di cambiare lo stato in 'Accettata', verificare se `PostiDisponibiliAttuali > 0`. Se sì, procedere; altrimenti, rifiutare o informare l'autista.

**4\. Pagina Web (Client/Server) per Informazioni Posti**

**Backend (Endpoint GET /api/viaggi/{id} modificato):**

Deve calcolare e restituire, oltre ai dati del viaggio, postiDisponibiliAttuali e prenotazioniInAttesa.

```cs
// Endpoint GET /api/viaggi/{id}
app.MapGet("/api/viaggi/{id}", async (int id, AppDbContext db) => {
    var viaggio = await db.Viaggi
        .Include(v => v.Autista.Utente)
        .Include(v => v.Auto)
        .FirstOrDefaultAsync(v => v.IdViaggio == id);

    if (viaggio == null) return Results.NotFound();

    var postiOccupati = await db.Prenotazioni.CountAsync(p => p.IdViaggio == id && p.StatoPrenotazione == StatoPrenotazione.Accettata);
    var prenotazioniInAttesa = await db.Prenotazioni.CountAsync(p => p.IdViaggio == id && p.StatoPrenotazione == StatoPrenotazione.Richiesta);
    var postiDisponibiliAttuali = viaggio.PostiDisponibiliIniziali - postiOccupati;

    // Creare un DTO per la risposta
    var resultDto = new ViaggioDetailDto {
        // ... altri dati del viaggio, autista, auto ...
        IdViaggio = viaggio.IdViaggio,
        CittaPartenza = viaggio.CittaPartenza,
        // ...
        PostiDisponibiliIniziali = viaggio.PostiDisponibiliIniziali,
        PostiDisponibiliAttuali = postiDisponibiliAttuali,
        PrenotazioniInAttesa = prenotazioniInAttesa
        // ... Dati Autista, Auto, Feedback Medio Autista etc.
    };

    return Results.Ok(resultDto);

}).Produces<ViaggioDetailDto>() // Definire ViaggioDetailDto
    .Produces(StatusCodes.Status404NotFound);

```

**Frontend (JavaScript nella pagina di dettaglio viaggio):**

Quando l'utente visualizza i dettagli di un viaggio specifico, viene eseguita una chiamata all'API `GET /api/viaggi/{id}`. La risposta JSON conterrà, tra le altre cose, i campi `postiDisponibiliAttuali`, `postiDisponibiliIniziali` e `prenotazioniInAttesa` calcolati dal backend. Il codice JavaScript lato client utilizzerà questi dati per aggiornare dinamicamente la pagina HTML, mostrando le informazioni sulla disponibilità dei posti e il pulsante di prenotazione solo se appropriato.

Ecco un esempio di come potrebbe essere implementato in un file `app.js` (o simile):

```javascript
/**
 * Funzione per recuperare e mostrare i dettagli di un viaggio, inclusa la disponibilità dei posti.
 * @param {number} idViaggio - L'ID del viaggio da visualizzare.
 */
function mostraDettagliViaggio(idViaggio) {
    // Effettua la chiamata fetch all'endpoint API per ottenere i dettagli del viaggio
    fetch(`/api/viaggi/${idViaggio}`)
        .then(response => {
            // Controlla se la risposta è andata a buon fine (es. status 200 OK)
            if (!response.ok) {
                // Se la risposta non è OK (es. 404 Not Found), lancia un errore
                throw new Error(`Viaggio non trovato o errore del server (status: ${response.status})`);
            }
            // Converte la risposta in formato JSON
            return response.json();
        })
        .then(viaggio => {
            // Una volta ottenuti i dati del viaggio in formato JSON:

            // --- Popola altri dettagli del viaggio ---
            // Esempio: aggiorna il titolo, la descrizione, i dati dell'autista, ecc.
            document.getElementById('viaggio-titolo').textContent = `${viaggio.cittaPartenza} - ${viaggio.cittaDestinazione}`;
            document.getElementById('viaggio-descrizione').textContent = viaggio.descrizioneAggiuntiva;
            // ... (altri aggiornamenti del DOM) ...

            // --- Gestione delle informazioni sui posti disponibili ---
            const infoPostiDiv = document.getElementById('info-posti'); // Trova il div nell'HTML destinato a contenere le info sui posti
            if (!infoPostiDiv) {
                console.error("Elemento con ID 'info-posti' non trovato nel DOM.");
                return; // Esce se l'elemento non esiste
            }

            // Controlla se ci sono posti disponibili
            if (viaggio.postiDisponibiliAttuali > 0) {
                // Se ci sono posti, mostra quanti, il totale iniziale, le prenotazioni in attesa e il bottone di prenotazione
                infoPostiDiv.innerHTML = `
                    <p><strong>Posti ancora disponibili:</strong> ${viaggio.postiDisponibiliAttuali} / ${viaggio.postiDisponibiliIniziali}</p>
                    <p><strong>Prenotazioni in attesa di approvazione:</strong> ${viaggio.prenotazioniInAttesa}</p>
                    <button id="prenota-btn" class="btn btn-primary" data-id="${viaggio.idViaggio}">Prenota Ora</button>
                `;
                // Aggiunge l'event listener al bottone SOLO DOPO averlo inserito nel DOM
                const prenotaBtn = document.getElementById('prenota-btn');
                if(prenotaBtn) {
                    prenotaBtn.addEventListener('click', handlePrenotaClick);
                }
            } else {
                // Se non ci sono posti disponibili, mostra un messaggio appropriato
                infoPostiDiv.innerHTML = `<p><strong>Viaggio completo!</strong> Non ci sono più posti disponibili.</p>`;
            }
        })
        .catch(error => {
            // Gestisce eventuali errori durante la fetch o l'elaborazione della risposta
            console.error('Errore nel caricare i dettagli del viaggio:', error);
            // Mostra un messaggio di errore all'utente nell'interfaccia, se possibile
            const infoPostiDiv = document.getElementById('info-posti');
            if (infoPostiDiv) {
                 infoPostiDiv.innerHTML = `<p class="text-danger">Impossibile caricare i dettagli del viaggio: ${error.message}</p>`;
            }
        });
}

/**
 * Funzione chiamata quando l'utente clicca sul pulsante "Prenota Ora".
 * @param {Event} event - L'oggetto evento del click.
 */
function handlePrenotaClick(event) {
    // Ottiene l'ID del viaggio dal data-attribute del bottone cliccato
    const idViaggio = event.target.dataset.id;
    console.log(`Tentativo prenotazione per viaggio ${idViaggio}`);

    // Qui andrebbe inserita la logica per effettuare la chiamata POST /api/prenotazioni
    // Esempio (semplificato):
    
    fetch('/api/prenotazioni', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            // Aggiungere eventuali header di autenticazione (es. CSRF token se necessario)
        },
        body: JSON.stringify({ idViaggio: parseInt(idViaggio) }) // Invia l'ID del viaggio nel body
    })
    .then(response => {
        if (!response.ok) {
             // Gestire errori specifici (es. 401 non autorizzato, 400 bad request, 409 conflitto)
            throw new Error(`Errore durante la prenotazione (status: ${response.status})`);
        }
        return response.json(); // O response.text() se non ritorna JSON
    })
    .then(data => {
        console.log('Prenotazione effettuata con successo:', data);
        alert('Prenotazione inviata con successo! Attendi la conferma dall\'autista.');
        // Aggiornare l'UI se necessario (es. disabilitare il bottone)
        event.target.disabled = true;
        event.target.textContent = 'Prenotazione Inviata';
    })
    .catch(error => {
        console.error('Errore nella prenotazione:', error);
        alert(`Errore durante la prenotazione: ${error.message}`);
    });
    
}

// Esempio di come chiamare la funzione quando si carica la pagina o si seleziona un viaggio
// Presumendo che l'ID del viaggio sia disponibile, ad esempio, da un parametro URL
const urlParams = new URLSearchParams(window.location.search);
const idViaggioDaUrl = urlParams.get('id');
if (idViaggioDaUrl) {
  mostraDettagliViaggio(parseInt(idViaggioDaUrl));
}

```

**Struttura HTML necessaria:**

Affinché questo codice funzioni, la pagina HTML che mostra i dettagli del viaggio dovrà contenere un elemento con `id="info-posti"`. Ad esempio:

```html
<!DOCTYPE html>
<html lang="it">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Dettaglio Viaggio - Car Pooling</title>
    <!-- Link a CSS  -->
</head>
<body>

    <header>
        <nav>
            <!-- Navigazione principale del sito (es. Logo, Home, Cerca, Profilo) -->
            <a href="/">Logo CarPooling</a> |
            <a href="/cerca">Cerca Viaggio</a> |
            <a href="/profilo">Mio Profilo</a>
        </nav>
    </header>

    <main>
        <h1>Dettaglio Viaggio</h1>

        <article class="viaggio-dettaglio">

            <section class="riepilogo-viaggio">
                <h2>Riepilogo Viaggio</h2>
                <p>
                    <strong>Da:</strong> <span id="viaggio-partenza">...</span>
                    <strong>A:</strong> <span id="viaggio-destinazione">...</span>
                </p>
                <p>
                    <strong>Data:</strong> <span id="viaggio-data">...</span>
                    <strong>Ora:</strong> <span id="viaggio-ora">...</span>
                </p>
            </section>

            <section class="info-autista">
                <h2>Autista</h2>
                <div>
                    <img id="autista-foto" src="/placeholder-foto.jpg" alt="Foto Autista" width="80">
                    <p><strong>Nome:</strong> <span id="autista-nome">...</span></p>
                    <p><strong>Voto Medio:</strong> <span id="autista-voto">...</span> / 5</p>
                </div>
                <div>
                    <h3>Auto Utilizzata</h3>
                    <p><span id="auto-marca-modello">...</span> (<span id="auto-targa">...</span>)</p>
                    <p>Colore: <span id="auto-colore">...</span></p>
                </div>
            </section>

            <section class="dettagli-costo">
                <h2>Dettagli e Costo</h2>
                <p><strong>Contributo Richiesto:</strong> €<span id="viaggio-costo">...</span> (a passeggero)</p>
                <p><strong>Durata Stimata:</strong> <span id="viaggio-durata">...</span> minuti</p>
                <h3>Note Addizionali</h3>
                <p id="viaggio-note">...</p> <!-- Es. Bagagli, Animali, Soste -->
            </section>

            <hr>

            <!-- ============================================= -->
            <!-- SEZIONE DISPONIBILITÀ E PRENOTAZIONE  -->
            <!-- ============================================= -->
            <section class="disponibilita-prenotazione">
                <h2>Disponibilità e Prenotazione</h2>
                <div id="info-posti">
                    <!-- Contenuto iniziale, verrà sostituito da JavaScript -->
                    <p>Caricamento informazioni sulla disponibilità...</p>

                    <!-- Esempio di come apparirà DOPO il caricamento JS (se posti > 0) -->
                    <!--
                    <p><strong>Posti ancora disponibili:</strong> 2 / 3</p>
                    <p><strong>Prenotazioni in attesa di approvazione:</strong> 1</p>
                    <button id="prenota-btn" class="btn btn-primary" data-id="123">Prenota Ora</button>
                    -->

                    <!-- Esempio di come apparirà DOPO il caricamento JS (se posti == 0) -->
                    <!--
                    <p><strong>Viaggio completo!</strong> Non ci sono più posti disponibili.</p>
                    -->
                </div>
            </section>
            <!-- ============================================= -->

            <hr>

            <section class="feedback-autista">
                <h2>Feedback Recenti sull'Autista</h2>
                <ul id="lista-feedback">
                    <!-- Feedback caricati dinamicamente o staticamente -->
                    <li>Utente1 (*****) - Ottimo viaggio!</li>
                    <li>Utente2 (****) - Puntuale e gentile.</li>
                    <!-- ... -->
                </ul>
            </section>

        </article>

    </main>

    <footer>
        <p>&copy; 2025 Car Pooling App - Contatti - Termini</p>
    </footer>

    <!-- Link a JavaScript (es. app.js) -->
    <script src="/js/app.js"></script>
    <script>
        // Potrebbe esserci qui o in app.js la logica per estrarre l'ID viaggio
        // dall'URL e chiamare mostraDettagliViaggio(id);
        // Esempio:
        // const idViaggioDaUrl = new URLSearchParams(window.location.search).get('id');
        // if (idViaggioDaUrl) {
        //    mostraDettagliViaggio(parseInt(idViaggioDaUrl));
        // }
    </script>

</body>
</html>

```

In questo modo, il JavaScript popolerà dinamicamente il `div#info-posti` con le informazioni corrette sulla disponibilità e il pulsante di prenotazione, basandosi sui dati ricevuti dall'API.

#### Quesito II: Relazione Tecnica sulla Piattaforma Car Pooling

Il candidato immagini di voler documentare al committente l'operatività della piattaforma proposta. A tal fine, imposti una relazione tecnica che presenti le principali caratteristiche dell'applicazione Web in termini di organizzazione e funzionalità. In particolare, imposti la struttura di tale relazione, motivando le scelte e scrivendo un esempio significativo dei relativi contenuti.

#### Struttura della Relazione Tecnica

Una relazione tecnica efficace per il committente dovrebbe essere chiara, concisa e focalizzata sui benefici e sul funzionamento generale della piattaforma, evitando tecnicismi eccessivi se non richiesti.

**Motivazione della Struttura:** La struttura proposta mira a guidare il committente attraverso una comprensione progressiva della piattaforma: dall'introduzione degli obiettivi fino ai dettagli funzionali e alle prospettive future, mantenendo un linguaggio accessibile.

1. **Introduzione e Obiettivi della Piattaforma (Capitolo 1)**

    - Scopo della relazione.
    - Breve descrizione del progetto "Piattaforma Car Pooling".
    - Obiettivi di business che la piattaforma intende raggiungere (es. mobilità flessibile, riduzione costi per i viaggiatori, ottimizzazione risorse).
    - Target di utenti (utenti-autisti, utenti-passeggeri).
2. **Architettura Generale della Piattaforma (Capitolo 2)**

    - Descrizione dell'architettura scelta (applicazione Web).
    - Componenti principali (Frontend, Backend, Database) e loro interazione a livello macroscopico.
    - Tecnologie chiave utilizzate (in modo comprensibile, es. "database relazionale per la gestione sicura dei dati", "tecnologie web moderne per un'interfaccia utente intuitiva").
    - Cenni sulla sicurezza dei dati e della piattaforma.
3. **Funzionalità Principali per gli Utenti (Capitolo 3)**

    - Suddiviso per tipologia di utente (Autista, Passeggero).
    - **3.1 Funzionalità per l'Utente Autista:**
        - Registrazione e gestione profilo autista (dati personali, patente, automobile).
        - Inserimento e gestione dei viaggi offerti (itinerario, date, costi, posti).
        - Gestione delle prenotazioni ricevute (accettazione/rifiuto).
        - Gestione della chiusura delle prenotazioni per un viaggio.
        - Inserimento feedback sui passeggeri.
    - **3.2 Funzionalità per l'Utente Passeggero:**
        - Registrazione e gestione profilo passeggero (dati personali, documento).
        - Ricerca e visualizzazione dei viaggi disponibili (filtri per città, data).
        - Visualizzazione dettagli viaggio e autista (inclusi feedback).
        - Processo di prenotazione di un viaggio.
        - Ricezione notifiche (accettazione/rifiuto).
        - Inserimento feedback sugli autisti.
    - **3.3 Funzionalità Comuni:**
        - Sistema di login/logout.
        - Visualizzazione e gestione dei feedback (medi e singoli giudizi).
4. **Flussi Operativi Principali (Capitolo 4)**

    - Descrizione passo-passo dei principali scenari d'uso (es. "Dalla ricerca alla prenotazione di un viaggio per un passeggero", "Dall'offerta di un viaggio alla gestione delle prenotazioni per un autista").
    - Utilizzo di diagrammi di flusso semplici o screenshot esemplificativi (mockup).
5. **Aspetti Tecnici Rilevanti (Capitolo 5 - Opzionale, se il committente ha background tecnico)**

    - Dettagli sulla gestione del database.
    - Infrastruttura di hosting e deployment ipotizzata.
    - Scalabilità e manutenzione.
6. **Conclusioni e Sviluppi Futuri (Capitolo 6)**

    - Riepilogo dei benefici della piattaforma.
    - Possibili evoluzioni e funzionalità aggiuntive future (es. pagamenti integrati, sistema di messaggistica interna, app mobile nativa).

##### Esempio Significativo di Contenuto (Estratto dal Capitolo 3.2)

**3.2 Funzionalità per l'Utente Passeggero:**

L'utente che desidera usufruire di un passaggio (denominato "Passeggero") ha a disposizione una serie di funzionalità intuitive per trovare e prenotare il viaggio più adatto alle proprie esigenze.

**3.2.1 Registrazione e Gestione del Profilo Passeggero** Prima di poter prenotare un viaggio, il passeggero deve creare un account personale. La procedura di registrazione è rapida e richiede l'inserimento di informazioni anagrafiche essenziali (cognome, nome), un documento di identità valido (per garantire la sicurezza e l'affidabilità della community), un recapito telefonico e un indirizzo email. Quest'ultimo sarà utilizzato per le comunicazioni relative alle prenotazioni. Una volta registrato, il passeggero potrà accedere al proprio profilo per visualizzare e, se necessario, modificare i propri dati.

**3.2.2 Ricerca e Visualizzazione dei Viaggi** La piattaforma offre un potente ma semplice strumento di ricerca. Il passeggero può indicare:

- **Città di partenza**
- **Città di destinazione**
- **Data desiderata per il viaggio**

Il sistema presenterà quindi un elenco chiaro e ordinato di tutti i viaggi disponibili che soddisfano i criteri inseriti e per i quali le prenotazioni sono ancora aperte. Per ciascun viaggio, il passeggero potrà visualizzare immediatamente:

- L'orario di partenza.
- Il contributo economico richiesto dall'autista.
- Informazioni sull'autista, come il nome e il voto medio ricevuto dagli altri passeggeri.
- Dettagli aggiuntivi forniti dall'autista, quali eventuali soste intermedie, la possibilità di trasportare bagagli o animali, e una stima dei tempi di percorrenza.

**3.2.3 Processo di Prenotazione di un Viaggio** Una volta individuato il viaggio di suo interesse, il passeggero può procedere con la prenotazione. Prima di confermare, ha la possibilità di esaminare in dettaglio il profilo dell'autista, inclusi i singoli giudizi lasciati dai precedenti passeggeri. Questa trasparenza aiuta a costruire fiducia all'interno della community. Selezionato il viaggio, il passeggero invia la richiesta di prenotazione. La piattaforma inoltrerà automaticamente tale richiesta all'autista via email. *(...omissis: altre funzionalità del passeggero come Ricezione Notifiche e Inserimento Feedback...)*

##### Utilizzo di Diagrammi UML

Diversi diagrammi UML possono essere molto utili per arricchire la relazione tecnica  e renderla più chiara per il committente, specialmente per illustrare aspetti strutturali e comportamentali della piattaforma.

Ecco alcuni esempi di diagrammi UML che potrebbero essere impiegati, con relativo codice PlantUML per generarli:

1. **Diagramma dei Casi d'Uso (Use Case Diagram)**

    **Scopo nella Relazione:** Illustra le principali funzionalità offerte dalla piattaforma e come i diversi tipi di utenti (attori) interagiscono con esse. Fornisce una visione d'insieme di "chi fa cosa". Ideale per il capitolo sulle "Funzionalità Principali".

    **Esempio PlantUML:**

    ```plantuml
    @startuml
    left to right direction

    actor Passeggero
    actor Autista
    actor "Utente Generico" as Utente

    rectangle "Piattaforma Car Pooling" {
    Passeggero -- (Cerca Viaggio)
    Passeggero -- (Prenota Viaggio)
    Passeggero -- (Lascia Feedback ad Autista)
    Passeggero -- (Visualizza Stato Prenotazioni)

    Autista -- (Offre Viaggio)
    Autista -- (Gestisce Prenotazioni Ricevute)
    Autista -- (Lascia Feedback a Passeggero)
    Autista -- (Gestisce Viaggi Offerti)

    (Cerca Viaggio) .> (Visualizza Dettagli Viaggio) : include
    (Visualizza Dettagli Viaggio) .> (Visualizza Feedback Autista) : include

    Utente --|> Passeggero : <<generalization>>
    Utente --|> Autista : <<generalization>>
    Utente -- (Registrazione)
    Utente -- (Login)
    (Cerca Viaggio) .. Utente : (può essere non loggato)
    }

    @enduml

    ```

2. **Diagramma dei Componenti (Component Diagram)**

    **Scopo nella Relazione:** Mostra l'architettura software di alto livello, identificando i principali componenti del sistema e le loro interdipendenze. Utile per il capitolo sull'"Architettura Generale della Piattaforma".

    **Esempio PlantUML:**

    ```plantuml
    @startuml
    !theme vibrant

    package "Browser Utente" {
        [Frontend WebApp] <<UI>>
    }

    package "Server Applicativo" {
        [API Service (Backend)] <<Application>>
        [Servizio Notifiche] <<Service>>
    }

    database "Database Server" {
        [Database CarPooling] <<DB>>
    }

    cloud "Servizi Esterni" {
        [Sistema Email Esterno] <<External>>
    }

    ' Connessioni principali
    [Frontend WebApp] --> [API Service (Backend)] : HTTPS/JSON
    [API Service (Backend)] --> [Database CarPooling] : ADO.NET/SQL
    [API Service (Backend)] --> [Servizio Notifiche] : Chiamata Interna

    ' Connessione verso servizi esterni
    [Servizio Notifiche] ..> [Sistema Email Esterno] : SMTP

    ' Note esplicative
    note right of [API Service (Backend)]
    Gestisce la logica di business
    e le API REST
    end note

    note bottom of [Servizio Notifiche]
    Invia notifiche email
    agli utenti registrati
    end note

    @enduml

    ```

3. **Diagramma di Attività (Activity Diagram)**

    **Scopo nella Relazione:** Descrive il flusso di lavoro di un processo o di una funzionalità complessa, mostrando la sequenza delle attività, i punti di decisione e i possibili percorsi. Ottimo per il capitolo sui "Flussi Operativi Principali". Riprendiamo l'esempio della "Registrazione Utente".

    **Esempio PlantUML:**

    ```plantuml
    @startuml
    title Flusso di Registrazione Utente

    |Utente|
    start
    :Accede a pagina registrazione;
    |Sistema|
    :Mostra form dati comuni;
    |Utente|
    :Inserisce dati comuni;
    :Sceglie ruolo (Autista/Passeggero);
    if (Ruolo è Autista?) then (Si)
    |Sistema|
    :Richiede dati Autista;
    |Utente|
    :Inserisce dati Autista;
    else (No, è Passeggero)
    |Sistema|
    :Richiede dati Passeggero;
    |Utente|
    :Inserisce dati Passeggero;
    endif
    |Sistema|
    :Valida tutti i dati;
    if (Dati validi?) then (Si)
    :Crea Account Utente;
    :Salva dati ruolo specifico;
    :Invia Email di Conferma;
    :Mostra messaggio di successo;
    |Utente|
    :Visualizza successo;
    stop
    else (No)
    :Mostra messaggio di errore;
    |Utente|
    :Visualizza errore;
    stop
    endif

    @enduml

    ```

4. **Diagramma di Sequenza (Sequence Diagram) - Semplificato**

    **Scopo nella Relazione:** Illustra le interazioni tra oggetti o attori in una sequenza temporale per uno specifico scenario o caso d'uso. Può essere utile per mostrare come i componenti collaborano (Capitolo 4 o per dettagliare interazioni chiave). Esempio: "Passeggero prenota un viaggio".

    **Esempio PlantUML:**


    ```plantuml
    @startuml
    title Sequenza: Passeggero Prenota Viaggio

    actor Passeggero
    participant "Frontend WebApp" as FE
    participant "API Service (Backend)" as BE
    database Database as DB
    participant "Servizio Notifiche" as SN

    Passeggero -> FE : Seleziona Viaggio e Clicca "Prenota"
    activate FE
    FE -> BE : POST /api/prenotazioni (idViaggio, infoPasseggero)
    activate BE
    BE -> DB : Verifica disponibilità viaggio e precondizioni
    activate DB
    DB --> BE : Risultato verifica
    deactivate DB

    alt Precondizioni OK
        BE -> DB : INSERT nuova Prenotazione (stato: Richiesta)
        activate DB
        DB --> BE : Conferma inserimento
        deactivate DB
        BE -> SN : Richiedi invio notifica ad Autista
        activate SN
        SN --> BE : (Conferma accettazione richiesta)
        deactivate SN
        BE --> FE : {success: true, message: "Richiesta inviata"}
    else Precondizioni NON OK
        BE --> FE : {success: false, error: "Impossibile prenotare"}
    end
    FE --> Passeggero : Mostra messaggio
    deactivate BE
    deactivate FE

    @enduml

    ```

L'uso di questi diagrammi UML, anche in forma semplificata, può migliorare notevolmente la qualità della relazione tecnica, facilitando la comprensione da parte del committente degli aspetti funzionali e architetturali della piattaforma car pooling. È importante che siano chiari, ben etichettati e accompagnati da una breve spiegazione nel testo della relazione.

#### Quesito III: Schema Relazionale Film/Attore/Recita

Dato il seguente schema relazionale: `film (id, titolo, durata, anno_produzione, genere);` `attore (id, nome, cognome, data_nascita, fotografia);` `recita (id_film, id_attore, ruolo);`

Il candidato:

- Determini la modalità di gestione del campo 'fotografia' che prevede la memorizzazione di una immagine dell'attore in un formato grafico (es. JPG).
- Formalizzi in linguaggio SQL lo schema fisico corrispondente allo schema relazionale, sapendo che:
    - Il campo 'genere' ammette solo i seguenti valori: fantasy, giallo, commedia, horror, drammatico, fantascienza, azione.
    - Per la relazione 'recita', i campi 'id\_film' e 'id\_attore' referenziano rispettivamente la chiave primaria delle relazioni 'film' e 'attore'.
- Discuta l'uso degli indici nel modello fisico di una base di dati e suggerisca con motivato giudizio indici appropriati per questo schema relazionale, definendoli in linguaggio SQL.

1. Gestione del Campo 'fotografia'

    Per la gestione del campo `fotografia` dell'entità `attore`, che deve memorizzare un'immagine in formato grafico (es. JPG), si possono considerare principalmente due approcci:

    - **Memorizzazione dell'Immagine come BLOB (Binary Large Object) nel Database:**
        - **Pro:** L'immagine è direttamente integrata con gli altri dati dell'attore, semplificando i backup e la consistenza dei dati. Non ci sono dipendenze da file system esterni.
        - **Contro:** Può appesantire significativamente il database, potenzialmente degradando le performance per query che non necessitano dell'immagine. La gestione (lettura/scrittura) dei BLOB può essere meno efficiente rispetto all'accesso a file. Le dimensioni dei BLOB sono spesso limitate o richiedono configurazioni specifiche.
    - **Memorizzazione del Percorso (Path) all'Immagine nel Database:**
        - **Pro:** Il database rimane leggero e performante. Le immagini sono file standard sul file system del server o su un servizio di storage dedicato (es. CDN, S3), ottimizzati per la distribuzione di contenuti statici. È più flessibile per la gestione delle immagini (ridimensionamento, caching).
        - **Contro:** Richiede la gestione della consistenza tra il percorso nel database e l'effettiva presenza del file nel file system/storage. I backup devono considerare sia il database sia lo storage delle immagini.
        - **Scelta Consigliata:** Per applicazioni web e la maggior parte degli scenari, **la memorizzazione del percorso è generalmente l'approccio preferito** per la sua scalabilità e le migliori performance nella distribuzione dei contenuti. Il campo `fotografia` nella tabella `attore` conterrebbe quindi una stringa VARCHAR con il percorso relativo o assoluto all'immagine (es. `/immagini/attori/nome_file.jpg` o un URL completo se su CDN).

1. Schema Fisico SQL (MariaDB/MySQL)

    ```sql
    -- Tabella film
    CREATE TABLE film (
        id INT AUTO_INCREMENT PRIMARY KEY,
        titolo VARCHAR(255) NOT NULL,
        durata INT, -- Durata in minuti
        anno_produzione YEAR, -- Anno a 4 cifre
        genere ENUM('fantasy', 'giallo', 'commedia', 'horror', 'drammatico', 'fantascienza', 'azione') NOT NULL, -- Vincolo sui valori [cite: 34]
        INDEX idx_titolo (titolo),
        INDEX idx_genere (genere),
        INDEX idx_anno_produzione (anno_produzione)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    -- Tabella attore
    CREATE TABLE attore (
        id INT AUTO_INCREMENT PRIMARY KEY,
        nome VARCHAR(100) NOT NULL,
        cognome VARCHAR(100) NOT NULL,
        data_nascita DATE,
        fotografia VARCHAR(512), -- Percorso al file dell'immagine (es. JPG)
        INDEX idx_cognome_nome (cognome, nome)
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

    -- Tabella di associazione recita
    CREATE TABLE recita (
        id_film INT NOT NULL,
        id_attore INT NOT NULL,
        ruolo VARCHAR(100),
        PRIMARY KEY (id_film, id_attore), -- Chiave primaria composta
        CONSTRAINT fk_recita_film FOREIGN KEY (id_film) REFERENCES film(id) ON DELETE CASCADE ON UPDATE CASCADE, -- Vincolo di integrità referenziale [cite: 35]
        CONSTRAINT fk_recita_attore FOREIGN KEY (id_attore) REFERENCES attore(id) ON DELETE CASCADE ON UPDATE CASCADE, -- Vincolo di integrità referenziale [cite: 35]
        INDEX idx_ruolo (ruolo) -- Indice sul ruolo se si cerca spesso per ruolo
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
    ```

    **Spiegazioni aggiuntive:**

    - `ENGINE=InnoDB`: Necessario per il supporto delle chiavi esterne e delle transazioni.
    - `DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci`: Per il supporto di un'ampia gamma di caratteri.
    - `ENUM` per `genere`: Implementa direttamente il vincolo sui valori ammessi.
    - `FOREIGN KEY ... ON DELETE CASCADE ON UPDATE CASCADE`: Assicura che se un film o un attore viene eliminato, anche le relative partecipazioni in `recita` vengano eliminate. Se l'ID di un film o attore cambiasse (raro con AUTO\_INCREMENT), verrebbe aggiornato anche in `recita`.

2. Uso degli Indici

    **Discussione:** Gli indici sono strutture dati speciali associate a tabelle (o viste in alcuni DBMS) che permettono di velocizzare significativamente le operazioni di ricerca (SELECT con clausole WHERE, JOIN) e ordinamento. Funzionano in modo simile all'indice analitico di un libro: invece di scorrere tutte le pagine (righe della tabella), il DBMS può consultare l'indice per trovare rapidamente la "pagina" (posizione fisica o logica della riga) desiderata.

    **Vantaggi:**

    - **Velocità:** Drastica riduzione dei tempi di esecuzione delle query che filtrano o ordinano per colonne indicizzate.
    - **Efficienza nei JOIN:** I join tra tabelle su colonne indicizzate (tipicamente chiavi primarie ed esterne) sono molto più veloci.
    - **Applicazione di Vincoli:** Gli indici `UNIQUE` garantiscono l'unicità dei valori in una colonna. Le chiavi primarie sono automaticamente indicizzate.

    **Svantaggi:**

    - **Spazio su Disco:** Gli indici occupano spazio aggiuntivo.
    - **Overhead in Scrittura:** Le operazioni di INSERT, UPDATE, DELETE diventano leggermente più lente perché il DBMS deve aggiornare anche gli indici associati, oltre ai dati della tabella.

    È quindi cruciale bilanciare il numero e il tipo di indici: crearli sulle colonne frequentemente usate nelle condizioni di ricerca e join, ma evitare di indicizzare indiscriminatamente ogni colonna.

    **Indici Suggeriti per lo Schema (con definizione SQL, già inclusi nello script sopra):**

    1. **Chiavi Primarie:**

        - `film(id)`: Automaticamente indicizzata. Essenziale per i join.
        - `attore(id)`: Automaticamente indicizzata. Essenziale per i join.
        - `recita(id_film, id_attore)`: Chiave primaria composta, automaticamente indicizzata. Ottima per join e per verificare se un attore ha recitato in un film.
    2. **Chiavi Esterne:**

        - `recita(id_film)`: Benché parte della PK, un indice separato su `id_film` (se non coperto ottimamente dalla PK composta per tutte le query) potrebbe essere utile se si cercano spesso tutti gli attori di un film. MariaDB/MySQL crea automaticamente indici per le FK se non esistono già indici compatibili.
        - `recita(id_attore)`: Similmente, un indice separato su `id_attore` potrebbe essere utile se si cercano spesso tutti i film di un attore. Un indice su `(id_attore, id_film)` sulla tabella `recita` sarebbe molto utile per questo.
    3. **Colonne Frequentemente Usate nelle Ricerche (Clausola `WHERE`):**

        - `film(titolo)`: `INDEX idx_titolo (titolo)`
            - Motivazione: La ricerca di film per titolo è un'operazione molto comune.
        - `film(genere)`: `INDEX idx_genere (genere)`
            - Motivazione: Permette di filtrare rapidamente i film per genere. Essendo un ENUM con bassa cardinalità, l'efficacia può variare ma è generalmente utile.
        - `film(anno_produzione)`: `INDEX idx_anno_produzione (anno_produzione)`
            - Motivazione: Per ricerche di film usciti in un certo anno o intervallo di anni.
        - `attore(cognome, nome)`: `INDEX idx_cognome_nome (cognome, nome)`
            - Motivazione: Ricerca di attori per cognome e nome. Un indice composto è efficiente.
        - `recita(ruolo)`: `INDEX idx_ruolo (ruolo)`
            - Motivazione: Se si effettuano ricerche per trovare film/attori basati su un ruolo specifico (es. tutti gli attori che hanno interpretato "Batman").

    **Definizione SQL degli indici (come già inserita negli script `CREATE TABLE`):** Gli indici sono stati definiti direttamente nelle istruzioni `CREATE TABLE` sopra con la sintassi `INDEX nome_indice (colonna1, ...)` o tramite la definizione di `PRIMARY KEY` e `FOREIGN KEY` (che spesso implicano la creazione di indici).

#### Quesito IV: Applicazione Web Responsive per Eventi Culturali

Il candidato esponga i punti critici da affrontare relativamente alle differenti proprietà di visualizzazione delle varie tipologie di dispositivi e alla rispettiva fruizione dei contenuti. Illustri possibili misure risolutive, con esempi relativi all'applicazione in questione.

Un'applicazione Web per la prenotazione on-line di eventi culturali deve essere facilmente fruibile sia da computer desktop che da dispositivi mobili (tablet, smartphone) per raggiungere il pubblico più ampio possibile. Questo introduce diverse sfide legate alla responsività e all'esperienza utente (UX).

**Punti Critici da Affrontare:**

1. **Dimensioni e Risoluzione dello Schermo Diverse:**

    - **Criticità:** Layout fissi progettati per desktop risultano illeggibili o difficili da navigare su schermi piccoli (testo troppo piccolo, elementi sovrapposti, necessità di zoom e scroll orizzontale). Immagini pesanti possono rallentare il caricamento su mobile.
    - **Misure Risolutive:**
        - **Responsive Web Design (RWD):** Utilizzo di griglie fluide (fluid grids), immagini flessibili e CSS media queries per adattare dinamicamente il layout al variare della larghezza dello schermo.
        - **Approccio Mobile-First:** Progettare inizialmente per l'esperienza mobile (con i vincoli più stringenti) e poi arricchire progressivamente il layout per schermi più grandi (progressive enhancement).
        - **Immagini Ottimizzate:** Utilizzo di immagini responsive (es. tag `<picture>` o attributi `srcset` e `sizes` per `<img>`) per servire versioni di immagini di dimensioni e risoluzioni appropriate al dispositivo. Compressione delle immagini (es. WebP).
    - **Esempio (App Eventi):** Su desktop, l'elenco degli eventi potrebbe essere visualizzato in una griglia a più colonne con dettagli visibili. Su smartphone, lo stesso elenco diventerebbe a colonna singola, con informazioni più concise per ciascun evento e dettagli accessibili tramite tap. Le immagini promozionali degli eventi verrebbero ridimensionate o tagliate in modo intelligente.
2. **Modalità di Interazione Diverse (Mouse vs. Touch):**

    - **Criticità:** Elementi interattivi troppo piccoli o vicini sono difficili da toccare con le dita. Funzionalità basate su hover (mouse-over) non funzionano su dispositivi touch.
    - **Misure Risolutive:**
        - **Target Tattili Adeguati:** Pulsanti, link e altri controlli devono avere dimensioni sufficienti (min. 44x44px CSS secondo le linee guida Apple/Google) e spaziatura adeguata per evitare tocchi accidentali.
        - **Alternative all'Hover:** Le informazioni o funzionalità rivelate tramite hover su desktop devono avere un meccanismo alternativo su mobile (es. visibili di default in forma concisa, o accessibili tramite un tap/click esplicito).
        - **Gesture Support:** Considerare l'uso di gesture comuni su mobile (swipe per caroselli, pinch-to-zoom per mappe/immagini se appropriato).
    - **Esempio (App Eventi):** Il calendario per la selezione delle date dell'evento dovrebbe avere giorni sufficientemente grandi per essere toccati. Se su desktop un tooltip appare al passaggio del mouse su un'icona, su mobile un tap sull'icona potrebbe aprire una piccola modale con la stessa informazione.
3. **Fruizione dei Contenuti e Navigazione:**

    - **Criticità:** Strutture di navigazione complesse (menu con molti livelli) sono difficili da implementare e usare su mobile. Grandi quantità di testo o tabelle complesse possono essere problematiche.
    - **Misure Risolutive:**
        - **Navigazione Semplificata su Mobile:** Utilizzo di pattern di navigazione mobile-friendly come il menu "hamburger" (off-canvas), navigazione a tab in fondo allo schermo, o una struttura gerarchica più piatta.
        - **Prioritizzazione dei Contenuti:** Mostrare le informazioni più importanti e le azioni principali in primo piano su mobile, nascondendo o rendendo accessibili tramite interazione contenuti secondari.
        - **Contenuti Adattivi:** Suddividere testi lunghi in sezioni più brevi con accordion o tab. Riformattare tabelle complesse per la visualizzazione verticale o permettere lo scroll orizzontale solo per la tabella.
    - **Esempio (App Eventi):** La pagina di dettaglio di un evento, che su desktop potrebbe mostrare tutte le informazioni (descrizione, mappa, recensioni, opzioni biglietto) in un'unica vista, su mobile potrebbe usare sezioni collassabili (accordion) o tab per "Descrizione", "Come Arrivare", "Acquista Biglietti". Il menu principale del sito potrebbe trasformarsi in un'icona "hamburger".
4. **Performance e Connettività:**

    - **Criticità:** Utenti mobili potrebbero avere connessioni più lente o instabili. Pagine pesanti (molti asset, JS non ottimizzato) portano a tempi di caricamento lunghi e frustrazione.
    - **Misure Risolutive:**
        - **Ottimizzazione delle Risorse:** Minificazione di CSS, JavaScript, HTML. Compressione (Gzip/Brotli). Lazy loading per immagini e video non immediatamente visibili.
        - **Caching:** Sfruttare il caching del browser e, se possibile, Service Workers per PWA (Progressive Web App) per migliorare i tempi di caricamento successivi e consentire funzionalità offline di base.
        - **Riduzione delle Richieste HTTP:** Combinare file CSS/JS ove possibile.
    - **Esempio (App Eventi):** Le immagini della galleria di un evento o le locandine degli eventi in un elenco potrebbero essere caricate solo quando diventano visibili nello viewport (lazy loading). Il codice JavaScript essenziale per il rendering iniziale viene caricato per primo, mentre script meno critici vengono differiti.
5. **Leggibilità del Testo:**

    - **Criticità:** Caratteri troppo piccoli o contrasto insufficiente rendono difficile la lettura, specialmente su schermi piccoli o all'aperto.
    - **Misure Risolutive:**
        - **Font Scalabili:** Utilizzare unità relative (em, rem, %) per le dimensioni dei caratteri, permettendo una facile scalabilità con le media queries.
        - **Contrasto Adeguato:** Assicurare un buon rapporto di contrasto tra testo e sfondo (seguire le linee guida WCAG).
        - **Interlinea e Lunghezza Riga:** Ottimizzare l'interlinea e la lunghezza delle righe di testo per una migliore leggibilità su ogni dispositivo.
    - **Esempio (App Eventi):** La dimensione del carattere base per la descrizione dell'evento potrebbe essere di 16px su mobile e leggermente maggiore o uguale su desktop, con un'interlinea di almeno 1.5. Il colore del testo per i dettagli del biglietto (prezzo, settore) deve avere un forte contrasto con lo sfondo.

Affrontando proattivamente questi punti critici attraverso un design responsivo e user-centered, è possibile creare un'applicazione per la prenotazione di eventi culturali che offra un'esperienza utente ottimale su qualsiasi dispositivo.

**Possibili misure risolutive:**

Per "illustrare possibili misure risolutive, con esempi relativi all'applicazione in questione" (App Web per prenotazione eventi culturali), possiamo dettagliare come segue:

1. **Responsive Web Design (RWD) tramite CSS Media Queries:**

    - **Concetto:** Le Media Queries CSS sono il fondamento del RWD. Permettono di applicare stili CSS diversi in base alle caratteristiche del dispositivo, principalmente la larghezza della finestra del browser (viewport).
    - **Implementazione:** Si definiscono dei "breakpoint" (punti di interruzione) a larghezze specifiche. Ad esempio:CSS

        ```css
        /* Stili di base (mobile-first) */
        .lista-eventi {
            display: flex;
            flex-direction: column; /* Eventi in colonna su mobile */
        }
        .evento-card {
            margin-bottom: 1em;
            border: 1px solid #ccc;
        }
        .evento-immagine {
            width: 100%; /* Immagine a piena larghezza del contenitore card */
            height: auto;
        }

        /* Tablet (es. > 768px) */
        @media (min-width: 768px) {
            .lista-eventi {
                flex-direction: row; /* Eventi affiancati */
                flex-wrap: wrap; /* Vanno a capo se non c'è spazio */
                justify-content: space-around;
            }
            .evento-card {
                width: 48%; /* Circa due card per riga */
            }
        }

        /* Desktop (es. > 1024px) */
        @media (min-width: 1024px) {
            .evento-card {
                width: 30%; /* Circa tre card per riga */
            }
            /* Mostra dettagli aggiuntivi dell'evento, nascosti su mobile */
            .dettagli-desktop-evento {
                display: block;
            }
        }
        ```

    - **Esempio (App Eventi):**
        - **Elenco Eventi:** Su mobile, ogni evento (`.evento-card`) occupa l'intera larghezza e sono impilati verticalmente. Su tablet, due eventi per riga. Su desktop, tre o più eventi per riga.
        - **Dettaglio Evento:** La locandina dell'evento (`.evento-immagine`) si adatta alla larghezza del suo contenitore. Elementi come una mappa dettagliata potrebbero essere visibili solo su schermi più grandi o presentati in modo diverso (es. un link su mobile, una mappa embeddata su desktop).
2. **Utilizzo di Framework CSS Responsivi (es. Bootstrap, Tailwind CSS):**

    - **Concetto:** Questi framework forniscono un insieme di classi CSS predefinite e componenti JavaScript per costruire rapidamente interfacce responsive. Il più noto per questo approccio è il sistema a griglia (Grid System) di Bootstrap.
    - **Implementazione (Bootstrap come esempio):** Bootstrap utilizza un sistema a griglia a 12 colonne. Si definisce come gli elementi devono occupare queste colonne a diverse dimensioni di schermo (xs, sm, md, lg, xl).HTML

        ```cs
        <div class="container">
            <div class="row">
                <div class="col-12 col-md-6 col-lg-4 mb-3"> <div class="card">
                        <img src="locandina_evento1.jpg" class="card-img-top" alt="Evento 1">
                        <div class="card-body">
                            <h5 class="card-title">Titolo Evento 1</h5>
                            <p class="card-text">Breve descrizione...</p>
                            <a href="#" class="btn btn-primary">Dettagli e Biglietti</a>
                        </div>
                    </div>
                </div>
                <div class="col-12 col-md-6 col-lg-4 mb-3">
                    </div>
                </div>
        </div>
        ```

    - **Esempio (App Eventi):** Oltre alla griglia per l'elenco eventi, Bootstrap offre componenti responsivi pronti all'uso come:
        - **Navbar:** La barra di navigazione principale collassa automaticamente in un menu "hamburger" su schermi piccoli.
        - **Modali:** Per visualizzare dettagli o form di prenotazione in finestre popup che si adattano allo schermo.
        - **Caroselli:** Per gallerie di immagini degli eventi.
3. **Layout CSS Moderni (Flexbox e CSS Grid):**

    - **Concetto:** Sono moduli CSS nativi per la creazione di layout complessi e flessibili, spesso usati anche internamente dai framework o in combinazione con essi.
        - **Flexbox:** Ideale per layout monodimensionali (righe o colonne).
        - **CSS Grid:** Ottimale per layout bidimensionali (righe e colonne contemporaneamente).
    - **Implementazione:**CSS

        ```css
        /* Esempio con Flexbox per il footer di un evento */
        .footer-evento {
            display: flex;
            justify-content: space-between; /* Spazia i figli */
            align-items: center;
        }
        .footer-evento .data { /*...*/ }
        .footer-evento .prezzo { /*...*/ }
        .footer-evento .bottone-acquista { /*...*/ }

        /* Su mobile, potrebbero andare in colonna */
        @media (max-width: 600px) {
            .footer-evento {
                flex-direction: column;
                align-items: flex-start;
            }
        }

        ```

    - **Esempio (App Eventi):**
        - Flexbox può essere usato per allineare elementi all'interno di una "card" di un evento (es. titolo, data, breve descrizione, bottone).
        - CSS Grid può definire il layout principale della pagina di dettaglio dell'evento, con aree per l'immagine principale, la descrizione, la mappa, e la sezione biglietti, riorganizzandole dinamicamente con le media query.
4. **Immagini Responsive:**

    - **Concetto:** Servire immagini di dimensioni e risoluzioni diverse in base al dispositivo per ottimizzare i tempi di caricamento e la qualità visiva.
    - **Implementazione:**
        - **Attributo `srcset` per `<img>`:** Fornisce al browser un elenco di immagini e le loro larghezze, lasciando che scelga la più adatta.HTML

            ```html
            <img src="evento-small.jpg"
                 srcset="evento-small.jpg 500w, evento-medium.jpg 1000w, evento-large.jpg 1500w"
                 sizes="(max-width: 600px) 90vw, (max-width: 1200px) 50vw, 33vw"
                 alt="Locandina evento">

            ```

        - **Elemento `<picture>`:** Offre un controllo ancora maggiore, permettendo di specificare sorgenti diverse per breakpoint diversi o per formati immagine differenti (es. WebP con fallback JPG).HTML

            ```html
            <picture>
               <source media="(min-width: 900px)" srcset="evento-desktop.webp" type="image/webp">
               <source media="(min-width: 600px)" srcset="evento-tablet.webp" type="image/webp">
               <source srcset="evento-mobile.webp" type="image/webp">
               <source media="(min-width: 900px)" srcset="evento-desktop.jpg">
               <source media="(min-width: 600px)" srcset="evento-tablet.jpg">
               <img src="evento-mobile.jpg" alt="Locandina evento">
            </picture>

            ```

    - **Esempio (App Eventi):** Le locandine degli eventi e le immagini nelle gallerie utilizzeranno queste tecniche per garantire che un utente su smartphone non scarichi inutilmente un'immagine ad altissima risoluzione pensata per un display 4K.

Adottando queste misure risolutive implementative, l'applicazione per la prenotazione di eventi culturali può offrire un'esperienza utente fluida e ottimizzata su un'ampia gamma di dispositivi.

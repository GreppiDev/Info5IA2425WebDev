# Requirements Analysis and Specification Document (RASD)

- [Requirements Analysis and Specification Document (RASD)](#requirements-analysis-and-specification-document-rasd)
  - [Introduzione alla scrittura di un RASD](#introduzione-alla-scrittura-di-un-rasd)
    - [Struttura Tipica di un RASD](#struttura-tipica-di-un-rasd)
    - [Consigli Aggiuntivi per un RASD Efficace](#consigli-aggiuntivi-per-un-rasd-efficace)
  - [Qual'è la differenza tra Progettazione Concettuale (E/R) e RASD?](#qualè-la-differenza-tra-progettazione-concettuale-er-e-rasd)
    - [In Sintesi: Differenze e Relazioni](#in-sintesi-differenze-e-relazioni)
  - [Approccio RASD (Requirements, Analysis, Specification, Design) nello sviluppo del software](#approccio-rasd-requirements-analysis-specification-design-nello-sviluppo-del-software)
    - [Legame tra Approccio RASD (Requirements, Analysis, Specification, Design) e Requirements Analysis and Specification Document (RASD) e Design Document (DD)](#legame-tra-approccio-rasd-requirements-analysis-specification-design-e-requirements-analysis-and-specification-document-rasd-e-design-document-dd)
    - [Fonti Accademiche e Industriali su RASD come Documento e Processo](#fonti-accademiche-e-industriali-su-rasd-come-documento-e-processo)
      - [RASD come documento](#rasd-come-documento)
      - [RASD come processo](#rasd-come-processo)

## Introduzione alla scrittura di un RASD

 **Requirements Analysis and Specification Document (RASD)**, in italiano **Documento di Analisi e Specifiche dei Requisiti**, è un artefatto fondamentale nello sviluppo di software e sistemi. Esso serve come ponte tra le esigenze degli stakeholder e la realizzazione tecnica del prodotto. Un RASD ben fatto garantisce che tutti comprendano cosa deve essere costruito, riducendo ambiguità e malintesi, e fornendo una base solida per le fasi successive del progetto.

Ecco una guida dettagliata su come dovrebbe essere strutturato e cosa dovrebbe contenere un RASD efficace:

### Struttura Tipica di un RASD

Un RASD segue solitamente una struttura standardizzata per garantire chiarezza e completezza. La struttura più comune include le seguenti sezioni principali:

1. **Introduzione (Introduction)**

   * **Scopo del Documento (Purpose of the Document):**  Definisci chiaramente lo scopo del RASD. Spiega perché questo documento è necessario e cosa si propone di raggiungere. Ad esempio: "Il presente documento specifica i requisiti per il nuovo sistema di gestione degli ordini (SGO) per l'azienda XYZ. Esso servirà come base per la progettazione, lo sviluppo e la verifica del sistema."
   * **Pubblico di Riferimento (Intended Audience):**  Identifica chi leggerà e userà questo documento. Tipicamente, il pubblico include:
       * Stakeholder aziendali (es. management, responsabili di reparto)
       * Utenti finali (se applicabile)
       * Team di sviluppo (analisti, progettisti, programmatori)
       * Team di test e qualità
       * Team di gestione del progetto
   * **Ambito di Applicazione (Scope):**  Descrivi in modo conciso il sistema o prodotto a cui si riferisce il RASD. Definisci i confini del sistema, cosa è incluso e cosa è escluso. Ad esempio, se stai sviluppando un'applicazione mobile per la gestione degli inventari, specifica se include la gestione degli ordini, l'integrazione con sistemi ERP esterni, ecc.
   * **Riferimenti (References):**  Elenca altri documenti che sono rilevanti per il RASD, come:
       * Documenti di visione del progetto
       * Documenti di marketing e vendite
       * Documenti di analisi di mercato
       * Standard e normative applicabili
       * Documenti di sistemi esistenti (se applicabile)
       * Glossari e terminologie aziendali

2. **Descrizione Generale (Overall Description)**

   * **Prospettiva del Prodotto (Product Perspective):**  Situa il prodotto nel suo contesto più ampio. Spiega se è un sistema completamente nuovo, un miglioramento di un sistema esistente, o parte di un sistema più grande. Illustra la relazione del prodotto con altri prodotti correlati. Ad esempio, se stai sviluppando un modulo aggiuntivo per un software esistente, spiega come si integrerà e interagirà con il sistema principale.
   * **Funzioni del Prodotto (Product Functions):**  Fornisci una sintesi ad alto livello delle principali funzionalità che il sistema dovrà fornire. Questa è una panoramica generale, non una lista dettagliata. Ad esempio, per un sistema bancario online, le funzioni potrebbero essere: gestione del conto corrente, bonifici, visualizzazione estratti conto, gestione investimenti.
   * **Classi di Utenti e Caratteristiche (User Classes and Characteristics):**  Identifica i diversi tipi di utenti che interagiranno con il sistema (es. amministratori, utenti standard, clienti, fornitori). Per ogni classe, descrivi le loro caratteristiche rilevanti (esperienza, livello di competenza tecnica, frequenza d'uso del sistema) e le loro esigenze specifiche.
   * **Ambiente Operativo (Operating Environment):**  Descrivi l'ambiente in cui il sistema opererà. Questo include:
       * Piattaforme hardware (server, client, dispositivi mobili)
       * Sistemi operativi (Windows, Linux, macOS, Android, iOS)
       * Database (Oracle, MySQL, PostgreSQL, MongoDB)
       * Reti (LAN, WAN, Internet, reti mobili)
       * Altri software o sistemi con cui il sistema deve interagire
       * Considerazioni ambientali (es. condizioni di temperatura, umidità, requisiti di alimentazione se applicabile per hardware specifico)
   * **Vincoli di Progettazione e Implementazione (Design and Implementation Constraints):**  Elenca qualsiasi vincolo che limiti le opzioni di progettazione e implementazione. Questi possono essere:
       * Vincoli tecnologici (linguaggi di programmazione, framework, librerie consentite o vietate)
       * Vincoli di performance (tempi di risposta massimi, throughput minimo)
       * Vincoli di sicurezza (standard di sicurezza da rispettare, politiche aziendali di sicurezza)
       * Vincoli di risorse (budget limitato, team di sviluppo con competenze specifiche limitate, tempi di consegna stringenti)
       * Standard di sviluppo (standard aziendali di codifica, linee guida di progettazione UI/UX)
       * Vincoli normativi e legali (normative sulla privacy, accessibilità, standard industriali)
   * **Ipotesi e Dipendenze (Assumptions and Dependencies):**  Elenca qualsiasi ipotesi fatta durante l'analisi dei requisiti e le dipendenze esterne che potrebbero influenzare il progetto. Le ipotesi sono affermazioni che si ritengono vere, ma che devono essere verificate. Le dipendenze sono fattori esterni al controllo del team di progetto che sono necessari per il successo del progetto. Esempi:
       * Ipotesi: "Si presume che la rete aziendale abbia una larghezza di banda minima di 10 Mbps."
       * Dipendenze: "Il successo dell'integrazione con il sistema di pagamento XYZ dipende dalla disponibilità e dalla cooperazione del team di sviluppo di XYZ."

3. **Requisiti Specifici (Specific Requirements)**

    Questa è la sezione più corposa e cruciale del RASD. Qui vengono definiti in dettaglio i requisiti che il sistema deve soddisfare. I requisiti sono solitamente suddivisi in categorie per facilitare l'organizzazione e la comprensione.

    * **Requisiti Funzionali (Functional Requirements):**  Descrivono *cosa* il sistema deve fare. Specificano le azioni, le operazioni e i servizi che il sistema deve fornire. Sono espressi in termini di input, processi e output. Esempi:
        * "Il sistema deve permettere agli utenti di autenticarsi in modo sicuro utilizzando nome utente e password."
        * "Il sistema deve consentire agli amministratori di aggiungere, modificare e cancellare prodotti dal catalogo."
        * "Il sistema deve generare report di vendita mensili in formato PDF ed Excel."
        * "Il sistema deve calcolare automaticamente le tasse e le spese di spedizione durante il processo di checkout."
        * "Il sistema deve notificare via email agli utenti la conferma dell'ordine e l'aggiornamento dello stato della spedizione."

        I requisiti funzionali possono essere ulteriormente organizzati in sottocategorie basate sulle principali funzionalità del sistema, o sui casi d'uso. È importante che siano:
        * **Completi:** Coprire tutte le funzionalità necessarie.
        * **Corretti:** Descritti in modo preciso e senza ambiguità.
        * **Verificabili:** Deve essere possibile verificare se il requisito è stato implementato correttamente.
        * **Tracciabili:** Ogni requisito dovrebbe essere collegato alla sua origine (es. stakeholder, obiettivo aziendale).
        * **Prioritizzati:**  Indicare l'importanza relativa di ogni requisito (es. essenziale, importante, desiderabile).

    * **Requisiti dell'Interfaccia Esterna (External Interface Requirements):**  Descrivono le interfacce del sistema con entità esterne. Queste si dividono in:
        * **Interfacce Utente (User Interfaces - UI):**  Definiscono l'aspetto, il comportamento e l'interazione dell'interfaccia utente. Possono includere:
            * Tipologia di interfaccia (GUI, CLI, API, Web UI, Mobile App)
            * Standard di progettazione UI/UX (stile, layout, navigazione, accessibilità)
            * Requisiti di localizzazione e internazionalizzazione (lingue supportate, formati di data e ora, valute)
            * Linee guida per l'usabilità e l'accessibilità (es. WCAG per l'accessibilità web)
            * Potenziali *mockup* o *wireframe* delle interfacce utente (è raccomandato includere [Image of Wireframe esempio interfaccia utente]  per visualizzare meglio l'aspetto).
        * **Interfacce Hardware (Hardware Interfaces):**  Specificano le interfacce tra il software e l'hardware esterno. Esempi:
            * Tipi di dispositivi supportati (stampanti, scanner, sensori, lettori di codici a barre)
            * Protocolli di comunicazione hardware (USB, seriale, Bluetooth, NFC)
            * Requisiti di performance hardware (velocità di trasferimento dati, latenza)
            * Requisiti di compatibilità hardware (versioni minime di firmware, driver)
        * **Interfacce Software (Software Interfaces):**  Definiscono le interazioni con altri sistemi software, librerie, API, servizi web. Esempi:
            * API da utilizzare (es. API di pagamento, API di social media, API di mapping)
            * Protocolli di comunicazione software (REST, SOAP, gRPC, MQTT)
            * Formati di dati per lo scambio (JSON, XML, CSV)
            * Requisiti di integrazione (modalità di autenticazione, gestione degli errori, sincronizzazione dei dati)
        * **Interfacce di Comunicazione (Communication Interfaces):**  Descrivono come il sistema comunica con il mondo esterno, sia utenti che altri sistemi. Esempi:
            * Protocolli di rete (TCP/IP, HTTP/HTTPS, SMTP, FTP)
            * Formati di messaggi (email, SMS, notifiche push)
            * Requisiti di sicurezza della comunicazione (crittografia, autenticazione reciproca)
            * Requisiti di banda e latenza per la comunicazione

    * **Requisiti di Sistema (System Features):**  Questa sezione può essere usata per descrivere le funzionalità del sistema da un punto di vista utente, a un livello più alto rispetto ai requisiti funzionali dettagliati. Le *features* rappresentano i servizi principali che il sistema offre. Esempi di *features* per un sistema di e-commerce:
        * Gestione del catalogo prodotti
        * Carrello della spesa e checkout
        * Gestione degli ordini
        * Gestione degli utenti e degli account
        * Sistema di ricerca e filtri prodotti
        * Sistema di recensioni e valutazioni dei prodotti
        * Integrazione con sistemi di pagamento online
        * Tracciamento delle spedizioni

        Per ogni *feature*, si può fornire una breve descrizione dei vantaggi per l'utente e delle funzionalità principali che la compongono.

    * **Casi d'Uso (Use Cases):**  I casi d'uso descrivono le interazioni tra gli utenti (o altri sistemi esterni) e il sistema per raggiungere specifici obiettivi. Ogni caso d'uso descrive una sequenza di azioni e reazioni. I casi d'uso sono utili per:
        * Chiarire i requisiti funzionali in modo pratico e orientato all'utente.
        * Validare i requisiti con gli stakeholder.
        * Fornire una base per i test di accettazione.

        Per ogni caso d'uso, dovresti includere:
        * **Nome del Caso d'Uso:** Un nome breve e descrittivo (es. "Effettuare un Ordine", "Modificare Profilo Utente").
        * **Attore Primario (Primary Actor):** L'utente o il sistema esterno che avvia il caso d'uso.
        * **Attori Secondari (Secondary Actors):** Altri utenti o sistemi esterni che partecipano al caso d'uso.
        * **Precondizioni (Preconditions):** Le condizioni che devono essere vere prima che il caso d'uso possa iniziare.
        * **Flusso Principale (Main Flow/Basic Flow):** La sequenza normale di passi che si verificano quando il caso d'uso ha successo.
        * **Flussi Alternativi (Alternative Flows):**  Sequenze di passi che si verificano in caso di eccezioni o condizioni particolari.
        * **Postcondizioni (Postconditions):** Le condizioni che sono vere una volta che il caso d'uso è completato con successo.
        * **Diagramma di Caso d'Uso (Use Case Diagram - opzionale ma raccomandato):**  Una rappresentazione grafica dei casi d'uso e degli attori ([Image of Diagramma caso d'uso esempio]).

    * **Requisiti dei Dati (Data Requirements):**  Descrivono i dati che il sistema gestirà. Include:
        * **Entità di Dati (Data Entities):**  Le principali categorie di informazioni che il sistema deve memorizzare e gestire (es. Clienti, Prodotti, Ordini, Fatture, Utenti).
        * **Attributi dei Dati (Data Attributes):**  Le proprietà di ogni entità (es. per l'entità "Prodotto": Nome, Descrizione, Prezzo, Immagine, Categoria, Codice Prodotto).
        * **Relazioni tra Dati (Data Relationships):**  Come le entità di dati sono correlate tra loro (es. un Cliente può avere più Ordini, un Ordine contiene più Prodotti).
        * **Tipi di Dati e Formati (Data Types and Formats):**  Specifiche sui tipi di dati (stringa, numero, data, booleano) e formati (formato data, formato numerico, lunghezza massima delle stringhe, formati di immagine).
        * **Validazione dei Dati (Data Validation Rules):**  Regole per garantire l'integrità e la correttezza dei dati (es. "Il prezzo deve essere un numero positivo", "L'indirizzo email deve essere in un formato valido").
        * **Conservazione dei Dati (Data Retention):**  Per quanto tempo i dati devono essere conservati e politiche di archiviazione.
        * **Requisiti di Migrazione Dati (Data Migration Requirements - se applicabile):**  Se è necessario migrare dati da sistemi esistenti, descrivere il processo, i formati di dati, le regole di trasformazione, la pulizia dei dati.
        * **Sicurezza dei Dati e Privacy (Data Security and Privacy):**  Requisiti relativi alla protezione dei dati sensibili (es. crittografia dei dati a riposo e in transito, controllo degli accessi, rispetto delle normative sulla privacy come GDPR).

    * **Requisiti Non Funzionali (Non-Functional Requirements - NFR):**  Descrivono le *qualità* del sistema, ovvero come il sistema deve essere. Si concentrano sugli aspetti non direttamente legati alle funzionalità, ma essenziali per il successo del sistema. Le categorie comuni di NFR includono:
        * **Requisiti di Performance (Performance Requirements):**  Specificano i livelli di performance che il sistema deve raggiungere. Esempi:
            * Tempo di risposta massimo per le transazioni (es. "Il tempo di risposta per la ricerca di prodotti non deve superare i 2 secondi").
            * Throughput (es. "Il sistema deve essere in grado di gestire almeno 1000 transazioni al secondo durante le ore di punta").
            * Scalabilità (es. "Il sistema deve essere scalabile per supportare un aumento del 50% del numero di utenti e transazioni nei prossimi 12 mesi").
            * Efficienza nell'uso delle risorse (memoria, CPU, banda di rete).
        * **Requisiti di Sicurezza (Security Requirements):**  Definiscono le politiche e i meccanismi di sicurezza per proteggere il sistema e i dati da minacce. Esempi:
            * Autenticazione e autorizzazione (es. "Solo gli utenti autorizzati devono poter accedere alle funzionalità amministrative").
            * Protezione contro vulnerabilità (es. "Il sistema deve essere protetto contro attacchi SQL Injection e Cross-Site Scripting").
            * Crittografia dei dati (a riposo e in transito).
            * Audit trail e logging delle attività di sicurezza.
            * Conformità a standard di sicurezza (es. ISO 27001, PCI DSS).
        * **Requisiti di Affidabilità (Reliability Requirements):**  Specificano quanto il sistema deve essere affidabile e disponibile. Esempi:
            * Disponibilità (es. "Il sistema deve essere disponibile il 99.99% del tempo").
            * Tempo medio tra guasti (MTBF - Mean Time Between Failures).
            * Tempo medio di ripristino (MTTR - Mean Time To Repair).
            * Tolleranza ai guasti (capacità di continuare a operare in caso di guasti parziali).
            * Backup e recovery dei dati.
        * **Requisiti di Usabilità (Usability Requirements):**  Definiscono quanto il sistema deve essere facile da usare e da imparare per gli utenti. Esempi:
            * Facilità d'uso e intuitività dell'interfaccia utente.
            * Curve di apprendimento minime per i nuovi utenti.
            * Efficienza nell'esecuzione delle attività comuni.
            * Feedback chiaro e comprensibile agli utenti.
            * Accessibilità per utenti con disabilità (conformità a standard di accessibilità).
        * **Requisiti di Manutenibilità (Maintainability Requirements):**  Specificano quanto il sistema deve essere facile da mantenere e modificare nel tempo. Esempi:
            * Modularità e leggibilità del codice.
            * Documentazione interna del codice.
            * Facilità di diagnosi e correzione degli errori.
            * Facilità di aggiungere nuove funzionalità o modificare quelle esistenti.
            * Testabilità del sistema.
        * **Requisiti di Portabilità (Portability Requirements):**  Specificano se e come il sistema deve essere portabile su diverse piattaforme o ambienti. Esempi:
            * Supporto di diversi sistemi operativi (Windows, Linux, macOS).
            * Supporto di diversi browser web (Chrome, Firefox, Safari, Edge).
            * Supporto di diversi database.
            * Standard di sviluppo che facilitano la portabilità.

4. **Appendici (Appendices - Opzionale)**

  Le appendici possono contenere informazioni aggiuntive utili, ma non essenziali per la comprensione dei requisiti principali. Esempi:

  * **Glossario (Glossary):** Definisce i termini tecnici, gli acronimi e la terminologia specifica del dominio utilizzati nel RASD. Aiuta a garantire una comprensione univoca dei termini.
  * **Modelli Aggiuntivi (Additional Models):**  Diagrammi, modelli, *flowchart*, tabelle, casi di esempio più dettagliati, analisi di dati, prototipi, *mockup* aggiuntivi che supportano la descrizione dei requisiti (es. [Image of Diagramma ER esempio] per i requisiti dei dati, [Image of Flowchart esempio processo] per i requisiti funzionali complessi).
  * **Matrice di Tracciabilità dei Requisiti (Requirements Traceability Matrix - RTM):**  Una tabella che collega ogni requisito ai suoi stakeholder, obiettivi aziendali, casi d'uso, elementi di progettazione, codice sorgente, casi di test. La RTM è fondamentale per garantire la tracciabilità e la verificabilità dei requisiti durante tutto il ciclo di vita del progetto.

### Consigli Aggiuntivi per un RASD Efficace

* **Coinvolgi gli Stakeholder:**  La stesura del RASD deve essere un processo collaborativo che coinvolge attivamente tutti gli stakeholder rilevanti (utenti, clienti, esperti di dominio, team di sviluppo, management). Organizza workshop, interviste, sessioni di brainstorming per raccogliere e validare i requisiti.
* **Usa un Linguaggio Chiaro e Conciso:**  Evita ambiguità, gergo tecnico eccessivo e frasi complesse. Usa un linguaggio semplice e diretto, comprensibile a tutti i lettori, inclusi gli stakeholder non tecnici.
* **Sii Specifico e Misurabile:**  I requisiti devono essere specifici, evitando descrizioni vaghe o generiche. Dove possibile, rendili misurabili e verificabili (es. invece di "Il sistema deve essere veloce", specifica "Il tempo di risposta per la ricerca non deve superare i 2 secondi").
* **Mantieni la Coerenza:**  Assicurati che i requisiti siano coerenti tra loro e non si contraddicano. Verifica la coerenza tra requisiti funzionali e non funzionali, e tra diverse sezioni del documento.
* **Gestisci le Modifiche ai Requisiti (Change Management):**  I requisiti possono cambiare durante il progetto. Definisci un processo formale per gestire le richieste di modifica, valutare l'impatto e aggiornare il RASD in modo controllato. Utilizza il *versioning* per tenere traccia delle modifiche al documento.
* **Revisiona e Valida il RASD:**  Organizza *review* del RASD con tutti gli stakeholder per individuare errori, omissioni, ambiguità e per validare i requisiti. Fai revisioni intermedie e una revisione finale prima di considerare il documento completo.
* **Usa Strumenti di Supporto (Requirements Management Tools):**  Per progetti complessi, considera l'utilizzo di strumenti software dedicati alla gestione dei requisiti (Requirements Management Tools). Questi strumenti facilitano la creazione, l'organizzazione, la tracciabilità, la gestione delle modifiche e la collaborazione sui requisiti.

Un RASD ben fatto è un investimento cruciale che ripaga nel corso di tutto il progetto. Aiuta a evitare costosi errori e rifacimenti successivi, assicura che il sistema soddisfi le reali esigenze degli utenti e degli stakeholder, e facilita la comunicazione e la collaborazione tra tutti i membri del team di progetto.

## Qual'è la differenza tra Progettazione Concettuale (E/R) e RASD?

La risposta breve è che **l'analisi basata su modello ER, requisiti funzionali/non funzionali e casi d'uso è *parte integrante* del processo di creazione di un RASD**. Queste tecniche *non* sono alternative al RASD, ma piuttosto *metodologie e strumenti* che vengono utilizzati *all'interno* del processo di analisi e specificazione dei requisiti, e i cui risultati vengono *documentati* nel RASD.

Vediamo nel dettaglio come si integrano:

**Analisi con Modello Concettuale Entity/Relationship (ER):**

* **Scopo:** Il modello ER si concentra sulla **modellazione dei dati**. Serve a identificare le **entità** principali del sistema informativo, i loro **attributi** e le **relazioni** tra queste entità. In altre parole, risponde alla domanda: "Quali dati dovrà gestire il sistema? Come sono strutturati e correlati questi dati?".
* **Contributo al RASD:**  Il modello ER, o una sua evoluzione/derivazione, **confluisce direttamente nella sezione "Requisiti dei Dati" del RASD**. In particolare:
    * Aiuta a identificare e descrivere le **Entità di Dati** (clienti, prodotti, ordini, ecc.).
    * Definisce gli **Attributi dei Dati** per ogni entità (nome cliente, descrizione prodotto, data ordine, ecc.).
    * Illustra le **Relazioni tra Dati** (un cliente *ha* molti ordini, un ordine *contiene* molti prodotti, ecc.).
    * In alcuni casi, il modello ER stesso (o un diagramma ER) può essere incluso come **modello aggiuntivo in Appendice** del RASD, oppure referenziato se documentato separatamente.
    * Le informazioni derivate dal modello ER influenzano anche la definizione dei **Requisiti Funzionali** (ad esempio, le operazioni per creare, leggere, aggiornare, cancellare - CRUD - le entità) e dei **Requisiti Non Funzionali** (ad esempio, requisiti di integrità e sicurezza dei dati).

**Analisi dei Requisiti Funzionali e Non Funzionali:**

* **Scopo:** Questa analisi è il cuore del processo di definizione dei requisiti. Mira a identificare e descrivere in dettaglio *cosa* il sistema deve fare (requisiti funzionali) e *come* deve farlo (requisiti non funzionali). Risponde alle domande: "Quali funzionalità deve offrire il sistema? Quali sono le caratteristiche di qualità attese?".
* **Contributo al RASD:** L'analisi dei requisiti funzionali e non funzionali costituisce la base principale per la **sezione "Requisiti Specifici" del RASD**. In particolare:
    * I **Requisiti Funzionali** vengono descritti in dettaglio, spesso organizzati per funzionalità principali o casi d'uso. Vengono definiti gli input, i processi e gli output per ogni funzione.
    * I **Requisiti Non Funzionali** (performance, sicurezza, affidabilità, usabilità, manutenibilità, portabilità, ecc.) vengono specificati in modo quantificabile e verificabile, definendo i livelli di qualità attesi per il sistema.
    * Le **"System Features"** del RASD sono spesso derivate da una sintesi ad alto livello dei requisiti funzionali, presentate da una prospettiva utente.

**Diagrammi dei Casi d'Uso:**

* **Scopo:** I diagrammi dei casi d'uso (e le descrizioni testuali associate) sono una tecnica per modellare le interazioni tra gli **attori** (utenti o sistemi esterni) e il **sistema** per raggiungere specifici **obiettivi**. Si concentrano sul "come" gli utenti interagiranno con il sistema per svolgere i loro compiti. Rispondono alla domanda: "Come gli utenti useranno il sistema per raggiungere i loro obiettivi?".
* **Contributo al RASD:** I diagrammi dei casi d'uso e le descrizioni dettagliate dei casi d'uso sono **direttamente inclusi nella sezione "Requisiti Specifici", sotto la sottosezione "Casi d'Uso" del RASD**. Inoltre:
    * I casi d'uso aiutano a **derivare e validare i Requisiti Funzionali**. Ogni caso d'uso dettagliato spesso si traduce in uno o più requisiti funzionali specifici.
    * I diagrammi di caso d'uso possono essere inclusi graficamente nel RASD ([Image of Diagramma caso d'uso esempio]).
    * Le descrizioni testuali dei casi d'uso (nome, attori, flusso principale, flussi alternativi, pre/post condizioni) vengono integralmente riportate nel RASD per ogni caso d'uso identificato.
    * I casi d'uso forniscono un contesto utente-centrico per comprendere e comunicare i requisiti, facilitando la validazione con gli stakeholder.

### In Sintesi: Differenze e Relazioni

| Tecnica di Analisi                     | Scopo Principale                                        | Contributo Principale al RASD                                                                     | Sezione del RASD Maggiormente Coinvolta                       |
| :--------------------------------------- | :------------------------------------------------------- | :------------------------------------------------------------------------------------------------- | :---------------------------------------------------------------- |
| **Modello Concettuale ER**           | Modellare i dati del sistema                               | Definire la struttura dei dati, entità, attributi, relazioni                                     | Requisiti dei Dati                                               |
| **Analisi Requisiti Funzionali/Non Funzionali** | Definire *cosa* e *come* il sistema deve funzionare         | Specificare in dettaglio le funzionalità e le qualità del sistema                                 | Requisiti Specifici (Funzionali e Non Funzionali)                |
| **Diagrammi dei Casi d'Uso**          | Modellare interazioni utente-sistema per obiettivi specifici | Descrivere le interazioni utente, derivare e validare requisiti funzionali, visualizzare interazioni | Requisiti Specifici (Casi d'Uso), Funzionali, System Features |

**La Differenza Fondamentale:**

* **Le Tecniche (ER, Analisi Funzionale/Non Funzionale, Casi d'Uso):** Sono *strumenti* e *metodologie* di analisi. Sono processi intellettuali e pratici che vengono messi in atto per *comprendere* e *definire* i requisiti. Producono *output specifici* (modelli ER, liste di requisiti, diagrammi di caso d'uso).
* **Il RASD:** È il *documento formale* e *completo* che *raccoglie, organizza e specifica* tutti i requisiti del sistema. *Utilizza* i risultati delle tecniche di analisi (inclusi modello ER, requisiti funzionali/non funzionali e casi d'uso) e li presenta in una struttura standardizzata e comprensibile a tutti gli stakeholder. È un *deliverable* del progetto, un artefatto di comunicazione e riferimento.

**Metafora:**

Si pensi alle tecniche di analisi come agli strumenti di un architetto (matita, squadra, software CAD) e al RASD come al progetto architettonico completo (disegni, specifiche, capitolato). L'architetto usa gli strumenti (tecniche) per creare il progetto (RASD). Il progetto (RASD) *contiene* e *formalizza* il lavoro svolto con gli strumenti (tecniche).

**In conclusione:**  L'analisi basata su modello ER, requisiti funzionali/non funzionali e casi d'uso è un *approccio metodologico* per *sviluppare* e *popolare* un RASD. Il RASD è il documento *finale* che contiene e formalizza i risultati di queste analisi, insieme ad altre informazioni contestuali e organizzative (introduzione, scopo, ambito, vincoli, ecc.). Non sono in contrapposizione, ma **sinergici e complementari**. Un RASD ben fatto si *basa* sull'applicazione efficace di queste tecniche di analisi.

## Approccio RASD (Requirements, Analysis, Specification, Design) nello sviluppo del software

In letteratura informatica quando si parla di RASD si intende il Requirements Analysis and Specification Document (RASD), tuttavia è possibile utilizzare in maniera iterativa le fasi di Requirements, Analysis, Specification e Design per arrivare allo sviluppo dettagliato di un software. **Rappresenta un approccio metodologico strutturato per guidare le fasi iniziali del ciclo di vita dello sviluppo software, assicurando che si comprendano a fondo i bisogni prima di iniziare la costruzione vera e propria del sistema**.

Ecco nel dettaglio cosa rappresenta ogni fase:

1. **Requirements (Requisiti):**

    - **Scopo:** Raccogliere, definire e documentare le esigenze degli stakeholder (utenti, committenti, ecc.) e le funzionalità che il sistema software deve possedere. Si cerca di rispondere alla domanda: "***Cosa*** *deve fare il sistema?*"
    - **Attività tipiche:** Interviste con gli utenti, analisi di documenti esistenti, workshop, questionari, definizione degli obiettivi del sistema, identificazione dei requisiti funzionali (cosa fa il sistema) e non funzionali (come lo fa: performance, sicurezza, usabilità, ecc.).
2. **Analysis (Analisi):**

    - **Scopo:** Comprendere e modellare il dominio del problema e i requisiti raccolti. Si analizzano i requisiti per chiarire ambiguità, identificare incongruenze e costruire una comprensione più profonda del sistema da sviluppare. Si risponde alla domanda: "***Qual è*** *il problema da risolvere e quali sono le sue componenti principali?*"
    - **Attività tipiche:** Identificazione degli attori (chi interagisce col sistema), definizione dei casi d'uso (scenari di interazione), creazione di modelli concettuali iniziali (come un primo schema E/R o un diagramma delle classi), analisi del flusso dei dati.
3. **Specification (Specifica):**

    - **Scopo:** Formalizzare i risultati dell'analisi in documenti e modelli precisi, dettagliati e non ambigui che descrivono esattamente cosa il sistema farà. Questo documento (la specifica dei requisiti software o SRS) serve come contratto tra il team di sviluppo e il committente. Si risponde alla domanda: "***Come*** *descriviamo in modo preciso e completo ciò che il sistema farà?*"
    - **Attività tipiche:** Raffinamento dei modelli di analisi (casi d'uso dettagliati, diagrammi di sequenza, diagrammi di stato), creazione di uno schema E/R più formale, definizione precisa di interfacce, vincoli, regole di business, criteri di accettazione.
4. **Design (Progettazione):**

    - **Scopo:** Definire l'architettura e la struttura interna del software per soddisfare le specifiche. Si decide come implementare le funzionalità richieste. Si risponde alla domanda: "***Come*** *verrà costruito il sistema a livello tecnico?*"
    - **Attività tipiche:** Progettazione dell'architettura software (es. client-server, microservizi), progettazione del database (schema logico e fisico), progettazione delle interfacce utente (UI/UX), definizione dei moduli software e delle loro interazioni, scelta delle tecnologie e degli algoritmi.

In sintesi, RASD è un percorso logico che parte dal capire le esigenze (Requirements), approfondisce la comprensione del problema (Analysis), definisce in modo formale cosa costruire (Specification) e infine stabilisce come costruirlo (Design), ponendo le basi per la successiva fase di implementazione (coding), testing e manutenzione. Anche se presentato linearmente, nella pratica moderna (specialmente con metodologie agili) queste fasi possono avere sovrapposizioni e cicli iterativi.

### Legame tra Approccio RASD (Requirements, Analysis, Specification, Design) e Requirements Analysis and Specification Document (RASD) e Design Document (DD)

Il legame tra il **processo RASD** (le fasi di Requirements, Analysis, Specification, Design) e i **documenti RASD** e **DD** è molto diretto: i documenti sono gli **artefatti** o **output** principali prodotti dalle fasi del processo.

Ecco come si collegano:

1. **Requirements Analysis and Specification Document (RASD Document / SRS):**

    - Questo documento è il risultato tangibile delle prime tre fasi del processo RASD: **Requirements (R)**, **Analysis (A)** e **Specification (S)**.
    - **Contenuto:** Raccoglie e formalizza tutto ciò che è stato scoperto e definito durante queste fasi. Tipicamente include:
        - Introduzione e obiettivi del sistema (dal Requirements).
        - Descrizione generale del sistema e del suo contesto (da Analysis).
        - Attori e Casi d'Uso (da Analysis e Specification).
        - Requisiti Funzionali dettagliati (cosa deve fare il sistema - da Requirements e Specification).
        - Requisiti Non Funzionali (performance, sicurezza, usabilità, ecc. - da Requirements e Specification).
        - Modelli concettuali (es. diagramma E/R, diagrammi di classi di analisi - da Analysis e Specification).
        - Vincoli, regole di business, ipotesi (da Specification).
        - Definizione delle interfacce esterne (da Specification).
        - Glossario dei termini (da tutte le fasi).
    - **Scopo:** Serve come "contratto" o punto di riferimento condiviso tra committente e team di sviluppo su ***cosa*** il software dovrà fare. È la base per la successiva fase di Design e per le attività di test (verifica che il sistema implementato soddisfi le specifiche). A volte viene chiamato **SRS (Software Requirements Specification)**.
2. **Design Document (DD / SDD):**

    - Questo documento è il risultato tangibile della quarta fase del processo RASD: **Design (D)**.
    - **Contenuto:** Descrive ***come*** il sistema sarà costruito per soddisfare i requisiti specificati nel documento RASD/SRS. Tipicamente include:
        - Descrizione dell'Architettura Software (es. architettura a livelli, microservizi, componenti principali e loro interazioni).
        - Progettazione del Database (schema logico relazionale, schema fisico, scelte implementative).
        - Progettazione dettagliata dei Moduli/Componenti (algoritmi chiave, strutture dati, interfacce interne).
        - Progettazione dell'Interfaccia Utente (UI/UX Design - a volte in un documento separato).
        - Strategie di gestione degli errori e della sicurezza a livello implementativo.
        - Scelte tecnologiche (linguaggi, framework, librerie).
    - **Scopo:** Serve come guida per gli sviluppatori durante la fase di implementazione (coding). Descrive la struttura interna del software e le decisioni tecniche prese. A volte viene chiamato **SDD (Software Design Document or Description)**.

In sintesi:

- Il **processo RASD** definisce le *attività* da svolgere (R -> A -> S -> D).
- Il **documento RASD/SRS** cattura l'output delle *attività* R, A, S (il ***cosa***).
- Il **documento DD/SDD** cattura l'output dell'*attività* D (il ***come***).

Quindi, i documenti sono la formalizzazione scritta e strutturata dei risultati ottenuti eseguendo le fasi del processo.

### Fonti Accademiche e Industriali su RASD come Documento e Processo

#### RASD come documento

RASD (Requirements Analysis and Specification Document) viene definito esplicitamente come acronimo in alcune risorse di ingegneria del software. Ad esempio, su AcronymFinder viene indicato che "RASD sta per Requirements Analysis and Specification Document (software engineering)". Nel contesto italiano, si parla comunemente di **"documento di analisi e specifica dei requisiti"**, equivalente all'SRS (Software Requirements Specification). Corsi universitari di ingegneria del software prevedono la redazione di un documento denominato proprio "analisi e specifica dei requisiti". Anche siti industriali e aziendali utilizzano questa dizione: per esempio il blog tecnico di Innovationweb descrive la "fase di analisi e raccolta dei requisiti" che produce proprio un *"documento di analisi e specifica dei requisiti"* (chiamato ASR). In termini di standard, gli IEEE definiscono pratiche di redazione dello SRS: il **IEEE Std. 830-1998** fornisce linee guida per la Software Requirements Specification (SRS), ora sostituite dal più recente **ISO/IEC/IEEE 29148** (2011/2018) per l'ingegneria dei requisiti. Ad esempio, il sito DocSheets sottolinea che IEEE 830-1998 descrive come strutturare un SRS, mentre la documentazione ISO/IEEE 29148 definisce i processi di *requirements engineering* nell'intero ciclo di vita.

- **Titolo:** *"RASD -- Requirements Analysis and Specification Document (software engineering)"*
    **Autori:** -- (Fonte: AcronymFinder) -- **Fonte:** AcronymFinder (sito web) -- **Link:** [acronymfinder.com](https://www.acronymfinder.com/Requirements-Analysis-and-Specification-Document-(software-engineering)-(RASD).html) -- **Lingua:** Inglese.

- **Titolo:** *"Specifiche di progetto (Ingegneria dei Sistemi Software -- Unipi)"*
    **Autori:** Laboratorio di Ingegneria dei Sistemi Software, Università di Pisa -- **Fonte:** Università di Pisa, Facoltà di Ingegneria -- **Link:** [docenti.ing.unipi.it/~a009435/issw/.../specifiche\_di\_progetto0910.pdf](http://docenti.ing.unipi.it/~a009435/issw/esercitazioni/progetti/specifiche_di_progetto0910.pdf) -- **Lingua:** Italiano.

- **Titolo:** *"Ingegneria del Software (Corso CT0090)"*
    **Autori:** Università Ca' Foscari Venezia -- **Fonte:** Sito ufficiale del corso -- **Link:** [unive.it/data/insegnamento/339906/programma](https://www.unive.it/data/insegnamento/339906/programma) -- **Lingua:** Italiano.

- **Titolo:** *"Sviluppo software"* (sezione blog)
    **Autori:** Innovationweb Srl (azienda) -- **Fonte:** Pagina web aziendale -- **Link:** [innovationweb.it/2031](https://www.innovationweb.it/2031) -- **Lingua:** Italiano.

- **Titolo:** *"Software Requirements Specifications"*
    **Autore:** Rahul Awati -- **Fonte:** IEEE Computer Society (computer.org) -- **Link:** [computer.org/resources/software-requirements-specifications](https://www.computer.org/resources/software-requirements-specifications/) -- **Lingua:** Inglese.

- **Titolo:** *"Enhancing Software Development with IEEE 830 Requirements Specifications: A Comprehensive Guide"*
    **Autori:** -- (Fonte: DocSheets) -- **Fonte:** DocSheets (sito web/blog aziendale) -- **Link:** [docsheets.com/ieee-830-requirements-specifications-guide](https://www.docsheets.com/ieee-830-requirements-specifications-guide) -- **Lingua:** Inglese.

- **Titolo:** *"ISO/IEC/IEEE 29148:2018 Standard"*
    **Autori:** -- (Fonte: documentazione ReqView) -- **Fonte:** ReqView (sito di strumenti RE) -- **Link:** [reqview.com/doc/iso-iec-ieee-29148-templates](https://www.reqview.com/doc/iso-iec-ieee-29148-templates/) -- **Lingua:** Inglese.

#### RASD come processo

Diversi testi e risorse didattiche illustrano fasi di sviluppo software che comprendono i passi "Requirements, Analysis, Specification, Design" nell'ordine. Ad esempio, la descrizione del modello a cascata riporta esplicitamente: *"requirements analysis, specification, design, implementation, testing, maintenance"*. Slide universitarie sul Requirements Engineering mostrano che l'analisi dei requisiti ("requirements analysis") confluisce nella redazione di un *"requirements specification document"* prima di proseguire con la progettazione. In sintesi, nell'approccio sequenziale classico (codice ingegneristico -- ingegneria dei requisiti a cascata) si pongono come prime fasi la raccolta e l'analisi dei requisiti e la loro specifica, seguite dalla fase di design. Queste suddivisioni sono sostenute anche dagli standard: ad esempio, lo standard internazionale **ISO/IEC/IEEE 29148:2018** definisce processi di Requirements Engineering strutturati (elinazione, analisi, specifica, ecc.), mentre il superseded IEEE 830-1998 (ora ISO/29148) si concentra sulla redazione del documento di requisiti (SRS).

- **Titolo:** *"The Waterfall Model"*
    **Autore:** Kaushik Mukherjee -- **Fonte:** Slide su Scribd -- **Link:** [scribd.com/document/186257914/The-Waterfall-Model](https://www.scribd.com/document/186257914/The-Waterfall-Model) -- **Lingua:** Inglese.

- **Titolo:** *"SEG3101: Introduction to Analysis and Specification (Fall 2010)"*
    **Autori:** Prof. A. Bochmann (Università di Ottawa) -- **Fonte:** Corso SEG3101, Università di Ottawa -- **Link:** [site.uottawa.ca/~bochmann/SEG3101/Notes/SEG3101-ch3-1.pdf](https://www.site.uottawa.ca/~bochmann/SEG3101/Notes/SEG3101-ch3-1%20-%20Intro%20to%20Analysis%20and%20Specification.pdf) -- **Lingua:** Inglese.

Ciascuna fonte riporta in maniera coerente il concetto di analisi e specifica dei requisiti (documento RASD) e/o descrive il processo organizzato in fasi che includono tali attività (Requirements → Analysis → Specification → Design). In particolare, gli standard di riferimento (IEEE 830-1998 e ISO/IEEE 29148) convalidano formalmente il ruolo del documento SRS/RASD.

**Fonti:** La bibliografia sopra riporta titoli, autori, fonte e link diretti alle risorse citate. Queste includono articoli e guide industriali, testi didattici universitari e documenti di standard internazionali, privilegiando l'inglese per la terminologia tecnica e l'italiano per fonti didattiche o normative utili nel contesto nazionale.

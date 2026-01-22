# Il Meccanismo CORS (Cross-Origin Resource Sharing)

- [Il Meccanismo CORS (Cross-Origin Resource Sharing)](#il-meccanismo-cors-cross-origin-resource-sharing)
  - [Guida teorica e pratica per architetture Web Decoupled](#guida-teorica-e-pratica-per-architetture-web-decoupled)
    - [1. Introduzione e Contesto: La Same-Origin Policy](#1-introduzione-e-contesto-la-same-origin-policy)
      - [1.1 Definizione di "Origin"](#11-definizione-di-origin)
      - [1.2 Lo scenario di studio](#12-lo-scenario-di-studio)
    - [2. Il Funzionamento del CORS](#2-il-funzionamento-del-cors)
      - [2.1 Richieste Semplici (Simple Requests)](#21-richieste-semplici-simple-requests)
      - [2.2 Richieste con Preflight (Non-Semplici)](#22-richieste-con-preflight-non-semplici)
    - [3. Implementazione in ASP.NET Core (Backend)](#3-implementazione-in-aspnet-core-backend)
      - [3.1 Configurazione della Policy (Services)](#31-configurazione-della-policy-services)
      - [3.2 Attivazione del Middleware (Pipeline)](#32-attivazione-del-middleware-pipeline)
    - [4. Ottimizzazione e Performance](#4-ottimizzazione-e-performance)
    - [5. Approfondimento sulla Sicurezza](#5-approfondimento-sulla-sicurezza)
      - [5.1 Scenario d'Attacco: Il "Sito Malvagio"](#51-scenario-dattacco-il-sito-malvagio)
      - [5.2 Il Ruolo Attivo del Browser](#52-il-ruolo-attivo-del-browser)
      - [5.3 Lettura vs Esecuzione (Side Effects)](#53-lettura-vs-esecuzione-side-effects)
      - [5.4 Gestione delle Credenziali](#54-gestione-delle-credenziali)
      - [5.5 Distinzione tra CORS, CSRF e XSS](#55-distinzione-tra-cors-csrf-e-xss)
        - [Meccanica dell'attacco CSRF (Cross-Site Request Forgery)](#meccanica-dellattacco-csrf-cross-site-request-forgery)
        - [Meccanica dell'attacco XSS (Cross-Site Scripting)](#meccanica-dellattacco-xss-cross-site-scripting)
    - [6. Troubleshooting e Analisi](#6-troubleshooting-e-analisi)

## Guida teorica e pratica per architetture Web Decoupled

### 1. Introduzione e Contesto: La Same-Origin Policy

Nel contesto dello sviluppo web moderno, è frequente adottare un'architettura disaccoppiata (Decoupled Architecture), dove il Frontend e il Backend risiedono su server logici o fisici differenti.

Per comprendere il CORS, è necessario partire dal concetto di sicurezza fondamentale dei browser: la **Same-Origin Policy (SOP)**. Questa politica di sicurezza impedisce a uno script caricato da un'origine di interagire con risorse provenienti da un'altra origine, a meno che non vi sia un'esplicita autorizzazione.

#### 1.1 Definizione di "Origin"

Due URL sono considerati appartenenti alla stessa origine (Same-Origin) solo se coincidono esattamente in tre componenti:

1. **Protocollo** (es. `http` vs `https`)

2. **Dominio** (es. `localhost` vs `127.0.0.1`)

3. **Porta** (es. `:5500` vs `:5001`)

#### 1.2 Lo scenario di studio

Si consideri il seguente scenario di sviluppo locale:

- **Frontend:** Pagina statica servita da Live Server all'indirizzo `http://localhost:5500`.

- **Backend:** API REST sviluppata in ASP.NET Core Minimal API all'indirizzo `http://localhost:5001`.

Nonostante entrambi i servizi risiedano sulla stessa macchina (`localhost`), le **porte differenti** (5500 e 5001) rendono le due entità *Cross-Origin*. Di conseguenza, qualsiasi richiesta AJAX/Fetch dal frontend verso il backend verrà bloccata dal browser, a meno che non venga implementato correttamente il protocollo CORS.

### 2. Il Funzionamento del CORS

Il **Cross-Origin Resource Sharing (CORS)** è un protocollo standard che permette ai server di allentare la Same-Origin Policy, indicando esplicitamente al browser quali origini esterne sono autorizzate ad accedere alle risorse.

Il meccanismo si basa sullo scambio di specifici **Header HTTP**. A seconda della complessità della richiesta, il browser adotta due comportamenti distinti.

#### 2.1 Richieste Semplici (Simple Requests)

Una richiesta è definita "semplice" se utilizza metodi sicuri (`GET`, `HEAD`, `POST`) e header standard (es. `text/plain` o `application/x-www-form-urlencoded`, ma **non** `application/json`).

**Flusso di esecuzione:**

1. Il browser invia immediatamente la richiesta al server, allegando l'header `Origin: http://localhost:5500`.

2. Il server elabora la richiesta ed esegue l'operazione (es. una query al database).

3. Il server invia la risposta. Se l'accesso è consentito, include l'header `Access-Control-Allow-Origin: http://localhost:5500`.

4. **Controllo Browser:** Il browser riceve la risposta. Se l'header è presente e corretto, consegna i dati al codice JavaScript. Se manca, genera un errore di rete e nasconde i dati al frontend.

> **Nota critica:** In una richiesta semplice, anche se il CORS fallisce, il server ha comunque ricevuto ed elaborato la richiesta (Side Effect).

#### 2.2 Richieste con Preflight (Non-Semplici)

Questo è il caso più comune nelle moderne Single Page Application (SPA). Si verifica quando la richiesta utilizza:

- Metodi diversi da GET/POST (es. `PUT`, `DELETE`, `PATCH`).

- Header personalizzati o specifici (es. `Content-Type: application/json` o `Authorization`).

**Flusso di esecuzione (Handshake):**

1. **Fase Preflight (OPTIONS):** Prima di inviare la richiesta reale, il browser invia automaticamente una richiesta HTTP con metodo `OPTIONS`. Chiede al server: *"L'origine X è autorizzata a inviare una POST con contenuto JSON?"*.

2. **Risposta Server:** Il backend, se configurato, risponde con `204 No Content` e gli header di autorizzazione (`Access-Control-Allow-Methods`, `Access-Control-Allow-Headers`).

3. **Richiesta Reale:** Solo se il Preflight ha successo, il browser invia la richiesta effettiva (`POST` con il payload JSON).

### 3. Implementazione in ASP.NET Core (Backend)

La configurazione del CORS in un ambiente .NET avviene nel file `Program.cs` e richiede due passaggi fondamentali: la definizione della Policy nei servizi e l'attivazione del Middleware nella pipeline.

#### 3.1 Configurazione della Policy (Services)

Il seguente codice definisce una policy robusta per l'ambiente di sviluppo, gestendo sia `localhost` che `127.0.0.1` (che il browser considera origini diverse).

```cs
var builder = WebApplication.CreateBuilder(args);

// Definizione del nome della policy per riutilizzo
var policyName = "FrontendDevPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(policyName, policy =>
        policy
            // 1. Definizione delle origini ammesse (White-list)
            .WithOrigins(
                "http://localhost:5500",
                "[http://127.0.0.1:5500](http://127.0.0.1:5500)"
            )
            // 2. Permette qualsiasi header (necessario per Content-Type: application/json)
            .AllowAnyHeader()
            // 3. Permette qualsiasi metodo (GET, POST, PUT, DELETE, ecc.)
            .AllowAnyMethod()
            // 4. Cache del Preflight (Ottimizzazione performance)
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10))
    );
});

```

#### 3.2 Attivazione del Middleware (Pipeline)

L'ordine di inserimento dei middleware è cruciale. `UseCors` deve essere invocato **dopo** il Routing ma **prima** dell'Autorizzazione e della mappatura degli endpoint.

```cs
var app = builder.Build();

// ... altri middleware (es. Swagger, Redirection) ...

app.UseRouting(); // Implicito in .NET 6+, ma concettualmente qui

// ATTIVAZIONE CORS: Deve avvenire prima che la richiesta raggiunga gli endpoint
app.UseCors(policyName);

app.UseAuthorization();

app.MapPost("/submit-json-data", (...) => { ... });

app.Run();

```

### 4. Ottimizzazione e Performance

L'uso del Preflight implica che per ogni operazione vengano effettuate due chiamate HTTP, raddoppiando potenzialmente la latenza di rete.

Per mitigare questo problema, si utilizza l'istruzione `.SetPreflightMaxAge(...)`.

Questa impostazione aggiunge l'header `Access-Control-Max-Age` alla risposta del Preflight. Istruisce il browser a memorizzare (cache) l'autorizzazione per un determinato periodo di tempo. Durante questo intervallo, le successive richieste verso lo stesso endpoint salteranno la fase `OPTIONS` ed eseguiranno direttamente la richiesta reale.

### 5. Approfondimento sulla Sicurezza

Il CORS è spesso frainteso come un "firewall" per il Backend. Al contrario, è un meccanismo di sicurezza **lato client** progettato per proteggere l'utente finale da attacchi malevoli, delegando al browser il compito di "poliziotto".

#### 5.1 Scenario d'Attacco: Il "Sito Malvagio"

Per comprendere l'importanza del CORS, analizziamo cosa accadrebbe senza di esso (o senza la Same-Origin Policy) in un tipico scenario di furto dati:

1. **L'Utente Autenticato:** L'utente accede al suo portale bancario `home-banking.it`. Il browser salva un cookie di sessione sicuro.

2. **L'Inganno:** In una nuova scheda, l'utente naviga inavvertitamente su `sito-malvagio.com` (magari tramite un link di phishing).

3. **L'Attacco:** Il `sito-malvagio.com` contiene uno script JavaScript invisibile che esegue:

    ```js
    fetch('[https://home-banking.it/api/estratto-conto](https://home-banking.it/api/estratto-conto)');

    ```

4. **Il Pericolo:** Senza protezioni, il browser invierebbe la richiesta includendo automaticamente i cookie di sessione dell'utente. Il server bancario, vedendo i cookie validi, risponderebbe con i dati sensibili, che verrebbero catturati dallo script malevolo.

**Come interviene il CORS in questo scenario:**

Quando lo script malevolo tenta la `fetch`, il browser allega l'header `Origin: https://sito-malvagio.com`. Il server bancario (correttamente configurato) controlla questo header, nota che non è nella sua lista di origini autorizzate e **non invia** l'header `Access-Control-Allow-Origin`. Il browser intercetta questa mancanza e blocca la consegna dei dati allo script.

#### 5.2 Il Ruolo Attivo del Browser

Il browser non è un semplice "tubo" che trasporta dati, ma agisce come un **User Agent attivo** con responsabilità di sicurezza:

- **Identificatore:** Aggiunge automaticamente e in modo immutabile l'header `Origin` a tutte le richieste Cross-Origin. JavaScript non può falsificare questo header.

- **Guardiano (Gatekeeper):** Dopo aver ricevuto la risposta dal server, il browser ispeziona gli header CORS.

    - Se il permesso (`Access-Control-Allow-Origin`) corrisponde all'origine della pagina, il browser **sblocca** l'oggetto risposta e permette al codice JavaScript di leggerne il contenuto (es. `response.json()`).

    - Se il permesso manca o non corrisponde, il browser **distrugge** i dati ricevuti e solleva un errore di rete nel codice JavaScript.

- **Negoziatore:** Gestisce autonomamente le richieste di Preflight (`OPTIONS`) senza che lo sviluppatore frontend debba scriverne il codice.

#### 5.3 Lettura vs Esecuzione (Side Effects)

È cruciale distinguere tra l'invio della richiesta e la lettura della risposta:

- **Protezione dalla Lettura:** Il CORS è eccellente nel prevenire che siti terzi *leggano* i tuoi dati.

- **Protezione dall'Esecuzione:** Nelle richieste "Semplici" (es. GET), il browser invia la richiesta *prima* di ottenere il permesso. Se quell'URL scatena un'azione (es. `GET /bonifico?importo=1000`), l'azione potrebbe essere eseguita sul server anche se il browser impedisce al sito attaccante di vedere la conferma. Per questo motivo, le operazioni che modificano lo stato devono sempre usare metodi che richiedono Preflight (come POST, PUT, DELETE) o essere protette da token anti-CSRF.

#### 5.4 Gestione delle Credenziali

Per scenari che coinvolgono cookie o autenticazione Windows tra domini diversi:

1. Il backend deve usare `.AllowCredentials()`.

2. Il backend **non può** usare il wildcard `*` in `.WithOrigins()`, ma deve elencare esplicitamente i domini.

3. Il frontend deve impostare `credentials: 'include'` nella chiamata Fetch.

#### 5.5 Distinzione tra CORS, CSRF e XSS

È fondamentale non confondere lo scenario CORS con altre vulnerabilità web comuni. Di seguito viene illustrata la meccanica di esecuzione di questi attacchi per evidenziarne le differenze.

##### Meccanica dell'attacco CSRF (Cross-Site Request Forgery)

In un attacco CSRF, l'attaccante induce la vittima (già autenticata su un sito legittimo) a visitare una pagina malevola o cliccare un link appositamente costruito.

Questa pagina contiene una richiesta nascosta (es. un form che si invia automaticamente via JS o un tag `<img>` con una src malevola) diretta verso il server target. Poiché il browser allega automaticamente i cookie di sessione per quel dominio, il server esegue l'azione credendola legittima (es. cambio password o bonifico).

- **Obiettivo:** Far eseguire un'azione indesiderata a nome della vittima (Scrittura/Modifica).

##### Meccanica dell'attacco XSS (Cross-Site Scripting)

In un attacco XSS, l'attaccante inietta codice JavaScript malevolo all'interno di una pagina legittima (ad esempio, inserendo uno script `<script>stealCookies()</script>` in un commento di un blog o in un parametro URL non validato).

Quando la vittima visita quella pagina del sito legittimo, il suo browser esegue lo script iniettato perché lo considera proveniente dall'origine fidata (Same-Origin). Questo permette allo script di accedere ai cookie o manipolare la pagina.

- **Obiettivo:** Eseguire codice arbitrario nel contesto della vittima (es. furto sessione).

| **Attacco** | **CSRF** | **XSS** | **Violazione CORS** |
| --- | --- | --- | --- |
| **Origine Attacco** | Sito Esterno Malevolo | **Sito Legittimo** (tramite iniezione) | Sito Esterno Malevolo |
| **Obiettivo** | **Eseguire** un'azione | Eseguire script arbitrari | **Leggere** dati sensibili |
| **Ruolo Browser** | Esegue la richiesta "alla cieca" | Esegue lo script (si fida del sito) | Blocca la lettura della risposta |
| **CORS protegge?** | **NO** (o parzialmente) | **NO** (è Same-Origin) | **SÌ** (difesa principale) |
| **Difesa corretta** | Anti-Forgery Token | Sanitizzazione Input / CSP | Configurazione CORS restrittiva |

- **CORS vs CSRF:** Nel CSRF, all'attaccante non interessa leggere la risposta, ma solo che il server esegua il comando.

- **CORS vs XSS:** Nell'XSS, l'attacco avviene "dall'interno" del sito fidato, quindi per il browser le regole Cross-Origin non si applicano.

### 6. Troubleshooting e Analisi

Durante lo sviluppo, la Developer Console del browser (Tab *Network*) è lo strumento principale di diagnosi.

- **Errore tipico:** `Access to fetch at '...' has been blocked by CORS policy: No 'Access-Control-Allow-Origin' header is present on the requested resource.`

    - *Causa:* Il server non ha riconosciuto l'origine o il middleware `UseCors` è posizionato nell'ordine sbagliato.

- **Analisi Traffico:**

    - Se si osserva una chiamata `OPTIONS` seguita da una `POST` (o altro metodo), il meccanismo Preflight sta funzionando correttamente.

    - Se la chiamata `OPTIONS` restituisce 405 o 500, la configurazione lato server rifiuta il verbo HTTP OPTIONS.

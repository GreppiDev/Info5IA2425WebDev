# Pericoli dei Cookie e Introduzione ai Token (JWT) per App Mobile

- [Pericoli dei Cookie e Introduzione ai Token (JWT) per App Mobile](#pericoli-dei-cookie-e-introduzione-ai-token-jwt-per-app-mobile)
  - [Introduzione](#introduzione)
  - [Pericoli e Vulnerabilità dei Cookie](#pericoli-e-vulnerabilità-dei-cookie)
    - [Cross-Site Scripting (XSS)](#cross-site-scripting-xss)
      - [Mitigazione del rischio XSS e Cookie](#mitigazione-del-rischio-xss-e-cookie)
    - [Cross-Site Request Forgery (CSRF)](#cross-site-request-forgery-csrf)
      - [Mitigazione del rischio CSRF](#mitigazione-del-rischio-csrf)
    - [Intercettazione (Man-in-the-Middle Attack)](#intercettazione-man-in-the-middle-attack)
      - [Mitigazione del rischio di Intercettazione](#mitigazione-del-rischio-di-intercettazione)
    - [Furto Fisico di Cookie](#furto-fisico-di-cookie)
      - [Mitigazione del rischio di Furto Fisico](#mitigazione-del-rischio-di-furto-fisico)
  - [Introduzione al riconoscimento dell'utente in App Mobile con Token (JWT)](#introduzione-al-riconoscimento-dellutente-in-app-mobile-con-token-jwt)
    - [Perché i Cookie non sono Ideali per le App Mobile?](#perché-i-cookie-non-sono-ideali-per-le-app-mobile)
    - [Vantaggi dei Token (JWT) per App Mobile e API](#vantaggi-dei-token-jwt-per-app-mobile-e-api)
  - [Token: Access Token e Refresh Token](#token-access-token-e-refresh-token)
    - [JWT (JSON Web Token): Struttura e Funzionamento](#jwt-json-web-token-struttura-e-funzionamento)
    - [Validazione JWT](#validazione-jwt)
    - [Esempio di generazione e validazione di JWT con librerie .NET (Minimal API)](#esempio-di-generazione-e-validazione-di-jwt-con-librerie-net-minimal-api)
  - [Riepilogo](#riepilogo)
  - [Risorse](#risorse)

## Introduzione

Questa lezione prosegue l'esplorazione del riconoscimento dell'utente, concentrandosi sui rischi di sicurezza associati all'uso dei cookie e introducendo i token, in particolare JWT (JSON Web Token), come alternativa più appropriata per le applicazioni mobile e le moderne API.

## Pericoli e Vulnerabilità dei Cookie

Nonostante la loro utilità per il riconoscimento dell'utente nel contesto web, i cookie non sono esenti da rischi di sicurezza. Una comprensione approfondita delle vulnerabilità associate ai cookie è fondamentale per sviluppare applicazioni web sicure e robuste.

**Principali Pericoli e Attacchi basati sui Cookie:**

### Cross-Site Scripting (XSS)

XSS è una delle vulnerabilità web più diffuse e sfruttabili. Un attacco XSS si verifica quando un attaccante riesce a iniettare codice malevolo (solitamente JavaScript) in una pagina web visualizzata da altri utenti.  Se un sito web è vulnerabile a XSS e i cookie di sessione *non* sono protetti con l'attributo `HttpOnly`, un attaccante può utilizzare JavaScript malevolo per rubare il cookie di sessione dell'utente.

```mermaid
sequenceDiagram
    actor Attaccante
    actor Utente
    participant SitoWeb as Sito Web Vulnerabile
    participant ServerMalevolo as Server dell'Attaccante
    
    Note over Attaccante, SitoWeb: Fase 1: Preparazione dell'attacco
    Attaccante->>SitoWeb: Individua vulnerabilità XSS
    Attaccante->>SitoWeb: Inietta codice JavaScript malevolo
    Note over SitoWeb: Il codice JS malevolo viene memorizzato nel database o nella risposta del server
    
    Note over Utente, SitoWeb: Fase 2: Esecuzione dell'attacco
    Utente->>SitoWeb: Visita la pagina web compromessa
    SitoWeb->>Utente: Ritorna la pagina con il codice JS malevolo
    Note over Utente: Il browser esegue il codice JS malevolo
    
    Note over Utente, ServerMalevolo: Fase 3: Furto del cookie
    Utente->>Utente: JS malevolo accede a document.cookie
    Utente->>ServerMalevolo: JS malevolo invia il cookie di sessione
    
    Note over Attaccante, ServerMalevolo: Fase 4: Impersonificazione
    ServerMalevolo->>Attaccante: Riceve il cookie di sessione
    Attaccante->>SitoWeb: Utilizza il cookie rubato per impersonare l'utente
    SitoWeb-->>Attaccante: Accesso autorizzato all'account dell'utente
```

*Diagramma semplificato di un attacco XSS che mira a rubare cookie di sessione.*

**Scenario di Attacco XSS e Furto di Cookie:**

  1. Un attaccante individua una vulnerabilità XSS in un sito web (es. un campo di input non correttamente validato).
  2. L'attaccante inietta codice JavaScript malevolo nel sito web attraverso la vulnerabilità XSS. Il codice JavaScript potrebbe essere memorizzato nel database del sito o iniettato in una risposta del server.
  3. Un utente legittimo visita la pagina web compromessa.
  4. Il browser dell'utente esegue il codice JavaScript malevolo iniettato.
  5. Il codice JavaScript malevolo utilizza `document.cookie` per accedere al cookie di sessione dell'utente.
  6. Il codice JavaScript invia il cookie di sessione a un server controllato dall'attaccante.
  7. L'attaccante, ora in possesso del cookie di sessione dell'utente legittimo, può impersonare l'utente e accedere all'account senza conoscerne le credenziali (username e password).

#### Mitigazione del rischio XSS e Cookie

 * **Sanificazione e Validazione Input:**  Implementare rigorose procedure di sanificazione e validazione di tutti gli input utente (sia lato client che lato server) per prevenire l'iniezione di codice malevolo.
 * **Utilizzo di `HttpOnly`:** Impostare sempre l'attributo `HttpOnly` per i cookie che contengono informazioni sensibili, come gli ID di sessione. Questo impedisce a JavaScript lato client di accedere a questi cookie, mitigando il rischio di furto tramite XSS.
 * **Content Security Policy (CSP):** Implementare Content Security Policy per controllare le risorse che il browser può caricare ed eseguire, riducendo la superficie di attacco per XSS.

### Cross-Site Request Forgery (CSRF)

CSRF è un tipo di attacco in cui un attaccante induce un utente autenticato a effettuare azioni indesiderate su un sito web a sua insaputa.  L'attaccante sfrutta il fatto che **il browser invia automaticamente i cookie (inclusi i cookie di sessione) con ogni richiesta verso il sito web target**.

```mermaid
sequenceDiagram
    actor Utente
    actor Attaccante
    participant SitoVulnerabile as Sito Web Vulnerabile (es. Banca)
    participant SitoMalevolo as Sito Web Malevolo
    
    Note over Utente, SitoVulnerabile: Fase 1: Autenticazione legittima
    Utente->>SitoVulnerabile: Si autentica
    SitoVulnerabile->>Utente: Imposta cookie di sessione
    Note over Utente: Browser memorizza il cookie di sessione

    Note over Attaccante, SitoMalevolo: Fase 2: Preparazione dell'attacco
    Attaccante->>SitoMalevolo: Crea sito malevolo/email con link
    
    Note over Utente, SitoMalevolo: Fase 3: Inganno dell'utente
    Attaccante->>Utente: Attira l'utente sul sito malevolo
    Utente->>SitoMalevolo: Visita il sito o clicca sul link malevolo
    Note over Utente: Utente è ancora autenticato sul sito vulnerabile
    
    Note over Utente, SitoVulnerabile: Fase 4: Esecuzione dell'attacco
    SitoMalevolo->>Utente: Contiene codice che genera richiesta automatica
    Note over Utente: Il browser genera richiesta al sito vulnerabile
    Utente->>SitoVulnerabile: Richiesta automatica con cookie di sessione allegati
    Note over SitoVulnerabile: Il sito non può distinguere tra richieste legittime e forgiate
    
    Note over SitoVulnerabile: Fase 5: Completamento dell'attacco
    SitoVulnerabile->>SitoVulnerabile: Elabora richiesta come legittima
    SitoVulnerabile->>SitoVulnerabile: Esegue azione indesiderata (es. bonifico)
    Note over Utente: L'utente non è consapevole dell'azione eseguita
```

*Diagramma di un attacco CSRF. L'attaccante inganna l'utente autenticato per eseguire azioni indesiderate.*

**Scenario di Attacco CSRF:**

1. Un utente è autenticato su un sito web vulnerabile (es. una piattaforma di home banking). Il browser ha memorizzato il cookie di sessione per quel sito.
2. Un attaccante crea un sito web malevolo o invia una email con un link malevolo.
3. L'utente, mentre è ancora autenticato sul sito web vulnerabile, visita il sito web malevolo o clicca sul link malevolo.
4. Il sito web malevolo contiene codice (es. immagini, form, JavaScript) che induce il browser dell'utente a inviare una richiesta HTTP al sito web vulnerabile (es. una richiesta per effettuare un bonifico bancario).
5. Poiché il browser invia automaticamente i cookie (incluso il cookie di sessione) con la richiesta, il sito web vulnerabile riceve una richiesta apparentemente legittima dall'utente autenticato.
6. Se il sito web vulnerabile non ha protezioni CSRF, elabora la richiesta, eseguendo l'azione indesiderata (es. bonifico bancario non autorizzato) *con l'identità dell'utente autenticato*, senza che l'utente ne sia consapevole o l'abbia intenzionalmente richiesta.

#### Mitigazione del rischio CSRF

 * **Token Anti-CSRF (Synchronizer Token Pattern):**  Utilizzare token anti-CSRF. Il server genera un token univoco per ogni sessione utente (o per ogni form). Questo token viene incluso in ogni form o richiesta "sensibile" (es. richieste POST, PUT, DELETE). Quando il server riceve una richiesta, verifica la presenza e la validità del token anti-CSRF. Se il token manca o non è valido, la richiesta viene rifiutata. Questo impedisce agli attaccanti di creare richieste valide per conto dell'utente, poiché non sono in grado di conoscere o generare il token anti-CSRF corretto.
 * **SameSite Cookie Attribute (con cautela):** L'attributo `SameSite` con valore `Strict` o `Lax` può offrire una certa protezione contro attacchi CSRF, limitando l'invio dei cookie in contesti cross-site. Tuttavia, `SameSite` non è una protezione CSRF completa e dovrebbe essere utilizzato in combinazione con altre misure (es. token anti-CSRF).

### Intercettazione (Man-in-the-Middle Attack)

Se i cookie vengono trasmessi su connessioni HTTP non sicure (senza HTTPS) e l'attributo `Secure` non è impostato, un attaccante che intercetta il traffico di rete (es. in una rete Wi-Fi pubblica non sicura) può rubare i cookie, inclusi i cookie di sessione.

#### Mitigazione del rischio di Intercettazione

* **Utilizzo di HTTPS:** Utilizzare sempre il protocollo HTTPS per tutte le comunicazioni web, in particolare per i siti web che gestiscono informazioni sensibili o autenticazione utente. HTTPS cifra il traffico di rete, proteggendo i cookie e altre informazioni da intercettazioni.
* **Attributo `Secure`:** Impostare sempre l'attributo `Secure` per i cookie, specialmente per i cookie di sessione. Questo assicura che i cookie vengano trasmessi solo su connessioni HTTPS.

### Furto Fisico di Cookie

In scenari meno comuni, un attaccante potrebbe riuscire ad accedere fisicamente al computer dell'utente e rubare i file cookie memorizzati dal browser. Questo potrebbe permettere all'attaccante di impersonare l'utente, specialmente se i cookie di sessione hanno una lunga durata.

#### Mitigazione del rischio di Furto Fisico

* **Limitare la durata dei cookie di sessione:** Ridurre al minimo la durata dei cookie di sessione. Sessioni più brevi riducono la finestra temporale in cui un cookie rubato può essere utilizzato.
* **Logout Inattivo (Session Timeout):** Implementare un meccanismo di logout automatico per inattività. Se l'utente è inattivo per un certo periodo di tempo, la sessione viene invalidata e il cookie di sessione diventa inutilizzabile.
* **Crittografia del disco (Full Disk Encryption):**  L'utilizzo della crittografia completa del disco del sistema operativo rende più difficile per un attaccante accedere ai file cookie memorizzati, anche in caso di accesso fisico al computer.

## Introduzione al riconoscimento dell'utente in App Mobile con Token (JWT)

Dopo aver analizzato i cookie e i loro limiti di sicurezza, si introduce un approccio alternativo e più adatto per il riconoscimento dell'utente in contesti diversi dal browser web, in particolare nelle **applicazioni mobile** e nelle moderne **API**: i **token**, con particolare attenzione ai **JSON Web Token (JWT)**.

### Perché i Cookie non sono Ideali per le App Mobile?

Sebbene sia tecnicamente *possibile* utilizzare i cookie anche nelle applicazioni mobile, questa pratica presenta diverse criticità e non è considerata la soluzione ottimale per diverse ragioni:

  * **Gestione complicata e meno integrata:** La gestione dei cookie nelle app mobile non è intrinsecamente supportata dal sistema operativo o dai framework di sviluppo mobile nello stesso modo in cui lo è nei browser web.  Gli sviluppatori dovrebbero implementare manualmente la memorizzazione, la gestione e l'invio dei cookie nelle richieste HTTP. Questo aumenta la complessità dello sviluppo e rende la gestione meno efficiente rispetto all'utilizzo di meccanismi più nativi per le app mobile.

  * **Natura browser-centrica dei cookie:** I cookie sono nati e si sono evoluti nel contesto dei browser web, strettamente legati al loro funzionamento. Il loro scopo principale è quello di persistere lo stato tra richieste *browser* e server web. Cercare di adattare i cookie a un contesto applicativo mobile, che è fondamentalmente diverso dal paradigma browser-server tradizionale, spesso risulta innaturale e inefficiente.

  * **Alternative più adatte e ottimizzate:**  Per le applicazioni mobile e le API, esistono alternative più adatte e ottimizzate rispetto ai cookie, che offrono maggiore flessibilità, sicurezza e performance. I **token**, in particolare i **JWT**, sono emersi come lo standard de facto per l'autenticazione e l'autorizzazione nelle moderne applicazioni mobile e API.

### Vantaggi dei Token (JWT) per App Mobile e API

  * **Statelessness (Lato Server):** Come i cookie stateless (con datastore esterno), i token JWT permettono di mantenere il backend **stateless**.  Il server non necessita di memorizzare informazioni di sessione per ogni utente. Tutte le informazioni necessarie per l'autenticazione e l'autorizzazione sono contenute all'interno del token stesso.  Questo semplifica la scalabilità e la gestione del backend, specialmente in architetture distribuite.
  * **Semplicità di utilizzo in App Mobile:** I token JWT sono semplici da gestire nelle app mobile. Possono essere facilmente memorizzati in storage sicuri del sistema operativo (es. Keychain/Keystore) e inclusi come header di autorizzazione (solitamente come "Bearer token") nelle richieste HTTP verso le API.
  * **Flessibilità e interoperabilità:**  I token JWT sono uno standard aperto e interoperabile. Possono essere generati e validati da diverse tecnologie e linguaggi di programmazione, facilitando l'integrazione tra diversi sistemi e piattaforme.
  * **Migliore scalabilità e performance:**  L'approccio basato su token JWT è tipicamente più performante e scalabile rispetto alle sessioni basate su cookie, specialmente in scenari ad alto traffico e architetture distribuite. La validazione del token è principalmente computazionale (verifica della firma digitale), richiedendo meno interazioni con database o sistemi di storage rispetto alla gestione delle sessioni cookie.
  * **Sicurezza migliorata (in alcuni aspetti):**  Se correttamente implementati, i token JWT possono offrire un livello di sicurezza elevato, grazie all'utilizzo di firme digitali e algoritmi crittografici robusti.  La natura stateless riduce anche la superficie di attacco legata alla gestione delle sessioni lato server.

## Token: Access Token e Refresh Token

L'autenticazione basata su token, in particolare con JWT, si basa sull'utilizzo di due tipi principali di token per gestire l'autenticazione e l'accesso alle risorse in modo efficiente e sicuro: **Access Token** e **Refresh Token**.

  * **Access Token:** Un **token di accesso** è un token di **breve durata** che viene utilizzato per autenticare le richieste dell'utente al server delle risorse (API).  L'access token viene incluso nell'header di autorizzazione (solitamente come **Bearer token**) di ogni richiesta che richiede autenticazione.  La sua breve durata (es. pochi minuti, ore) limita la finestra temporale in cui un token compromesso potrebbe essere utilizzato.

    **Esempio di Header di Autorizzazione con Access Token (Bearer Token):**

    ```text
    Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
    ```

  * **Refresh Token:** Un **token di refresh** è un token di **lunga durata** che viene utilizzato per ottenere un **nuovo access token** quando quello corrente è scaduto. Il refresh token viene rilasciato insieme all'access token durante il processo di login iniziale.  Viene memorizzato in modo sicuro nel client (app mobile) e utilizzato solo quando è necessario ottenere un nuovo access token.  La sua lunga durata (es. giorni, mesi) permette di mantenere l'utente autenticato per periodi prolungati, senza richiedere il reinserimento delle credenziali ad ogni scadenza dell'access token.

```mermaid
sequenceDiagram
    actor U as Utente
    participant C as Client (App)
    participant A as Server di Autenticazione
    participant R as Server delle Risorse (API)
    
    Note over U, A: Fase 1: Login iniziale e rilascio dei token
    U->>C: Fornisce credenziali (username/password)
    C->>A: Richiesta di autenticazione con credenziali
    A->>A: Verifica credenziali
    A->>C: Rilascia Access Token (breve durata) e Refresh Token (lunga durata)
    C->>C: Memorizza entrambi i token
    
    Note over C, R: Fase 2: Utilizzo dell'Access Token per accedere alle risorse
    C->>R: Richiesta API con Access Token nell'header
    R->>R: Verifica validità dell'Access Token
    R->>C: Risposta con le risorse richieste
    
    Note over C, A: Fase 3: Scadenza e rinnovo dell'Access Token
    C->>R: Richiesta API con Access Token scaduto
    R->>C: Errore 401 Unauthorized (token scaduto)
    C->>A: Richiesta nuovo Access Token usando il Refresh Token
    A->>A: Verifica validità del Refresh Token
    A->>C: Rilascia nuovo Access Token (e opzionalmente nuovo Refresh Token)
    
    Note over C, R: Fase 4: Ripresa delle operazioni con il nuovo Access Token
    C->>R: Richiesta API con nuovo Access Token
    R->>C: Risposta con le risorse richieste
```

**Flusso di Autenticazione con Access Token e Refresh Token:**

1. **Login (Autenticazione Iniziale):** L'utente inserisce le credenziali (username/password) nell'app mobile. L'app invia le credenziali al server di autenticazione.
2. **Generazione Token:** Se le credenziali sono valide, il server di autenticazione genera un **access token** (di breve durata) e un **refresh token** (di lunga durata).
3. **Rilascio Token:** Il server di autenticazione restituisce l'access token e il refresh token all'app mobile (solitamente in formato JSON).
4. **Memorizzazione Refresh Token:** L'app mobile memorizza in modo sicuro il **refresh token**. La posizione di memorizzazione sicura dipende dalla piattaforma mobile (es. Keychain su iOS, Keystore su Android). L'access token viene tipicamente memorizzato in memoria o in uno storage meno persistente, poiché ha una durata limitata.
5. **Accesso alle Risorse Protette:** Per ogni successiva richiesta di risorse protette (API), l'app mobile include l'**access token** nell'header di autorizzazione (Bearer token).
6. **Validazione Access Token:** Il server delle risorse (API) riceve la richiesta con l'access token. Il server valida l'access token. Se il token è valido (non scaduto, firma corretta, etc.), il server elabora la richiesta e restituisce la risposta. Se l'access token non è valido, il server restituisce un errore di autenticazione (es. HTTP 401 Unauthorized).
7. **Refresh Token Grant (Rinnovo Access Token):** Quando l'access token scade, l'app mobile utilizza il **refresh token** memorizzato per richiedere al server di autenticazione un **nuovo access token** (e opzionalmente un nuovo refresh token).  Questa richiesta di refresh token grant avviene "dietro le quinte", senza richiedere nuovamente l'intervento dell'utente (re-login).
8. **Rilascio Nuovo Access Token:** Se il refresh token è valido, il server di autenticazione genera un **nuovo access token** (e opzionalmente un nuovo refresh token) e li restituisce all'app mobile. L'app sostituisce l'access token scaduto con quello nuovo e può continuare ad accedere alle risorse protette senza interruzioni. Se il refresh token non è valido (es. scaduto, revocato, manomesso), il server restituisce un errore e l'utente deve essere re-autenticato (login iniziale).

### JWT (JSON Web Token): Struttura e Funzionamento

**JSON Web Token (JWT)** è uno standard aperto (RFC 7519) ampiamente utilizzato per creare token di accesso sicuri e compatti. Un JWT è una stringa codificata in formato JSON, composta da tre parti principali separate da punti:

[Image of JWT Structure Diagram]
*Diagramma della struttura di un JWT: Header, Payload, Signature.*

1. **Header (Intestazione):** L'header contiene metadati sul token, solitamente in formato JSON, indicando il **tipo di token** (tipicamente `"JWT"`) e l'**algoritmo di firma** utilizzato per proteggere il token (es. `"HS256"` per HMAC-SHA256, `"RS256"` per RSA-SHA256). L'header viene codificato in Base64Url.

    **Esempio di Header JWT:**

    ```json
    {
      "alg": "HS256",
      "typ": "JWT"
    }
    ```

2. **Payload (Payload o Claims):** Il payload contiene le **claims**, ovvero le "affermazioni" o le informazioni relative all'entità (utente, applicazione) a cui il token si riferisce. Le claims sono coppie chiave-valore in formato JSON e vengono codificate in Base64Url.  Esistono tre tipi di claims:

      * **Registered Claims:** Claims "registrate" e standardizzate dallo standard JWT (RFC 7519). Esempi: `iss` (issuer - emittente), `sub` (subject - soggetto del token, solitamente l'ID utente), `aud` (audience - destinatario), `exp` (expiration time - data di scadenza), `nbf` (not before - non valido prima di), `iat` (issued at - data di emissione), `jti` (JWT ID - identificativo univoco del token).
      * **Public Claims:** Claims definite pubblicamente e utilizzabili da chiunque. È importante evitare collisioni di nomi con le Registered Claims.
      * **Private Claims:** Claims personalizzate e specifiche per l'applicazione.

    **Esempio di Payload JWT (Claims):**

    ```json
    {
      "iss": "https://example.com",
      "sub": "utente123",
      "name": "Mario Rossi",
      "role": "administrator",
      "exp": 1678886400 // Data di scadenza (timestamp Unix)
    }
    ```

3. **Signature (Firma):** La firma digitale è calcolata sull'header codificato in Base64Url, sul payload codificato in Base64Url e su una **chiave segreta** (per algoritmi simmetrici come HS256) o **chiave privata** (per algoritmi asimmetrici come RS256).  La firma garantisce l'**integrità** del token (verifica che non sia stato manomesso dopo l'emissione) e la **provenienza** (verifica che il token sia stato emesso dal server di autenticazione che possiede la chiave segreta o la chiave privata).  La firma viene codificata in Base64Url.

    **Processo di Firma JWT (Esempio con HS256):**

    ```js
    Firma = HMACSHA256(
      base64UrlEncode(Header) + "." +
      base64UrlEncode(Payload),
      segreto // Chiave segreta condivisa
    )
    ```

    **JWT Completo:** L'header codificato in Base64Url, il payload codificato in Base64Url e la firma codificata in Base64Url vengono concatenati separati da punti (`.`) per formare il JWT completo:

    ```js
    JWT = base64UrlEncode(Header) + "." + base64UrlEncode(Payload) + "." + base64UrlEncode(Signature)
    ```

### Validazione JWT

Quando un server risorse (API) riceve un JWT, deve validarlo per assicurarsi che sia autentico e non sia stato manomesso.  La validazione JWT tipicamente include i seguenti passaggi:

1. **Verifica della Firma:** Il server utilizza la **chiave pubblica** (per algoritmi asimmetrici come RS256) o la **chiave segreta condivisa** (per algoritmi simmetrici come HS256) per **verificare la firma digitale** del JWT. Se la firma non è valida, il token è considerato non autentico o manomesso.
2. **Verifica della Scadenza (`exp` Claim):** Il server controlla la claim `exp` (expiration time) per verificare se il token è scaduto. Se il token è scaduto, non è più valido.
3. **Altre Validazioni (Opzionali):** Il server può effettuare ulteriori validazioni, come verificare la claim `iss` (issuer) per assicurarsi che il token provenga da un emittente attendibile, o la claim `aud` (audience) per verificare che il token sia destinato a quel particolare server risorse.

### Esempio di generazione e validazione di JWT con librerie .NET (Minimal API)

Per generare e validare JWT in .NET, si possono utilizzare librerie come `System.IdentityModel.Tokens.Jwt`.

**Generazione JWT (Esempio Minimal API):**

```cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configurazione JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/login", [AllowAnonymous] (HttpContext httpContext) =>
{
    // Logica di autenticazione utente (es. verifica credenziali)
    // ...

    // Generazione JWT
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new[] {
        new Claim(JwtRegisteredClaimNames.Sub, "utente123"), // Subject (ID Utente)
        new Claim(JwtRegisteredClaimNames.Email, "utente@example.com"), // Altre claims
        new Claim(ClaimTypes.Role, "Administrator") // Ruolo
    };

    var token = new JwtSecurityToken(
        builder.Configuration["Jwt:Issuer"],
        builder.Configuration["Jwt:Audience"],
        claims,
        expires: DateTime.Now.AddMinutes(30), // Scadenza token
        signingCredentials: credentials);

    return Results.Ok(new JwtSecurityTokenHandler().WriteToken(token)); // Restituisci JWT come stringa
});

app.MapGet("/risorsa-protetta", [Authorize] () => {
    return Results.Ok("Risorsa protetta accessibile!");
});

app.Run();
```

**Validazione JWT (Middleware di autenticazione JWT Bearer in ASP.NET Core):**

La validazione dei JWT in ASP.NET Core è gestita automaticamente dal middleware di autenticazione JWT Bearer, configurato nel metodo `AddJwtBearer` in `Startup.cs` o `Program.cs` come mostrato nell'esempio precedente.  Il middleware intercetta le richieste HTTP in ingresso, estrae il JWT dall'header di autorizzazione (Bearer token), lo valida utilizzando le `TokenValidationParameters` configurate e, se valido, autentica l'utente.

**Configurazione (Estratto da `appsettings.json`):**

```json
{
  "Jwt": {
    "Key": "ChiaveSegretaMoltoLungaEComplessa", // Sostituire con una chiave segreta sicura in produzione
    "Issuer": "https://issuer-example.com",
    "Audience": "https://audience-example.com"
  }
}
```

**Esercitazione:** Eseguire l'esempio di codice Minimal API.  Utilizzare un client HTTP (es. Postman, Insomnia) per inviare una richiesta POST a `/login`.  La risposta conterrà un JWT.  Utilizzare lo stesso client HTTP per inviare una richiesta GET a `/risorsa-protetta`, includendo il JWT nell'header di autorizzazione (Bearer token). Verificare che la richiesta a `/risorsa-protetta` abbia successo solo se si include un JWT valido nell'header di autorizzazione.  Modificare il JWT (es. alterando la firma) o utilizzare un JWT scaduto e verificare che la richiesta a `/risorsa-protetta` venga rifiutata con errore di autenticazione.  Analizzare la struttura del JWT decodificandolo (es. utilizzando siti web come [https://jwt.io](https://jwt.io)).

## Riepilogo

Questa lezione ha esplorato in dettaglio i pericoli di sicurezza associati ai cookie, sottolineando l'importanza di adottare misure di mitigazione come `HttpOnly`, `Secure`, token anti-CSRF e HTTPS.  È stato introdotto il concetto di token, in particolare JWT, come alternativa più adatta ai cookie per le app mobile e le API. Sono stati discussi i vantaggi dei JWT, la loro struttura interna (Header, Payload, Signature), il flusso di autenticazione con access token e refresh token, e sono stati forniti esempi pratici di generazione e validazione JWT con ASP.NET Minimal API.

La prossima lezione proseguirà l'approfondimento dei token JWT, analizzando in dettaglio la gestione stateless del backend, i refresh token opachi, il carico di lavoro rispetto ai cookie, la gestione dei ruoli utente e la gestione del logout con i JWT e il Security Stamp di ASP.NET Core Identity.

## Risorse

[JWT Introduction](https://jwt.io/introduction)

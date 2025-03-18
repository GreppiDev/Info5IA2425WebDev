# Introduzione ai Token (JWT)

- [Introduzione ai Token (JWT)](#introduzione-ai-token-jwt)
  - [Introduzione](#introduzione)
  - [Introduzione al riconoscimento dell'utente in App Mobile con Token (JWT)](#introduzione-al-riconoscimento-dellutente-in-app-mobile-con-token-jwt)
    - [Perché i Cookie non sono Ideali per le App Mobile?](#perché-i-cookie-non-sono-ideali-per-le-app-mobile)
    - [Vantaggi dei Token (JWT) per App Mobile e API](#vantaggi-dei-token-jwt-per-app-mobile-e-api)
  - [Token: Access Token e Refresh Token](#token-access-token-e-refresh-token)
    - [JWT (JSON Web Token): Struttura e Funzionamento](#jwt-json-web-token-struttura-e-funzionamento)
    - [Validazione JWT](#validazione-jwt)
    - [Esempio di generazione e validazione di JWT con librerie .NET (Minimal API)](#esempio-di-generazione-e-validazione-di-jwt-con-librerie-net-minimal-api)
  - [Un esempio completo di autenticazione basata su token JWT con Minimal API](#un-esempio-completo-di-autenticazione-basata-su-token-jwt-con-minimal-api)
  - [Riepilogo](#riepilogo)
  - [Risorse](#risorse)

## Introduzione

Questa lezione prosegue l'esplorazione del riconoscimento dell'utente introducendo i token, in particolare JWT (JSON Web Token), come alternativa più appropriata per le applicazioni mobile e le moderne API.

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

*Diagramma di flusso che mostra l'autenticazione basata su `Access Token` e `Refresh Token`*

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

![Image of JWT Structure Diagram](jwt-structure.svg)
*Struttura di un JWT: Header, Payload, Signature.*

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

Quando un server di risorse (API) riceve un JWT, deve validarlo per assicurarsi che sia autentico e non sia stato manomesso.  La validazione JWT tipicamente include i seguenti passaggi:

1. **Verifica della Firma:** Il server utilizza la **chiave pubblica** (per algoritmi asimmetrici come RS256) o la **chiave segreta condivisa** (per algoritmi simmetrici come HS256) per **verificare la firma digitale** del JWT. Se la firma non è valida, il token è considerato non autentico o manomesso.
2. **Verifica della Scadenza (`exp` Claim):** Il server controlla la claim `exp` (expiration time) per verificare se il token è scaduto. Se il token è scaduto, non è più valido.
3. **Altre Validazioni (Opzionali):** Il server può effettuare ulteriori validazioni, come verificare la claim `iss` (issuer) per assicurarsi che il token provenga da un emittente attendibile, o la claim `aud` (audience) per verificare che il token sia destinato a quel particolare server risorse.

### Esempio di generazione e validazione di JWT con librerie .NET (Minimal API)

Per generare e validare JWT in .NET, si possono utilizzare librerie come `System.IdentityModel.Tokens.Jwt`.

**Generazione JWT (Esempio Minimal API):**

```cs
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

// Configurazione dell'autenticazione tramite JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ??
            throw new InvalidOperationException("Jwt:Issuer non configurata"),
        ValidAudience = builder.Configuration["Jwt:Audience"] ??
            throw new InvalidOperationException("Jwt:Audience non configurata"),
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ??
                throw new InvalidOperationException("Jwt:Key non configurata")))
    };

});

// Add authorization services
builder.Services.AddAuthorization(options =>
{
    // Definizione di una policy per gli amministratori
    options.AddPolicy("RequireAdministratorRole", policy =>
        policy.RequireRole("Administrator"));

});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Metodi helper per la gestione dei token
string GenerateAccessToken(IConfiguration config, IEnumerable<Claim> claims)
{
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"] ??
        throw new InvalidOperationException("Jwt:Key non configurata")));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: config["Jwt:Issuer"] ??
            throw new InvalidOperationException("Jwt:Issuer non configurata"),
        audience: config["Jwt:Audience"] ??
            throw new InvalidOperationException("Jwt:Audience non configurata"),
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(30), // Usare UTC per timestamp
        signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
}

string GenerateRefreshToken()
{
    // Genera un token più sicuro rispetto al semplice Guid
    // var randomNumber = new byte[32]; // 256 bit
    // using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
    // rng.GetBytes(randomNumber);

    // // Converte in una stringa base64 (più sicura di un GUID e con una maggiore entropia)
    // return Convert.ToBase64String(randomNumber);

    // Generazione di un refresh token (opaco)
    var refreshToken = Guid.NewGuid().ToString();
    return refreshToken;
}

// Endpoint per il login che restituisce un access token e un refresh token
app.MapPost("/login", (UserLogin login) =>
{
    // Validazione input
    if (login is null || string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.Password))
    {
        return Results.BadRequest("Username e password sono richiesti");
    }

    // Verifica per l'utente normale con privilegi limitati
    if (login.Username == "user" && login.Password == "pass")
    {
        string userId = "user123";
        string userEmail = "user@example.com";

        // Utente standard con ruolo di sola visualizzazione
        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, userEmail),
            new Claim(ClaimTypes.Name, login.Username),
            new Claim(ClaimTypes.Role, "Viewer")
        };

        var accessToken = GenerateAccessToken(builder.Configuration, claims);
        var refreshToken = GenerateRefreshToken();

        // Uso di thread-safe ConcurrentDictionary
        refreshTokenStore[userId] = new RefreshTokenInfo
        {
            Token = refreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        refreshTokenToUserMap[refreshToken] = userId;

        return Results.Ok(new
        {
            accessToken,
            refreshToken,
        });
    }
    // Verifica per l'amministratore con privilegi completi
    else if (login.Username == "admin" && login.Password == "Admin123!")
    {
        string userId = "admin456";
        string userEmail = "admin@example.com";

        // Amministratore con ruoli multipli
        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, userEmail),
            new Claim(ClaimTypes.Name, login.Username),
            new Claim(ClaimTypes.Role, "Administrator"),
            new Claim(ClaimTypes.Role, "SuperAdministrator")  // Utente con più ruoli
        };

        var accessToken = GenerateAccessToken(builder.Configuration, claims);
        var refreshToken = GenerateRefreshToken();

        // Uso di thread-safe ConcurrentDictionary
        refreshTokenStore[userId] = new RefreshTokenInfo
        {
            Token = refreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        refreshTokenToUserMap[refreshToken] = userId;

        return Results.Ok(new
        {
            accessToken,
            refreshToken,

        });
    }

    return Results.Unauthorized();
})
.WithName("Login")
.WithOpenApi(operation =>
{
    operation.Summary = "Effettua il login e ottiene token di accesso";
    operation.Description = "Autentica l'utente e restituisce access token e refresh token";
    return operation;
});

// Endpoint protetto, accessibile solo con un token valido
app.MapGet("/protected", (HttpContext context) =>
{
    // Recupera il nome dell'utente dalle claims del token
    var username = context.User?.Identity?.Name;
    var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    var userEmail = context.User?.FindFirstValue(ClaimTypes.Email);
    var userRole = context.User?.FindFirstValue(ClaimTypes.Role);

    return Results.Ok(new
    {
        Message = $"Benvenuto {username}! Questo è un contenuto riservato.",
        UserDetails = new
        {
            Id = userId,
            Username = username,
            Email = userEmail,
            Role = userRole
        }
    });
})
.RequireAuthorization()
.WithName("Protected")
.WithOpenApi(operation =>
{
    operation.Summary = "Endpoint protetto ad accesso limitato";
    operation.Description = "Restituisce informazioni sull'utente autenticato";
    return operation;
});

// Endpoint per recuperare le informazioni dell'utente autenticato
app.MapGet("/user-info", (HttpContext context) =>
{
    // Recupera i dettagli dell'utente dalle claims
    var isAuthenticated = context.User?.Identity?.IsAuthenticated;
    var username = context.User?.Identity?.Name;
    var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    var email = context.User?.FindFirstValue(ClaimTypes.Email);

    // Recupera tutti i ruoli dell'utente
    var roles = context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ?? [];

    // Determina il tipo di utente in base ai ruoli
    bool isAdmin = roles.Contains("Administrator");
    bool isViewer = roles.Contains("Viewer");
    bool isSuperAdmin = roles.Contains("SuperAdministrator");

    return Results.Ok(new
    {
        isAuthenticated,
        userId,
        username,
        email,
        roles,
        accountType = isAdmin ? "Administrator" : isViewer ? "Standard User" : "Unknown",
        permissions = new
        {
            canCreate = isAdmin || isSuperAdmin,
            canRead = true, // Tutti gli utenti autenticati possono leggere
            canUpdate = isAdmin || isSuperAdmin,
            canDelete = isAdmin || isSuperAdmin,
            canManageUsers = isSuperAdmin
        }
    });
})
.RequireAuthorization() // Richiede l'autenticazione
.WithName("UserInfo")
.WithOpenApi(operation =>
{
    operation.Summary = "Recupera informazioni sull'utente corrente";
    operation.Description = "Restituisce i dettagli dell'utente autenticato inclusi ruoli e permessi";
    return operation;
});

// Endpoint protetto, accessibile solo agli amministratori
app.MapGet("/admin", (HttpContext context) =>
{
    // Recupera il nome dell'utente dalle claims del token
    var username = context.User?.Identity?.Name;

    // Ottieni tutti i ruoli dell'utente dalle claims
    // Garantisce che userRoles non sia mai null - sarà un array vuoto nel caso peggiore
    var userRoles = context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ?? [];

    // Ottieni altri dettagli utente
    var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    return Results.Ok(new
    {
        Message = $"Benvenuto {username}! Hai accesso all'area amministrativa.",
        AdminInfo = new
        {
            UserId = userId,
            AccessLevel = "Full",
            Roles = userRoles,
            IsAdmin = userRoles.Contains("Administrator"),
            IsSuperAdmin = userRoles.Contains("SuperAdministrator"),
            AllowedOperations = GetAllowedOperationsForRoles(userRoles)
        }
    });
})
.RequireAuthorization("RequireAdministratorRole") // Richiede la policy specifica
.WithName("AdminArea")
.WithOpenApi(operation =>
{
    operation.Summary = "Area amministrativa riservata";
    operation.Description = "Questo endpoint è accessibile solo agli utenti con ruolo Administrator";
    return operation;
});

// Endpoint per il rinnovo del token - NON richiede l'autenticazione
app.MapPost("/refresh", (RefreshRequest request) =>
{
    // Validazione input
    if (request is null || string.IsNullOrEmpty(request.RefreshToken))
    {
        return Results.BadRequest("Refresh token richiesto");
    }

    // Cerca l'utente associato al refresh token - Thread safe con ConcurrentDictionary
    if (refreshTokenToUserMap.TryGetValue(request.RefreshToken, out var userId) &&
        refreshTokenStore.TryGetValue(userId, out var tokenInfo) &&
        tokenInfo.Token == request.RefreshToken &&
        tokenInfo.ExpiresAt > DateTime.UtcNow)
    {
        // Recupera i dati dell'utente in base all'ID
        List<Claim> claims;

        if (userId == "user123")
        {
            claims = [
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, "user@example.com"),
                new Claim(ClaimTypes.Name, "user"),
                new Claim(ClaimTypes.Role, "Viewer")
            ];
        }
        else if (userId == "admin456")
        {
            claims = [
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "Administrator"),
                new Claim(ClaimTypes.Role, "SuperAdministrator")
            ];
        }
        else
        {
            claims = [
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "unknown")
            ];
        }

        var newAccessToken = GenerateAccessToken(builder.Configuration, claims);
        var newRefreshToken = GenerateRefreshToken();

        // Thread safe operations con ConcurrentDictionary
        refreshTokenToUserMap.TryRemove(request.RefreshToken, out _);

        // Aggiorna il refresh token memorizzato con nuova scadenza
        refreshTokenStore[userId] = new RefreshTokenInfo
        {
            Token = newRefreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        // Aggiungi il nuovo mapping - Thread safe
        refreshTokenToUserMap[newRefreshToken] = userId;

        return Results.Ok(new
        {
            accessToken = newAccessToken,
            refreshToken = newRefreshToken
        });
    }

    return Results.Unauthorized();
})
.WithName("RefreshToken")
.WithOpenApi(operation =>
{
    operation.Summary = "Rinnova l'access token utilizzando un refresh token valido";
    return operation;
});

app.Run();
```

**Validazione JWT (Middleware di autenticazione JWT Bearer in ASP.NET Core):**

La validazione dei JWT in ASP.NET Core è gestita automaticamente dal middleware di autenticazione JWT Bearer, configurato nel metodo `AddJwtBearer` in `Program.cs` come mostrato nell'esempio precedente.  Il middleware intercetta le richieste HTTP in ingresso, estrae il JWT dall'header di autorizzazione (Bearer token), lo valida utilizzando le `TokenValidationParameters` configurate e, se valido, autentica l'utente.

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

## Un esempio completo di autenticazione basata su token JWT con Minimal API

Nell'esempio [Token Based Login](../../../../asp.net/api-samples/minimal-api/AuthenticationAuthorizationDemos/BasicExamples/token-based-login-with-roles/BasicTokenDemo/) viene mostrato un progetto di Minimal API .NET che implementa uno schema di autenticazione e autorizzazione basato token JWT.

**Esercitazione:** Eseguire l'esempio di codice Minimal API.  Utilizzare un client HTTP (es. Postman, Insomnia) per inviare una richiesta POST a `/login`.  La risposta conterrà un JWT.  Utilizzare lo stesso client HTTP per inviare una richiesta GET a `/protected`, includendo il JWT nell'header di autorizzazione (Bearer token). Verificare che la richiesta a `/protected` abbia successo solo se si include un JWT valido nell'header di autorizzazione.  Modificare il JWT (es. alterando la firma) o utilizzare un JWT scaduto e verificare che la richiesta a `/protected` venga rifiutata con errore di autenticazione.  Analizzare la struttura del JWT decodificandolo (es. utilizzando siti web come [https://jwt.io](https://jwt.io)).

## Riepilogo

Questa lezione ha esplorato in dettaglio i pericoli di sicurezza associati ai cookie, sottolineando l'importanza di adottare misure di mitigazione come `HttpOnly`, `Secure`, token anti-CSRF e HTTPS.  È stato introdotto il concetto di token, in particolare JWT, come alternativa più adatta ai cookie per le app mobile e le API. Sono stati discussi i vantaggi dei JWT, la loro struttura interna (Header, Payload, Signature), il flusso di autenticazione con access token e refresh token, e sono stati forniti esempi pratici di generazione e validazione JWT con ASP.NET Minimal API.

La prossima lezione proseguirà l'approfondimento dei token JWT, analizzando in dettaglio la gestione stateless del backend, i refresh token opachi, il carico di lavoro rispetto ai cookie, la gestione dei ruoli utente e la gestione del logout con i JWT e il Security Stamp di ASP.NET Core Identity.

## Risorse

[JWT Introduction](https://jwt.io/introduction)

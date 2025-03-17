# Documentazione dell'Esempio di Autenticazione con Token JWT in ASP.NET Core

```csharp
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Collections.Concurrent; // Aggiunto per ConcurrentDictionary
```

* **`using System.Text;`**: Importa lo spazio dei nomi `System.Text`, che fornisce classi per la codifica dei caratteri, come `Encoding.UTF8` utilizzato per codificare la chiave JWT.
* **`using Microsoft.AspNetCore.Authentication.JwtBearer;`**: Importa lo spazio dei nomi `Microsoft.AspNetCore.Authentication.JwtBearer`, che contiene le classi necessarie per l'autenticazione tramite JWT Bearer, come `JwtBearerDefaults` e `JwtBearerOptions`.
* **`using Microsoft.IdentityModel.Tokens;`**: Importa lo spazio dei nomi `Microsoft.IdentityModel.Tokens`, che fornisce classi per la gestione dei token di sicurezza, come `TokenValidationParameters` e `SymmetricSecurityKey`.
* **`using System.IdentityModel.Tokens.Jwt;`**: Importa lo spazio dei nomi `System.IdentityModel.Tokens.Jwt`, che contiene classi per la creazione e la gestione dei token JWT, come `JwtSecurityToken` e `JwtSecurityTokenHandler`.
* **`using System.Security.Claims;`**: Importa lo spazio dei nomi `System.Security.Claims`, che fornisce classi per rappresentare le richieste di identità (claims) associate a un utente autenticato.
* **`using System.Collections.Concurrent; // Aggiunto per ConcurrentDictionary`**: Importa lo spazio dei nomi `System.Collections.Concurrent`, che fornisce classi di raccolta thread-safe, come `ConcurrentDictionary`, utilizzata per la gestione dei refresh token in modo sicuro in un ambiente concorrente.

```csharp
// Dictionary thread-safe per la gestione dei token
ConcurrentDictionary<string, RefreshTokenInfo> refreshTokenStore = new();
ConcurrentDictionary<string, string> refreshTokenToUserMap = new();
```

* **`ConcurrentDictionary<string, RefreshTokenInfo> refreshTokenStore = new();`**: Dichiara e inizializza un dizionario thread-safe chiamato `refreshTokenStore`.
    * La chiave del dizionario è una stringa che rappresenta l'ID dell'utente.
    * Il valore è un oggetto di tipo `RefreshTokenInfo` (definito più avanti nel codice) che contiene le informazioni sul refresh token associato a quell'utente.
    * L'uso di `ConcurrentDictionary` garantisce che più thread possano accedere e modificare questo dizionario contemporaneamente senza causare race condition.
* **`ConcurrentDictionary<string, string> refreshTokenToUserMap = new();`**: Dichiara e inizializza un altro dizionario thread-safe chiamato `refreshTokenToUserMap`.
    * La chiave del dizionario è una stringa che rappresenta il refresh token stesso.
    * Il valore è una stringa che rappresenta l'ID dell'utente a cui appartiene quel refresh token.
    * Questo dizionario serve come una "mappa inversa" per trovare l'ID utente dato un refresh token.

```csharp
var builder = WebApplication.CreateBuilder(args);
```

* **`var builder = WebApplication.CreateBuilder(args);`**: Crea un'istanza di `WebApplicationBuilder`. Questo oggetto viene utilizzato per configurare i servizi e la pipeline dell'applicazione web. `args` sono gli argomenti della riga di comando passati all'applicazione.

```csharp
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
//aggiunge il servizio che permette ad OpenAPI di leggere i metadati delle API
builder.Services.AddEndpointsApiExplorer();
//configura il servizio OpenAPI
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "Basic Token v1";
    config.DocumentName = "Basic Token API";
    config.Version = "v1";

    // Configurazione dell'autenticazione JWT per Swagger/OpenAPI
    config.AddSecurity("JWT",, new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Inserisci SOLO il token JWT (senza il prefisso 'Bearer').\nIl prefisso 'Bearer ' viene aggiunto automaticamente."
    });

    // Applica il requisito di sicurezza a tutti gli endpoint
    config.OperationProcessors.Add(new NSwag.Generation.Processors.Security.OperationSecurityScopeProcessor("JWT"));
});
```

* **`builder.Services.AddOpenApi();`**: Aggiunge i servizi necessari per il supporto di OpenAPI (specifica per la documentazione delle API).
* **`builder.Services.AddEndpointsApiExplorer();`**: Aggiunge il servizio che permette ad OpenAPI di scoprire gli endpoint definiti nell'applicazione.
* **`builder.Services.AddOpenApiDocument(config => { ... });`**: Configura il documento OpenAPI (la specifica dell'API).
    * **`config.Title = "Basic Token v1";`**: Imposta il titolo del documento API.
    * **`config.DocumentName = "Basic Token API";`**: Imposta il nome del documento API.
    * **`config.Version = "v1";`**: Imposta la versione dell'API.
    * **`config.AddSecurity("JWT",, new NSwag.OpenApiSecurityScheme { ... });`**: Configura uno schema di sicurezza chiamato "JWT" per l'autenticazione tramite Bearer token (JWT).
        * **`Type = NSwag.OpenApiSecuritySchemeType.Http,`**: Specifica che il tipo di sicurezza è HTTP.
        * **`Scheme = "Bearer",`**: Specifica lo schema di autenticazione come "Bearer".
        * **`BearerFormat = "JWT",`**: Indica che il formato del Bearer token è JWT.
        * **`Description = "Inserisci SOLO il token JWT (senza il prefisso 'Bearer').\nIl prefisso 'Bearer ' viene aggiunto automaticamente.";`**: Fornisce una descrizione per l'inserimento del token nell'interfaccia di Swagger.
    * **`config.OperationProcessors.Add(new NSwag.Generation.Processors.Security.OperationSecurityScopeProcessor("JWT"));`**: Aggiunge un processore di operazioni che applica il requisito di sicurezza "JWT" a tutti gli endpoint dell'API, richiedendo un token JWT valido per accedervi (a meno che non siano specificamente contrassegnati come anonimi).

```csharp
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

    // Gestione degli eventi di autenticazione
    options.Events = new JwtBearerEvents
    {
        // Gestione del fallimento dell'autenticazione (401 Unauthorized)
        OnChallenge = async context =>
        {
            // Previene che il middleware gestisca automaticamente la risposta
            context.HandleResponse();

            // Se la richiesta accetta HTML (browser)
            if (IsHtmlRequest(context.Request))
            {
                // Reindirizza al login
                context.Response.Redirect("/login-page");
                return;
            }

            // Altrimenti invia una risposta 401 con dettagli in JSON
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var response = new
            {
                status = 401,
                message = "Non sei autenticato. Effettua il login per accedere a questa risorsa.",
                timestamp = DateTime.UtcNow,
                path = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(response);
        },

        // Gestione di altre eccezioni di autenticazione come token scaduti o firme non valide
        OnAuthenticationFailed = context =>
        {
            // Log dell'errore di autenticazione
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Errore autenticazione JWT: {Error} - Path: {Path}",
                context.Exception.Message,
                context.Request.Path);

            // Se il token è scaduto, aggiungi un header specifico che il client può utilizzare
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Append("Token-Expired", "true");
                // Non gestire la risposta qui, lasceremo che OnChallenge lo faccia
            }

            return Task.CompletedTask;
        }
    };
});
```

* **`builder.Services.AddAuthentication(options => { ... })`**: Configura il servizio di autenticazione.
    * **`options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;`**: Imposta lo schema di autenticazione predefinito per le richieste autenticate su `JwtBearerDefaults.AuthenticationScheme` (che è "Bearer").
    * **`options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;`**: Imposta lo schema di autenticazione predefinito da utilizzare quando è necessario "sfidare" un utente non autenticato (ad esempio, restituendo un errore 401).
* **.AddJwtBearer(options => { ... })**: Aggiunge il supporto per l'autenticazione tramite JWT Bearer.
    * **`options.TokenValidationParameters = new TokenValidationParameters { ... };`**: Configura i parametri di validazione del token.
        * **`ValidateIssuer = true,`**: Indica se validare l'emittente del token.
        * **`ValidateAudience = true,`**: Indica se validare il destinatario del token.
        * **`ValidateLifetime = true,`**: Indica se validare la scadenza del token.
        * **`ValidateIssuerSigningKey = true,`**: Indica se validare la firma del token utilizzando la chiave di firma.
        * **`ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer non configurata"),`**: Imposta l'emittente valido atteso, recuperandolo dalla configurazione (`Jwt:Issuer`). Se non configurato, lancia un'eccezione.
        * **`ValidAudience = builder.Configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience non configurata"),`**: Imposta il destinatario valido atteso, recuperandolo dalla configurazione (`Jwt:Audience`). Se non configurato, lancia un'eccezione.
        * **`IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key non configurata")))`**: Imposta la chiave di firma utilizzata per validare il token. Recupera la chiave dalla configurazione (`Jwt:Key`), la codifica in byte UTF-8 e crea un'istanza di `SymmetricSecurityKey`. Se la chiave non è configurata, lancia un'eccezione.
    * **`options.Events = new JwtBearerEvents { ... };`**: Configura i gestori di eventi per il processo di autenticazione JWT.
        * **`OnChallenge = async context => { ... };`**: Questo evento viene chiamato quando l'autenticazione fallisce (ad esempio, a causa di un token mancante o non valido).
            * **`context.HandleResponse();`**: Impedisce al middleware di gestire automaticamente la risposta di challenge (solitamente un reindirizzamento al login).
            * **`if (IsHtmlRequest(context.Request)) { ... }`**: Verifica se la richiesta sembra provenire da un browser web (controllando l'header `Accept` o `User-Agent` tramite la funzione `IsHtmlRequest` definita più avanti).
                * **`context.Response.Redirect("/login-page");`**: Se è una richiesta da browser, reindirizza l'utente alla pagina di login (`/login-page`).
                * **`return;`**: Interrompe l'esecuzione dell'handler.
            * **`context.Response.StatusCode = StatusCodes.Status401Unauthorized;`**: Imposta il codice di stato della risposta su 401 Unauthorized.
            * **`context.Response.ContentType = "application/json";`**: Imposta il tipo di contenuto della risposta su JSON.
            * Crea un oggetto anonimo con dettagli sull'errore di autenticazione (status code, messaggio, timestamp, percorso della richiesta) e lo scrive come JSON nella risposta.
        * **`OnAuthenticationFailed = context => { ... };`**: Questo evento viene chiamato quando si verifica un'eccezione durante il processo di autenticazione (ad esempio, token scaduto o firma non valida).
            * Recupera un logger dal container di servizi e registra un avviso con i dettagli dell'errore e il percorso della richiesta.
            * **`if (context.Exception is SecurityTokenExpiredException) { ... }`**: Verifica se l'eccezione è di tipo `SecurityTokenExpiredException` (indica che il token è scaduto).
                * **`context.Response.Headers.Append("Token-Expired", "true");`**: Aggiunge un header HTTP chiamato `Token-Expired` con il valore "true" alla risposta. Questo può essere utilizzato dal client per rilevare che il token è scaduto e intraprendere azioni appropriate (ad esempio, richiedere un nuovo token tramite il refresh token).
                * Il commento indica che la gestione della risposta in caso di token scaduto è lasciata all'handler `OnChallenge`.
            * **`return Task.CompletedTask;`**: Indica che l'handler dell'evento è completato.

```csharp
// Add authorization services
builder.Services.AddAuthorization(options =>
{
    // Definizione di una policy per gli amministratori
    options.AddPolicy("RequireAdministratorRole", policy =>
        policy.RequireRole("Administrator"));

});
```

* **`builder.Services.AddAuthorization(options => { ... });`**: Configura il servizio di autorizzazione.
    * **`options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Administrator"));`**: Definisce una policy di autorizzazione chiamata "RequireAdministratorRole".
        * **`policy.RequireRole("Administrator")`**: Specifica che per soddisfare questa policy, l'utente autenticato deve avere un claim di ruolo con il valore "Administrator".

```csharp
var app = builder.Build();
```

* **`var app = builder.Build();`**: Costruisce l'applicazione web utilizzando le configurazioni definite nel `WebApplicationBuilder`. L'oggetto `app` rappresenta l'applicazione web vera e propria.

```csharp
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

    app.MapOpenApi();
    //permette a Swagger (NSwag) di generare un file JSON con le specifiche delle API
    app.UseOpenApi();
    //permette di configurare l'interfaccia SwaggerUI (l'interfaccia grafica web di Swagger (NSwag) che permette di interagire con le API)
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "Basic Token v1";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";

    });

    app.UseDeveloperExceptionPage();
}
```

* **`if (app.Environment.IsDevelopment()) { ... }`**: Esegue il codice all'interno solo se l'applicazione è in ambiente di sviluppo.
    * **`app.MapOpenApi();`**: Registra gli endpoint necessari per servire la specifica OpenAPI.
    * **`app.UseOpenApi();`**: Abilita il middleware per servire la specifica OpenAPI.
    * **`app.UseSwaggerUi(config => { ... });`**: Abilita e configura l'interfaccia utente di Swagger (Swagger UI) per interagire con l'API.
        * **`config.DocumentTitle = "Basic Token v1";`**: Imposta il titolo della pagina di Swagger UI.
        * **`config.Path = "/swagger";`**: Imposta il percorso in cui sarà disponibile l'interfaccia Swagger UI (ad esempio, `https://localhost:xxxx/swagger`).
        * **`config.DocumentPath = "/swagger/{documentName}/swagger.json";`**: Imposta il percorso del file JSON contenente la specifica OpenAPI.
        * **`config.DocExpansion = "list";`**: Configura l'espansione iniziale delle sezioni nella documentazione API.
    * **`app.UseDeveloperExceptionPage();`**: Abilita la pagina di eccezione per sviluppatori, che mostra dettagliate informazioni sugli errori durante lo sviluppo.

```csharp
// Middleware
app.UseAuthentication();
app.UseAuthorization();
```

* **`app.UseAuthentication();`**: Aggiunge il middleware di autenticazione alla pipeline di richiesta. Questo middleware esamina le richieste in entrata e tenta di autenticare l'utente in base alle configurazioni di autenticazione registrate (in questo caso, JWT Bearer).
* **`app.UseAuthorization();`**: Aggiunge il middleware di autorizzazione alla pipeline di richiesta. Questo middleware viene eseguito dopo l'autenticazione e verifica se l'utente autenticato ha i permessi necessari per accedere alla risorsa richiesta (in base alle policy di autorizzazione configurate).

```csharp
// Gestione personalizzata degli errori di autorizzazione (403 Forbidden)
app.Use(async (context, next) =>
{
    await next();

    // Intercetta solo le risposte 403 Forbidden
    if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
    {
        // Se la richiesta accetta HTML (browser)
        if (IsHtmlRequest(context.Request))
        {
            // Reindirizza alla pagina di accesso negato
            context.Response.Redirect("/access-denied");
            return;
        }

        // Altrimenti invia una risposta 403 con dettagli in JSON
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = 403,
            message = "Non hai i permessi necessari per accedere a questa risorsa.",
            timestamp = DateTime.UtcNow,
            path = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(response);
    }
});
```

* **`app.Use(async (context, next) => { ... });`**: Aggiunge un middleware personalizzato alla pipeline. Questo middleware intercetta le risposte HTTP dopo che sono state elaborate dal middleware successivo.
    * **`await next();`**: Chiama il middleware successivo nella pipeline.
    * **`if (context.Response.StatusCode == StatusCodes.Status403Forbidden)`**: Verifica se il codice di stato della risposta è 403 Forbidden (indicando un errore di autorizzazione).
    * **`if (IsHtmlRequest(context.Request)) { ... }`**: Similmente all'handler `OnChallenge`, verifica se la richiesta sembra provenire da un browser.
        * **`context.Response.Redirect("/access-denied");`**: Se è una richiesta da browser, reindirizza l'utente alla pagina di accesso negato (`/access-denied`).
        * **`return;`**: Interrompe l'esecuzione del middleware.
    * **`context.Response.ContentType = "application/json";`**: Se la richiesta non è da browser, imposta il tipo di contenuto della risposta su JSON.
    * Crea un oggetto anonimo con dettagli sull'errore di autorizzazione (status code, messaggio, timestamp, percorso della richiesta) e lo scrive come JSON nella risposta.

```csharp
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
```

* **`string GenerateAccessToken(IConfiguration config, IEnumerable<Claim> claims)`**: Questo metodo genera un nuovo access token JWT.
    * **`var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key non configurata")));`**: Recupera la chiave segreta dalla configurazione, la codifica in byte UTF-8 e crea una chiave simmetrica.
    * **`var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);`**: Crea le credenziali di firma utilizzando la chiave simmetrica e l'algoritmo di firma HMAC SHA256.
    * **`var token = new JwtSecurityToken(...)`**: Crea un nuovo token JWT.
        * **`issuer: config["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer non configurata"),`**: Imposta l'emittente del token (recuperato dalla configurazione).
        * **`audience: config["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience non configurata"),`**: Imposta il destinatario del token (recuperato dalla configurazione).
        * **`claims: claims,`**: Imposta le richieste (informazioni sull'utente) da includere nel token.
        * **`expires: DateTime.UtcNow.AddMinutes(30),`**: Imposta la data di scadenza del token a 30 minuti nel futuro (utilizzando l'ora UTC).
        * **`signingCredentials: credentials`**: Imposta le credenziali di firma per il token.
    * **`return new JwtSecurityTokenHandler().WriteToken(token);`**: Utilizza un `JwtSecurityTokenHandler` per serializzare il token JWT in una stringa formattata.
* **`string GenerateRefreshToken()`**: Questo metodo genera un nuovo refresh token.
    * Il codice commentato mostra un approccio più sicuro per generare un refresh token utilizzando un generatore di numeri casuali crittograficamente sicuro e codificandolo in Base64.
    * L'implementazione attuale genera un refresh token come una stringa GUID (Globally Unique Identifier).

```csharp
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
        var claims = new{
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
        var claims = new{
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
```

* **`app.MapPost("/login", (UserLogin login) => { ... })`**: Definisce un endpoint HTTP POST su `/login`.
    * Accetta un oggetto `UserLogin` dal corpo della richiesta (presumibilmente in formato JSON).
    * **Validazione input**: Verifica se l'oggetto `login` e le sue proprietà `Username` e `Password` non siano nulli o vuoti. Se lo sono, restituisce una risposta 400 BadRequest.
    * **Verifica utente "user"**: Se l'username è "user" e la password è "pass" (credenziali hardcoded per un utente standard):
        * Definisce un `userId` e un `userEmail`.
        * Crea un array di `Claim` (richieste) per l'utente, includendo l'ID, l'email, il nome utente e il ruolo "Viewer".
        * Chiama `GenerateAccessToken` per ottenere un nuovo access token JWT.
        * Chiama `GenerateRefreshToken` per ottenere un nuovo refresh token.
        * Crea un nuovo oggetto `RefreshTokenInfo` con il refresh token, la data di creazione e una data di scadenza di 7 giorni nel futuro.
        * Aggiunge (o aggiorna) l'oggetto `RefreshTokenInfo` nel `refreshTokenStore` utilizzando l'`userId` come chiave.
        * Aggiunge una voce nel `refreshTokenToUserMap` mappando il `refreshToken` all'`userId`.
        * Restituisce una risposta 200 OK con un oggetto anonimo contenente l'`accessToken` e il `refreshToken`.
    * **Verifica utente "admin"**: Se l'username è "admin" e la password è "Admin123!" (credenziali hardcoded per un amministratore):
        * Definisce un `userId` e un `userEmail`.
        * Crea un array di `Claim` per l'amministratore, includendo l'ID, l'email, il nome utente e i ruoli "Administrator" e "SuperAdministrator".
        * Genera un access token e un refresh token nello stesso modo dell'utente "user".
        * Memorizza il refresh token nel `refreshTokenStore` e la sua associazione all'utente nel `refreshTokenToUserMap`.
        * Restituisce una risposta 200 OK con l'`accessToken` e il `refreshToken`.
    * Se le credenziali non corrispondono a nessuno degli utenti hardcoded, restituisce una risposta 401 Unauthorized.
    * **.WithName("Login")**: Assegna un nome all'endpoint, utile per la generazione di link e la documentazione.
    * **.WithOpenApi(...)**: Configura i metadati OpenAPI per questo endpoint (summary e description).

```csharp
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
```

* **`app.MapGet("/protected", (HttpContext context) => { ... })`**: Definisce un endpoint HTTP GET su `/protected`.
    * Accetta l'oggetto `HttpContext` che contiene informazioni sulla richiesta corrente, inclusa l'identità dell'utente autenticato.
    * **`var username = context.User?.Identity?.Name;`**: Recupera il nome dell'utente autenticato dalla proprietà `Identity.Name` dell'oggetto `User` nel contesto HTTP.
    * **`var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);`**: Recupera il valore della claim con il tipo `ClaimTypes.NameIdentifier` (solitamente l'ID univoco dell'utente).
    * **`var userEmail = context.User?.FindFirstValue(ClaimTypes.Email);`**: Recupera il valore della claim con il tipo `ClaimTypes.Email`.
    * **`var userRole = context.User?.FindFirstValue(ClaimTypes.Role);`**: Recupera il valore della claim con il tipo `ClaimTypes.Role`.
    * Restituisce una risposta 200 OK con un oggetto anonimo contenente un messaggio di benvenuto e un oggetto `UserDetails` con l'ID, l'username, l'email e il ruolo dell'utente.
    * **.RequireAuthorization()**: Indica che questo endpoint richiede che l'utente sia autenticato per poterlo accedere. Il middleware di autenticazione (aggiunto tramite `app.UseAuthentication()`) avrà precedentemente validato il token JWT presente nell'header della richiesta.
    * **.WithName("Protected")**: Assegna un nome all'endpoint.
    * **.WithOpenApi(...)**: Configura i metadati OpenAPI per questo endpoint.

```csharp
// Endpoint per recuperare le informazioni dell'utente autenticato
app.MapGet("/user-info", (HttpContext context) =>
{
    // Recupera i dettagli dell'utente dalle claims
    var username = context.User?.Identity?.Name;
    var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    var email = context.User?.FindFirstValue(ClaimTypes.Email);

    // Recupera tutti i ruoli dell'utente
    var roles = context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ??;

    // Determina il tipo di utente in base ai ruoli
    bool isAdmin = roles.Contains("Administrator");
    bool isViewer = roles.Contains("Viewer");
    bool isSuperAdmin = roles.Contains("SuperAdministrator");

    return Results.Ok(new
    {
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
```

* **`app.MapGet("/user-info", (HttpContext context) => { ... })`**: Definisce un endpoint HTTP GET su `/user-info`.
    * Accetta l'oggetto `HttpContext`.
    * Recupera l'username, l'userId e l'email dalle claims dell'utente autenticato in modo simile all'endpoint `/protected`.
    * **`var roles = context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ??;`**: Recupera tutte le claims di tipo `ClaimTypes.Role`, seleziona il loro valore (il nome del ruolo) e le converte in un array di stringhe. L'operatore `??` garantisce che se `context.User` o il risultato di `FindAll` è null, `roles` sarà un array vuoto.
    * Determina il tipo di account in base ai ruoli: se l'utente ha il ruolo "Administrator", è un amministratore; se ha il ruolo "Viewer", è un utente standard; altrimenti, è sconosciuto.
    * Definisce un oggetto anonimo contenente:
        * `userId`, `username`, `email`, `roles`: Informazioni di base sull'utente.
        * `accountType`: Il tipo di account determinato dai ruoli.
        * `permissions`: Un oggetto anonimo che definisce i permessi dell'utente in base ai suoi ruoli (ad esempio, se può creare, leggere, aggiornare, eliminare o gestire utenti).
    * **`.RequireAuthorization()`**: Richiede che l'utente sia autenticato.
    * **`.WithName("UserInfo")`**: Assegna un nome all'endpoint.
    * **`.WithOpenApi(...)`**: Configura i metadati OpenAPI.

```csharp
// Endpoint protetto, accessibile solo agli amministratori
app.MapGet("/admin", (HttpContext context) =>
{
    // Recupera il nome dell'utente dalle claims del token
    var username = context.User?.Identity?.Name;

    // Ottieni tutti i ruoli dell'utente dalle claims
    // Garantisce che userRoles non sia mai null - sarà un array vuoto nel caso peggiore
    var userRoles = context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ??;

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
```

* **`app.MapGet("/admin", (HttpContext context) => { ... })`**: Definisce un endpoint HTTP GET su `/admin`.
    * Accetta l'oggetto `HttpContext`.
    * Recupera l'username e l'userId dalle claims dell'utente autenticato.
    * **`var userRoles = context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ??;`**: Recupera tutti i ruoli dell'utente come un array di stringhe.
    * Restituisce una risposta 200 OK con un messaggio di benvenuto e un oggetto `AdminInfo` contenente:
        * `UserId`: L'ID dell'utente.
        * `AccessLevel`: Un valore hardcoded "Full".
        * `Roles`: L'array dei ruoli dell'utente.
        * `IsAdmin`: Un booleano che indica se l'utente ha il ruolo "Administrator".
        * `IsSuperAdmin`: Un booleano che indica se l'utente ha il ruolo "SuperAdministrator".
        * `AllowedOperations`: Un array di stringhe contenente le operazioni permesse per l'utente, ottenuto chiamando la funzione `GetAllowedOperationsForRoles` (definita più avanti).
    * **`.RequireAuthorization("RequireAdministratorRole")`**: Richiede che l'utente sia autenticato e che soddisfi la policy di autorizzazione "RequireAdministratorRole", che a sua volta richiede il ruolo "Administrator".
    * **`.WithName("AdminArea")`**: Assegna un nome all'endpoint.
    * **`.WithOpenApi(...)`**: Configura i metadati OpenAPI.

```csharp
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
```

* **`app.MapPost("/refresh", (RefreshRequest request) => { ... })`**: Definisce un endpoint HTTP POST su `/refresh`.
    * Accetta un oggetto `RefreshRequest` dal corpo della richiesta, contenente il `RefreshToken`.
    * **Validazione input**: Verifica se l'oggetto `request` e la sua proprietà `RefreshToken` non siano nulli o vuoti. Se lo sono, restituisce una risposta 400 BadRequest.
    * **Ricerca e validazione del refresh token**:
        * **`refreshTokenToUserMap.TryGetValue(request.RefreshToken, out var userId)`**: Tenta di ottenere l'ID utente associato al refresh token fornito dal dizionario `refreshTokenToUserMap`.
        * **`refreshTokenStore.TryGetValue(userId, out var tokenInfo)`**: Se l'ID utente viene trovato, tenta di ottenere le informazioni del refresh token (`RefreshTokenInfo`) dal `refreshTokenStore`.
        * **`tokenInfo.Token == request.RefreshToken`**: Verifica che il refresh token memorizzato corrisponda a quello fornito nella richiesta.
        * **`tokenInfo.ExpiresAt > DateTime.UtcNow`**: Verifica che il refresh token non sia scaduto.
        * Se tutte queste condizioni sono vere, il refresh token è considerato valido.
    * **Recupero delle claims dell'utente**: In base all'`userId` trovato, recupera le claims appropriate per l'utente "user" o "admin" (utilizzando la stessa logica dell'endpoint `/login`). Se l'utente non è "user123" o "admin456", crea un set di claims di base con solo l'ID utente e un nome utente "unknown".
    * **Generazione di nuovi token**:
        * Chiama `GenerateAccessToken` per generare un nuovo access token JWT con le claims dell'utente.
        * Chiama `GenerateRefreshToken` per generare un nuovo refresh token.
    * **Aggiornamento dei refresh token**:
        * **`refreshTokenToUserMap.TryRemove(request.RefreshToken, out _);`**: Rimuove la vecchia associazione tra il vecchio refresh token e l'ID utente dal `refreshTokenToUserMap`.
        * Crea un nuovo oggetto `RefreshTokenInfo` con il nuovo refresh token e una nuova data di scadenza.
        * Aggiorna (o aggiunge) l'oggetto `RefreshTokenInfo` nel `refreshTokenStore` per l'`userId`.
        * **`refreshTokenToUserMap[newRefreshToken] = userId;`**: Aggiunge una nuova associazione tra il nuovo refresh token e l'ID utente nel `refreshTokenToUserMap`.
    * Restituisce una risposta 200 OK con il nuovo `accessToken` e il nuovo `refreshToken`.
    * Se il refresh token fornito non è valido o è scaduto, restituisce una risposta 401 Unauthorized.
    * **`.WithName("RefreshToken")`**: Assegna un nome all'endpoint.
    * **`.WithOpenApi(...)`**: Configura i metadati OpenAPI.

```csharp
// Implementazione semplificata dell'endpoint logout che gestisce solo i refresh token
app.MapPost("/logout", (HttpContext context, LogoutRequest? request) =>
{
    var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    if (string.IsNullOrEmpty(userId))
    {
        return Results.BadRequest("Utente non identificato");
    }

    // Caso 1: Se viene fornito un refresh token specifico
    if (request != null && !string.IsNullOrEmpty(request.RefreshToken))
    {
        // Verifica che il refresh token appartenga all'utente corrente
        if (refreshTokenToUserMap.TryGetValue(request.RefreshToken, out var tokenUserId) &&
            tokenUserId == userId)
        {
            // Rimuove solo il refresh token specifico
            refreshTokenToUserMap.TryRemove(request.RefreshToken, out _);

            // Se è l'unico refresh token dell'utente, rimuovi anche l'entry dallo store
            if (refreshTokenStore.TryGetValue(userId, out var storedToken) &&
                storedToken.Token == request.RefreshToken)
            {
                refreshTokenStore.TryRemove(userId, out _);
            }

            return Results.Ok(new
            {
                message = "Logout effettuato con successo",
                details = "Refresh token specifico invalidato"
            });
        }

        return Results.BadRequest("Il refresh token fornito non è valido o non appartiene all'utente corrente");
    }

    // Caso 2: Se non viene fornito un refresh token, invalida tutti i refresh token dell'utente
    if (refreshTokenStore.TryGetValue(userId, out var userRefreshTokenInfo))
    {
        var userRefreshToken = userRefreshTokenInfo.Token;

        // Rimuovi dalla mappa inversa
        refreshTokenToUserMap.TryRemove(userRefreshToken, out _);

        // Rimuovi dallo store
        refreshTokenStore.TryRemove(userId, out _);

        return Results.Ok(new
        {
            message = "Logout effettuato con successo",
            details = "Tutti i refresh token dell'utente sono stati invalidati"
        });
    }
    return Results.Ok(new
    {
        message = "Nessun token da invalidare",
        details = "L'utente non aveva sessioni attive"
    });
})
.RequireAuthorization()
.WithName("Logout")
.WithOpenApi(operation =>
{
    operation.Summary = "Effettua il logout invalidando i refresh token";
    operation.Description = "Invalida il refresh token dell'utente, forzando un nuovo login per ottenere nuovi token";
    return operation;
});
```

* **`app.MapPost("/logout", (HttpContext context, LogoutRequest? request) => { ... })`**: Definisce un endpoint HTTP POST su `/logout`.
    * Accetta l'oggetto `HttpContext` e un oggetto opzionale `LogoutRequest` dal corpo della richiesta (potrebbe contenere un `RefreshToken` specifico da invalidare).
    * Recupera l'`userId` dall'utente autenticato. Se non viene trovato, restituisce un errore 400 BadRequest.
    * **Caso 1: Invalidazione di un refresh token specifico**: Se l'oggetto `request` non è null e contiene un `RefreshToken` non vuoto:
        * Verifica che il refresh token fornito appartenga all'utente corrente cercando nel `refreshTokenToUserMap`.
        * Se il refresh token appartiene all'utente, lo rimuove dal `refreshTokenToUserMap`.
        * Verifica se il refresh token rimosso era l'unico refresh token memorizzato per quell'utente nel `refreshTokenStore` e, in tal caso, rimuove anche l'entry dal `refreshTokenStore`.
        * Restituisce una risposta 200 OK indicando che il logout è avvenuto con successo e che il refresh token specifico è stato invalidato.
        * Se il refresh token fornito non è valido o non appartiene all'utente corrente, restituisce una risposta 400 BadRequest.
    * **Caso 2: Invalidazione di tutti i refresh token dell'utente**: Se non viene fornito un refresh token specifico:
        * Tenta di ottenere le informazioni del refresh token dell'utente dal `refreshTokenStore`.
        * Se vengono trovate informazioni (l'utente aveva un refresh token attivo), recupera il refresh token dall'oggetto `RefreshTokenInfo`.
        * Rimuove l'associazione tra il refresh token e l'ID utente dal `refreshTokenToUserMap`.
        * Rimuove l'entry dell'utente dal `refreshTokenStore`.
        * Restituisce una risposta 200 OK indicando che il logout è avvenuto con successo e che tutti i refresh token dell'utente sono stati invalidati.
    * Se l'utente non aveva refresh token attivi nel `refreshTokenStore`, restituisce una risposta 200 OK indicando che non c'erano token da invalidare.
    * **`.RequireAuthorization()`**: Richiede che l'utente sia autenticato per poter effettuare il logout.
    * **`.WithName("Logout")`**: Assegna un nome all'endpoint.
    * **`.WithOpenApi(...)`**: Configura i metadati OpenAPI.

```csharp
// Endpoint semplice di login per redirect da browser (solo per dimostrazione)
app.MapGet("/login-page", () => Results.Content(
    @"<!DOCTYPE html>
<html lang='it'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>JWT Authentication Demo</title>
    <style>
        body { ... }
        .login-container { ... }
        .login-header { ... }
        .form-group { ... }
        .form-group label { ... }
        .form-group input { ... }
        .form-group .hint { ... }
        .btn { ... }
        .btn:hover { ... }
        .error-message { ... }
        .user-options { ... }
        .user-option { ... }
        .user-option:hover { ... }
        .token-display { ... }
        .token-display pre { ... }
    </style>
</head>
<body>
    <div class='login-container'>
        <div class='login-header'>
            <h1>Login</h1>
            <p>Accedi per ottenere il token JWT</p>
        </div>

        <div id='errorMessage' class='error-message'></div>

        <form id='loginForm' onsubmit='return false;'>
            <div class='form-group'>
                <label for='username'>Username</label>
                <input type='text' id='username' name='username' required>
                <div class='hint'>Prova con 'user' o 'admin'</div>
            </div>

            <div class='form-group'>
                <label for='password'>Password</label>
                <input type='password' id='password' name='password' required>
                <div class='hint'>Password: 'pass' per user, 'Admin123!' per admin</div>
            </div>

            <button type='button' class='btn' onclick='login()'>Login</button>
        </form>

        <div class='user-options'>
            <div class='user-option' onclick=""fillForm('user', 'pass')"">
                Utente Standard
            </div>
            <div class='user-option' onclick=""fillForm('admin', 'Admin123!')"">
                Amministratore
            </div>
        </div>

        <div id='tokenDisplay' class='token-display'>
            <h3>Token ottenuto:</h3>
            <pre id='accessToken'></pre>
            <h3>Refresh Token:</h3>
            <pre id='refreshToken'></pre>
            <button class='btn' style='margin-top:10px;' onclick='testProtectedEndpoint()'>Testa API protetta</button>
            <button class='btn' style='margin-top:10px; background-color: #27ae60;' onclick='testAdminEndpoint()'>Testa API amministrativa</button>
        </div>
    </div>

    <script>
        function fillForm(username, password) { ... }
        let currentToken = null;

        async function login() { ... }

        function showError(message) { ... }

        async function testProtectedEndpoint() { ... }

        async function testAdminEndpoint() { ... }
    </script>
</body>
</html>",
    "text/html"))
.AllowAnonymous();
```

* **`app.MapGet("/login-page", () => Results.Content(...))`**: Definisce un endpoint HTTP GET su `/login-page`.
    * Restituisce una stringa contenente codice HTML per una semplice pagina di login. Questa pagina include un form per inserire username e password, suggerimenti per le credenziali di test e script JavaScript per effettuare la chiamata di login all'API e testare gli endpoint protetti.
    * **`.AllowAnonymous()`**: Indica che questo endpoint può essere acceduto senza autenticazione.

```csharp
// Endpoint di accesso negato per redirect da browser
app.MapGet("/access-denied", () => Results.Content(
    "<html><body><h1>Accesso Negato</h1><p>Non hai i permessi necessari per accedere alla risorsa richiesta.</p>" +
    "<p><a href='/login-page'>Torna al login</a></p></body></html>",
    "text/html"))
// Assicuriamoci che questo endpoint sia accessibile senza autenticazione
.AllowAnonymous();
```

* **`app.MapGet("/access-denied", () => Results.Content(...))`**: Definisce un endpoint HTTP GET su `/access-denied`.
    * Restituisce una stringa contenente codice HTML per una semplice pagina di "Accesso Negato". Questa pagina informa l'utente che non ha i permessi per accedere alla risorsa richiesta e fornisce un link per tornare alla pagina di login.
    * **`.AllowAnonymous()`**: Indica che questo endpoint può essere acceduto senza autenticazione.

```csharp
app.Run();
```

* **`app.Run();`**: Avvia l'applicazione web, facendo sì che inizi ad ascoltare le richieste HTTP in entrata sulla porta configurata (solitamente definita in `launchSettings.json`).

```csharp
// Helper per determinare se una richiesta proviene da un browser web
bool IsHtmlRequest(HttpRequest request)
{
    // Controlla se l'header Accept include HTML
    if (request.Headers.TryGetValue("Accept", out var acceptHeader))
    {
        return acceptHeader.ToString().Contains("text/html");
    }

    // Controlla l'header User-Agent per identificare i browser più comuni
    if (request.Headers.TryGetValue("User-Agent", out var userAgent))
    {
        string ua = userAgent.ToString().ToLower();
        if (ua.Contains("mozilla") || ua.Contains("chrome") || ua.Contains("safari") ||
            ua.Contains("edge") || ua.Contains("firefox") || ua.Contains("webkit"))
        {
            return true;
        }
    }

    return false;
}
```

* **`bool IsHtmlRequest(HttpRequest request)`**: Questa funzione helper determina se una richiesta HTTP sembra provenire da un browser web.
    * Controlla se l'header `Accept` della richiesta contiene il valore "text/html".
    * Se l'header `Accept` non indica HTML, controlla l'header `User-Agent` per identificare le stringhe comuni presenti negli user agent dei browser web più diffusi (Mozilla, Chrome, Safari, Edge, Firefox, WebKit).
    * Restituisce `true` se la richiesta sembra provenire da un browser, `false` altrimenti.

```csharp
stringGetAllowedOperationsForRoles(stringroles)
{
    if (roles.Length == 0)
        return UserRoles.EmptyRoles;

    var operations = new HashSet<string>();

    foreach (var role in roles)
    {
        switch (role)
        {
            case "Administrator":
                operations.Add("Create");
                operations.Add("Read");
                operations.Add("Update");
                operations.Add("Delete");
                break;

            case "Editor":
                operations.Add("Read");
                operations.Add("Update");
                break;

            case "Viewer":
                operations.Add("Read");
                break;

            case "SuperAdministrator":
                operations.Add("Create");
                operations.Add("Read");
                operations.Add("Update");
                operations.Add("Delete");
                operations.Add("Manage Users");
                operations.Add("System Configuration");
                break;
        }
    }

    return [.. operations];
}
```

* **`stringGetAllowedOperationsForRoles(stringroles)`**: Questa funzione helper determina le operazioni permesse per un utente in base ai suoi ruoli.
    * Se l'array di `roles` è vuoto, restituisce l'array `EmptyRoles` definito nella classe `UserRoles`.
    * Crea un nuovo `HashSet<string>` chiamato `operations` per memorizzare le operazioni permesse (l'uso di `HashSet` garantisce che non ci siano duplicati).
    * Itera attraverso ogni ruolo nell'array `roles`.
    * Utilizza un'istruzione `switch` per determinare le operazioni permesse per ciascun ruolo:
        * **"Administrator"**: Può "Create", "Read", "Update", "Delete".
        * **"Editor"**: Può "Read", "Update".
        * **"Viewer"**: Può "Read".
        * **"SuperAdministrator"**: Può "Create", "Read", "Update", "Delete", "Manage Users", "System Configuration".
    * Restituisce un nuovo array di stringhe contenente tutte le operazioni uniche aggiunte all'`HashSet`.

```csharp
public class RefreshTokenInfo
{
    public required string Token { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public record UserLogin(string Username, string Password);
public record RefreshRequest(string RefreshToken);
public record LogoutRequest(string RefreshToken);

static class UserRoles
{
    public static readonly stringViewerRoles = ["Viewer"];
    public static readonly stringAdminRoles = ["Administrator", "SuperAdministrator"];
    public static readonly stringEmptyRoles =;
}
```

* **`public class RefreshTokenInfo { ... }`**: Definisce una classe per contenere le informazioni di un refresh token.
    * **`public required string Token { get; set; }`**: La stringa del refresh token. La parola chiave `required` indica che questa proprietà deve essere inizializzata durante la creazione dell'oggetto (a partire da C# 11).
    * **`public DateTime CreatedAt { get; set; }`**: La data e l'ora in cui il refresh token è stato creato.
    * **`public DateTime ExpiresAt { get; set; }`**: La data e l'ora in cui il refresh token scade.
* **`public record UserLogin(string Username, string Password);`**: Definisce un record (tipo di riferimento che fornisce un comportamento simile a un valore) per rappresentare le credenziali di login di un utente.
* **`public record RefreshRequest(string RefreshToken);`**: Definisce un record per rappresentare una richiesta di rinnovo del token, contenente solo il refresh token.
* **`public record LogoutRequest(string RefreshToken);`**: Definisce un record per rappresentare una richiesta di logout, contenente opzionalmente un refresh token specifico da invalidare.
* **`static class UserRoles { ... }`**: Definisce una classe statica per contenere array di stringhe che rappresentano i ruoli utente predefiniti.
    * **`public static readonly stringViewerRoles = ["Viewer"];`**: Un array contenente il ruolo "Viewer".
    * **`public static readonly stringAdminRoles = ["Administrator", "SuperAdministrator"];`**: Un array contenente i ruoli "Administrator" e "SuperAdministrator".
    * **`public static readonly stringEmptyRoles =;`**: Un array vuoto di ruoli.

## Note Importanti per la Produzione

* **Protezione della Chiave Segreta:** La chiave segreta utilizzata per firmare i token JWT (`Jwt:Key` nella configurazione) è estremamente sensibile e deve essere protetta in modo sicuro. Non dovrebbe essere hardcoded nel codice o memorizzata in file di configurazione non protetti in ambienti di produzione. Si consiglia di utilizzare meccanismi come Azure Key Vault, AWS Secrets Manager o HashiCorp Vault per la gestione sicura delle chiavi.
* **Memorizzazione dei Refresh Token:** L'esempio utilizza dizionari in-memory per memorizzare i refresh token. Questa soluzione non è adatta per ambienti di produzione in quanto i token vengono persi al riavvio dell'applicazione e non sono scalabili. In produzione, è necessario utilizzare un database o una cache distribuita per memorizzare i refresh token in modo persistente e sicuro.
* **Generazione di Refresh Token Sicuri:** La generazione di refresh token tramite `Guid.NewGuid().ToString()` è semplice ma potrebbe non essere sufficientemente sicura per applicazioni critiche. Considerare l'utilizzo di librerie crittografiche per generare refresh token più robusti e con maggiore entropia.
* **Validazione Input:** L'esempio include una validazione di base dell'input nel login e nel refresh token endpoint. In un'applicazione reale, è necessario implementare una validazione più completa per prevenire vulnerabilità.
* **HTTPS:** È fondamentale utilizzare HTTPS in produzione per proteggere la trasmissione dei token JWT.

**Nota Importante sul Logout e l'Invalidazione degli Access Token:**

È fondamentale comprendere che l'implementazione dell'endpoint `/logout` nel codice fornito si concentra principalmente sull'invalidazione del **refresh token**. Questo significa che, una volta chiamato l'endpoint di logout, il refresh token dell'utente viene rimosso o invalidato, impedendo di ottenere nuovi access token in futuro con quel refresh token specifico o con tutti i refresh token associati all'utente.

Tuttavia, **l'access token JWT precedentemente rilasciato rimane valido fino alla sua naturale scadenza** (in questo caso, 30 minuti). Questo è un comportamento standard dei token JWT stateless. Il server che ospita l'API non tiene traccia degli access token emessi e non ha un meccanismo diretto per invalidarli prima della loro scadenza.

Per ottenere un'**invalidazione immediata** anche dell'access token, sono necessarie tecniche più sofisticate, poiché i JWT sono auto-contenuti e la loro validità è determinata dalla firma crittografica e dalla data di scadenza, non da una sessione attiva sul server. Ecco le principali tecniche utilizzate per affrontare questo scenario:

1. **Blacklisting (o Revocation List):**
    * Il server mantiene una lista (in memoria, in un database o in un sistema di caching distribuito come Redis) degli access token che sono stati revocati.
    * Ogni volta che un'API protetta riceve una richiesta con un access token, il server deve consultare questa blacklist per verificare se il token è stato invalidato.
    * Questo introduce uno stato nel sistema (rendendo di fatto i token meno "stateless") e può avere implicazioni sulle prestazioni se la blacklist diventa molto grande.

2. **Short Access Token Expiration Times:**
    * Si utilizzano tempi di scadenza molto brevi per gli access token (ad esempio, pochi minuti).
    * Questo riduce la finestra temporale in cui un token compromesso potrebbe essere utilizzato dopo il logout.
    * Richiede un uso più frequente dei refresh token per ottenere nuovi access token, il che può aumentare il carico sul server di autenticazione.

3. **Distributed Cache/Store:**
    * Si utilizza un sistema di caching distribuito (come Redis) o un database per memorizzare lo stato degli access token (ad esempio, una flag che indica se è ancora valido).
    * Quando un utente effettua il logout, lo stato del suo access token (o di tutti i suoi access token attivi) viene aggiornato nel cache/store come "invalidato".
    * Il middleware di autenticazione, ad ogni richiesta, interroga il cache/store per verificare la validità del token.

4. **Centralized Authentication Service (Token Introspection):**
    * L'API delega la validazione del token a un servizio di autenticazione centralizzato.
    * Quando un'API riceve un token, contatta il servizio di autenticazione per "ispezionare" il token e verificarne la validità (inclusa la possibilità che sia stato revocato).
    * Questo introduce una dipendenza da un servizio esterno ad ogni richiesta, ma permette un controllo centralizzato sullo stato dei token.

5. **Token Versioning o Rolling:**
    * Si introduce un meccanismo di versionamento dei token.
    * Quando un utente effettua il logout, la versione corrente dei suoi token viene invalidata.
    * I token precedenti (con la versione invalidata) vengono rifiutati dalle API.
    * Questo richiede una gestione più complessa del ciclo di vita dei token e delle versioni.

È importante scegliere la tecnica più adatta in base ai requisiti di sicurezza, alle prestazioni e alla complessità del sistema. L'invalidazione immediata degli access token introduce una maggiore complessità rispetto alla semplice invalidazione dei refresh token.

## Riferimenti Utili

* [Autenticazione JWT Bearer in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-bearer)

* [Claims-based authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/claims)

* [JSON Web Tokens (JWT) - jwt.io](https://jwt.io/)

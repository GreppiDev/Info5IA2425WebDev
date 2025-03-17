# Documentazione dell'Esempio di Autenticazione Base con Cookie e Handling API in ASP.NET Core

Questo documento fornisce una spiegazione dettagliata del codice fornito, illustrando l'implementazione dell'autenticazione base basata su cookie in un'applicazione ASP.NET Core Minimal API. L'attenzione è focalizzata sulla gestione delle richieste non autenticate, distinguendo tra richieste provenienti da browser e richieste API.

## Panoramica

L'applicazione implementa un sistema di autenticazione semplice basato su cookie. Gli utenti possono effettuare il login tramite un endpoint dedicato, e le richieste successive vengono autenticate tramite un cookie. L'esempio include una logica specifica per gestire le richieste non autenticate provenienti da client API, restituendo un codice di stato 401 Unauthorized anziché reindirizzare a una pagina di login.

## Analisi del Codice

### Importazione dei Namespace

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies; //aggiunge il supporto per l'autenticazione tramite cookie
```

* `System.Security.Claims`: Utilizzato per rappresentare l'identità dell'utente tramite claims (in questo caso, solo il nome).
* `Microsoft.AspNetCore.Authentication`: Namespace base per la gestione dell'autenticazione in ASP.NET Core.
* `Microsoft.AspNetCore.Authentication.Cookies`: Fornisce il supporto specifico per l'autenticazione basata su cookie. Per maggiori dettagli, consultare la [documentazione sull'autenticazione con cookie](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-9.0).

### Configurazione dei Servizi

#### Supporto per OpenAPI

```csharp
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
    {
        config.Title = "Basic Cookie v1";
        config.DocumentName = "Basic Cookie API";
        config.Version = "v1";
    }
);
```

Queste righe configurano il supporto per OpenAPI (Swagger), consentendo la generazione di documentazione interattiva per le API.

#### Configurazione dell'Autenticazione con Cookie

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = ".AspNetCore.Authentication"; // Nome standard non predittivo
        options.Cookie.HttpOnly = true; // Protegge il cookie da accessi via JavaScript
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Solo su HTTPS
        options.Cookie.SameSite = SameSiteMode.Strict; // Previene CSRF
        options.LoginPath = "/login"; // Percorso per il login

        // Content-type based redirect handling
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                // Check if this is an API request based on Accept header or Content-Type
                bool isApiRequest = context.Request.Headers.Accept.Any(h => h != null &&
                    (h.Contains("application/json") || h.Contains("application/xml")));

                // Also check if the Accept header DOESN'T contain "text/html" - typical for API clients
                isApiRequest = isApiRequest ||
                    (context.Request.Headers.Accept.Count != 0 &&
                     !context.Request.Headers.Accept.Any(h => h != null && h.Contains("text/html")));

                // Also check X-Requested-With header commonly used for AJAX
                isApiRequest = isApiRequest ||
                    context.Request.Headers.XRequestedWith == "XMLHttpRequest";


                if (isApiRequest)
                {
                    // For API requests, return 401 status code
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }

                // For browser requests, redirect to login page (default behavior)
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            }
        };
    });
```

Questa sezione configura l'autenticazione basata su cookie:

* `AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)`: Aggiunge i servizi di autenticazione, impostando lo schema predefinito come "Cookies".
* `.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => { ... })`: Configura le opzioni specifiche per i cookie di autenticazione:
    * `options.Cookie.Name`: Definisce il nome del cookie di autenticazione (`.AspNetCore.Authentication`).
    * `options.Cookie.HttpOnly = true`: Impedisce l'accesso al cookie tramite JavaScript per mitigare attacchi XSS.
    * `options.Cookie.SecurePolicy = CookieSecurePolicy.Always`: Assicura che il cookie venga trasmesso solo su connessioni HTTPS.
    * `options.Cookie.SameSite = SameSiteMode.Strict`: Aiuta a prevenire attacchi CSRF.
    * `options.LoginPath = "/login"`: Specifica il percorso a cui reindirizzare gli utenti non autenticati.
    * `options.Events.OnRedirectToLogin`: Questo gestore di eventi personalizza il comportamento quando un utente non autenticato tenta di accedere a una risorsa protetta.
        * Verifica se la richiesta è un'API controllando l'header `Accept` (se contiene `application/json` o `application/xml` oppure se non contiene `text/html`) o l'header `X-Requested-With` (se è `XMLHttpRequest`, tipico delle richieste AJAX).
        * Se la richiesta è identificata come proveniente da un client API, imposta il codice di stato della risposta a 401 Unauthorized e completa la richiesta, evitando il reindirizzamento alla pagina di login. Questo è un comportamento tipico per le API.
        * Se la richiesta non è identificata come un'API (si presume provenga da un browser), viene eseguito il comportamento predefinito, ovvero il reindirizzamento all'URL di login specificato in `options.LoginPath`.

#### Configurazione dell'Autorizzazione

```csharp
// Add authorization services
builder.Services.AddAuthorization();
```

Questa riga aggiunge i servizi di autorizzazione, necessari per proteggere gli endpoint. In questo esempio base, l'autorizzazione viene applicata semplicemente richiedendo che l'utente sia autenticato. Per configurazioni più complesse basate su ruoli o policies, si farebbe in modo diverso (come mostrato negli esempi precedenti). Per maggiori informazioni, consultare la [documentazione sull'autorizzazione in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction?view=aspnetcore-9.0).

### Costruzione dell'Applicazione e Configurazione della Pipeline di Middleware

```csharp
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseOpenApi();
    app.UseSwaggerUi(config => { ... });
    app.UseDeveloperExceptionPage();
}

// Middleware
app.UseAuthentication();
app.UseAuthorization();
```

* `app.UseAuthentication()`: Aggiunge il middleware di autenticazione alla pipeline. Questo middleware è responsabile dell'identificazione dell'utente in base al cookie di autenticazione presente nella richiesta.
* `app.UseAuthorization()`: Aggiunge il middleware di autorizzazione, che controlla se l'utente autenticato ha il permesso di accedere alla risorsa richiesta. In questo caso, viene utilizzato principalmente per verificare se l'utente è autenticato.

### Endpoint

#### Endpoint di Login (`/login`)

```csharp
app.MapPost("/login", async (HttpContext ctx, LoginModel model) =>
{
    // Simulazione della validazione delle credenziali
    //in questo caso andrebbero verificate le credenziali sul database...
    if (model.Username == "user" && model.Password == "pass")
    {
        var claims = new{ new Claim(ClaimTypes.Name, model.Username) };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        return Results.Ok("Login effettuato con successo");
    }
    return Results.Unauthorized();
});
```

Questo endpoint gestisce le richieste di login. Simula la validazione delle credenziali (in una vera applicazione, si verificherebbero le credenziali rispetto a un database). Se le credenziali sono corrette:

* Viene creato un array di claims contenente il nome utente.
* Viene creata un'identità basata su questi claims e sullo schema di autenticazione con cookie.
* Viene creato un `ClaimsPrincipal` contenente l'identità.
* `ctx.SignInAsync()` viene chiamato per creare e impostare il cookie di autenticazione nel browser dell'utente.
* Viene restituito un risultato Ok con un messaggio di successo.
* Se le credenziali non sono valide, viene restituito un risultato Unauthorized (codice di stato 401).

#### Endpoint per Impostare un Cookie (`/set-cookie`)

```csharp
app.MapGet("/set-cookie", (HttpContext context) =>
{
    // Generazione di un identificativo univoco
    var uniqueIdentifier = Guid.NewGuid().ToString();
    // Configurazione delle opzioni del cookie
    var cookieOptions = new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Expires = DateTimeOffset.Now.AddMinutes(30) // Cookie persistente per 30 minuti
    };
    context.Response.Cookies.Append("uniqueIdentifier", uniqueIdentifier, cookieOptions);
    return Results.Ok("Cookie impostato correttamente!");
});
```

Questo endpoint dimostra come impostare un cookie aggiuntivo con opzioni di sicurezza. Non è direttamente correlato all'autenticazione, ma mostra la manipolazione dei cookie in ASP.NET Core.

#### Endpoint per Leggere un Cookie (`/read-cookie`)

```csharp
app.MapGet("/read-cookie", (HttpContext context) =>
{
    var uniqueIdentifier = context.Request.Cookies["uniqueIdentifier"];
    return uniqueIdentifier != null
        ? Results.Ok($"Il valore del cookie è: {uniqueIdentifier}")
        : Results.NotFound("Cookie non trovato");
});
```

Questo endpoint mostra come leggere il valore di un cookie dalla richiesta. Anche questo non è direttamente legato all'autenticazione principale.

#### Endpoint Protetto (`/profile`)

```csharp
app.MapGet("/profile", (HttpContext ctx) =>
{
    // Verifica che l'utente sia autenticato, altrimenti restituisce 401 Unauthorized
    // Questo controllo è ridondante con RequireAuthorization, ma aiuta a chiarire il flusso
    if (ctx.User.Identity != null && ctx.User.Identity.IsAuthenticated)
        return Results.Ok($"Benvenuto, {ctx.User.Identity.Name}");
    return Results.Unauthorized();
}).RequireAuthorization();
```

Questo endpoint è protetto tramite `.RequireAuthorization()`. Solo gli utenti che hanno un cookie di autenticazione valido possono accedere a questo endpoint. La logica all'interno verifica anche lo stato di autenticazione tramite `ctx.User.Identity.IsAuthenticated` (anche se `RequireAuthorization()` rende questo controllo ridondante).

#### Endpoint di Logout (`/logout`)

```csharp
app.MapPost("/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok("Logout effettuato con successo");
});
```

Questo endpoint gestisce il logout dell'utente. Chiama `ctx.SignOutAsync()` per rimuovere il cookie di autenticazione dal browser dell'utente, invalidando la sessione di autenticazione.

### Classe di Supporto

#### `LoginModel`

```csharp
public class LoginModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

Modello semplice per rappresentare le credenziali di login inviate dall'utente.

## Funzionalità Chiave

La funzionalità più importante di questo esempio è la gestione personalizzata del reindirizzamento per le richieste non autenticate nell'evento `OnRedirectToLogin`. Questo permette all'applicazione di rispondere in modo appropriato a diversi tipi di client:

* **Browser:** Vengono reindirizzati alla pagina di login come comportamento standard.
* **API Client:** Ricevono un codice di stato HTTP 401 Unauthorized, indicando che la richiesta non è stata autenticata.

Questo approccio è fondamentale per le applicazioni che espongono sia un'interfaccia utente web tradizionale che un'API per altri client.

## Riferimenti Utili

* [Autenticazione con cookie in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie)

* [Autorizzazione in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction)
* [Claims-based identity in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/claims)

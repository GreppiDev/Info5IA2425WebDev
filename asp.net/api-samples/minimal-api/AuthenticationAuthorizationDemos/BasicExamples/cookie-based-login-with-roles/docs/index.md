# Documentazione dell'Esempio di Autenticazione e Autorizzazione Basata su Ruoli con Cookie in ASP.NET Core

Questo documento fornisce una spiegazione dettagliata del codice fornito, illustrando l'implementazione dell'autenticazione basata su cookie e l'autorizzazione basata su ruoli in un'applicazione ASP.NET Core Minimal API. Questa versione include una modifica importante nella gestione del reindirizzamento per le richieste API non autenticate. Verranno richiamati i concetti fondamentali e forniti riferimenti alla documentazione ufficiale Microsoft per una comprensione più approfondita.

## Panoramica

Questa applicazione dimostra come proteggere le Minimal APIs utilizzando l'autenticazione con cookie e come implementare l'autorizzazione basata su ruoli per controllare l'accesso a diverse aree dell'applicazione. L'esempio include la gestione di scenari di accesso negato sia per richieste browser che per API. **Una modifica significativa rispetto alla versione precedente è che per le richieste API non autenticate, ora viene restituito un codice di stato 401 senza tentare un reindirizzamento.**

## Analisi del Codice

### Importazione dei Namespace

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc; //aggiunge il supporto per l'autenticazione tramite cookie
```

* `System.Security.Claims`: Utilizzato per rappresentare l'identità dell'utente tramite claims, inclusi i ruoli.
* `Microsoft.AspNetCore.Authentication`: Namespace base per la gestione dell'autenticazione in ASP.NET Core.
* `Microsoft.AspNetCore.Authentication.Cookies`: Fornisce il supporto specifico per l'autenticazione basata su cookie. Per maggiori dettagli, consultare la [documentazione sull'autenticazione con cookie](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-9.0).
* `Microsoft.AspNetCore.Mvc`: Introduce l'attributo `[FromQuery]` utilizzato nell'endpoint di logout e accesso negato.

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
        options.AccessDeniedPath = "/access-denied"; // Percorso per l'accesso negato

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
                    // For API requests, return 401 status code without redirect
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    // Prevent the default redirect which can cause the 405 error
                    context.Response.Headers["Location"] = string.Empty;
                    return Task.CompletedTask;
                }

                // For browser requests, redirect to login page (default behavior)
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            },

            // Gestione della risposta quando l'accesso è negato a un utente autenticato (Forbidden)
            OnRedirectToAccessDenied = context =>
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
                    // Return 403 Forbidden for API requests
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }

                // Per richieste browser, personalizza l'URL aggiungendo parametri
                var redirectUri = context.RedirectUri;
                var returnUrl = context.Request.Path;

                // Aggiungi il returnUrl come parametro di query per poter tornare
                // alla pagina originale dopo l'autenticazione
                redirectUri = $"{options.AccessDeniedPath}?returnUrl={Uri.EscapeDataString(returnUrl)}";

                // Default behavior for browser requests with custom URL
                context.Response.Redirect(redirectUri);
                return Task.CompletedTask;
            }
        };
    });
```

Questa sezione configura l'autenticazione basata su cookie. La parte più rilevante è l'oggetto `options.Events` di tipo `CookieAuthenticationEvents`:

* **`OnRedirectToLogin`**: Questo gestore di eventi viene invocato quando un utente non autenticato tenta di accedere a una risorsa protetta.
    * **Identificazione Richieste API**: Verifica se la richiesta è un'API controllando gli header `Accept` (se contiene `application/json` o `application/xml` oppure se non contiene `text/html`) o `X-Requested-With` (se è `XMLHttpRequest`, tipico delle richieste AJAX).
    * **Gestione Richieste API**: Se identificata come richiesta API, imposta il codice di stato della risposta a `StatusCodes.Status401Unauthorized` (401 Unauthorized). **La novità in questa versione è l'aggiunta di `context.Response.Headers["Location"] = string.Empty;`. Questa riga impedisce il reindirizzamento predefinito che potrebbe causare un errore 405 (Method Not Allowed) in alcuni scenari di client API.**
    * **Gestione Richieste Browser**: Se non è una richiesta API, viene eseguito il comportamento predefinito, ovvero il reindirizzamento all'URL di login specificato in `options.LoginPath`.
* **`OnRedirectToAccessDenied`**: Questo gestore di eventi viene invocato quando un utente autenticato tenta di accedere a una risorsa per la quale non ha l'autorizzazione.
    * **Identificazione Richieste API**: Similmente a `OnRedirectToLogin`, verifica se la richiesta è un'API.
    * **Gestione Richieste API**: Se è una richiesta API, imposta il codice di stato della risposta a `StatusCodes.Status403Forbidden` (403 Forbidden).
    * **Gestione Richieste Browser**: Per le richieste browser, reindirizza all'`AccessDeniedPath`, aggiungendo l'URL originale (`returnUrl`) come parametro di query.

#### Configurazione dell'Autorizzazione Basata su Ruoli (Policies)

```csharp
// Add authorization services - senza specificare policy
//builder.Services.AddAuthorization();
// Add authorization services con policy per ruoli multipli
builder.Services.AddAuthorization(options =>
{
    // Policy che richiede un solo ruolo specifico
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("PowerUserOnly", policy => policy.RequireRole("PowerUser"));

    // Policy che accetta uno qualsiasi tra più ruoli (OR logico)
    options.AddPolicy("AdminOrPowerUser", policy =>
        policy.RequireRole("Admin", "PowerUser"));

    // Policy per tutti gli utenti registrati
    options.AddPolicy("RegisteredUsers", policy =>
        policy.RequireRole("Admin", "PowerUser", "User"));

    // Policy che richiede TUTTI i ruoli specificati (AND logico)
    options.AddPolicy("AdminAndPowerUser", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") && context.User.IsInRole("PowerUser")));

    // Policy con logica personalizzata più complessa
    options.AddPolicy("ComplexRoleRequirement", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") ||
            (context.User.IsInRole("PowerUser") && context.User.HasClaim(c =>
                c.Type == "Permission" && c.Value == "CanEditContent"))));
});
```

Questa sezione configura i servizi di autorizzazione e definisce diverse **policies** basate sui ruoli. Le policies sono un modo per definire regole di autorizzazione riutilizzabili. Per maggiori informazioni, consultare la [documentazione sull'autorizzazione basata su ruoli](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-9.0). Le policies definite sono le stesse dell'esempio precedente.

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

* `app.UseAuthentication()`: Aggiunge il middleware di autenticazione per identificare l'utente.
* `app.UseAuthorization()`: Aggiunge il middleware di autorizzazione per applicare le policies definite.

### Endpoint

Gli endpoint definiti in questo codice sono gli stessi dell'esempio precedente e forniscono funzionalità per:

* `/login`: Autenticare gli utenti e assegnare ruoli (Admin, PowerUser, User).
* `/set-cookie`: Impostare un cookie generico.
* `/read-cookie`: Leggere il valore di un cookie generico.
* `/profile`: Endpoint protetto che richiede l'autenticazione.
* `/admin-area`: Endpoint accessibile solo agli utenti con il ruolo "Admin".
* `/power-area`: Endpoint accessibile agli utenti con il ruolo "Admin" o "PowerUser".
* `/super-area`: Endpoint accessibile agli utenti con entrambi i ruoli "Admin" e "PowerUser".
* `/my-roles`: Endpoint per visualizzare i ruoli dell'utente autenticato.
* `/login-with-permissions`: Endpoint di login che assegna anche claims di "Permission".
* `/edit-content`: Endpoint che richiede l'autenticazione e verifica un claim di "Permission".
* `/logout`: Endpoint per effettuare il logout.
* `/access-denied`: Endpoint personalizzato per visualizzare un messaggio di accesso negato.

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

## Modifica Chiave

La modifica più importante in questo codice si trova nel gestore dell'evento `OnRedirectToLogin` all'interno della configurazione dell'autenticazione con cookie. Per le richieste API non autenticate, oltre a impostare il codice di stato 401, viene ora impostato l'header `Location` della risposta su una stringa vuota. **Questo impedisce il tentativo di reindirizzamento da parte del middleware, il che può risolvere problemi come l'errore 405 che a volte si verifica quando un client API invia una richiesta a un endpoint che richiede l'autenticazione.**

## Riferimenti Utili

* [Autenticazione con cookie in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie)

* [Autorizzazione in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction)

* [Autorizzazione basata su ruoli in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles)

* [Claims-based identity in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/claims)
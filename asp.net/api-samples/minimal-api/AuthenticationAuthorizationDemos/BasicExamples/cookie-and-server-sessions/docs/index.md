# Documentazione dell'Esempio di Autenticazione con Cookie e Gestione delle Sessioni in ASP.NET Core

Questo documento fornisce una spiegazione dettagliata del codice fornito, illustrando l'implementazione dell'autenticazione basata su cookie e l'aggiunta della gestione delle sessioni in un'applicazione ASP.NET Core Minimal API. L'esempio introduce l'uso delle sessioni per memorizzare dati specifici dell'utente, come un carrello della spesa.

## Panoramica

Questa applicazione dimostra come proteggere le Minimal APIs con l'autenticazione tramite cookie e come utilizzare le sessioni per mantenere lo stato dell'utente tra le richieste. L'esempio include funzionalità per l'autenticazione, la gestione di cookie aggiuntivi e l'utilizzo delle sessioni per implementare un semplice carrello della spesa.

## Analisi del Codice

### Importazione dei Namespace

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies; //aggiunge il supporto per l'autenticazione tramite cookie
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
```

* `System.Security.Claims`: Utilizzato per rappresentare l'identità dell'utente tramite claims.
* `Microsoft.AspNetCore.Authentication`: Namespace base per la gestione dell'autenticazione in ASP.NET Core.
* `Microsoft.AspNetCore.Authentication.Cookies`: Fornisce il supporto specifico per l'autenticazione basata su cookie. Per maggiori dettagli, consultare la [documentazione sull'autenticazione con cookie](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-9.0).
* `Microsoft.AspNetCore.DataProtection`: Utilizzato per proteggere i dati sensibili, come i cookie di sessione.
* `Microsoft.AspNetCore.Mvc`: Introduce l'attributo `[FromQuery]` utilizzato nell'endpoint di logout.

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

Questa sezione configura l'autenticazione basata su cookie. Le opzioni di configurazione sono le stesse spiegate nel primo esempio.

#### Configurazione delle Sessioni

```csharp
//aggiunta delle sessioni
// Aggiunta dei servizi per la gestione delle sessioni
builder.Services.AddDistributedMemoryCache();

//Per utilizzare una cache distribuita con Redis (si vedrà in dettaglio nei prossimi esempi)
//Nota: Se si utilizza Redis, è necessario installare il pacchetto NuGet: Microsoft.Extensions.Caching.StackExchangeRedis

// Aggiunta dei servizi per la gestione delle sessioni con Redis
// builder.Services.AddStackExchangeRedisCache(options =>
// {
//     options.Configuration = "your-redis-connection-string";
//     options.InstanceName = "BasicCookieDemo_";
// });

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    // Add application discriminator to avoid conflicts
    options.Cookie.Name = ".MyApp.Session";
    // Add missing security settings to match authentication cookie
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Solo su HTTPS
    options.Cookie.SameSite = SameSiteMode.Strict; // Previene CSRF
});
```

Questa sezione configura i servizi per la gestione delle sessioni in ASP.NET Core. Per maggiori dettagli, consultare la [documentazione sulle sessioni](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-9.0).

* **`AddDistributedMemoryCache()`**: Configura una cache in-memory per memorizzare i dati della sessione. In ambienti di produzione, è comune utilizzare una cache distribuita come Redis o SQL Server.
* **Configurazione Redis (commentata)**: Il codice include un esempio commentato di come configurare una cache distribuita utilizzando Redis. Per utilizzarlo, è necessario installare il pacchetto NuGet `Microsoft.Extensions.Caching.StackExchangeRedis` e fornire la stringa di connessione al server Redis.
* **`AddSession(options => { ... })`**: Configura le opzioni per la gestione delle sessioni:
    * `options.Cookie.HttpOnly = true`: Impedisce l'accesso al cookie di sessione tramite JavaScript.
    * `options.Cookie.IsEssential = true`: Indica che il cookie di sessione è fondamentale per il funzionamento dell'applicazione.
    * `options.IdleTimeout = TimeSpan.FromMinutes(30)`: Imposta la durata massima di inattività della sessione a 30 minuti. Se la sessione non viene utilizzata entro questo periodo, viene considerata scaduta.
    * `options.Cookie.Name = ".MyApp.Session"`: Definisce il nome del cookie di sessione, aggiungendo un discriminatore per l'applicazione.
    * `options.Cookie.SecurePolicy = CookieSecurePolicy.Always`: Assicura che il cookie di sessione venga trasmesso solo su connessioni HTTPS.
    * `options.Cookie.SameSite = SameSiteMode.Strict`: Aiuta a prevenire attacchi CSRF.

#### Configurazione di Data Protection

```csharp
//per gestire le chiavi in un file
// Configura Data Protection con un percorso persistente per le chiavi usate per proteggere i cookie di sessione

// builder.Services.AddDataProtection()
//     .SetApplicationName("BasicCookieDemo")
//     .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")))
//     .SetDefaultKeyLifetime(TimeSpan.FromDays(14)); // Set key lifetime explicitly


//per gestire le chiavi in memoria
// Configura Data Protection con un percorso persistente per le chiavi usate per proteggere i dati di sessione
builder.Services.AddDataProtection()
    .SetApplicationName("BasicCookieDemo")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(14))
    .DisableAutomaticKeyGeneration() // Disabilita la generazione automatica di nuove chiavi
    .UseEphemeralDataProtectionProvider(); // Usa un provider di protezione dati effimero (in-memory)
```

Data Protection è un sistema in ASP.NET Core utilizzato per proteggere i dati sensibili, inclusi i cookie di sessione.

* **Configurazione con File System (commentata)**: Il codice include un esempio commentato di come configurare Data Protection per persistere le chiavi di crittografia nel file system. Questo è il metodo raccomandato per ambienti di produzione.
* **Configurazione In-Memory (attiva)**: La configurazione attiva utilizza `UseEphemeralDataProtectionProvider()`, che memorizza le chiavi in memoria. Questo approccio è adatto per ambienti di sviluppo o test, ma non è consigliato per la produzione poiché le chiavi vengono perse al riavvio dell'applicazione. `DisableAutomaticKeyGeneration()` impedisce la generazione automatica di nuove chiavi, e `SetDefaultKeyLifetime()` imposta la durata delle chiavi.

#### Configurazione dell'Autorizzazione

```csharp
// Add authorization services
builder.Services.AddAuthorization();
```

Questa riga aggiunge i servizi di autorizzazione, necessari per proteggere gli endpoint.

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
//per l'utilizzo delle sessioni
app.UseSession();

// Add middleware to load session asynchronously for better performance
//[https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?#load-session-state-asynchronously](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?#load-session-state-asynchronously)
// Default Behavior: By default, session state is loaded synchronously at the beginning of the request pipeline.
// Performance Issue: Reading from distributed stores like Redis can introduce latency if done synchronously, potentially blocking threads.
// Solution: The LoadAsync() method allows loading session data asynchronously before it's needed.
app.Use(async (context, next) =>
{
    // Load session data asynchronously at the start of the request
    await context.Session.LoadAsync();

    // Continue processing the HTTP request
    await next();
});
```

* `app.UseAuthentication()`: Aggiunge il middleware di autenticazione.
* `app.UseAuthorization()`: Aggiunge il middleware di autorizzazione.
* `app.UseSession()`: Aggiunge il middleware per abilitare le sessioni nell'applicazione. **È importante che `UseSession()` venga chiamato dopo `UseAuthentication()` e `UseAuthorization()` nella pipeline.**
* **Middleware per Caricamento Asincrono della Sessione**: Il codice include un middleware personalizzato che chiama `context.Session.LoadAsync()` all'inizio della pipeline. Questo carica i dati della sessione in modo asincrono, migliorando le prestazioni, soprattutto quando si utilizzano cache distribuite con potenziale latenza.

### Endpoint

#### Endpoint di Login (`/login`)

```csharp
app.MapPost("/login", async (HttpContext ctx, LoginModel model) =>
{
    // Simulazione validazione credenziali
    if (model.Username == "user" && model.Password == "pass")
    {
        // Clear any existing session to start fresh
        ctx.Session.Clear();

        // Regenerate session ID to prevent session fixation
        if (ctx.Request.Cookies.ContainsKey(".MyApp.Session"))
        {
            ctx.Response.Cookies.Delete(".MyApp.Session");
        }

        var claims = new{ new Claim(ClaimTypes.Name, model.Username) };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return Results.Ok("Login effettuato con successo");
    }
    return Results.Unauthorized();
});
```

Questo endpoint gestisce il login dell'utente. Oltre alla logica di autenticazione tramite cookie, include anche:

* `ctx.Session.Clear()`: Cancella qualsiasi dato di sessione esistente per l'utente che sta effettuando il login.
* Rigenerazione dell'ID di Sessione: Se esiste già un cookie di sessione, viene eliminato. Questo è una misura di sicurezza per prevenire attacchi di session fixation. Un nuovo ID di sessione verrà generato automaticamente alla successiva interazione con la sessione.

#### Endpoint per Impostare un Cookie (`/set-cookie`)

```csharp
app.MapGet("/set-cookie", (HttpContext context) => { ... });
```

Questo endpoint è lo stesso dell'esempio precedente e dimostra come impostare un cookie aggiuntivo.

#### Endpoint per Leggere un Cookie (`/read-cookie`)

```csharp
app.MapGet("/read-cookie", (HttpContext context) => { ... });
```

Questo endpoint è lo stesso dell'esempio precedente e dimostra come leggere un cookie.

#### Endpoint Protetto (`/profile`)

```csharp
app.MapGet("/profile", (HttpContext ctx) => { ... }).RequireAuthorization();
```

Questo endpoint è protetto e richiede che l'utente sia autenticato.

#### Endpoint Protetto per Aggiungere Articoli al Carrello (`/cart/add`)

```csharp
app.MapPost("/cart/add", (HttpContext ctx, CartItem item) =>
{
    // Verifica che l'utente sia autenticato
    if (ctx.User.Identity == null || !ctx.User.Identity.IsAuthenticated)
        return Results.Unauthorized();

    // Ottiene il carrello dell'utente dalla sessione o ne crea uno nuovo
    var cart = ctx.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

    // Controlla se l'articolo è già presente nel carrello
    var existingItem = cart.FirstOrDefault(i => i.Id == item.Id);
    if (existingItem != null)
    {
        // Aggiorna la quantità dell'articolo esistente
        existingItem.Quantity += item.Quantity;
    }
    else
    {
        // Aggiunge il nuovo articolo al carrello
        cart.Add(item);
    }

    // Salva il carrello aggiornato nella sessione
    ctx.Session.SetObjectAsJson("Cart", cart);

    return Results.Ok(new { Message = "Articolo aggiunto al carrello", Cart = cart });
}).RequireAuthorization();
```

Questo endpoint protetto consente agli utenti autenticati di aggiungere articoli al loro carrello. Utilizza la sessione per memorizzare il carrello:

* `ctx.Session.GetObjectFromJson<List<CartItem>>("Cart")`: Tenta di recuperare un oggetto `List<CartItem>` dalla sessione con la chiave "Cart". Se non esiste, ne crea uno nuovo.
* La logica successiva gestisce l'aggiunta o l'aggiornamento degli articoli nel carrello.
* `ctx.Session.SetObjectAsJson("Cart", cart)`: Salva il carrello aggiornato nella sessione come JSON.

#### Endpoint Protetto per Visualizzare il Carrello (`/cart`)

```csharp
app.MapGet("/cart", (HttpContext ctx) =>
{
    // This check is redundant with RequireAuthorization, but helps clarify the flow
    if (ctx.User.Identity == null || !ctx.User.Identity.IsAuthenticated)
        return Results.Unauthorized(); // Explicitly return 401 Unauthorized

    // Ottiene il carrello dell'utente dalla sessione
    var cart = ctx.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
    return Results.Ok(cart);
}).RequireAuthorization();
```

Questo endpoint protetto consente agli utenti autenticati di visualizzare il contenuto del loro carrello, recuperato dalla sessione.

#### Endpoint di Logout (`/logout`)

```csharp
app.MapPost("/logout", async (HttpContext ctx, [FromQuery] string? returnUrl) => { ... });
```

Questo endpoint è lo stesso dell'esempio precedente e gestisce il logout dell'utente.

### Classi di Supporto

#### `LoginModel`

```csharp
public class LoginModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

Modello per le credenziali di login.

#### `CartItem`

```csharp
// Modello per gli articoli del carrello
public class CartItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
}
```

Modello per rappresentare un articolo nel carrello della spesa.

#### `SessionExtensions`

```csharp
// Estensioni per gestire oggetti JSON nelle sessioni
public static class SessionExtensions
{
    public static void SetObjectAsJson(this ISession session, string key, object value)
    {
        session.SetString(key, System.Text.Json.JsonSerializer.Serialize(value));
    }

    public static T? GetObjectFromJson<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default(T) : System.Text.Json.JsonSerializer.Deserialize<T>(value);
    }
}
```

Questa classe di estensione fornisce metodi utili per serializzare e deserializzare oggetti JSON da e verso la sessione. Poiché l'interfaccia `ISession` memorizza solo stringhe, questi metodi semplificano la gestione di oggetti complessi.

## Funzionalità Chiave

La principale aggiunta a questo esempio è l'implementazione della gestione delle sessioni per mantenere lo stato dell'utente. Questo consente di memorizzare informazioni come il carrello della spesa tra le diverse richieste dell'utente. Le misure di sicurezza aggiuntive nell'endpoint di login (cancellazione e rigenerazione dell'ID di sessione) contribuiscono a proteggere l'applicazione da attacchi di session fixation.

## Riferimenti Utili

* **Autenticazione con cookie in ASP.NET Core:** [https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-9.0](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-9.0)
* **Gestione dello stato della sessione in ASP.NET Core:** [https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-9.0](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-9.0)
* **Data Protection in ASP.NET Core:** [https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/introduction?view=aspnetcore-9.0](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/introduction?view=aspnetcore-9.0)
* **Autorizzazione in ASP.NET Core:** [https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction?view=aspnetcore-9.0](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction?view=aspnetcore-9.0)

## Potenziali Problemi con la Memorizzazione di Dati di Sessione Lato Server

L'utilizzo di sessioni per memorizzare dati lato server, come il carrello in questo esempio, può presentare alcuni problemi, specialmente in architetture multi-server o in scenari con elevato traffico. Come indicato nella [documentazione](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-9.0#session-state):

* **Dipendenza dal Server (con `DistributedMemoryCache`):** Con la configurazione attuale che utilizza `DistributedMemoryCache`, i dati di sessione vengono memorizzati nella memoria del server. Se l'applicazione viene eseguita su più server (load balancing), le richieste dello stesso utente potrebbero essere indirizzate a server diversi, e il carrello memorizzato nella sessione di un server non sarebbe disponibile su un altro. Questo può portare a un'esperienza utente incoerente.
* **Perdita di Dati al Riavvio:** I dati memorizzati in `DistributedMemoryCache` vengono persi al riavvio del server o dell'applicazione.
* **Scalabilità:** Per applicazioni con un numero elevato di utenti, la memorizzazione di grandi quantità di dati di sessione in memoria su più server può diventare inefficiente e costosa.

**Soluzioni per Architetture Multi-Server:**

Per superare questi problemi in scenari multi-server, è consigliabile utilizzare una **cache distribuita esterna**, come Redis o un altro database. Come mostrato nel codice commentato, ASP.NET Core supporta facilmente l'integrazione con Redis tramite il pacchetto `Microsoft.Extensions.Caching.StackExchangeRedis`. L'utilizzo di una cache distribuita consente di centralizzare lo storage dei dati di sessione, rendendoli accessibili a tutti i server dell'applicazione e garantendo la persistenza dei dati.

## Conclusioni

Questo esempio di codice illustra in modo efficace l'implementazione dell'autenticazione basata su cookie e la gestione delle sessioni in un'applicazione ASP.NET Core Minimal API. È fondamentale comprendere la distinzione tra cookie di autenticazione e cookie di sessione, nonché le implicazioni della scelta del meccanismo di storage per i dati di sessione, specialmente in ambienti di produzione e in applicazioni scalabili. L'utilizzo di una cache distribuita come Redis è spesso la soluzione preferita per applicazioni multi-server per garantire coerenza e persistenza dei dati di sessione.

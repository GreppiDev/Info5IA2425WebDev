# Server Side Sessions in ASP.NET Core Minimal APIs

- [Server Side Sessions in ASP.NET Core Minimal APIs](#server-side-sessions-in-aspnet-core-minimal-apis)
  - [Cosa sono le Sessioni?](#cosa-sono-le-sessioni)
  - [Sessioni in ASP.NET Core Minimal APIs](#sessioni-in-aspnet-core-minimal-apis)
  - [Configurazione delle Sessioni](#configurazione-delle-sessioni)
  - [Lavorare con i Dati di Sessione](#lavorare-con-i-dati-di-sessione)
  - [Cookie di Sessione](#cookie-di-sessione)
  - [Potenziali Problemi con le Sessioni Lato Server](#potenziali-problemi-con-le-sessioni-lato-server)
  - [Metodi migliori per organizzare il recupero dei dati dell'utente lato Server](#metodi-migliori-per-organizzare-il-recupero-dei-dati-dellutente-lato-server)
  - [Un esempio completo di autenticazione basata su cookie e sessioni server side con Minimal API](#un-esempio-completo-di-autenticazione-basata-su-cookie-e-sessioni-server-side-con-minimal-api)
  - [Riferimenti Utili](#riferimenti-utili)

Questa lezione introduttiva fornisce una panoramica sull'utilizzo delle sessioni per gestire lo stato dell'utente nelle applicazioni ASP.NET Core, con un focus specifico sulle Minimal APIs. Comprendere come funzionano le sessioni è fondamentale per creare applicazioni web interattive che mantengano informazioni sull'utente tra le diverse richieste. Per una comprensione più approfondita, si consiglia di consultare la [documentazione ufficiale di ASP.NET Core sullo stato della sessione](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?#session-state).

## Cosa sono le Sessioni?

In un contesto web, una **sessione** rappresenta una sequenza di richieste HTTP provenienti dallo stesso utente durante un certo periodo di tempo. Le sessioni permettono di memorizzare informazioni relative a un utente specifico sul server e di associarle alle richieste successive provenienti da quel particolare utente. Questo è utile per memorizzare dati temporanei come preferenze dell'utente, contenuti del carrello degli acquisti o lo stato di un processo multi-step.

## Sessioni in ASP.NET Core Minimal APIs

ASP.NET Core semplifica l'utilizzo delle sessioni anche nelle Minimal APIs. Il framework fornisce un meccanismo per creare e gestire sessioni uniche per ogni utente che visita l'applicazione.

## Configurazione delle Sessioni

Per abilitare il supporto per le sessioni in una Minimal API, è necessario configurare alcuni servizi nel file `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// ... altri servizi ...

// Aggiunta del servizio di cache distribuita in memoria (per lo storage delle sessioni)
builder.Services.AddDistributedMemoryCache();

// Configurazione del middleware per le sessioni
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true; // Impedisce l'accesso al cookie tramite JavaScript
    options.Cookie.IsEssential = true; // Indica se il cookie di sessione è essenziale
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Durata di inattività della sessione
    options.Cookie.Name = ".MyApp.Session"; // Nome del cookie di sessione
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Trasmissione solo su HTTPS
    options.Cookie.SameSite = SameSiteMode.Strict; // Protezione contro CSRF
});

// ... costruzione dell'app ...

var app = builder.Build();

// ... altri middleware ...

// Aggiunta del middleware per l'utilizzo delle sessioni
app.UseSession();

// ... routing degli endpoint ...

app.Run();
```

Come si può vedere, la configurazione prevede due passaggi principali:

1. **Aggiungere un'implementazione di `IDistributedCache`:** Le sessioni in ASP.NET Core si basano su una cache distribuita per memorizzare i dati. L'esempio più semplice è `AddDistributedMemoryCache()`, che utilizza la memoria del server per lo storage. Per applicazioni più complesse o distribuite, è possibile utilizzare implementazioni come `AddStackExchangeRedisCache()` per Redis o altre opzioni (come spiegato nella [documentazione](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state)).
2. **Aggiungere e configurare il middleware della sessione:** Il metodo `AddSession()` configura le opzioni per la sessione, inclusa la configurazione del cookie di sessione. Il middleware `app.UseSession()` viene poi aggiunto alla pipeline di richiesta per abilitare la funzionalità di sessione per le richieste in entrata.

## Lavorare con i Dati di Sessione

Una volta configurate le sessioni, è possibile accedere ai dati di sessione all'interno degli endpoint tramite la proprietà `HttpContext.Session`. L'interfaccia `ISession` fornisce metodi per interagire con i dati di sessione:

* `SetString(string key, string value)`: Memorizza una stringa nella sessione.
* `GetString(string key)`: Recupera una stringa dalla sessione.
* `SetInt32(string key, int value)`: Memorizza un intero nella sessione.
* `GetInt32(string key)`: Recupera un intero dalla sessione.
* `Set<T>(string key, T value)` (tramite estensione): Memorizza un oggetto serializzabile in JSON nella sessione.
* `Get<T>(string key)` (tramite estensione): Recupera un oggetto serializzato da JSON dalla sessione.
* `Remove(string key)`: Rimuove un elemento dalla sessione.
* `Clear()`: Cancella tutti i dati dalla sessione.

**Esempio di utilizzo in un endpoint:**

```csharp
app.MapPost("/cart/add/{itemId:int}", (HttpContext ctx, int itemId) =>
{
    var cart = ctx.Session.Get<List<int>>("Cart") ?? new List<int>();
    cart.Add(itemId);
    ctx.Session.Set("Cart", cart);
    return Results.Ok($"Articolo {itemId} aggiunto al carrello.");
});

app.MapGet("/cart", (HttpContext ctx) =>
{
    var cart = ctx.Session.Get<List<int>>("Cart");
    return cart is not null ? Results.Ok(cart) : Results.Ok("Il carrello è vuoto.");
});
```

In questo esempio, un elenco di ID di articoli viene memorizzato e recuperato dalla sessione per rappresentare un carrello degli acquisti. Si noti l'uso di metodi di estensione `Get` e `Set` per gestire oggetti complessi (che internamente vengono serializzati in JSON). Le estensioni `SetObjectAsJson` e `GetObjectFromJson` nell'esempio di codice fornito precedentemente sono un modo comune per semplificare questa operazione.

## Cookie di Sessione

Per mantenere lo stato della sessione tra le richieste, ASP.NET Core utilizza un **cookie di sessione**. Questo cookie contiene un ID di sessione univoco che viene utilizzato dal server per identificare la sessione di un particolare utente. Le opzioni di configurazione del cookie di sessione (come `HttpOnly`, `SecurePolicy`, `SameSite`) sono definite nel blocco `AddSession` in `Program.cs`. È importante configurare queste opzioni in modo appropriato per garantire la sicurezza dell'applicazione.

## Potenziali Problemi con le Sessioni Lato Server

Sebbene le sessioni siano utili per memorizzare dati temporanei, è importante essere consapevoli dei potenziali problemi:

* **Dipendenza dal Server (con `DistributedMemoryCache`):** Come accennato in precedenza, l'utilizzo della cache in memoria (`DistributedMemoryCache`) significa che i dati di sessione sono locali al server che ha gestito la richiesta. In un ambiente con più server (load balancing), questo può portare a problemi se le richieste dello stesso utente vengono indirizzate a server diversi, poiché la sessione potrebbe non essere disponibile.
* **Scalabilità:** La memorizzazione di grandi quantità di dati di sessione per molti utenti può consumare risorse del server (memoria).
* **Persistenza:** I dati memorizzati in sessione sono tipicamente temporanei e vengono persi quando la sessione scade (in base all'`IdleTimeout`) o quando l'applicazione viene riavviata (nel caso di `DistributedMemoryCache`).

Per applicazioni più robuste e scalabili, specialmente in architetture distribuite, è consigliabile utilizzare una **cache distribuita esterna** come Redis o un database per lo storage delle sessioni. Questo garantisce che i dati di sessione siano accessibili a tutti i server e che persistano anche in caso di riavvio dell'applicazione.

## Metodi migliori per organizzare il recupero dei dati dell'utente lato Server

Sebbene le sessioni possano essere utilizzate per memorizzare temporaneamente alcune informazioni relative all'utente (come lo stato del carrello), **non sono il meccanismo migliore per memorizzare e recuperare informazioni persistenti sull'utente**, come il profilo utente, le impostazioni o i dati dell'account.

In architetture distribuite con un database condiviso, il modo più efficiente e scalabile per gestire i dati dell'utente è il seguente:

1. **Autenticazione:** L'utente si autentica (ad esempio, tramite cookie o token). Questo processo stabilisce l'identità dell'utente.
2. **Identificazione tramite Claims:** Durante l'autenticazione, vengono creati dei **claims** che identificano univocamente l'utente (ad esempio, un claim con il tipo `ClaimTypes.NameIdentifier` contenente l'ID utente dal database). Questi claims vengono memorizzati nel cookie di autenticazione o nel token.
3. **Recupero dei Dati dal Database:** Ad ogni richiesta successiva, il middleware di autenticazione legge il cookie o il token e stabilisce l'identità dell'utente (il `ClaimsPrincipal`). All'interno degli endpoint o dei servizi dell'applicazione, è possibile accedere all'ID utente univoco dai claims (`ctx.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value`). Questo ID può quindi essere utilizzato per **interrogare il database condiviso e recuperare le informazioni specifiche dell'utente** (profilo, impostazioni, ecc.).

**Vantaggi di questo approccio:**

* **Scalabilità:** I dati dell'utente sono memorizzati in un database centralizzato e scalabile.
* **Persistenza:** I dati persistono anche tra sessioni e riavvii dell'applicazione.
* **Efficienza:** Si recuperano solo i dati necessari per la richiesta corrente, evitando di sovraccaricare la sessione con informazioni statiche.
* **Consistenza:** Tutti i server dell'applicazione accedono alla stessa fonte di verità per i dati dell'utente.

**In sintesi, mentre le sessioni sono utili per lo stato temporaneo, per i dati persistenti dell'utente, è preferibile fare affidamento sul meccanismo di autenticazione per identificarlo e quindi recuperare le informazioni necessarie da un database condiviso.**

## Un esempio completo di autenticazione basata su cookie e sessioni server side con Minimal API

Nell'esempio [Cookie and Server Sessions](../../../../asp.net/api-samples/minimal-api/AuthenticationAuthorizationDemos/BasicExamples/cookie-and-server-sessions/BasicCookieDemo/) viene mostrato un progetto di Minimal API .NET che implementa uno schema di autenticazione e autorizzazione basato su cookie con in aggiunta un meccanismo di sessioni lato server per simulare un carrello degli acquisti.

La documentazione di questo esempio è riportata in [questa pagina](../../../../asp.net/api-samples/minimal-api/AuthenticationAuthorizationDemos/BasicExamples/cookie-and-server-sessions/docs/index.md).

## Riferimenti Utili

* [Session State in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state)

* [Distributed Caching in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed)

* [Claims-based authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/claims)

Comprendere l'utilizzo delle sessioni e le alternative per la gestione dei dati dell'utente è fondamentale per sviluppare applicazioni web ASP.NET Core Minimal API efficienti e scalabili.

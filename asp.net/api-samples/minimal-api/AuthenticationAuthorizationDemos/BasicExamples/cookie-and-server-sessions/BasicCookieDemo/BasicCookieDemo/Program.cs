using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies; //aggiunge il supporto per l'autenticazione tramite cookie
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
//aggiunge il servizio che permette ad OpenAPI di leggere i metadati delle API
builder.Services.AddEndpointsApiExplorer();
//configura il servizio OpenAPI
builder.Services.AddOpenApiDocument(config =>
    {
        config.Title = "Basic Cookie v1";
        config.DocumentName = "Basic Cookie API";
        config.Version = "v1";
    }
);


// Configurazione dell'autenticazione con cookie usando lo schema predefinito
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

// Add authorization services
builder.Services.AddAuthorization();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    //permette a Swagger (NSwag) di generare un file JSON con le specifiche delle API
    app.UseOpenApi();
    //permette di configurare l'interfaccia SwaggerUI (l'interfaccia grafica web di Swagger (NSwag) che permette di interagire con le API)
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "Basic Cookie v1";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });

    app.UseDeveloperExceptionPage();
}

// Middleware
app.UseAuthentication();
app.UseAuthorization();
//per l'utilizzo delle sessioni
app.UseSession();

// Add middleware to load session asynchronously for better performance
//https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?#load-session-state-asynchronously
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

// Endpoint di login
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

        var claims = new[] { new Claim(ClaimTypes.Name, model.Username) };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return Results.Ok("Login effettuato con successo");
    }
    return Results.Unauthorized();
});

// Endpoint per impostare un cookie altro cookie
//in questo esempio viene impostato un cookie con un identificativo univoco
//in realtà il cookie potrebbe essere un valore qualsiasi in base alle necessità
app.MapGet("/set-cookie", (HttpContext context) =>
{
    // Generazione di un identificativo univoco 
    var uniqueIdentifier = Guid.NewGuid().ToString();
    // Configurazione delle opzioni del cookie
    var cookieOptions = new CookieOptions
    {
        HttpOnly = true,                  // Impedisce l'accesso tramite JS
        Secure = true,                    // Trasmissione solo via HTTPS
        SameSite = SameSiteMode.Strict,   // Protegge da CSRF
        Expires = DateTimeOffset.Now.AddMinutes(30) // Cookie persistente per 30 minuti
    };
    context.Response.Cookies.Append("uniqueIdentifier", uniqueIdentifier, cookieOptions);
    return Results.Ok("Cookie impostato correttamente!");
});

//endpoint per leggere il cookie sicuro
app.MapGet("/read-cookie", (HttpContext context) =>
{
    var uniqueIdentifier = context.Request.Cookies["uniqueIdentifier"];
    return uniqueIdentifier != null
        ? Results.Ok($"Il valore del cookie è: {uniqueIdentifier}")
        : Results.NotFound("Cookie non trovato");
});

// Endpoint protetto
app.MapGet("/profile", (HttpContext ctx) =>
{
    // Verifica che l'utente sia autenticato, altrimenti restituisce 401 Unauthorized
    // Questo controllo è ridondante con RequireAuthorization, ma aiuta a chiarire il flusso
    if (ctx.User.Identity != null && ctx.User.Identity.IsAuthenticated)
        return Results.Ok($"Benvenuto, {ctx.User.Identity.Name}");
    return Results.Unauthorized();
}).RequireAuthorization();



// Endpoint protetto per aggiungere articoli al carrello
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

// Endpoint protetto per visualizzare il carrello
app.MapGet("/cart", (HttpContext ctx) =>
{
    // This check is redundant with RequireAuthorization, but helps clarify the flow
    if (ctx.User.Identity == null || !ctx.User.Identity.IsAuthenticated)
        return Results.Unauthorized(); // Explicitly return 401 Unauthorized

    // Ottiene il carrello dell'utente dalla sessione
    var cart = ctx.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
    return Results.Ok(cart);
}).RequireAuthorization(); // This applies the authorization policy

// Endpoint di logout
// app.MapPost("/logout", async (HttpContext ctx) =>
// {
//     await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
//     return Results.Ok("Logout effettuato con successo");
// });

// Endpoint di logout migliorato con reindirizzamento opzionale
app.MapPost("/logout", async (HttpContext ctx, [FromQuery] string? returnUrl) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    // Se è specificato un URL di ritorno e la richiesta proviene da un browser,
    // reindirizza l'utente
    if (!string.IsNullOrEmpty(returnUrl) &&
        ctx.Request.Headers.Accept.Any(h => h != null && h.Contains("text/html")))
    {
        // Assicurati che l'URL sia sicuro (evita open redirect vulnerabilities)
        if (Uri.IsWellFormedUriString(returnUrl, UriKind.Relative) ||
            returnUrl.StartsWith(ctx.Request.Scheme + "://" + ctx.Request.Host))
        {
            return Results.Redirect(returnUrl);
        }
    }

    return Results.Ok("Logout effettuato con successo");
});

app.Run();

public class LoginModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// Modello per gli articoli del carrello
public class CartItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
}

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


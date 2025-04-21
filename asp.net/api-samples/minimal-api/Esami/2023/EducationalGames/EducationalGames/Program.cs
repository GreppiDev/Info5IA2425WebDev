using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth; 
using Microsoft.AspNetCore.DataProtection; 
using Microsoft.AspNetCore.HttpOverrides; 
using Microsoft.AspNetCore.Identity; 
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives; 
using EducationalGames.Data;
using EducationalGames.Endpoints;
using EducationalGames.Middlewares;
using EducationalGames.Models;
using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.Mvc.Routing; 
using Microsoft.AspNetCore.Mvc.Abstractions;
using EducationalGames.Auth;
using System.Net;
var builder = WebApplication.CreateBuilder(args);

// Configura DataProtection esplicitamente
var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys");
Directory.CreateDirectory(keysFolder);
builder.Services.AddDataProtection()
    .SetApplicationName("EducationalGames")
    .PersistKeysToFileSystem(new DirectoryInfo(keysFolder))
    .SetDefaultKeyLifetime(TimeSpan.FromDays(30));

// *** Configurazione CORS (da appsettings) ***
var corsSettings = builder.Configuration.GetSection("CorsSettings"); 
var tunnelOrProxyOrigin = corsSettings["TunnelOrProxyOrigin"]; // Legge l'URL del tunnel/proxy
var allowedLocalOrigins = corsSettings.GetSection("AllowedLocalOrigins").Get<string[]>(); // Legge l'array di URL locali

var AllowTunnelPolicy = "_allowTunnelPolicy"; 
var AllowLocalhostPolicy = "_allowLocalhostPolicy"; 

builder.Services.AddCors(options =>
{
    // Policy per Tunnel/Proxy (aggiunta solo se l'URL è configurato)
    if (!string.IsNullOrEmpty(tunnelOrProxyOrigin) && Uri.TryCreate(tunnelOrProxyOrigin, UriKind.Absolute, out var tunnelUri))
    {
        options.AddPolicy(name: AllowTunnelPolicy, policy =>
        {
            policy.WithOrigins(tunnelUri.GetLeftPart(UriPartial.Authority)) // Usa solo scheme://host:port
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    }

    // Policy per Localhost (usa la lista letta da appsettings)
    if (allowedLocalOrigins != null && allowedLocalOrigins.Length > 0)
    {
        options.AddPolicy(name: AllowLocalhostPolicy, policy =>
        {
            policy.WithOrigins([.. allowedLocalOrigins.Select(u => u.TrimEnd('/'))]) // Pulisce e usa l'array
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    }
    else
    {
        // Fallback o log se non configurato? Per ora non aggiunge la policy localhost.
        Console.WriteLine("WARN: CorsSettings:AllowedLocalOrigins non trovato o vuoto in appsettings.json");
    }
});
// *** FINE Configurazione CORS ***

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "Educational Games v1";
    config.DocumentName = "Educational Games API";
    config.Version = "v1";
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}


// Configura le opzioni di System.Text.Json per gestire gli enum come stringhe
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    // Aggiunge il convertitore che permette di leggere/scrivere enum come stringhe
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());

    // Opzionale: Rende i nomi delle proprietà JSON case-insensitive durante la deserializzazione
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

// Se si stesse usando AddControllers() invece di Minimal API, la configurazione sarebbe simile:
// builder.Services.AddControllers().AddJsonOptions(options => {
//     options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
// });

// --- Configurazione DbContext ---
var connectionString = builder.Configuration.GetConnectionString("EducationalGamesConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'EducationalGamesConnection' not found.");
}
var serverVersion = ServerVersion.AutoDetect(connectionString);
builder.Services.AddDbContext<AppDbContext>(
    opt => opt.UseMySql(connectionString, serverVersion)
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()) // Log sensibili solo in DEV
        .EnableDetailedErrors(builder.Environment.IsDevelopment())      // Errori dettagliati solo in DEV
);

// --- Configurazione Autenticazione ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; 
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{ // Configurazione Cookie
    options.Cookie.Name = ".AspNetCore.Authentication.EducationalGames";
    options.Cookie.HttpOnly = true;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
    options.Cookie.SameSite = builder.Environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.Strict;
    options.LoginPath = "/login-required";
    options.AccessDeniedPath = "/access-denied";

    // Gestione personalizzata redirect per API vs HTML
    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(">>> OnRedirectToLogin triggered for path: {Path}", context.Request.Path);
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                logger.LogWarning(">>> API path detected. Setting status code 401 for path: {Path}", context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            }
            else { logger.LogWarning(">>> Non-API path detected. Redirecting to: {RedirectUri}", context.RedirectUri); context.Response.Redirect(context.RedirectUri); }
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(">>> OnRedirectToAccessDenied triggered for path: {Path}", context.Request.Path);
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                logger.LogWarning(">>> API path detected. Setting status code 403 for path: {Path}", context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
            }
            else { logger.LogWarning(">>> Non-API path detected. Redirecting to: {RedirectUri}", context.RedirectUri); context.Response.Redirect(context.RedirectUri); }
            return Task.CompletedTask;
        }
    };
})
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{ // Configurazione Google
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId not configured.");
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret not configured.");
    options.CallbackPath = "/signin-google"; // Il middleware ascolta qui
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.SaveTokens = true; // Salva i token per eventuale uso futuro

    
    options.Events = new OAuthEvents
    {
        //Logica Callback in seguito alla verifica delle credenziali di Google spostata nell'evento OnTicketReceived
        //OnTicketReceived : Scatta quando il ticket di autenticazione è stato ricevuto e validato con successo da Google.
        OnTicketReceived = GoogleAuthEvents.HandleTicketReceived,
        //OnRemoteFailure: Scatta quando si verifica un errore durante la comunicazione con il provider esterno (Google) 
        //o durante l'elaborazione della sua risposta (ad esempio, se Google restituisce un errore, o se si verificano problemi di rete,...
        OnRemoteFailure = GoogleAuthEvents.HandleRemoteFailure,
        //OnAccessDenied: Scatta specificamente se l'utente, sulla pagina di consenso di Google, nega esplicitamente l'accesso alla tua applicazione.
        OnAccessDenied = GoogleAuthEvents.HandleAccessDenied   
    };
});

// --- Configurazione Autorizzazione con Policy ---
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    .AddPolicy("DocenteOnly", policy => policy.RequireRole("Docente"))
    .AddPolicy("AdminOrDocente", policy => policy.RequireRole("Admin", "Docente"))
    .AddPolicy("RegisteredUsers", policy => policy.RequireAuthenticatedUser()); // Richiede solo utente autenticato

// Aggiungi PasswordHasher come servizio
builder.Services.AddScoped<PasswordHasher<Utente>>();

// --- Configurazione Forwarded Headers Options ---
// Lasciamo configurato nel caso si usi un tunnel o proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    
    options.KnownNetworks.Clear();// NON USARE .Clear() in produzione!
    options.KnownProxies.Clear();// NON USARE .Clear() in produzione!
    // Aggiungi qui eventuali reti/proxy noti se necessario
    // Esempio: Aggiungi gli IP specifici dei tuoi reverse proxy/load balancer fidati
    //options.KnownProxies.Add(IPAddress.Parse("10.0.5.23")); // IP del tuo proxy 1
    //options.KnownProxies.Add(IPAddress.Parse("10.0.5.24")); // IP del tuo proxy 2

    // Oppure, se i proxy sono in una subnet specifica:
    //options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Parse("10.0.5.0"), 24)); // Esempio: Subnet 10.0.5.0/24
    // ...
});
// --- FINE Configurazione Forwarded Headers Options ---

var app = builder.Build();

// --- APPLICA MIGRAZIONI E SEEDING ADMIN ALL'AVVIO (Refactored) ---
// Crea uno scope per risolvere i servizi necessari all'inizializzatore
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    // Chiama il metodo statico dell'inizializzatore passando i servizi e l'ambiente
    await DatabaseInitializer.InitializeAndSeedAsync(services, app.Environment);
}
// --- FINE MIGRAZIONI E SEEDING ---

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "Educational Games v1";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();

}

// Middleware Forwarded Headers (se si usa tunnel/proxy)
app.UseForwardedHeaders();
app.UseHttpsRedirection();

// --- Applica CORS (usando i nomi delle policy) ---
// Applica la policy del tunnel solo se è stata definita (cioè se l'URL era presente)
if (!string.IsNullOrEmpty(tunnelOrProxyOrigin) && Uri.TryCreate(tunnelOrProxyOrigin, UriKind.Absolute, out _))
{
    app.UseCors(AllowTunnelPolicy);
}
// Applica la policy localhost solo se è stata definita
if (allowedLocalOrigins != null && allowedLocalOrigins.Length > 0)
{
    app.UseCors(AllowLocalhostPolicy);
}
// --- FINE Applica CORS ---

// Middleware per file statici
app.UseDefaultFiles();
app.UseStaticFiles();

// Middleware Autenticazione/Autorizzazione
app.UseAuthentication();
app.UseAuthorization();

// Middleware per gestire gli errori di stato delle API
// Riepilogo della Logica Risultante:
// Richiesta API(/api/...) con Errore(401/403/404): 
//  Il StatusCodeMiddleware intercetta l'errore e restituisce una risposta JSON standardizzata.
// Richiesta NON API (/pagina-protetta) che richiede Login (401): 
//  CookieAuthenticationEvents reindirizza a /login-required, che poi reindirizza a /login-page.html?ReturnUrl=....
// Richiesta NON API (/area-admin) senza Permessi (403): 
//  CookieAuthenticationEvents reindirizza a /access-denied, che poi reindirizza a /access-denied.html?ReturnUrl=....
// Richiesta NON API (/pagina-inesistente.html) non trovata (404):
//   UseStatusCodePagesWithRedirects reindirizza a /not-found.html.

app.UseMiddleware<StatusCodeMiddleware>();

// Reindirizza a /not-found.html per errori 404 che non sono stati gestiti
// e non sono richieste API (perché il middleware StatusCodeMiddleware
// intercetterebbe gli errori API prima che questo venga eseguito completamente)

// NOTA: Questo catturerà anche richieste a file statici non esistenti.
app.UseStatusCodePagesWithRedirects("/not-found.html");

// Map API endpoints
app.MapGroup("/api/account")
   .WithTags("Account")
   .MapAccountEndpoints();


// Endpoint di sfida a Google
app.MapGet("/login-google", async (HttpContext httpContext, [FromQuery] string? returnUrl) =>
{
    // Logica per validare returnUrl e costruire target...
    var actionContext = new ActionContext(httpContext, httpContext.GetRouteData(), new ActionDescriptor());
    var urlHelper = new UrlHelper(actionContext);
    var target = "/";
    if (!string.IsNullOrEmpty(returnUrl) && urlHelper.IsLocalUrl(returnUrl)) { target = returnUrl; }

    // Proprietà passate alla sfida. 
    var props = new AuthenticationProperties
    {
        Items = { ["returnUrl"] = target } // Passiamo solo la destinazione finale
    };

    // Esegui la sfida esplicita
    await httpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, props);

}).AllowAnonymous();


// Endpoint per gestire i redirect a pagine HTML specifiche
// Questi endpoint vengono chiamati dal middleware dei cookie quando rileva
// una richiesta non API che richiede login o non ha i permessi.
app.MapGet("/login-required", (HttpContext context) =>
{
    // Legge il parametro ReturnUrl aggiunto automaticamente dal middleware
    context.Request.Query.TryGetValue("ReturnUrl", out StringValues returnUrlSv);
    var returnUrl = returnUrlSv.FirstOrDefault();

    // Costruisce l'URL per la pagina di login HTML
    var redirectUrl = "/login-page.html";
    if (!string.IsNullOrEmpty(returnUrl))
    {
        // Aggiunge il ReturnUrl alla pagina di login, così può reindirizzare dopo il login
        redirectUrl += $"?ReturnUrl={Uri.EscapeDataString(returnUrl)}";
    }
    // Esegue il redirect alla pagina di login HTML
    return Results.Redirect(redirectUrl);

}).AllowAnonymous();

app.MapGet("/access-denied", (HttpContext context) =>
{
    // Legge il parametro ReturnUrl aggiunto automaticamente dal middleware
    context.Request.Query.TryGetValue("ReturnUrl", out StringValues returnUrlSv);
    var returnUrl = returnUrlSv.FirstOrDefault();

    // Costruisce l'URL per la pagina di accesso negato HTML
    var redirectUrl = "/access-denied.html";
    if (!string.IsNullOrEmpty(returnUrl))
    {
        // Aggiunge il ReturnUrl alla pagina di accesso negato (utile per logging o messaggi)
        redirectUrl += $"?ReturnUrl={Uri.EscapeDataString(returnUrl)}";
    }
    // Esegue il redirect alla pagina di accesso negato HTML
    return Results.Redirect(redirectUrl);

}).AllowAnonymous();

// Endpoint di fallback per errori generici
app.MapGet("/error", () => Results.Problem("Si è verificato un errore interno.")).AllowAnonymous();


app.Run();
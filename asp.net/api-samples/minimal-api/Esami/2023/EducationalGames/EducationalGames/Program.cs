
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EducationalGames.Data;
using EducationalGames.Endpoints;
using EducationalGames.Middlewares;
using EducationalGames.Models;
using EducationalGames.Auth;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Identity.UI.Services;
using EducationalGames.Services;
var builder = WebApplication.CreateBuilder(args);

// Configurazione del sistema DataProtection per supportare più istanze
var dataProtectionSection = builder.Configuration.GetSection("DataProtection");

// Legge il percorso configurato o usa un default se mancante/vuoto
string? configuredKeysFolder = dataProtectionSection["KeysFolder"];
string defaultProductionPath = "/app/shared/keys"; // Default se non specificato

// Determina il percorso effettivo per le chiavi in produzione
string productionKeysPath = string.IsNullOrEmpty(configuredKeysFolder)
    ? defaultProductionPath
    : configuredKeysFolder;

string keysPath = builder.Environment.IsDevelopment()
    ? Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys")
    : productionKeysPath; // Usa il valore da appsettings (o il default se mancava)

// Assicurarsi che la directory esista (particolarmente utile in sviluppo)
if (!Directory.Exists(keysPath))
{
    try
    {
        Directory.CreateDirectory(keysPath);
    }
    catch (Exception ex)
    {
        // Logga un errore se non è possibile creare la directory,
        // specialmente rilevante per il percorso di produzione.
        // Potrebbe indicare un problema di permessi o configurazione.
        var loggerFactoryTemp = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Error));
        var loggerTemp = loggerFactoryTemp.CreateLogger("DataProtectionSetup");
        loggerTemp.LogError(ex, "Impossibile creare la directory per le chiavi DataProtection: {KeysPath}", keysPath);
        // Potrebbe essere utile sollevare l'eccezione o gestire l'errore in modo appropriato con un throw
    }
}

var dataProtectionBuilder = builder.Services.AddDataProtection()
    .SetApplicationName("EducationalGames") // Importante: stesso nome su tutte le istanze
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath)) // Usa il percorso determinato
    .SetDefaultKeyLifetime(TimeSpan.FromDays(
        dataProtectionSection.GetValue<int?>("KeyLifetime") ?? 30)); // Usa GetValue<int?> per sicurezza

// Applica l'impostazione AutoGenerateKeys
// GetValue<bool> restituisce false se la chiave non è trovata o non è un booleano valido.
// Dato che hai true in appsettings, questo leggerà true.
if (!dataProtectionSection.GetValue<bool>("AutoGenerateKeys"))
{
    dataProtectionBuilder.DisableAutomaticKeyGeneration();

    // Crea un logger temporaneo per il logging durante la configurazione
    using var loggerFactory = LoggerFactory.Create(logging =>
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Warning);
    });
    var logger = loggerFactory.CreateLogger("DataProtectionSetup");

    logger.LogWarning("DataProtection:AutoGenerateKeys è impostato a false. La generazione automatica delle chiavi è disabilitata. " +
                   "Assicurarsi che le chiavi di protezione dati siano gestite manualmente o distribuite.");
}
else
{
    // Log opzionale per confermare che la generazione automatica è attiva (se desiderato)
    // using var loggerFactory = LoggerFactory.Create(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Information));
    // var logger = loggerFactory.CreateLogger("DataProtectionSetup");
    // logger.LogInformation("DataProtection:AutoGenerateKeys è impostato a true. La generazione automatica delle chiavi è abilitata.");
}

// Configure Email Settings and Service
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();
// Aggiungi l'adattatore per IEmailSender (Identity UI) se necessario
builder.Services.AddTransient<IEmailSender, EmailSenderAdapter>();


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

// --- Configurazione Autenticazione (Cookie + Google + Microsoft) ---
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
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsJsonAsync(new
                {
                    status = 401,
                    title = "Unauthorized",
                    detail = "Devi eseguire il login per accedere a questa risorsa.",
                    path = context.Request.Path,
                    timestamp = DateTime.UtcNow
                });
            }
            else
            {
                logger.LogWarning(">>> Non-API path detected. Redirecting to: {RedirectUri}", context.RedirectUri);
                context.Response.Redirect(context.RedirectUri);
            }
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
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsJsonAsync(new
                {
                    status = 403,
                    title = "Forbidden",
                    detail = "Non hai i permessi necessari per visualizzare questa risorsa.",
                    path = context.Request.Path,
                    timestamp = DateTime.UtcNow
                });
            }
            else
            {
                logger.LogWarning(">>> Non-API path detected. Redirecting to: {RedirectUri}", context.RedirectUri);
                context.Response.Redirect(context.RedirectUri);
            }
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
})
.AddMicrosoftAccount(MicrosoftAccountDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"] ?? throw new InvalidOperationException("Microsoft ClientId not configured.");
    options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"] ?? throw new InvalidOperationException("Microsoft ClientSecret not configured.");
    options.CallbackPath = "/signin-microsoft";

    // --- Configura Endpoint Specifici del Tenant ---
    // Leggi il Tenant ID dalla configurazione
    var tenantId = builder.Configuration["Authentication:Microsoft:TenantId"];
    if (string.IsNullOrEmpty(tenantId))
    {
        // È FONDAMENTALE per app single-tenant
        throw new InvalidOperationException("Microsoft TenantId not configured for single-tenant application.");
    }
    else
    {
        // Costruisci gli URL degli endpoint specifici per il tenant
        // e assegnali alle proprietà corrette delle opzioni.
        options.AuthorizationEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize";
        options.TokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
    }
    // --- FINE Configurazione Endpoint ---

    // Usa i metodi statici dalla classe helper MicrosoftAuthEvents
    options.Events = new OAuthEvents
    {
        OnTicketReceived = MicrosoftAuthEvents.HandleTicketReceived,
        OnRemoteFailure = MicrosoftAuthEvents.HandleRemoteFailure,
        OnAccessDenied = MicrosoftAuthEvents.HandleAccessDenied
    };
});
// --- FINE Configurazione Autenticazione ---

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

// Validate required configuration on startup

builder.Services.AddOptions<EmailSettings>()
    .Bind(builder.Configuration.GetSection("EmailSettings"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<AdminCredentialsOptions>()
    .Bind(builder.Configuration.GetSection("DefaultAdminCredentials"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<DataProtectionOptions>()
    .Bind(builder.Configuration.GetSection("DataProtection"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpContextAccessor();

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
   .MapAccountEndpoints(app.Services.GetRequiredService<ILoggerFactory>())
   .WithTags("Account");

// Map Game Endpoints
app.MapGroup("/api") // Gruppo API generico
   .WithTags("Games & Topics") // Tag per Swagger
   .MapGameEndpoints(app.Services.GetRequiredService<ILoggerFactory>());

app.MapGroup("/api") // Gruppo API generico
   .WithTags("Classi")
   .MapClassiEndpoints(app.Services.GetRequiredService<ILoggerFactory>());
   
app.MapGroup("/api")
   .WithTags("Iscrizioni") // Tag per Swagger
   .MapIscrizioniEndpoints(app.Services.GetRequiredService<ILoggerFactory>());

app.MapGroup("/api")
   .WithTags("Progressi") // Tag per Swagger
   .MapProgressoEndpoints(app.Services.GetRequiredService<ILoggerFactory>());
   
app.MapGroup("/api")
   .WithTags("Classifiche") // Tag per Swagger
   .MapClassificheEndpoints(app.Services.GetRequiredService<ILoggerFactory>());

app.MapGroup("/api") 
   .WithTags("Dashboard") // Tag per Swagger
   .MapDashboardEndpoints(app.Services.GetRequiredService<ILoggerFactory>());

app.MapGroup("/api/admin") // Prefisso specifico per API admin
   .WithTags("Admin") // Tag per Swagger
   .MapAdminEndpoints(app.Services.GetRequiredService<ILoggerFactory>());

//Map pages endpoints
app.MapGroup("")
    .WithTags("Main")
    .MapPageEndpoints();
    

app.Run();
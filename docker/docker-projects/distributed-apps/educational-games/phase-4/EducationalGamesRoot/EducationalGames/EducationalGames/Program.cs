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
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
var builder = WebApplication.CreateBuilder(args);

// Configurazione del sistema DataProtection per supportare più istanze
var dataProtectionSection = builder.Configuration.GetSection("DataProtection");

// Determina il percorso per le chiavi di Data Protection
string keysPath;
// Variabile d'ambiente per il percorso delle chiavi (ha la priorità in non-sviluppo)
string? dataProtectionKeysPathEnvVar = Environment.GetEnvironmentVariable("DATA_PROTECTION_KEYS_PATH");
// Percorso da appsettings.json
string? configuredKeysFolderAppsettings = dataProtectionSection["KeysFolder"];

if (builder.Environment.IsDevelopment())
{
    // In sviluppo, usa una cartella locale nel progetto
    keysPath = Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys");
}
else // Produzione o ambienti simili (Docker Compose non-dev, Azure Container Apps)
{
    if (!string.IsNullOrEmpty(dataProtectionKeysPathEnvVar))
    {
        // Priorità 1: Variabile d'ambiente (per ACA o Docker Compose)
        keysPath = dataProtectionKeysPathEnvVar;
    }
    else if (!string.IsNullOrEmpty(configuredKeysFolderAppsettings))
    {
        // Priorità 2: Configurazione da appsettings.json (se presente)
        keysPath = configuredKeysFolderAppsettings;
    }
    else
    {
        // Priorità 3: Default per produzione se nient'altro è specificato.
        // Per ACA, questo path sarà /mnt/dataprotectionkeys (configurato tramite volume mount e env var)
        // Per Docker Compose, sarà il path del volume condiviso (configurato tramite volume mount e env var)
        keysPath = "/app/shared/default_keys_production"; // Un default generico se non specificato altrimenti
        // È FONDAMENTALE che questo path sia un volume condiviso in ambienti multi-istanza.
    }
}

// Assicurarsi che la directory esista (particolarmente utile in sviluppo o se il volume non è auto-creato)
if (!Directory.Exists(keysPath))
{
    try
    {
        Directory.CreateDirectory(keysPath);
        Console.WriteLine($"DataProtection keys directory created at: {keysPath}");
    }
    catch (Exception ex)
    {
        // Logga un errore se non è possibile creare la directory.
        // Questo è critico per il corretto funzionamento in produzione.
        var loggerFactoryTemp = LoggerFactory.Create(b => b.AddConsole().SetMinimumLevel(LogLevel.Error));
        var loggerTemp = loggerFactoryTemp.CreateLogger("DataProtectionSetup");
        loggerTemp.LogError(ex, "CRITICAL: Impossibile creare la directory per le chiavi DataProtection: {KeysPath}. L'autenticazione e altre funzionalità di sicurezza potrebbero non funzionare correttamente in un ambiente multi-istanza.", keysPath);
        // Considerare di terminare l'applicazione se le chiavi non possono essere persistite correttamente in produzione.
        // throw new InvalidOperationException($"Impossibile creare la directory per le chiavi DataProtection: {keysPath}", ex);
    }
}
else
{
    Console.WriteLine($"DataProtection keys will be persisted to: {keysPath}");
}


var dataProtectionBuilder = builder.Services.AddDataProtection()
    .SetApplicationName("EducationalGames") // Importante: stesso nome su tutte le istanze
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath)) // Usa il percorso determinato
    .SetDefaultKeyLifetime(TimeSpan.FromDays(
        dataProtectionSection.GetValue<int?>("KeyLifetime") ?? 30));

if (!dataProtectionSection.GetValue<bool>("AutoGenerateKeys")) // Default a true se non specificato
{
    dataProtectionBuilder.DisableAutomaticKeyGeneration();
    using var loggerFactory = LoggerFactory.Create(logging =>
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Warning);
    });
    var logger = loggerFactory.CreateLogger("DataProtectionSetup");
    logger.LogWarning("DataProtection:AutoGenerateKeys è impostato a false. La generazione automatica delle chiavi è disabilitata. " +
                      "Assicurarsi che le chiavi di protezione dati siano gestite manualmente o distribuite.");
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

// --- Configurazione OpenTelemetry ---
var serviceName = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME") ?? "EducationalGames";
var serviceVersion = builder.Configuration.GetValue<string>("OTEL_SERVICE_VERSION") ?? "1.0.0";
var otlpEndpoint = builder.Configuration.GetValue<string>("OTEL_EXPORTER_OTLP_ENDPOINT") ?? "http://localhost:4317";
var otlpProtocol = builder.Configuration.GetValue<string>("OTEL_EXPORTER_OTLP_PROTOCOL")?.ToLower() == "http"
    ? OtlpExportProtocol.HttpProtobuf : OtlpExportProtocol.Grpc;
var metricExportInterval = builder.Configuration.GetValue<int>("OTEL_METRIC_EXPORT_INTERVAL", 5000);

// Parse resource attributes from configuration
var resourceAttributes = new Dictionary<string, object>
{
    ["deployment.environment"] = builder.Environment.EnvironmentName,
    ["service.instance.id"] = Environment.MachineName,
    ["service.namespace"] = "educationalgames",
    ["host.name"] = Environment.MachineName
};

// Add custom resource attributes from OTEL_RESOURCE_ATTRIBUTES if present
var customResourceAttributes = builder.Configuration.GetValue<string>("OTEL_RESOURCE_ATTRIBUTES");
if (!string.IsNullOrEmpty(customResourceAttributes))
{
    var attributes = customResourceAttributes.Split(',');
    foreach (var attr in attributes)
    {
        var keyValue = attr.Split('=');
        if (keyValue.Length == 2)
        {
            resourceAttributes[keyValue[0].Trim()] = keyValue[1].Trim();
        }
    }
}

// --- LOGGING CON OPENTELEMETRY ---
// Pulisce i provider di logging predefiniti (come Console, Debug) per evitare log duplicati.
// L'exporter di OpenTelemetry si occuperà di scrivere sulla console se configurato.
builder.Logging.ClearProviders();

builder.Logging.AddOpenTelemetry(logging =>
{
    var resourceBuilder = ResourceBuilder.CreateDefault()
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
        .AddAttributes(resourceAttributes);

    logging.SetResourceBuilder(resourceBuilder);
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
    logging.ParseStateValues = true;

    // Configura il processore per l'esportazione in batch verso OTLP
    logging.AddProcessor(new BatchLogRecordExportProcessor(new OtlpLogExporter(new OtlpExporterOptions
    {
        Endpoint = new Uri(otlpEndpoint),
        Protocol = otlpProtocol,
        Headers = "x-source=educationalgames" // Header personalizzato per identificare la fonte
    })));

    // Aggiunge anche l'exporter per la console per il debug locale
    if (builder.Environment.IsDevelopment())
    {
        logging.AddConsoleExporter();
    }
});

// Configurazione dei livelli di log
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Information);
builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);

// --- FINE LOGGING CON OPENTELEMETRY ---


builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
        .AddAttributes(resourceAttributes))
        .WithTracing(tracerProvider => 
            tracerProvider
            // NUOVO: Rende esplicita la volontà di campionare tutto a livello di applicazione
            // per delegare la decisione finale al Collector (Tail-Based Sampling).
            .SetSampler(new ParentBasedSampler(new AlwaysOnSampler()))
            .AddSource($"{serviceName}.Demo") // Activity source personalizzato dinamico
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.EnrichWithHttpRequest = (activity, httpRequest) =>
            {
                activity.SetTag("http.request.user_agent", httpRequest.Headers.UserAgent.ToString());
                activity.SetTag("http.request.content_type", httpRequest.ContentType);
                if (httpRequest.Headers.TryGetValue("X-Real-IP", out Microsoft.Extensions.Primitives.StringValues value))
                {
                    activity.SetTag("http.request.real_ip", value.ToString());
                }
            };
                options.EnrichWithHttpResponse = (activity, httpResponse) =>
                {
                    activity.SetTag("http.response.content_type", httpResponse.ContentType);
                    activity.SetTag("http.response.content_length", httpResponse.ContentLength);
                };
                options.Filter = httpContext =>
                {
                    var path = httpContext.Request.Path.Value;
                    // Escludere le richieste per health check e metriche
                    return !path?.StartsWith("/health") == true &&
                           !path?.StartsWith("/metrics") == true &&
                           !path?.StartsWith("/favicon.ico") == true;
                };
            })
            // La versione seguente di AddAspNetCoreInstrumentation aggiunge l'instrumentation ASP.NET Core con filtro per endpoint specifici
            // .AddAspNetCoreInstrumentation(options =>
            // {
            //     options.RecordException = true;

            //     // DEFINIAMO GLI ENDPOINT SPECIFICI DA TRACCIARE
            //     var endpointsToTrace = new[]
            //     {
            //         "/api/telemetry/test",
            //         "/api/account", //Ad esempio: tracciare tutto ciò che è sotto /api/account/
            //         "/api/iscrizioni" //Ad esempio tracciare le iscrizioni
            //     };

            //     options.Filter = httpContext =>
            //     {
            //         var path = httpContext.Request.Path.Value;
            //         if (string.IsNullOrEmpty(path))
            //         {
            //             return false; // Non tracciare richieste senza path
            //         }

            //         // Restituisce 'true' (e quindi traccia la richiesta)
            //         // SOLO SE il path inizia con uno degli endpoint che abbiamo definito.
            //         return endpointsToTrace.Any(endpoint => path.StartsWith(endpoint, StringComparison.OrdinalIgnoreCase));
            //     };
            // })
            .AddHttpClientInstrumentation(options =>
            {
                options.RecordException = true;
                options.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) =>
                {
                    activity.SetTag("http.client.request.content_type", httpRequestMessage.Content?.Headers?.ContentType?.ToString());
                };
                options.EnrichWithHttpResponseMessage = (activity, httpResponseMessage) =>
                {
                    activity.SetTag("http.client.response.content_type", httpResponseMessage.Content?.Headers?.ContentType?.ToString());
                };
            })
            .AddEntityFrameworkCoreInstrumentation(options =>
            {
                options.SetDbStatementForText = true;
                options.SetDbStatementForStoredProcedure = true;
                options.EnrichWithIDbCommand = (activity, command) =>
                {
                    activity.SetTag("db.command.type", command.CommandType.ToString());
                    activity.SetTag("db.command.timeout", command.CommandTimeout.ToString());
                };
            })
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(otlpEndpoint);
                options.Protocol = otlpProtocol;
                options.Headers = "x-source=educationalgames";
            })
            .AddConsoleExporter() // Solo per debugging
        )
        .WithMetrics(meterProvider => 
            meterProvider
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            // Aggiungi metriche personalizzate
            .AddMeter($"{serviceName}.Metrics")
            .AddPrometheusExporter()
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(otlpEndpoint);
                options.Protocol = otlpProtocol;
                options.ExportProcessorType = ExportProcessorType.Batch;
                options.Headers = "x-source=educationalgames";
                options.BatchExportProcessorOptions = new BatchExportProcessorOptions<System.Diagnostics.Activity>
                {
                    ExporterTimeoutMilliseconds = metricExportInterval
                };
            })
            .AddConsoleExporter() // Utile per debugging
        );

// --- FINE Configurazione OpenTelemetry ---

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

// Registrazione servizio demo per telemetria
builder.Services.AddScoped<EducationalGames.Services.TelemetryDemoService>();

// --- Configurazione Forwarded Headers Options ---
// Lasciamo configurato nel caso si usi un tunnel o proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;


    // Quando si esegue dietro un reverse proxy fidato (come Nginx nel tuo Docker Compose),
    // è necessario indicare all'applicazione di fidarsi degli header inviati da questo proxy.
    // Nginx e la tua webapp sono sulla stessa rete Docker.
    // Svuotare KnownProxies e KnownNetworks indica all'applicazione di
    // elaborare gli header X-Forwarded-* da qualsiasi proxy.
    // Questo è generalmente sicuro in una configurazione Docker Compose dove
    // il container della webapp non è esposto direttamente all'esterno
    // e solo Nginx può raggiungerlo sulla rete Docker interna.

    options.KnownNetworks.Clear();// NON USARE .Clear() in produzione se non si è sicuri di non avere reti fidate!
    options.KnownProxies.Clear();// NON USARE .Clear() in produzione se non si è sicuri di non avere proxy fidati!
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

// Map OpenTelemetry/Prometheus metrics endpoint
app.MapPrometheusScrapingEndpoint();

// Endpoint per health check (per monitoraggio)
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
   .WithTags("Health")
   .WithOpenApi();

// Endpoint di test per la telemetria
app.MapPost("/api/telemetry/test", async (
    string? testData,
    TelemetryDemoService telemetryService) =>
{
    try
    {
        var result = await telemetryService.ProcessDataAsync(testData ?? "sample_data");
        return Results.Ok(new
        {
            success = true,
            result = result,
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new
        {
            success = false,
            error = ex.Message,
            timestamp = DateTime.UtcNow
        });
    }
})
.WithTags("Telemetry")
.WithOpenApi()
.WithSummary("Test endpoint per dimostrare OpenTelemetry tracing");

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
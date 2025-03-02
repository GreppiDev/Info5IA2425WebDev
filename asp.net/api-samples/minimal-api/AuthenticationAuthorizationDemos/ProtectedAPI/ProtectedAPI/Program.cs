using Microsoft.EntityFrameworkCore;
using ProtectedAPI.Data;
using ProtectedAPI.Endpoints;
using ProtectedAPI.Model;
using ProtectedAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.OpenApi.Models;
using NSwag.Generation.Processors.Security;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies; // Aggiunto per CookieAuthenticationEvents
using Microsoft.AspNetCore.DataProtection; // Aggiunto per DataProtection

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// Configure OpenAPI/Swagger
builder.Services.AddOpenApiDocument(config =>
{
	config.Title = "ProtectedAPI v1";
	config.DocumentName = "v1";
	config.Version = "v1";

	// Utilizzo dello schema Http per la sicurezza JWT
	config.AddSecurity("Bearer", new NSwag.OpenApiSecurityScheme
	{
		Type = NSwag.OpenApiSecuritySchemeType.Http,
		Scheme = "Bearer",
		BearerFormat = "JWT",
		Description = "Inserisci il token JWT (senza il prefisso 'Bearer')"
	});

	// Utilizza l'OperationProcessor standard per aggiungere i requisiti di sicurezza
	config.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("Bearer"));
});

if (builder.Environment.IsDevelopment())
{
	builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

var connectionString = builder.Configuration.GetConnectionString("ProtectedAPIConnection");
var serverVersion = ServerVersion.AutoDetect(connectionString);

builder.Services.AddDbContext<AppDbContext>(
	opt => opt.UseMySql(connectionString, serverVersion)
	.LogTo(Console.WriteLine, LogLevel.Information)
	.EnableSensitiveDataLogging()
	.EnableDetailedErrors()
);

// Configure Identity settings
var identitySettings = builder.Configuration.GetSection("IdentitySettings").Get<IdentitySettings>();

// Add Identity services with API endpoints and configure password rules
var identityBuilder = builder.Services.AddIdentityApiEndpoints<ApplicationUser>(options =>
{
	// Password settings
	options.Password.RequiredLength = 8;
	options.Password.RequireDigit = true;
	options.Password.RequireLowercase = true;
	options.Password.RequireUppercase = true;
	options.Password.RequireNonAlphanumeric = true;

	// Lockout settings
	options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
	options.Lockout.MaxFailedAccessAttempts = 5;
	options.Lockout.AllowedForNewUsers = true;

	// User settings
	options.User.RequireUniqueEmail = true;
	options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

	// Email confirmation setting from configuration
	options.SignIn.RequireConfirmedEmail = identitySettings?.RequireEmailConfirmation ?? true;
}).AddRoles<IdentityRole>().AddEntityFrameworkStores<AppDbContext>();

// Configura le opzioni dei cookie di Identity indipendentemente dal tipo di endpoint utilizzato
builder.Services.ConfigureApplicationCookie(options =>
{
	options.Cookie.Name = "ProtectedAPI.Identity";
	options.Cookie.HttpOnly = true;
	options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
	options.ExpireTimeSpan = TimeSpan.FromHours(24);
	options.SlidingExpiration = true;

	// Configurazioni ottimali per ambienti multi-istanza con load balancer
	options.Cookie.Path = "/";                      // Garantisce disponibilità su tutti i percorsi
	options.Cookie.SameSite = SameSiteMode.Lax;     // Bilanciamento tra sicurezza e usabilità

	// IMPORTANTE: Imposta un dominio se utilizzi sottodomini diversi per le istanze
	// options.Cookie.Domain = ".tuodominio.com";   // Decommentare e sostituire con il tuo dominio

	// Imposta i percorsi per login/logout/accessdenied (usati nei reindirizzamenti per client browser)
	options.LoginPath = "/login";
	options.LogoutPath = "/logout";
	options.AccessDeniedPath = "/access-denied";

	// Approccio intelligente: rispondi in modo diverso in base al Content-Type
	options.Events = new CookieAuthenticationEvents
	{
		OnRedirectToLogin = context =>
		{
			// Controlla se la richiesta accetta HTML o cerca JSON/API
			if (IsApiRequest(context.Request))
			{
				// Per richieste API, restituisci 401 Unauthorized invece di reindirizzare
				context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			}
			return Task.CompletedTask;
		},
		OnRedirectToAccessDenied = context =>
		{
			if (IsApiRequest(context.Request))
			{
				// Per richieste API, restituisci 403 Forbidden invece di reindirizzare
				context.Response.StatusCode = StatusCodes.Status403Forbidden;
			}
			return Task.CompletedTask;
		},
		// Gestione ottimizzata per sessioni distribuite
		OnValidatePrincipal = context =>
		{
			// La validazione originale rimane al suo posto,
			// ma verrà eseguita dopo questa parte

			// Rileva istanze di load balancer NON sticky e imposta attributi del cookie di conseguenza
			// Questo codice può essere esteso per rilevare specifici proxy/load balancer
			var forwardedHost = context.Request.Headers["X-Forwarded-Host"].FirstOrDefault();
			if (!string.IsNullOrEmpty(forwardedHost))
			{
				// Modifica il context per garantire consistenza tra istanze
				context.Properties.IsPersistent = true; // Garantisce persistenza in scenari load-balanced
			}

			// Poiché non eseguiamo operazioni asincrone, restituiamo direttamente un Task completato
			return Task.CompletedTask;
		}
	};
});

if (identitySettings?.UseCustomIdentityEndpoints ?? false)
{
	identityBuilder.AddTokenProvider<RefreshTokenProvider<ApplicationUser>>("RefreshTokenProvider");
	// Configure refresh token provider options
	builder.Services.Configure<RefreshTokenProviderOptions>(options =>
	{
		var refreshTokenDuration = int.Parse(builder.Configuration["Jwt:RefreshTokenDurationInDays"] ?? "7");
		options.TokenLifespan = TimeSpan.FromDays(refreshTokenDuration);
	});

	// Configura l'autenticazione con supporto sia per JWT che per cookie
	builder.Services.AddAuthentication()
		.AddJwtBearer(options =>
		{
			var jwtKey = builder.Configuration["Jwt:SecretKey"] ??
				throw new InvalidOperationException("JWT SecretKey not configured");
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

			// Configurazione JWT Bearer
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "ProtectedAPI",
				ValidAudience = builder.Configuration["Jwt:Audience"] ?? "ProtectedAPI",
				IssuerSigningKey = key,
				NameClaimType = ClaimTypes.Name,
				RoleClaimType = ClaimTypes.Role,
				ClockSkew = TimeSpan.Zero
			};

			// Eventi di gestione per il debugging
			options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
			{
				OnMessageReceived = context =>
				{
					var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

					// Controlla se c'è un token nell'header Authorization
					var authHeader = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
					if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
					{
						var token = authHeader.Substring("Bearer ".Length).Trim();
						logger.LogInformation("Token JWT ricevuto nell'header Authorization");

						// Non loggare token completi in produzione!
						if (builder.Environment.IsDevelopment())
						{
							logger.LogDebug("Token: {Token}", token);
						}
					}
					else
					{
						logger.LogWarning("Nessun token Bearer trovato nell'header Authorization");
					}

					return Task.CompletedTask;
				},
				OnAuthenticationFailed = context =>
				{
					var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

					// Log dettagliato dell'errore
					logger.LogError(context.Exception, "Errore durante l'autenticazione JWT: {ErrorMessage}", context.Exception.Message);

					if (context.Exception is SecurityTokenExpiredException)
					{
						logger.LogInformation("Il token è scaduto");
						context.Response.Headers.Append("Token-Expired", "true");
						context.Response.Headers.Append("Access-Control-Expose-Headers", "Token-Expired");
					}

					return Task.CompletedTask;
				},
				OnTokenValidated = context =>
				{
					var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

					// Log quando un token è stato validato con successo
					var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
					var username = context.Principal?.FindFirstValue(ClaimTypes.Name);
					logger.LogInformation("Token validato per utente: {UserId}, {Username}", userId, username);

					// Verifica la presenza del security stamp
					var securityStampClaim = context.Principal?.FindFirstValue("AspNet.Identity.SecurityStamp");
					if (securityStampClaim != null)
					{
						logger.LogInformation("Security stamp trovato nel token: {SecurityStamp}", securityStampClaim);
					}
					else
					{
						logger.LogWarning("Security stamp non presente nel token!");
					}

					return Task.CompletedTask;
				},
				OnChallenge = context =>
				{
					var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

					// Informazioni dettagliate sul challenge
					logger.LogWarning("Autenticazione fallita, emessa challenge. Error: {Error}, ErrorDescription: {ErrorDescription}",
						context.Error, context.ErrorDescription);

					return Task.CompletedTask;
				}
			};
		});

	// Configura opzioni aggiuntive dei cookie specifiche per gli endpoint personalizzati
	builder.Services.ConfigureApplicationCookie(options =>
	{
		// Approccio intelligente alla validazione del cookie che evita blocchi
		options.Events.OnValidatePrincipal = async context =>
		{
			// 1. Salta la validazione se non ci sono claim di identità (utente non autenticato)
			if (!context.Principal?.Identity?.IsAuthenticated ?? true)
			{
				return;
			}

			// 2. Verifica se l'endpoint richiede autorizzazione
			var endpoint = context.HttpContext.GetEndpoint();
			if (endpoint != null)
			{
				// Ottieni tutti i requisiti di autorizzazione associati all'endpoint
				var authRequirements = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();

				// Se non ci sono requisiti di autorizzazione, o c'è AllowAnonymous, salta la validazione
				if (!authRequirements.Any() || endpoint.Metadata.GetMetadata<IAllowAnonymous>() != null)
				{
					return;
				}
			}

			// 3. Limita la frequenza del refresh (ogni 10 minuti)
			var userId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return;
			}

			// Cache per memorizzare quando è stato fatto l'ultimo refresh
			var cacheKey = $"LastRefresh_{userId}";
			var now = DateTimeOffset.UtcNow;

			if (context.HttpContext.Items.TryGetValue(cacheKey, out var lastRefreshObj) &&
				lastRefreshObj is DateTimeOffset lastRefresh &&
				(now - lastRefresh).TotalMinutes < 10)
			{
				// Ultimo refresh meno di 10 minuti fa, salta
				return;
			}

			// 4. Solo validazione del security stamp, senza rinnovare il cookie
			var signInManager = context.HttpContext.RequestServices
				.GetRequiredService<SignInManager<ApplicationUser>>();

			var validatedPrincipal = await signInManager.ValidateSecurityStampAsync(context.Principal);
			if (validatedPrincipal == null)
			{
				// Security stamp non valido, logout
				context.RejectPrincipal();
				await signInManager.SignOutAsync();
				return;
			}

			// Memorizza l'ultimo refresh nella cache
			context.HttpContext.Items[cacheKey] = now;
		};
	});

	// Configurazione degli schemi di autenticazione multipli
	builder.Services.AddAuthorization(options =>
	{
		var defaultPolicy = new AuthorizationPolicyBuilder()
			.RequireAuthenticatedUser()
			.AddAuthenticationSchemes(IdentityConstants.ApplicationScheme, JwtBearerDefaults.AuthenticationScheme)
			.Build();

		options.DefaultPolicy = defaultPolicy;

		// Configura le policy esistenti per usare entrambi gli schemi
		options.AddPolicy("RequireMemberRole", policy =>
			policy.RequireRole("Member")
				.AddAuthenticationSchemes(IdentityConstants.ApplicationScheme, JwtBearerDefaults.AuthenticationScheme));

		options.AddPolicy("RequireAdminRole", policy =>
			policy.RequireRole("Admin")
				.AddAuthenticationSchemes(IdentityConstants.ApplicationScheme, JwtBearerDefaults.AuthenticationScheme));

		options.AddPolicy("RequireMemberOrAdmin", policy =>
			policy.RequireRole("Member", "Admin")
				.AddAuthenticationSchemes(IdentityConstants.ApplicationScheme, JwtBearerDefaults.AuthenticationScheme));
	});
}

// Configure authorization policies
builder.Services.AddAuthorizationBuilder()
	.AddPolicy("RequireMemberRole", policy =>
		policy.RequireRole("Member"))
	.AddPolicy("RequireAdminRole", policy =>
		policy.RequireRole("Admin"))
	.AddPolicy("RequireMemberOrAdmin", policy =>
		policy.RequireRole("Member", "Admin"));

// Add logging configuration
builder.Services.AddLogging(logging =>
{
	logging.ClearProviders();
	logging.AddConsole();

	// In production, you might want to add other logging providers
	if (!builder.Environment.IsDevelopment())
	{
		// Example for Serilog with various sinks (uncomment when needed):
		/*
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Day)
            // Add more sinks as needed
            .CreateLogger();
        logging.AddSerilog();
        */
	}
});

// Configure Email Settings and Service
builder.Services.Configure<EmailSettings>(
	builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<IdentitySettings>(
	builder.Configuration.GetSection("IdentitySettings"));
builder.Services.AddTransient<IEmailSender, EmailSenderAdapter>();
builder.Services.AddTransient<IEmailService, EmailService>();

// Configurazione del sistema DataProtection per supportare più istanze
var dataProtectionBuilder = builder.Services.AddDataProtection()
	.SetApplicationName("ProtectedAPI") // Importante: stesso nome su tutte le istanze
										// Persistenza su file system condiviso (per ambienti di produzione multi-istanza)
										// In produzione, configura un percorso accessibile da tutte le istanze
	.PersistKeysToFileSystem(new DirectoryInfo(
		builder.Environment.IsDevelopment() ?
		Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys") :
		builder.Configuration["DataProtection:KeysFolder"] ?? "/app/shared/keys"))
	// Configurazione avanzata delle chiavi
	.SetDefaultKeyLifetime(TimeSpan.FromDays(
		int.Parse(builder.Configuration["DataProtection:KeyLifetime"] ?? "14"))); // Durata predefinita delle chiavi

// Applica l'impostazione AutoGenerateKeys in base al valore configurato in appsettings.json
// Se false, disabilita la generazione automatica di nuove chiavi
if (builder.Configuration.GetValue<bool>("DataProtection:AutoGenerateKeys") == false)
{
	dataProtectionBuilder.DisableAutomaticKeyGeneration();

	// Crea un logger temporaneo per il logging durante la configurazione
	using var loggerFactory = LoggerFactory.Create(logging =>
	{
		logging.AddConsole();
		logging.SetMinimumLevel(LogLevel.Warning);
	});
	var logger = loggerFactory.CreateLogger("DataProtection");
	
	logger.LogWarning("AutoGenerateKeys è impostato a false. La generazione automatica delle chiavi è disabilitata. " +
				   "Assicurarsi che le chiavi di protezione dati siano gestite manualmente.");
}

// Validate required configuration
builder.Services.AddOptions<AdminCredentialsOptions>()
	.Bind(builder.Configuration.GetSection("AdminCredentials"))
	.ValidateDataAnnotations()
	.ValidateOnStart();

builder.Services.AddOptions<EmailSettings>()
	.Bind(builder.Configuration.GetSection("EmailSettings"))
	.ValidateDataAnnotations()
	.ValidateOnStart();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.UseOpenApi();
	app.UseSwaggerUi(config =>
	{
		config.DocumentTitle = "ProtectedAPI v1";
		config.Path = "/swagger";
		config.DocumentPath = "/swagger/{documentName}/swagger.json";
		config.DocExpansion = "list";
	});
	app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Map either default Identity API endpoints or our custom Identity endpoints based UseCustomIdentityEndpoints parameter settings
if (identitySettings?.UseCustomIdentityEndpoints ?? false)
{
	// Use our custom endpoints for auth
	app.MapCustomIdentityEndpoints();
}
else
{
	// Use standard Identity endpoints 
	app.MapIdentityApi<ApplicationUser>().WithOpenApi().WithTags("Identity");
}

// Map additional endpoints that don't conflict with either implementation
app.MapAdditionalIdentityEndpoints();

// Initialize the database
using (var scope = app.Services.CreateScope())
{
	await DbInitializer.Initialize(
		scope.ServiceProvider,
		applyMigrations: app.Environment.IsDevelopment()
	);
}

app.MapGroup("/api").MapTodoEndpoints().WithOpenApi().WithTags("Todos API");

app.Run();

static bool IsApiRequest(HttpRequest request)
{
	// Verifica se l'header Accept è presente prima di usarlo
	if (!request.Headers.TryGetValue("Accept", out Microsoft.Extensions.Primitives.StringValues value))
	{
		// Se l'header non esiste, verifica altri possibili indicatori di API
		// come X-Requested-With o Content-Type
		return request.Headers.ContainsKey("X-Requested-With") ||
			   (request.Headers.ContainsKey("Content-Type") &&
				// Controlla in modo sicuro che ContentType non sia null
				(request.ContentType != null &&
				 (request.ContentType.Contains("application/json") ||
				  request.ContentType.Contains("application/xml"))));
	}

	var accept = value.ToString();
	return accept.Contains("application/json") ||
		   accept.Contains("text/json") ||
		   accept.Contains("application/xml") ||
		   accept.Contains("text/xml");
}



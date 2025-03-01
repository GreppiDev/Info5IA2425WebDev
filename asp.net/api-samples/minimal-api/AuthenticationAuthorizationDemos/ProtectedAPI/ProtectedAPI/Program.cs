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
using Microsoft.AspNetCore.Authorization; // Add this for AllowAnonymousAttribute

var builder = WebApplication.CreateBuilder(args);

// Configure different configuration sources based on environment
if (!builder.Environment.IsDevelopment())
{
	// In production, we can use different configuration providers
	// Example for Azure Key Vault (uncomment and configure when needed):
	/*
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
        new DefaultAzureCredential());
    */

	// Example for AWS Secrets Manager (uncomment and configure when needed):
	/*
    builder.Configuration.AddSecretsManager(region: "eu-west-1",
        configurator: options =>
        {
            options.SecretFilter = entry => entry.Name.StartsWith("ProtectedAPI/");
            options.KeyGenerator = (entry, key) => key
                .Replace("ProtectedAPI/", string.Empty)
                .Replace("__", ":");
        });
    */
}

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

if (identitySettings?.UseCustomIdentityEndpoints ?? false)
{
	identityBuilder.AddTokenProvider<RefreshTokenProvider<ApplicationUser>>("RefreshTokenProvider");
	// Configure refresh token provider options
	builder.Services.Configure<RefreshTokenProviderOptions>(options =>
	{
		var refreshTokenDuration = int.Parse(builder.Configuration["Jwt:RefreshTokenDurationInDays"] ?? "7");
		options.TokenLifespan = TimeSpan.FromDays(refreshTokenDuration);
	});

	// Configura JWT Bearer authentication
	builder.Services.AddAuthentication(options => 
	{
		// Imposta lo schema di autenticazione predefinito a JWT Bearer
		options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
		options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
		options.DefaultScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
	})
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

	// Configura le opzioni dei cookie di Identity
	builder.Services.ConfigureApplicationCookie(options =>
	{
		options.Cookie.Name = "ProtectedAPI.Identity";
		options.Cookie.HttpOnly = true;
		options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
		options.ExpireTimeSpan = TimeSpan.FromHours(24);
		options.SlidingExpiration = true;

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
			
			// Non esegue il refresh del cookie qui, evitando il blocco
			// Il rinnovo del cookie viene gestito automaticamente dal middleware grazie a SlidingExpiration = true
		};
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
		// Example for Application Insights (uncomment when needed):
		// logging.AddApplicationInsights();

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



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
	config.DocumentName = "ProtectedAPI API";
	config.Version = "v1";

	// Add JWT authentication support to Swagger
	config.AddSecurity("JWT", new NSwag.OpenApiSecurityScheme
	{
		Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
		Name = "Authorization",
		In = NSwag.OpenApiSecurityApiKeyLocation.Header,
		Description = "Type into the textbox: Bearer {your JWT token}."
	});
	config.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
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
//Note: this configuration is required for both default and custom Identity Endpoints
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

//add RefreshTokenProvider only if CustomIdentityEndpoints is used 
if (identitySettings?.UseCustomIdentityEndpoints ?? false)
{
	identityBuilder.AddTokenProvider<RefreshTokenProvider<ApplicationUser>>("RefreshTokenProvider");
	// Configure refresh token provider options
	builder.Services.Configure<RefreshTokenProviderOptions>(options =>
	{
		var refreshTokenDuration = int.Parse(builder.Configuration["Jwt:RefreshTokenDurationInDays"] ?? "7");
		options.TokenLifespan = TimeSpan.FromDays(refreshTokenDuration);
	});

}

var authenticationBuilder = builder.Services.AddAuthentication();

// Configure JWT Authentication only if CustomIdentityEndpoints is used 
if (identitySettings?.UseCustomIdentityEndpoints ?? false)
{
	authenticationBuilder.AddJwtBearer(options =>
	{
		var jwtKey = builder.Configuration["Jwt:SecretKey"] ??
			throw new InvalidOperationException("JWT SecretKey not configured");
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "ProtectedAPI",
			ValidAudience = builder.Configuration["Jwt:Audience"] ?? "ProtectedAPI",
			IssuerSigningKey = key
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



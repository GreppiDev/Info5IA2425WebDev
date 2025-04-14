using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using EducationalGames.Models;
using EducationalGames.ModelsDTO;
using EducationalGames.Utils;
using Microsoft.EntityFrameworkCore;
using EducationalGames.Data;
using EducationalGames.Middlewares;
using EducationalGames.Endpoints;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "Educational Games v1";
    config.DocumentName = "Educational Games API";
    config.Version = "v1";
});

//adding services to the container
if (builder.Environment.IsDevelopment())
{
    //il servizio AddDatabaseDeveloperPageExceptionFilter andrebbe usato solo in fase di testing e non in produzione.
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}
var connectionString = builder.Configuration.GetConnectionString("EducationalGamesConnection");
var serverVersion = ServerVersion.AutoDetect(connectionString);
builder.Services.AddDbContext<AppDbContext>(
        opt => opt.UseMySql(connectionString, serverVersion)
            // The following three options help with debugging, but should
            // be changed or removed for production.
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
    );

// Configurazione dell'autenticazione con cookie usando lo schema predefinito
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = ".AspNetCore.Authentication"; // Nome standard non predittivo
        options.Cookie.HttpOnly = true; // Protegge il cookie da accessi via JavaScript

        // In sviluppo, permetti cookie su HTTP per semplificare il testing
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.None
            : CookieSecurePolicy.Always;

        // SameSite meno restrittivo in sviluppo per facilitare il testing
        options.Cookie.SameSite = builder.Environment.IsDevelopment()
            ? SameSiteMode.Lax
            : SameSiteMode.Strict;

        options.LoginPath = "/login-page.html";
        options.AccessDeniedPath = "/access-denied.html";


        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                if (HttpUtils.IsHtmlRequest(context.Request))
                {
                    context.Response.Redirect(context.RedirectUri);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.Headers.Remove("Location");
                }
                return Task.CompletedTask;
            },

            OnRedirectToAccessDenied = context =>
            {
                if (HttpUtils.IsHtmlRequest(context.Request))
                {
                    // For HTML requests, manually add the returnUrl parameter to the redirection
                    var returnUrl = context.Request.Path.Value ?? string.Empty;

                    // Build the redirect URL with returnUrl parameter
                    var redirectUrl = $"/access-denied.html?returnUrl={Uri.EscapeDataString(returnUrl)}";

                    context.Response.Redirect(redirectUrl);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.Headers.Remove("Location");
                }
                return Task.CompletedTask;
            }

        };
    });

// Add authorization services con policy per ruoli multipli
builder.Services.AddAuthorizationBuilder()
    // Add authorization services con policy per ruoli multipli
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    // Add authorization services con policy per ruoli multipli
    .AddPolicy("DocenteOnly", policy => policy.RequireRole("Docente"))
    // Add authorization services con policy per ruoli multipli
    .AddPolicy("AdminOrDocente", policy => policy.RequireRole("Admin", "Docente"))
    // Add authorization services con policy per ruoli multipli
    .AddPolicy("RegisteredUsers", policy => policy.RequireRole("Admin", "Docente", "Studente"));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Disabilita la validazione HTTPS per sviluppo locale
    app.UseCookiePolicy(new CookiePolicyOptions
    {
        MinimumSameSitePolicy = SameSiteMode.Lax,
        Secure = CookieSecurePolicy.None
    });

    app.MapOpenApi();
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
    app.UseHsts();
    app.UseHttpsRedirection();
}


// Middleware per file statici

// 1. Configura il middleware per servire index.html dalla cartella root
app.UseDefaultFiles();

// 2. Middleware per file statici in wwwroot (CSS, JS, ecc.)
app.UseStaticFiles();

// Middleware
app.UseAuthentication();
app.UseAuthorization();

// Custom Middleware per gestire 401 e 403
app.UseMiddleware<StatusCodeMiddleware>();

// Map API endpoints


app
.MapGroup("/api/account")
.MapAccountEndpoints();

app.Run();







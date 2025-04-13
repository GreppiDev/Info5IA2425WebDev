using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using EducationalGames.Models;
using Microsoft.EntityFrameworkCore;
using EducationalGames.Data;

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
                if (IsHtmlRequest(context.Request))
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
                if (IsHtmlRequest(context.Request))
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

// Middleware per gestire le risposte di errore per autenticazione e autorizzazione 
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;

    // Verifica che la risposta non sia già iniziata
    if (response.HasStarted)
        return;

    if (response.StatusCode == StatusCodes.Status401Unauthorized)
    {
        // Per richieste HTML, redirect alla pagina di login
        if (IsHtmlRequest(context.HttpContext.Request))
        {
            var returnUrl = context.HttpContext.Request.Path;
            response.Redirect($"/login-page.html?returnUrl={Uri.EscapeDataString(returnUrl)}");
            return;
        }

        // Per API, restituisci risposta JSON
        response.ContentType = "application/json";
        await response.WriteAsJsonAsync(new
        {
            status = 401,
            message = "Non sei autenticato. Effettua il login per accedere a questa risorsa.",
            timestamp = DateTime.UtcNow,
            path = context.HttpContext.Request.Path
        });
    }
    else if (response.StatusCode == StatusCodes.Status403Forbidden)
    {
        // Per richieste HTML, redirect alla pagina di accesso negato
        if (IsHtmlRequest(context.HttpContext.Request))
        {
            // Ensure we get the full path including query string
            var returnUrl = context.HttpContext.Request.Path.Value ?? string.Empty;

            // Special handling for paths with query strings
            if (context.HttpContext.Request.QueryString.HasValue)
            {
                returnUrl += context.HttpContext.Request.QueryString.Value;
            }

            var encodedReturnUrl = Uri.EscapeDataString(returnUrl);

            // Make sure the querystring is formatted correctly
            response.Redirect($"/access-denied.html?returnUrl={encodedReturnUrl}");
            return;
        }

        // Per API, restituisci risposta JSON
        response.ContentType = "application/json";
        await response.WriteAsJsonAsync(new
        {
            status = 403,
            message = "Non hai i permessi necessari per accedere a questa risorsa.",
            timestamp = DateTime.UtcNow,
            path = context.HttpContext.Request.Path
        });
    }
});

// Funzione helper per determinare se la richiesta è da un browser o API
static bool IsHtmlRequest(HttpRequest request)
{
    // Controlla se l'header Accept include HTML
    if (request.Headers.TryGetValue("Accept", out var acceptHeader))
    {
        return acceptHeader.ToString().Contains("text/html");
    }

    // Controlla l'header User-Agent per identificare i browser più comuni
    if (request.Headers.TryGetValue("User-Agent", out var userAgent))
    {
        string ua = userAgent.ToString().ToLower();
        if (ua.Contains("mozilla") || ua.Contains("chrome") || ua.Contains("safari") ||
            ua.Contains("edge") || ua.Contains("firefox") || ua.Contains("webkit"))
        {
            return true;
        }
    }
    return false;
}

// Endpoint di login
app.MapPost("/login", async (HttpContext ctx, LoginModel model, [FromQuery] string? returnUrl) =>
{
    bool success = false;
    string message = "";

    // Simulazione della validazione delle credenziali
    if (model.Username == "admin" && model.Password == "adminpass")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, model.Username),
            new(ClaimTypes.Role, "Admin"),
            new(ClaimTypes.Role, "Docente")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        message = "Login effettuato con successo come Admin+Docente";
        success = true;
    }
    else if (model.Username == "docente" && model.Password == "docentepass")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, model.Username),
            new(ClaimTypes.Role, "Docente")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        message = "Login effettuato con successo come Docente";
        success = true;
    }
    else if (model.Username == "studente" && model.Password == "pass")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, model.Username),
            new(ClaimTypes.Role, "Studente")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        message = "Login effettuato con successo come Studente";
        success = true;
    }

    // Se il login è riuscito e c'è un returnUrl
    if (success && !string.IsNullOrEmpty(returnUrl))
    {
        // Verifica che l'URL sia sicuro prima di eseguire il redirect
        if (Uri.IsWellFormedUriString(returnUrl, UriKind.Relative) ||
            returnUrl.StartsWith(ctx.Request.Scheme + "://" + ctx.Request.Host))
        {
            return Results.Redirect(returnUrl);
        }
    }

    return success ? Results.Ok(message) : Results.Unauthorized();
});


// Endpoint protetto
app.MapGet("/profile", (HttpContext ctx) =>
{
    if (ctx.User.Identity != null && ctx.User.Identity.IsAuthenticated)
        return Results.Ok($"Benvenuto, {ctx.User.Identity.Name}");
    return Results.Unauthorized();
}).RequireAuthorization();

// Endpoint accessibile solo agli amministratori
app.MapGet("/admin-area", (HttpContext ctx) =>
{
    return Results.Ok($"Benvenuto nell'area amministrativa, {ctx.User.Identity?.Name}");
}).RequireAuthorization("AdminOnly");

// Endpoint accessibile a Docente o Admin (OR logico)
app.MapGet("/power-area", (HttpContext ctx) =>
{
    return Results.Ok($"Benvenuto nell'area power, {ctx.User.Identity?.Name}");
}).RequireAuthorization("AdminOrDocente");


// Endpoint per verificare i ruoli attuali dell'utente
app.MapGet("/my-roles", (HttpContext ctx) =>
{
    if (ctx.User.Identity?.IsAuthenticated != true)
        return Results.Unauthorized();

    var roles = ctx.User.Claims
        .Where(c => c.Type == ClaimTypes.Role)
        .Select(c => c.Value)
        .ToList();

    return Results.Ok(new
    {
        Username = ctx.User.Identity.Name,
        Roles = roles,
        IsAdmin = ctx.User.IsInRole("Admin"),
        IsDocente = ctx.User.IsInRole("Docente"),
        IsUser = ctx.User.IsInRole("Studente"),
        HasAllRoles = ctx.User.IsInRole("Admin") && ctx.User.IsInRole("Docente") && ctx.User.IsInRole("Studente")
    });
});

app.MapPost("/logout", async (HttpContext ctx, [FromQuery] string? returnUrl) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    if (!string.IsNullOrEmpty(returnUrl) && IsHtmlRequest(ctx.Request))
    {
        if (Uri.IsWellFormedUriString(returnUrl, UriKind.Relative) ||
            returnUrl.StartsWith(ctx.Request.Scheme + "://" + ctx.Request.Host))
        {
            return Results.Redirect(returnUrl);
        }
    }

    return Results.Ok("Logout effettuato con successo");
});



app.Run();







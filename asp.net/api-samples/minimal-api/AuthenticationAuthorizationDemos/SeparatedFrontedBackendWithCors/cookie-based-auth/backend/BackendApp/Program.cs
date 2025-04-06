using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using BackendApp.Models;
using BackendApp.Data;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
    {
        config.Title = "Auth Cookie Demo Backend v1";
        config.DocumentName = "Auth Cookie Demo Backend API";
        config.Version = "v1";
    }
);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

var connectionString = builder.Configuration.GetConnectionString("DemoDbConnection");
var serverVersion = ServerVersion.AutoDetect(connectionString);
builder.Services.AddDbContext<AppDbContext>(opt => opt
    .UseMySql(connectionString, serverVersion)
    .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging()
    .EnableDetailedErrors()
);

// Configurazione CORS - deve essere prima di AddAuthentication
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // Legge le origini consentite dalla configurazione
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ??
            ["http://localhost:5263"]; // Default fallback se la configurazione non esiste

        policy.WithOrigins(allowedOrigins)
        .AllowCredentials()
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithExposedHeaders("Set-Cookie"); // Espone l'header Set-Cookie al client in contesti CORS
    });
});

// Configurazione Authentication con cookie
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/unauthorized";
    options.Cookie.HttpOnly = true;

    // Per sviluppo imposta SameSite=Lax in modo che i cookie funzionino cross-origin
    // Lax è più compatibile per request GET senza Secure
    options.Cookie.SameSite = SameSiteMode.Lax;

    // SameSite=Lax non richiede Secure
    //in produzione, per HTTPS, impostare SecurePolicy Always
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Per sviluppo HTTP

    options.Cookie.Name = "AuthCookieDemo";
    options.Cookie.Path = "/";
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;

    // Gestire le risposte REST invece di redirect per le API
    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        },
        // Log per debug
        OnValidatePrincipal = context =>
        {
            var principal = context.Principal;
            if (principal?.Identity?.IsAuthenticated == true)
            {
                var name = principal.Identity.Name;
                var role = principal.FindFirst(ClaimTypes.Role)?.Value;
                Console.WriteLine($"Cookie validation: User {name} with role {role}");
            }
            else
            {
                Console.WriteLine("Cookie validation: No authenticated user");
            }
            return Task.CompletedTask;
        }
    };
});

// Configurazione Authorization
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

var app = builder.Build();

// L'ordine dei middleware è importante!

// Prima i middleware di sviluppo
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "Auth Cookie Demo v1";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

// Poi CORS - deve essere prima di Authentication e Authorization
app.UseCors("AllowFrontend"); // Policy CORS globale

// Middleware di autenticazione/autorizzazione
app.UseAuthentication();
app.UseAuthorization();

// Endpoint API
app.MapPost("/register", async (UserCredentials creds, AppDbContext db) =>
{
    var hasher = new PasswordHasher<User>();
    if (await db.Users.AnyAsync(u => u.Username == creds.Username))
        return Results.BadRequest("Username already taken");

    var user = new User
    {
        Username = creds.Username,
        Role = "user"
    };
    user.PasswordHash = hasher.HashPassword(user, creds.Password);
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok(new { Message = "Registration successful" });
});

app.MapPost("/login", async (UserCredentials creds, AppDbContext db, HttpContext ctx) =>
{
    var hasher = new PasswordHasher<User>();
    var user = await db.Users.FirstOrDefaultAsync(u => u.Username == creds.Username);
    if (user is null) return Results.Unauthorized();

    var result = hasher.VerifyHashedPassword(user, user.PasswordHash, creds.Password);
    if (result == PasswordVerificationResult.Failed)
        return Results.Unauthorized();

    var claims = new List<Claim> {
        new(ClaimTypes.Name, user.Username),
        new(ClaimTypes.Role, user.Role.ToLower())
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    var authProperties = new AuthenticationProperties
    {
        IsPersistent = true,
        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1),
        AllowRefresh = true
    };

    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

    // Aggiungiamo header espliciti per la gestione dei cookie
    ctx.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
    ctx.Response.Headers.Append("Pragma", "no-cache");
    ctx.Response.Headers.Append("Expires", "0");

    return Results.Ok(new
    {
        user.Username,
        user.Role,
        Message = "Login successful. If you're still having authentication issues, check that your browser is sending cookies."
    });
});

app.MapPost("/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok(new { Message = "Logout successful" });
});

app.MapGet("/me", (ClaimsPrincipal user, HttpContext ctx) =>
{
    if (user.Identity?.IsAuthenticated != true)
    {
        Console.WriteLine("User not authenticated");
        // Restituisci dettagli sul motivo dell'errore per il debug
        return Results.Unauthorized();
    }

    var username = user.Identity.Name;
    var role = user.FindFirst(ClaimTypes.Role)?.Value;
    Console.WriteLine($"User authenticated: {username}, role: {role}");

    return Results.Ok(new
    {
        Username = username,
        Role = role
    });
})
.RequireAuthorization();

// Aggiungiamo un nuovo endpoint di debug specifico per i cookie
app.MapGet("/debug-cookies", (HttpContext ctx) =>
{
    var allCookies = ctx.Request.Cookies;
    var authCookie = ctx.Request.Cookies["AuthCookieDemo"];
    string? authCookiePreview = null;

    if (authCookie != null && authCookie.Length > 0)
    {
        authCookiePreview = authCookie.Length > 20
            ? $"{authCookie[..20]}..."
            : authCookie;
    }

    var cookieList = allCookies.Select(c =>
    {
        string preview;
        if (!string.IsNullOrEmpty(c.Value) && c.Value.Length > 20)
        {
            preview = $"{c.Value[..20]}...";
        }
        else
        {
            preview = c.Value ?? "(null)";
        }
        return new { c.Key, ValuePreview = preview };
    }).ToList();

    return Results.Ok(new
    {
        HasAuthCookie = authCookie != null,
        AuthCookieValue = authCookiePreview,
        AllCookies = cookieList
    });
});

app.MapPost("/change-password", async (PasswordChangeRequest request, ClaimsPrincipal user, AppDbContext db) =>
{
    if (user.Identity?.IsAuthenticated != true)
        return Results.Unauthorized();

    var username = user.Identity.Name;
    var dbUser = await db.Users.FirstOrDefaultAsync(u => u.Username == username);

    if (dbUser is null)
        return Results.Unauthorized();

    var hasher = new PasswordHasher<User>();
    var result = hasher.VerifyHashedPassword(dbUser, dbUser.PasswordHash, request.CurrentPassword);

    if (result == PasswordVerificationResult.Failed)
        return Results.BadRequest("Current password is incorrect");

    dbUser.PasswordHash = hasher.HashPassword(dbUser, request.NewPassword);
    await db.SaveChangesAsync();

    return Results.Ok(new { Message = "Password changed successfully" });
})
.RequireAuthorization();

app.MapGet("/admin", (ClaimsPrincipal user, HttpContext context) =>
{
    //la parte che verifica se l'utente è autenticato e ha il ruolo admin
    //è stata messa solo per mostrare il funzionamento
    //in realtà non serve, perché l'endpoint è protetto da RequireAuthorization
    //e quindi non viene mai eseguito se l'utente non è autenticato
    //e non ha il ruolo admin
    //
    //----------------- Questa parte è solo per il debug
    var role = user.FindFirst(ClaimTypes.Role)?.Value;
    var name = user.Identity?.Name;
    Console.WriteLine($"User {name} requesting admin access with role: {role}");

    if (!user.Identity?.IsAuthenticated == true)
    {
        Console.WriteLine("Access denied: user not authenticated");
        return Results.Unauthorized();
    }

    if (role?.ToLower() != "admin")
    {
        Console.WriteLine($"Access denied: role is '{role}' but needs to be 'admin'");
        return Results.Forbid();
    }
    //-----------------

    // Se l'utente è autenticato e ha il ruolo admin, restituisci il messaggio
    return Results.Ok($"Area riservata agli amministratori. User role: {role}, Username: {name}");
})
.RequireAuthorization("AdminOnly");

app.MapGet("/unauthorized", () => Results.Unauthorized());

app.MapGet("/debug-auth", (HttpContext context, ClaimsPrincipal user) =>
{
    // Check request cookies
    var cookies = context.Request.Cookies;
    var cookieList = cookies.Select(c => new { c.Key, Value = c.Value }).ToList();

    return Results.Ok(new
    {
        IsAuthenticated = user.Identity?.IsAuthenticated == true,
        Username = user.Identity?.Name,
        Role = user.FindFirst(ClaimTypes.Role)?.Value,
        Cookies = cookieList,
        Headers = context.Request.Headers.Select(h => new { h.Key, Value = h.Value.ToString() }).ToList()
    });
});

app.Run();
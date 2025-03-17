using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc; //aggiunge il supporto per l'autenticazione tramite cookie

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
        options.AccessDeniedPath = "/access-denied"; // Percorso per l'accesso negato

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
                    // For API requests, return 401 status code without redirect
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    // Prevent the default redirect which can cause the 405 error
                    context.Response.Headers["Location"] = string.Empty;
                    return Task.CompletedTask;
                }

                // For browser requests, redirect to login page (default behavior)
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            },

            // Gestione della risposta quando l'accesso è negato a un utente autenticato (Forbidden)
            OnRedirectToAccessDenied = context =>
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
                    // Return 403 Forbidden for API requests
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }

                // // Default behavior for browser requests
                // context.Response.Redirect(context.RedirectUri);
                // return Task.CompletedTask;

                // Per richieste browser, personalizza l'URL aggiungendo parametri
                var redirectUri = context.RedirectUri;
                var returnUrl = context.Request.Path;

                // Aggiungi il returnUrl come parametro di query per poter tornare
                // alla pagina originale dopo l'autenticazione
                redirectUri = $"{options.AccessDeniedPath}?returnUrl={Uri.EscapeDataString(returnUrl)}";

                // Default behavior for browser requests with custom URL
                context.Response.Redirect(redirectUri);
                return Task.CompletedTask;
            }
        };
    });


// Add authorization services - senza specificare policy
//builder.Services.AddAuthorization();
// Add authorization services con policy per ruoli multipli
builder.Services.AddAuthorization(options =>
{
    // Policy che richiede un solo ruolo specifico
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("PowerUserOnly", policy => policy.RequireRole("PowerUser"));

    // Policy che accetta uno qualsiasi tra più ruoli (OR logico)
    options.AddPolicy("AdminOrPowerUser", policy =>
        policy.RequireRole("Admin", "PowerUser"));

    // Policy per tutti gli utenti registrati
    options.AddPolicy("RegisteredUsers", policy =>
        policy.RequireRole("Admin", "PowerUser", "User"));

    // Policy che richiede TUTTI i ruoli specificati (AND logico)
    options.AddPolicy("AdminAndPowerUser", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") && context.User.IsInRole("PowerUser")));

    // Policy con logica personalizzata più complessa
    options.AddPolicy("ComplexRoleRequirement", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Admin") ||
            (context.User.IsInRole("PowerUser") && context.User.HasClaim(c =>
                c.Type == "Permission" && c.Value == "CanEditContent"))));
});

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

// Endpoint di login - versione semplice con credenziali fisse e senza ruoli
// app.MapPost("/login", async (HttpContext ctx, LoginModel model) =>
// {
//     // Simulazione della validazione delle credenziali
//     //in questo caso andrebbero verificate le credenziali sul database...
//     if (model.Username == "user" && model.Password == "pass")
//     {
//         var claims = new[] { new Claim(ClaimTypes.Name, model.Username) };
//         var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
//         await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
//         return Results.Ok("Login effettuato con successo");
//     }
//     return Results.Unauthorized();
// });

// Endpoint di login
app.MapPost("/login", async (HttpContext ctx, LoginModel model) =>
{
    // Simulazione della validazione delle credenziali
    if (model.Username == "admin" && model.Password == "adminpass")
    {
        // Utente con ruoli multipli: Admin e PowerUser
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, model.Username),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, "PowerUser")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        return Results.Ok("Login effettuato con successo come Admin+PowerUser");
    }
    else if (model.Username == "poweruser" && model.Password == "powerpass")
    {
        // Utente con un solo ruolo: PowerUser
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, model.Username),
            new Claim(ClaimTypes.Role, "PowerUser")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        return Results.Ok("Login effettuato con successo come PowerUser");
    }
    else if (model.Username == "user" && model.Password == "pass")
    {
        // Utente normale con ruolo base User
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, model.Username),
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        return Results.Ok("Login effettuato con successo come User");
    }
    return Results.Unauthorized();
});

// Endpoint per impostare un altro cookie
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

// Endpoint accessibile solo agli amministratori
app.MapGet("/admin-area", (HttpContext ctx) =>
{
    return Results.Ok($"Benvenuto nell'area amministrativa, {ctx.User.Identity?.Name}");
}).RequireAuthorization("AdminOnly");

// Endpoint accessibile a PowerUser o Admin (OR logico)
app.MapGet("/power-area", (HttpContext ctx) =>
{
    return Results.Ok($"Benvenuto nell'area power, {ctx.User.Identity?.Name}");
}).RequireAuthorization("AdminOrPowerUser");

// Endpoint che richiede ENTRAMBI i ruoli Admin E PowerUser (AND logico)
app.MapGet("/super-area", (HttpContext ctx) =>
{
    return Results.Ok($"Benvenuto nell'area super-admin, {ctx.User.Identity?.Name}");
}).RequireAuthorization("AdminAndPowerUser");

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
        IsPowerUser = ctx.User.IsInRole("PowerUser"),
        IsUser = ctx.User.IsInRole("User"),
        HasAllRoles = ctx.User.IsInRole("Admin") && ctx.User.IsInRole("PowerUser")
    });
});

// Aggiunta di endpoint per test con permessi specifici
app.MapPost("/login-with-permissions", async (HttpContext ctx, LoginModel model) =>
{
    // Simulazione autenticazione
    if (model.Username == "editor" && model.Password == "editorpass")
    {
        // Utente con ruolo PowerUser e permessi specifici
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, model.Username),
            new Claim(ClaimTypes.Role, "PowerUser"),
            new Claim("Permission", "CanEditContent"),
            new Claim("Permission", "CanPublishContent")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        return Results.Ok("Login effettuato con successo come Editor con permessi");
    }
    return Results.Unauthorized();
});

// Endpoint che richiede un permesso specifico
app.MapGet("/edit-content", (HttpContext ctx) =>
{
    bool canEdit = ctx.User.HasClaim(c => c.Type == "Permission" && c.Value == "CanEditContent");

    if (!canEdit)
        return Results.Forbid();

    return Results.Ok("Puoi modificare i contenuti");
}).RequireAuthorization(); // Richiede autenticazione ma il permesso specifico è verificato nel codice

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


// Endpoint per l'accesso negato
// app.MapGet("/access-denied", () =>
// {
//     return Results.Problem(
//         title: "Accesso negato",
//         detail: "Non hai i permessi necessari per accedere a questa risorsa.",
//         statusCode: StatusCodes.Status403Forbidden
//     );
// });

app.MapGet("/access-denied", (HttpContext context, [FromQuery] string? returnUrl) =>
{
    // Ottieni informazioni sull'utente corrente
    string username = context.User.Identity?.Name ?? "Utente";

    // Costruisci una risposta HTML personalizzata
    var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Accesso Negato</title>
    <style>
        body {{ font-family: Arial, sans-serif; text-align: center; padding-top: 50px; }}
        .container {{ max-width: 600px; margin: 0 auto; }}
        .error {{ color: #d9534f; }}
        .btn {{ 
            display: inline-block; 
            padding: 10px 20px; 
            margin: 10px; 
            background-color: #337ab7; 
            color: white; 
            text-decoration: none; 
            border-radius: 4px; 
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <h1 class=""error"">Accesso Negato</h1>
        <p>Ci dispiace, {username}, ma non hai i permessi necessari per accedere a questa risorsa.</p>
        
        {(string.IsNullOrEmpty(returnUrl) ? "" : $"<p>Hai tentato di accedere a: {returnUrl}</p>")}
        
        <div>
            <a href=""/"" class=""btn"">Torna alla Home</a>
            
            {(string.IsNullOrEmpty(returnUrl) ? "" :
              $"<a href=\"/request-access?resource={Uri.EscapeDataString(returnUrl)}\" class=\"btn\">Richiedi Accesso</a>")}
        </div>
    </div>
</body>
</html>";

    return Results.Content(html, "text/html");
});

app.Run();

public class LoginModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}





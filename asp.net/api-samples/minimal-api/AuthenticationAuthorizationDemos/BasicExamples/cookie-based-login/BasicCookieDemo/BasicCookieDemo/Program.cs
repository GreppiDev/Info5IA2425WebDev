using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies; //aggiunge il supporto per l'autenticazione tramite cookie

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
        // In sviluppo, permetti cookie su HTTP per semplificare il testing
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.None
            : CookieSecurePolicy.Always;
        // SameSite meno restrittivo in sviluppo per facilitare il testing
        options.Cookie.SameSite = builder.Environment.IsDevelopment()
            ? SameSiteMode.Lax
            : SameSiteMode.Strict;
        options.LoginPath = "/login.html"; // Percorso per il login

        // Content-type based redirect handling
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
           }
        };
    });


// Add authorization services
builder.Services.AddAuthorization();

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

// Endpoint di login
app.MapPost("/login", async (HttpContext ctx, LoginModel model) =>
{
    // Simulazione della validazione delle credenziali
    //in questo caso andrebbero verificate le credenziali sul database...
    if (model.Username == "user" && model.Password == "pass")
    {
        var claims = new[] { new Claim(ClaimTypes.Name, model.Username) };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        return Results.Ok("Login effettuato con successo");
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

// Endpoint di logout
app.MapPost("/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok("Logout effettuato con successo");
});

app.Run();

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
public class LoginModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}




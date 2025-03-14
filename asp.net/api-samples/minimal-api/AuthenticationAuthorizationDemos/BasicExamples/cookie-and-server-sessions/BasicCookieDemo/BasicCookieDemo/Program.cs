using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

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


// Configurazione dell’autenticazione con cookie
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.Cookie.Name = "MyCookie";
        options.Cookie.HttpOnly = true; // Protegge il cookie da accessi via JavaScript
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Solo su HTTPS
        options.Cookie.SameSite = SameSiteMode.Strict; // Previene CSRF
        options.LoginPath = "/login"; // Percorso per il login
    });

//aggiunta delle sessioni
// Aggiunta dei servizi per la gestione delle sessioni
builder.Services.AddDistributedMemoryCache(); // Per lo storage in memoria
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    // Add application discriminator to avoid conflicts
    options.Cookie.Name = ".MyApp.Session";
});

// Add authorization services
builder.Services.AddAuthorization();

// Configura Data Protection con un percorso persistente per le chiavi
// e un'identificatore di applicazione specifico
builder.Services.AddDataProtection()
    .SetApplicationName("BasicCookieDemo")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")))
    .SetDefaultKeyLifetime(TimeSpan.FromDays(14)); // Set key lifetime explicitly


//per gestire le chiavi in memoria
// Configura Data Protection per mantenere le chiavi solo in memoria
// builder.Services.AddDataProtection()
//     .SetApplicationName("BasicCookieDemo")
//     .SetDefaultKeyLifetime(TimeSpan.FromDays(14))
//     .DisableAutomaticKeyGeneration() // Disabilita la generazione automatica di nuove chiavi
//     .UseEphemeralDataProtectionProvider(); // Usa un provider di protezione dati effimero (in-memory)

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
app.UseSession();


// Endpoint di login
app.MapPost("/login", async (HttpContext ctx, LoginModel model) =>
{
    // Simulazione validazione credenziali
    if (model.Username == "user" && model.Password == "pass")
    {
        // Clear any existing session to start fresh
        ctx.Session.Clear();

        // Regenerate session ID to prevent session fixation
        if (ctx.Request.Cookies.ContainsKey(".MyApp.Session"))
        {
            ctx.Response.Cookies.Delete(".MyApp.Session");
        }

        var claims = new[] { new Claim(ClaimTypes.Name, model.Username) };
        var identity = new ClaimsIdentity(claims, "CookieAuth");
        await ctx.SignInAsync("CookieAuth", new ClaimsPrincipal(identity));

        return Results.Ok("Login effettuato con successo");
    }
    return Results.Unauthorized();
});

// Endpoint per impostare un cookie altro cookie sicuro
app.MapGet("/set-cookie", (HttpContext context) =>
{
    // Generazione di un identificativo univoco 
    var secretId = Guid.NewGuid().ToString();
    // Configurazione delle opzioni del cookie
    var cookieOptions = new CookieOptions
    {
        HttpOnly = true,                  // Impedisce l'accesso tramite JS
        Secure = true,                    // Trasmissione solo via HTTPS
        SameSite = SameSiteMode.Strict,   // Protegge da CSRF
        Expires = DateTimeOffset.Now.AddMinutes(30) // Cookie persistente per 30 minuti
    };
    context.Response.Cookies.Append("secretId", secretId, cookieOptions);
    return Results.Ok("Cookie impostato correttamente!");
});

//endpoint per leggere il cookie sicuro
app.MapGet("/read-cookie", (HttpContext context) =>
{
    var secretId = context.Request.Cookies["secretId"];
    return secretId != null
        ? Results.Ok($"Il valore del cookie è: {secretId}")
        : Results.NotFound("Cookie non trovato");
});

// Endpoint protetto
app.MapGet("/profile", (HttpContext ctx) =>
{
    if (ctx.User.Identity != null && ctx.User.Identity.IsAuthenticated)
        return Results.Ok($"Benvenuto, {ctx.User.Identity.Name}");
    return Results.Unauthorized();
}).RequireAuthorization();

// Endpoint protetto per aggiungere articoli al carrello
app.MapPost("/cart/add", (HttpContext ctx, CartItem item) =>
{
    // Verifica che l'utente sia autenticato
    if (ctx.User.Identity == null || !ctx.User.Identity.IsAuthenticated)
        return Results.Unauthorized();

    // Ottiene il carrello dell'utente dalla sessione o ne crea uno nuovo
    var cart = ctx.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

    // Controlla se l'articolo è già presente nel carrello
    var existingItem = cart.FirstOrDefault(i => i.Id == item.Id);
    if (existingItem != null)
    {
        // Aggiorna la quantità dell'articolo esistente
        existingItem.Quantity += item.Quantity;
    }
    else
    {
        // Aggiunge il nuovo articolo al carrello
        cart.Add(item);
    }

    // Salva il carrello aggiornato nella sessione
    ctx.Session.SetObjectAsJson("Cart", cart);

    return Results.Ok(new { Message = "Articolo aggiunto al carrello", Cart = cart });
}).RequireAuthorization();

// Endpoint protetto per visualizzare il carrello
app.MapGet("/cart", (HttpContext ctx) =>
{
    // Verifica che l'utente sia autenticato
    if (ctx.User.Identity == null || !ctx.User.Identity.IsAuthenticated)
        return Results.Unauthorized();

    // Ottiene il carrello dell'utente dalla sessione
    var cart = ctx.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
    return Results.Ok(cart);
}).RequireAuthorization();

app.Run();

public class LoginModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// Modello per gli articoli del carrello
public class CartItem
{
    public int Id { get; set; }
    public int Quantity { get; set; }
}

// Estensioni per gestire oggetti JSON nelle sessioni
public static class SessionExtensions
{
    public static void SetObjectAsJson(this ISession session, string key, object value)
    {
        session.SetString(key, System.Text.Json.JsonSerializer.Serialize(value));
    }

    public static T? GetObjectFromJson<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default(T) : System.Text.Json.JsonSerializer.Deserialize<T>(value);
    }
}


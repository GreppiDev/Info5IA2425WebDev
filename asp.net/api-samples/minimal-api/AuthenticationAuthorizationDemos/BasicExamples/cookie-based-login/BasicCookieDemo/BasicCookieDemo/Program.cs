using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

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
    // Simulazione validazione credenziali
    //in questo caso andrebbero verificate le credenziali sul database...
    if (model.Username == "user" && model.Password == "pass")
    {
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



app.Run();

public class LoginModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}




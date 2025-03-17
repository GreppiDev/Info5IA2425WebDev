using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Collections.Concurrent; // Aggiunto per ConcurrentDictionary

// Dictionary thread-safe per la gestione dei token
ConcurrentDictionary<string, RefreshTokenInfo> refreshTokenStore = new();
ConcurrentDictionary<string, string> refreshTokenToUserMap = new();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
//aggiunge il servizio che permette ad OpenAPI di leggere i metadati delle API
builder.Services.AddEndpointsApiExplorer();
//configura il servizio OpenAPI
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "Basic Token v1";
    config.DocumentName = "Basic Token API";
    config.Version = "v1";

    // Configurazione dell'autenticazione JWT per Swagger/OpenAPI
    config.AddSecurity("JWT", [], new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Inserisci SOLO il token JWT (senza il prefisso 'Bearer').\nIl prefisso 'Bearer ' viene aggiunto automaticamente."
    });

    // Applica il requisito di sicurezza a tutti gli endpoint
    config.OperationProcessors.Add(new NSwag.Generation.Processors.Security.OperationSecurityScopeProcessor("JWT"));
});

// Configurazione dell'autenticazione tramite JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ??
            throw new InvalidOperationException("Jwt:Issuer non configurata"),
        ValidAudience = builder.Configuration["Jwt:Audience"] ??
            throw new InvalidOperationException("Jwt:Audience non configurata"),
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ??
                throw new InvalidOperationException("Jwt:Key non configurata")))
    };

    // Gestione degli eventi di autenticazione
    options.Events = new JwtBearerEvents
    {
        // Gestione del fallimento dell'autenticazione (401 Unauthorized)
        OnChallenge = async context =>
        {
            // Previene che il middleware gestisca automaticamente la risposta
            context.HandleResponse();

            // Se la richiesta accetta HTML (browser)
            if (IsHtmlRequest(context.Request))
            {
                // Reindirizza al login
                context.Response.Redirect("/login-page");
                return;
            }

            // Altrimenti invia una risposta 401 con dettagli in JSON
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var response = new
            {
                status = 401,
                message = "Non sei autenticato. Effettua il login per accedere a questa risorsa.",
                timestamp = DateTime.UtcNow,
                path = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(response);
        },

        // Gestione di altre eccezioni di autenticazione come token scaduti o firme non valide
        OnAuthenticationFailed = context =>
        {
            // Log dell'errore di autenticazione
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Errore autenticazione JWT: {Error} - Path: {Path}",
                context.Exception.Message,
                context.Request.Path);

            // Se il token è scaduto, aggiungi un header specifico che il client può utilizzare
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Append("Token-Expired", "true");
                // Non gestire la risposta qui, lasceremo che OnChallenge lo faccia
            }

            return Task.CompletedTask;
        }
    };
});

// Add authorization services
builder.Services.AddAuthorization(options =>
{
    // Definizione di una policy per gli amministratori
    options.AddPolicy("RequireAdministratorRole", policy =>
        policy.RequireRole("Administrator"));

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
        config.DocumentTitle = "Basic Token v1";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";

    });

    app.UseDeveloperExceptionPage();
}

// Middleware
app.UseAuthentication();
app.UseAuthorization();

// Gestione personalizzata degli errori di autorizzazione (403 Forbidden)
app.Use(async (context, next) =>
{
    await next();

    // Intercetta solo le risposte 403 Forbidden
    if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
    {
        // Se la richiesta accetta HTML (browser)
        if (IsHtmlRequest(context.Request))
        {
            // Reindirizza alla pagina di accesso negato
            context.Response.Redirect("/access-denied");
            return;
        }

        // Altrimenti invia una risposta 403 con dettagli in JSON
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = 403,
            message = "Non hai i permessi necessari per accedere a questa risorsa.",
            timestamp = DateTime.UtcNow,
            path = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(response);
    }
});

// Metodi helper per la gestione dei token
string GenerateAccessToken(IConfiguration config, IEnumerable<Claim> claims)
{
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"] ??
        throw new InvalidOperationException("Jwt:Key non configurata")));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: config["Jwt:Issuer"] ??
            throw new InvalidOperationException("Jwt:Issuer non configurata"),
        audience: config["Jwt:Audience"] ??
            throw new InvalidOperationException("Jwt:Audience non configurata"),
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(30), // Usare UTC per timestamp
        signingCredentials: credentials);

    return new JwtSecurityTokenHandler().WriteToken(token);
}

string GenerateRefreshToken()
{
    // Genera un token più sicuro rispetto al semplice Guid
    // var randomNumber = new byte[32]; // 256 bit
    // using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
    // rng.GetBytes(randomNumber);

    // // Converte in una stringa base64 (più sicura di un GUID e con una maggiore entropia)
    // return Convert.ToBase64String(randomNumber);

    // Generazione di un refresh token (opaco)
    var refreshToken = Guid.NewGuid().ToString();
    return refreshToken;
}

// Endpoint per il login che restituisce un access token e un refresh token
app.MapPost("/login", (UserLogin login) =>
{
    // Validazione input
    if (login is null || string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.Password))
    {
        return Results.BadRequest("Username e password sono richiesti");
    }

    // Verifica per l'utente normale con privilegi limitati
    if (login.Username == "user" && login.Password == "pass")
    {
        string userId = "user123";
        string userEmail = "user@example.com";

        // Utente standard con ruolo di sola visualizzazione
        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, userEmail),
            new Claim(ClaimTypes.Name, login.Username),
            new Claim(ClaimTypes.Role, "Viewer")
        };

        var accessToken = GenerateAccessToken(builder.Configuration, claims);
        var refreshToken = GenerateRefreshToken();

        // Uso di thread-safe ConcurrentDictionary
        refreshTokenStore[userId] = new RefreshTokenInfo
        {
            Token = refreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        refreshTokenToUserMap[refreshToken] = userId;

        return Results.Ok(new
        {
            accessToken,
            refreshToken,
        });
    }
    // Verifica per l'amministratore con privilegi completi
    else if (login.Username == "admin" && login.Password == "Admin123!")
    {
        string userId = "admin456";
        string userEmail = "admin@example.com";

        // Amministratore con ruoli multipli
        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, userEmail),
            new Claim(ClaimTypes.Name, login.Username),
            new Claim(ClaimTypes.Role, "Administrator"),
            new Claim(ClaimTypes.Role, "SuperAdministrator")  // Utente con più ruoli
        };

        var accessToken = GenerateAccessToken(builder.Configuration, claims);
        var refreshToken = GenerateRefreshToken();

        // Uso di thread-safe ConcurrentDictionary
        refreshTokenStore[userId] = new RefreshTokenInfo
        {
            Token = refreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        refreshTokenToUserMap[refreshToken] = userId;

        return Results.Ok(new
        {
            accessToken,
            refreshToken,

        });
    }

    return Results.Unauthorized();
})
.WithName("Login")
.WithOpenApi(operation =>
{
    operation.Summary = "Effettua il login e ottiene token di accesso";
    operation.Description = "Autentica l'utente e restituisce access token e refresh token";
    return operation;
});

// Endpoint protetto, accessibile solo con un token valido
app.MapGet("/protected", (HttpContext context) =>
{
    // Recupera il nome dell'utente dalle claims del token
    var username = context.User?.Identity?.Name;
    var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    var userEmail = context.User?.FindFirstValue(ClaimTypes.Email);
    var userRole = context.User?.FindFirstValue(ClaimTypes.Role);

    return Results.Ok(new
    {
        Message = $"Benvenuto {username}! Questo è un contenuto riservato.",
        UserDetails = new
        {
            Id = userId,
            Username = username,
            Email = userEmail,
            Role = userRole
        }
    });
})
.RequireAuthorization()
.WithName("Protected")
.WithOpenApi(operation =>
{
    operation.Summary = "Endpoint protetto ad accesso limitato";
    operation.Description = "Restituisce informazioni sull'utente autenticato";
    return operation;
});

// Endpoint per recuperare le informazioni dell'utente autenticato
app.MapGet("/user-info", (HttpContext context) =>
{
    // Recupera i dettagli dell'utente dalle claims
    var username = context.User?.Identity?.Name;
    var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    var email = context.User?.FindFirstValue(ClaimTypes.Email);

    // Recupera tutti i ruoli dell'utente
    var roles = context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ?? [];

    // Determina il tipo di utente in base ai ruoli
    bool isAdmin = roles.Contains("Administrator");
    bool isViewer = roles.Contains("Viewer");
    bool isSuperAdmin = roles.Contains("SuperAdministrator");

    return Results.Ok(new
    {
        userId,
        username,
        email,
        roles,
        accountType = isAdmin ? "Administrator" : isViewer ? "Standard User" : "Unknown",
        permissions = new
        {
            canCreate = isAdmin || isSuperAdmin,
            canRead = true, // Tutti gli utenti autenticati possono leggere
            canUpdate = isAdmin || isSuperAdmin,
            canDelete = isAdmin || isSuperAdmin,
            canManageUsers = isSuperAdmin
        }
    });
})
.RequireAuthorization() // Richiede l'autenticazione
.WithName("UserInfo")
.WithOpenApi(operation =>
{
    operation.Summary = "Recupera informazioni sull'utente corrente";
    operation.Description = "Restituisce i dettagli dell'utente autenticato inclusi ruoli e permessi";
    return operation;
});

// Endpoint protetto, accessibile solo agli amministratori
app.MapGet("/admin", (HttpContext context) =>
{
    // Recupera il nome dell'utente dalle claims del token
    var username = context.User?.Identity?.Name;

    // Ottieni tutti i ruoli dell'utente dalle claims
    // Garantisce che userRoles non sia mai null - sarà un array vuoto nel caso peggiore
    var userRoles = context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ?? [];

    // Ottieni altri dettagli utente
    var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    return Results.Ok(new
    {
        Message = $"Benvenuto {username}! Hai accesso all'area amministrativa.",
        AdminInfo = new
        {
            UserId = userId,
            AccessLevel = "Full",
            Roles = userRoles,
            IsAdmin = userRoles.Contains("Administrator"),
            IsSuperAdmin = userRoles.Contains("SuperAdministrator"),
            AllowedOperations = GetAllowedOperationsForRoles(userRoles)
        }
    });
})
.RequireAuthorization("RequireAdministratorRole") // Richiede la policy specifica
.WithName("AdminArea")
.WithOpenApi(operation =>
{
    operation.Summary = "Area amministrativa riservata";
    operation.Description = "Questo endpoint è accessibile solo agli utenti con ruolo Administrator";
    return operation;
});

// Endpoint per il rinnovo del token - NON richiede l'autenticazione
app.MapPost("/refresh", (RefreshRequest request) =>
{
    // Validazione input
    if (request is null || string.IsNullOrEmpty(request.RefreshToken))
    {
        return Results.BadRequest("Refresh token richiesto");
    }

    // Cerca l'utente associato al refresh token - Thread safe con ConcurrentDictionary
    if (refreshTokenToUserMap.TryGetValue(request.RefreshToken, out var userId) &&
        refreshTokenStore.TryGetValue(userId, out var tokenInfo) &&
        tokenInfo.Token == request.RefreshToken &&
        tokenInfo.ExpiresAt > DateTime.UtcNow)
    {
        // Recupera i dati dell'utente in base all'ID
        List<Claim> claims;

        if (userId == "user123")
        {
            claims = [
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, "user@example.com"),
                new Claim(ClaimTypes.Name, "user"),
                new Claim(ClaimTypes.Role, "Viewer")
            ];
        }
        else if (userId == "admin456")
        {
            claims = [
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, "admin@example.com"),
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "Administrator"),
                new Claim(ClaimTypes.Role, "SuperAdministrator")
            ];
        }
        else
        {
            claims = [
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "unknown")
            ];
        }

        var newAccessToken = GenerateAccessToken(builder.Configuration, claims);
        var newRefreshToken = GenerateRefreshToken();

        // Thread safe operations con ConcurrentDictionary
        refreshTokenToUserMap.TryRemove(request.RefreshToken, out _);

        // Aggiorna il refresh token memorizzato con nuova scadenza
        refreshTokenStore[userId] = new RefreshTokenInfo
        {
            Token = newRefreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        // Aggiungi il nuovo mapping - Thread safe
        refreshTokenToUserMap[newRefreshToken] = userId;

        return Results.Ok(new
        {
            accessToken = newAccessToken,
            refreshToken = newRefreshToken
        });
    }

    return Results.Unauthorized();
})
.WithName("RefreshToken")
.WithOpenApi(operation =>
{
    operation.Summary = "Rinnova l'access token utilizzando un refresh token valido";
    return operation;
});

// Implementazione semplificata dell'endpoint logout che gestisce solo i refresh token
app.MapPost("/logout", (HttpContext context, LogoutRequest? request) =>
{
    var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    if (string.IsNullOrEmpty(userId))
    {
        return Results.BadRequest("Utente non identificato");
    }

    // Caso 1: Se viene fornito un refresh token specifico
    if (request != null && !string.IsNullOrEmpty(request.RefreshToken))
    {
        // Verifica che il refresh token appartenga all'utente corrente
        if (refreshTokenToUserMap.TryGetValue(request.RefreshToken, out var tokenUserId) &&
            tokenUserId == userId)
        {
            // Rimuove solo il refresh token specifico
            refreshTokenToUserMap.TryRemove(request.RefreshToken, out _);

            // Se è l'unico refresh token dell'utente, rimuovi anche l'entry dallo store
            if (refreshTokenStore.TryGetValue(userId, out var storedToken) &&
                storedToken.Token == request.RefreshToken)
            {
                refreshTokenStore.TryRemove(userId, out _);
            }

            return Results.Ok(new
            {
                message = "Logout effettuato con successo",
                details = "Refresh token specifico invalidato"
            });
        }

        return Results.BadRequest("Il refresh token fornito non è valido o non appartiene all'utente corrente");
    }

    // Caso 2: Se non viene fornito un refresh token, invalida tutti i refresh token dell'utente
    if (refreshTokenStore.TryGetValue(userId, out var userRefreshTokenInfo))
    {
        var userRefreshToken = userRefreshTokenInfo.Token;

        // Rimuovi dalla mappa inversa
        refreshTokenToUserMap.TryRemove(userRefreshToken, out _);

        // Rimuovi dallo store
        refreshTokenStore.TryRemove(userId, out _);

        return Results.Ok(new
        {
            message = "Logout effettuato con successo",
            details = "Tutti i refresh token dell'utente sono stati invalidati"
        });
    }

    return Results.Ok(new
    {
        message = "Nessun token da invalidare",
        details = "L'utente non aveva sessioni attive"
    });
})
.RequireAuthorization()
.WithName("Logout")
.WithOpenApi(operation =>
{
    operation.Summary = "Effettua il logout invalidando i refresh token";
    operation.Description = "Invalida il refresh token dell'utente, forzando un nuovo login per ottenere nuovi token";
    return operation;
});

// Endpoint semplice di login per redirect da browser (solo per dimostrazione)
app.MapGet("/login-page", () => Results.Content(
    @"<!DOCTYPE html>
<html lang='it'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>JWT Authentication Demo</title>
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f7f9fc;
            margin: 0;
            padding: 0;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
        }
        .login-container {
            background-color: white;
            padding: 40px;
            border-radius: 10px;
            box-shadow: 0 0 20px rgba(0, 0, 0, 0.1);
            width: 100%;
            max-width: 400px;
        }
        .login-header {
            text-align: center;
            margin-bottom: 30px;
        }
        .login-header h1 {
            color: #3498db;
            margin: 0;
        }
        .form-group {
            margin-bottom: 20px;
        }
        .form-group label {
            display: block;
            margin-bottom: 8px;
            font-weight: 500;
            color: #333;
        }
        .form-group input {
            width: 100%;
            padding: 12px;
            border-radius: 5px;
            border: 1px solid #ddd;
            box-sizing: border-box;
            font-size: 16px;
            transition: border-color 0.3s;
        }
        .form-group input:focus {
            border-color: #3498db;
            outline: none;
        }
        .form-group .hint {
            font-size: 12px;
            margin-top: 8px;
            color: #666;
        }
        .btn {
            background-color: #3498db;
            color: white;
            border: none;
            padding: 12px;
            width: 100%;
            border-radius: 5px;
            cursor: pointer;
            font-size: 16px;
            font-weight: 600;
            transition: background-color 0.3s;
        }
        .btn:hover {
            background-color: #2980b9;
        }
        .error-message {
            color: #e74c3c;
            background-color: #fde8e6;
            padding: 10px;
            border-radius: 5px;
            margin-bottom: 20px;
            display: none;
        }
        .user-options {
            display: flex;
            justify-content: space-between;
            margin-top: 30px;
        }
        .user-option {
            padding: 10px;
            background-color: #f1f1f1;
            border-radius: 5px;
            cursor: pointer;
            flex: 1;
            margin: 0 5px;
            text-align: center;
            font-size: 14px;
            transition: all 0.3s;
        }
        .user-option:hover {
            background-color: #e0e0e0;
        }
        .token-display {
            margin-top: 20px;
            display: none;
            word-break: break-all;
        }
        .token-display pre {
            background-color: #f1f1f1;
            padding: 15px;
            border-radius: 5px;
            font-size: 12px;
            max-height: 150px;
            overflow-y: auto;
        }
    </style>
</head>
<body>
    <div class='login-container'>
        <div class='login-header'>
            <h1>Login</h1>
            <p>Accedi per ottenere il token JWT</p>
        </div>
        
        <div id='errorMessage' class='error-message'></div>
        
        <form id='loginForm' onsubmit='return false;'>
            <div class='form-group'>
                <label for='username'>Username</label>
                <input type='text' id='username' name='username' required>
                <div class='hint'>Prova con 'user' o 'admin'</div>
            </div>
            
            <div class='form-group'>
                <label for='password'>Password</label>
                <input type='password' id='password' name='password' required>
                <div class='hint'>Password: 'pass' per user, 'Admin123!' per admin</div>
            </div>
            
            <button type='button' class='btn' onclick='login()'>Login</button>
        </form>
        
        <div class='user-options'>
            <div class='user-option' onclick=""fillForm('user', 'pass')"">
                Utente Standard
            </div>
            <div class='user-option' onclick=""fillForm('admin', 'Admin123!')"">
                Amministratore
            </div>
        </div>
        
        <div id='tokenDisplay' class='token-display'>
            <h3>Token ottenuto:</h3>
            <pre id='accessToken'></pre>
            <h3>Refresh Token:</h3>
            <pre id='refreshToken'></pre>
            <button class='btn' style='margin-top:10px;' onclick='testProtectedEndpoint()'>Testa API protetta</button>
            <button class='btn' style='margin-top:10px; background-color: #27ae60;' onclick='testAdminEndpoint()'>Testa API amministrativa</button>
        </div>
    </div>

    <script>
        function fillForm(username, password) {
            document.getElementById('username').value = username;
            document.getElementById('password').value = password;
        }

        let currentToken = null;
        
        async function login() {
            const username = document.getElementById('username').value;
            const password = document.getElementById('password').value;
            const errorMessage = document.getElementById('errorMessage');
            
            if (!username || !password) {
                showError('Inserisci username e password');
                return;
            }
            
            try {
                const response = await fetch('/login', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Accept': 'application/json'
                    },
                    body: JSON.stringify({
                        username: username,
                        password: password
                    })
                });
                
                if (!response.ok) {
                    throw new Error('Credenziali non valide');
                }
                
                const data = await response.json();
                currentToken = data.accessToken;
                
                document.getElementById('accessToken').textContent = data.accessToken;
                document.getElementById('refreshToken').textContent = data.refreshToken;
                document.getElementById('tokenDisplay').style.display = 'block';
                errorMessage.style.display = 'none';
                
            } catch (error) {
                showError(error.message);
                document.getElementById('tokenDisplay').style.display = 'none';
            }
        }
        
        function showError(message) {
            const errorMessage = document.getElementById('errorMessage');
            errorMessage.textContent = message;
            errorMessage.style.display = 'block';
        }
        
        async function testProtectedEndpoint() {
            if (!currentToken) {
                showError('Login necessario prima di testare gli endpoint protetti');
                return;
            }
            
            try {
                const response = await fetch('/protected', {
                    method: 'GET',
                    headers: {
                        'Authorization': `Bearer ${currentToken}`,
                        'Accept': 'application/json'
                    }
                });
                
                if (!response.ok) {
                    throw new Error('Accesso negato all\'endpoint protetto');
                }
                
                const data = await response.json();
                alert(`Risposta dall'endpoint protetto: ${JSON.stringify(data, null, 2)}`);
                
            } catch (error) {
                showError(error.message);
            }
        }
        
        async function testAdminEndpoint() {
            if (!currentToken) {
                showError('Login necessario prima di testare gli endpoint protetti');
                return;
            }
            
            try {
                const response = await fetch('/admin', {
                    method: 'GET',
                    headers: {
                        'Authorization': `Bearer ${currentToken}`,
                        'Accept': 'application/json'
                    }
                });
                
                if (!response.ok) {
                    if (response.status === 403) {
                        throw new Error('Non hai i permessi per accedere all\'area amministrativa');
                    } else {
                        throw new Error('Errore di accesso all\'endpoint amministrativo');
                    }
                }
                
                const data = await response.json();
                alert(`Risposta dall'endpoint admin: ${JSON.stringify(data, null, 2)}`);
                
            } catch (error) {
                showError(error.message);
            }
        }
    </script>
</body>
</html>",
    "text/html"))
// Assicuriamoci che questo endpoint sia accessibile senza autenticazione
.AllowAnonymous();

// Endpoint di accesso negato per redirect da browser
app.MapGet("/access-denied", () => Results.Content(
    "<html><body><h1>Accesso Negato</h1><p>Non hai i permessi necessari per accedere alla risorsa richiesta.</p>" +
    "<p><a href='/login-page'>Torna al login</a></p></body></html>",
    "text/html"))
// Assicuriamoci che questo endpoint sia accessibile senza autenticazione
.AllowAnonymous();

app.Run();

// Helper per determinare se una richiesta proviene da un browser web
bool IsHtmlRequest(HttpRequest request)
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

// Helper per determinare le operazioni consentite in base ai ruoli
string[] GetAllowedOperationsForRoles(string[] roles)
{
    if (roles.Length == 0)
        return UserRoles.EmptyRoles;

    var operations = new HashSet<string>();

    foreach (var role in roles)
    {
        switch (role)
        {
            case "Administrator":
                operations.Add("Create");
                operations.Add("Read");
                operations.Add("Update");
                operations.Add("Delete");
                break;

            case "Editor":
                operations.Add("Read");
                operations.Add("Update");
                break;

            case "Viewer":
                operations.Add("Read");
                break;

            case "SuperAdministrator":
                operations.Add("Create");
                operations.Add("Read");
                operations.Add("Update");
                operations.Add("Delete");
                operations.Add("Manage Users");
                operations.Add("System Configuration");
                break;
        }
    }

    return [.. operations];
}

// Definizione di una classe per memorizzare informazioni aggiuntive sul refresh token
public class RefreshTokenInfo
{
    public required string Token { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

// Modelli di record per le richieste
public record UserLogin(string Username, string Password);
public record RefreshRequest(string RefreshToken);
public record LogoutRequest(string RefreshToken);

// Array statici per ruoli comuni
static class UserRoles
{
    public static readonly string[] ViewerRoles = ["Viewer"];
    public static readonly string[] AdminRoles = ["Administrator", "SuperAdministrator"];
    public static readonly string[] EmptyRoles = [];
}
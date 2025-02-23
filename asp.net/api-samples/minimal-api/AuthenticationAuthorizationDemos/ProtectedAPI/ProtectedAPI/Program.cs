using Microsoft.EntityFrameworkCore;
using ProtectedAPI.Data;
using ProtectedAPI.Endpoints;
using ProtectedAPI.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using NSwag.Generation.Processors.Security;

var builder = WebApplication.CreateBuilder(args);

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

// Add Identity services with API endpoints
builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
	.AddRoles<IdentityRole>()
	.AddEntityFrameworkStores<AppDbContext>();

// Configure authorization policies
builder.Services.AddAuthorizationBuilder()
	.AddPolicy("RequireMemberRole", policy =>
		policy.RequireRole("Member"))
	.AddPolicy("RequireAdminRole", policy =>
		policy.RequireRole("Admin"));

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

// Map Identity endpoints
app.MapIdentityApi<ApplicationUser>();

// Add custom registration endpoint with role (only for Admin)
app.MapPost("/register-with-role", async (
    UserManager<ApplicationUser> userManager,
    string email,
    string password,
    string? role) =>
{
    // Validate role
    if (!string.IsNullOrEmpty(role) && role != "Member" && role != "Admin")
    {
        return Results.BadRequest("Invalid role specified. Role must be either 'Member' or 'Admin'");
    }

    var user = new ApplicationUser
    {
        UserName = email,
        Email = email
    };

    var result = await userManager.CreateAsync(user, password);

    if (result.Succeeded && !string.IsNullOrEmpty(role))
    {
        await userManager.AddToRoleAsync(user, role);
        return Results.Ok($"User created successfully with role {role}");
    }
    else if (result.Succeeded)
    {
        return Results.Ok("User created successfully without role");
    }

    return Results.BadRequest(result.Errors);
})
.RequireAuthorization("RequireAdminRole")  // Only admin can create users with roles
.WithOpenApi()
.WithTags("Identity");

// Create default roles and admin user if they don't exist
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    
    var roles = new[] { "Admin", "Member" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    // Create default admin if it doesn't exist
    var adminEmail = "admin@example.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        
        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

app.MapGroup("/api").MapTodoEndpoints().WithOpenApi().WithTags("Todos API");

app.Run();



using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ProtectedAPI.Model;

namespace ProtectedAPI.Data;

public static class DbInitializer
{
    public static async Task Initialize(IServiceProvider serviceProvider, bool applyMigrations = false)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<AppDbContext>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        try
        {
            // Get the DbContext
            var context = serviceProvider.GetRequiredService<AppDbContext>();

            if (applyMigrations)
            {
                logger.LogInformation("Applying database migrations...");
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully");
            }

            // Continue with seeding
            await InitializeRoles(serviceProvider);
            await InitializeAdminUser(serviceProvider, configuration);
            await InitializeTodos(serviceProvider);

            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database");
            throw; // Rilancia l'eccezione per permettere all'applicazione di gestirla appropriatamente
        }
    }

    private static async Task InitializeRoles(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var roles = new[] { "Admin", "Member" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task InitializeAdminUser(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var adminEmail = configuration["AdminCredentials:Email"];
        var adminPassword = configuration["AdminCredentials:Password"];

        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
        {
            throw new InvalidOperationException("Admin credentials are not configured. Please check your configuration.");
        }

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                throw new InvalidOperationException($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }

    private static async Task InitializeTodos(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        // Check if there are already todos
        if (await context.Todos.AnyAsync())
            return;

        // Add sample todos
        var todos = new[]
        {
            new Todo
            {
                Title = "Completare il tutorial ASP.NET",
                Description = "Studiare e implementare autenticazione e autorizzazione",
                IsComplete = false
            },
            new Todo
            {
                Title = "Preparare la presentazione",
                Description = "Creare slides sul progetto web",
                IsComplete = false
            },
            new Todo
            {
                Title = "Testing API",
                Description = "Verificare il funzionamento delle API con Swagger",
                IsComplete = false
            }
        };

        context.Todos.AddRange(todos);
        await context.SaveChangesAsync();
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using EducationalGames.Models; 

namespace EducationalGames.Data; 

public static class DatabaseInitializer
{
    // Metodo per inizializzare DB e fare seeding
    public static async Task InitializeAndSeedAsync(IServiceProvider services, IWebHostEnvironment environment)
    {
        // Ottieni i servizi necessari dallo scope
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        // Crea il logger usando una stringa per la categoria
        var logger = loggerFactory.CreateLogger("EducationalGames.Data.DatabaseInitializer");
        var dbContext = services.GetRequiredService<AppDbContext>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var passwordHasher = services.GetRequiredService<PasswordHasher<Utente>>();

        logger.LogInformation("Starting database initialization and seeding...");

        try
        {
            // Applica migrazioni (solo in sviluppo per sicurezza)
            if (environment.IsDevelopment())
            {
                logger.LogInformation("Development environment detected. Applying database migrations...");
                // Nota: MigrateAsync() è idempotente, applica solo migrazioni pendenti.
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully (or database already up-to-date).");
            }

            // Seed Admin User
            logger.LogInformation("Checking for existing Admin user...");
            // Usiamo ToLower per confronto case-insensitive standard, anche se Enum di solito non serve
            if (!await dbContext.Utenti.AnyAsync(u => u.Ruolo == RuoloUtente.Admin))
            {
                logger.LogWarning("No Admin user found. Attempting to seed default Admin...");

                var adminEmail = configuration["DefaultAdminCredentials:Email"];
                var adminPassword = configuration["DefaultAdminCredentials:Password"];
                var adminNome = configuration["DefaultAdminCredentials:Nome"] ?? "Admin";
                var adminCognome = configuration["DefaultAdminCredentials:Cognome"] ?? "Default";

                if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
                {
                    logger.LogError("Default Admin Email or Password not found in configuration. Cannot seed Admin user.");
                }
                else if (adminPassword.Length < 8)
                {
                    logger.LogError("Default Admin Password must be at least 8 characters long. Cannot seed Admin user.");
                }
                else
                {
                    var adminUser = new Utente
                    {
                        Nome = adminNome,
                        Cognome = adminCognome,
                        Email = adminEmail,
                        Ruolo = RuoloUtente.Admin
                    };
                    adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, adminPassword);

                    dbContext.Utenti.Add(adminUser);
                    await dbContext.SaveChangesAsync();
                    logger.LogInformation("Default Admin user '{Email}' created successfully.", adminEmail);
                }
            }
            else
            {
                logger.LogInformation("Admin user already exists. Skipping seeding.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database initialization or seeding.");
            // Considera se terminare l'applicazione qui in caso di errore critico di inizializzazione DB
            // throw;
        }
    }
}
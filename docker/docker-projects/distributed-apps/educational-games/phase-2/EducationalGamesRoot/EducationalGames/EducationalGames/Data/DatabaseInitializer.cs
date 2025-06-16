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
            // // Applica migrazioni (solo in sviluppo per sicurezza)
            // if (environment.IsDevelopment())
            // {
                logger.LogInformation("Development environment detected. Applying database migrations...");
                // Nota: MigrateAsync() è idempotente, applica solo migrazioni pendenti.
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully (or database already up-to-date).");
            //}

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
                // Attempt to parse the configuration value, default to true if missing or invalid
                bool adminEmailVerified = true; // Default value
                string? emailVerifiedString = configuration["DefaultAdminCredentials:EmailVerificata"];
                if (!string.IsNullOrEmpty(emailVerifiedString) && bool.TryParse(emailVerifiedString, out bool parsedValue))
                {
                    adminEmailVerified = parsedValue;
                }

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
                        Ruolo = RuoloUtente.Admin,
                        EmailVerificata = adminEmailVerified
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
            // --- SEEDING MATERIE, ARGOMENTI, GIOCHI ---

            // Seed Materie (se non presenti)
            if (!await dbContext.Materie.AnyAsync())
            {
                logger.LogInformation("Seeding Materie...");
                var materie = new List<Materia>
                {
                    new Materia { Nome = "Matematica" },
                    new Materia { Nome = "Fisica" },
                    new Materia { Nome = "Italiano" },
                    new Materia { Nome = "Storia" },
                    new Materia { Nome = "Inglese" }
                };
                await dbContext.Materie.AddRangeAsync(materie);
                await dbContext.SaveChangesAsync();
            }
            else { logger.LogInformation("Materie already exist."); }

            // Seed Argomenti (se non presenti)
            List<Argomento> argomentiDb; // Lista per tenere traccia degli argomenti (nuovi o esistenti)
            if (!await dbContext.Argomenti.AnyAsync())
            {
                logger.LogInformation("Seeding Argomenti...");
                argomentiDb =
                 [
                     new Argomento { Nome = "Algebra di Base" },
                     new Argomento { Nome = "Geometria Piana" },
                     new Argomento { Nome = "Meccanica Classica" },
                     new Argomento { Nome = "Elettromagnetismo" },
                     new Argomento { Nome = "Analisi Grammaticale" },
                     new Argomento { Nome = "Analisi Logica" },
                     new Argomento { Nome = "Letteratura Italiana Medievale" }
                 ];
                await dbContext.Argomenti.AddRangeAsync(argomentiDb);
                await dbContext.SaveChangesAsync(); // Salva per ottenere gli ID generati
            }
            else
            {
                logger.LogInformation("Argomenti already exist. Fetching existing ones...");
                argomentiDb = await dbContext.Argomenti.ToListAsync(); // Recupera argomenti esistenti
            }

            // Seed Videogiochi (se non presenti)
            if (!await dbContext.Videogiochi.AnyAsync())
            {
                logger.LogInformation("Seeding Videogiochi...");
                // Recupera gli argomenti specifici per l'associazione
                var algebra = argomentiDb.FirstOrDefault(a => a.Nome == "Algebra di Base");
                var geometria = argomentiDb.FirstOrDefault(a => a.Nome == "Geometria Piana");
                var meccanica = argomentiDb.FirstOrDefault(a => a.Nome == "Meccanica Classica");
                var grammatica = argomentiDb.FirstOrDefault(a => a.Nome == "Analisi Grammaticale");
                var logica = argomentiDb.FirstOrDefault(a => a.Nome == "Analisi Logica"); 

                // Verifica che gli argomenti necessari siano stati trovati
                if (algebra != null && geometria != null && meccanica != null && grammatica != null && logica != null)
                {
                    var giochi = new List<Videogioco>
                        {
                            new() {
                                Titolo = "Quiz Equazioni 1° Grado",
                                DescrizioneBreve = "Risolvi equazioni lineari semplici.",
                                DescrizioneEstesa = "Un quiz a risposta multipla per testare la capacità di risolvere equazioni di primo grado con una incognita.",
                                Immagine1 = "/assets/images/math.png",
                                MaxMonete = 100,
                                DefinizioneGioco = """
                                {
                                    "versioneFormato": "1.0-quiz-multiplo",
                                    "domande": [
                                        {
                                            "id": "q1",
                                            "testo": "2x + 3 = 7, x = ?",
                                            "risposte": [
                                                {"id": "a", "testo": "1"},
                                                {"id": "b", "testo": "2"},
                                                {"id": "c", "testo": "3"},
                                                {"id": "d", "testo": "4"}
                                            ],
                                            "corretta": "b"
                                        }
                                    ]
                                }
                                """,
                                Argomenti = [algebra]
                            },
                            new() {
                                Titolo = "Aree Poligoni Fondamentali",
                                DescrizioneBreve = "Calcola l'area di quadrati e rettangoli.",
                                MaxMonete = 120,
                                DescrizioneEstesa = "Un quiz a risposta multipla per testare la capacità di calcolare le aree di poligoni fondamentali.",
                                Immagine1 = "/assets/images/geometry.png",
                                DefinizioneGioco =
                                """
                                {
                                    "versioneFormato": "1.0-quiz-multiplo",
                                    "domande": [
                                        {
                                            "id": "q1",
                                            "testo": "Area rettangolo base 5, altezza 3?",
                                            "risposte": [
                                                {"id": "a", "testo": "15"},
                                                {"id": "b", "testo": "8"},
                                                {"id": "c", "testo": "16"}
                                            ],
                                            "corretta": "a"
                                        },
                                        {
                                            "id": "q2",
                                            "testo": "Area quadrato lato 4?",
                                            "risposte": [
                                                {"id": "a", "testo": "8"},
                                                {"id": "b", "testo": "16"},
                                                {"id": "c", "testo": "12"}
                                            ],
                                            "corretta": "b"
                                        }
                                    ]
                                }
                                """,
                                Argomenti = [geometria]
                            },
                            new() {
                                Titolo = "Quiz Principi della Dinamica",
                                DescrizioneBreve = "Domande sui tre principi di Newton.",
                                DescrizioneEstesa = "Un quiz a risposta multipla per testare la conoscenza dei principi della dinamica.",
                                Immagine1 = "/assets/images/physics.png",
                                MaxMonete = 150,
                                DefinizioneGioco =
                                """
                                {
                                    "versioneFormato": "1.0-quiz-multiplo",
                                    "domande": [
                                        {
                                            "id": "q1",
                                            "testo": "F = ?",
                                            "risposte": [
                                                {"id": "a", "testo": "m/a"},
                                                {"id": "b", "testo": "m*a"},
                                                {"id": "c", "testo": "a/m"}
                                            ],
                                            "corretta": "b"
                                        }
                                    ]
                                }
                                """,
                                Argomenti = [meccanica]
                            },
                            new() {
                                Titolo = "Soggetto e Predicato",
                                DescrizioneBreve = "Identifica il soggetto e il predicato nelle frasi.",
                                DescrizioneEstesa = "Un quiz a risposta multipla per testare la conoscenza della grammatica italiana.",
                                Immagine1 = "/assets/images/grammar.png",
                                MaxMonete = 80,
                                DefinizioneGioco =
                                """
                                {
                                    "versioneFormato": "1.0-quiz-multiplo",
                                    "domande": [
                                        {
                                            "id": "q1",
                                            "testo": "Il cane abbaia. Soggetto?",
                                            "risposte": [
                                                {"id": "a", "testo": "abbaia"},
                                                {"id": "b", "testo": "Il cane"}
                                            ],
                                            "corretta": "b"
                                        },
                                        {
                                            "id": "q2",
                                            "testo": "Il cane abbaia. Predicato?",
                                            "risposte": [
                                                {"id": "a", "testo": "abbaia"},
                                                {"id": "b", "testo": "Il cane"}
                                            ],
                                            "corretta": "a"
                                        }
                                    ]
                                }
                                """,
                                Argomenti = [grammatica, logica]
                            }
                        };
                    await dbContext.Videogiochi.AddRangeAsync(giochi);
                    // SaveChanges si occuperà di creare i record nelle tabelle GIOCHI_ARGOMENTI e VIDEOGIOCHI
                    await dbContext.SaveChangesAsync();
                    logger.LogInformation("Videogiochi and relations seeded successfully.");
                }
                else
                {
                    logger.LogError("Cannot seed Videogiochi: required Argomenti not found in database.");
                }
            }
            else { logger.LogInformation("Videogiochi already exist."); }

            // --- FINE NUOVO SEEDING ---

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database initialization or seeding.");
            // Considera se terminare l'applicazione qui in caso di errore critico di inizializzazione DB
            // throw;
        }
    }
}
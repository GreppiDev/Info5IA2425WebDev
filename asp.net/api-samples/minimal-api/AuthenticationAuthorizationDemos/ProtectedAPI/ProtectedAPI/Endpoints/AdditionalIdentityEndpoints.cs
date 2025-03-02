using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ProtectedAPI.Model;
using ProtectedAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using ProtectedAPI.Data;
using System.Text.Json;

namespace ProtectedAPI.Endpoints;

// Rimosso il record LogoutRequest perché non è più necessario
public record RegisterWithRoleRequest(
    [Description("The user's email address")]
    [EmailAddress]
    string Email,

    [Description("The user's password")]
    string Password,

    [Description("The role to assign to the user ('Member' or 'Admin')")]
    string? Role);

public record UserRoleRequest(
    [Description("The user's email address")]
    [EmailAddress]
    [Required]
    string Email,

    [Description("The role to assign or remove ('Member' or 'Admin')")]
    [Required]
    string Role);

public record UserDeleteRequest(
    [Description("The user's email address")]
    [EmailAddress]
    [Required]
    string Email);

public class PersonalDataExport
{
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public int AccessFailedCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public IList<string> Roles { get; set; } = new List<string>();
    public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
}

public static class AdditionalIdentityEndpoints
{
    public static void MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        // Logout endpoint supporting both cookie and token authentication
        app.MapPost("/logout", async (
            HttpContext context,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<Program> logger) =>
        {
            logger.LogInformation("Ricevuta richiesta di logout");

            // Verifica se c'è un token JWT nell'header
            bool isJwtAuth = false;
            bool tokenProcessed = false;

            if (context.Request.Headers.TryGetValue("Authorization", out var authValues))
            {
                foreach (var authHeader in authValues)
                {
                    if (string.IsNullOrEmpty(authHeader))
                        continue;

                    if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        isJwtAuth = true;
                        logger.LogInformation("Rilevato token JWT nell'header Authorization");
                        
                        // Ottieni l'ID utente dal ClaimsPrincipal
                        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                        if (!string.IsNullOrEmpty(userId))
                        {
                            logger.LogInformation("ID utente trovato nel token: {UserId}", userId);
                            
                            var user = await userManager.FindByIdAsync(userId);
                            if (user != null)
                            {
                                logger.LogInformation("Utente trovato nel database, aggiorno il security stamp");
                                
                                // Invalida tutti i token cambiando il security stamp
                                await userManager.UpdateSecurityStampAsync(user);
                                
                                // Trova e revoca eventuali refresh token
                                var oldStamp = context.User.FindFirstValue("AspNet.Identity.SecurityStamp");
                                if (!string.IsNullOrEmpty(oldStamp))
                                {
                                    var tokenPurpose = $"RefreshToken:{user.Id}:{oldStamp}";
                                    await userManager.RemoveAuthenticationTokenAsync(user, "RefreshTokenProvider", tokenPurpose);
                                    logger.LogInformation("Refresh token revocato per il vecchio security stamp");
                                }
                                
                                tokenProcessed = true;
                                break;
                            }
                            else
                            {
                                logger.LogWarning("Utente {UserId} non trovato nel database", userId);
                            }
                        }
                        else
                        {
                            logger.LogWarning("Nessun ID utente trovato nel token JWT");
                        }
                    }
                }
            }

            // Se l'utente è autenticato e NON sta usando JWT, allora è autenticazione basata su cookie
            if (context.User.Identity?.IsAuthenticated == true && !isJwtAuth)
            {
                logger.LogInformation("Autenticazione basata su cookie rilevata");
                
                await signInManager.SignOutAsync();
                logger.LogInformation("Logout cookie completato con successo");
                
                return Results.Ok(new { message = "Logout effettuato con successo." });
            }
            
            // Se è stata processata l'invalidazione del token JWT
            if (tokenProcessed)
            {
                return Results.Ok(new { message = "Token JWT invalidato correttamente." });
            }
            
            logger.LogWarning("Nessuna sessione attiva trovata");
            return Results.Ok(new { message = "Nessuna sessione attiva." });
        })
        .RequireAuthorization() // Richiede autenticazione per accedere a questo endpoint
        .WithName("Logout")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Log out";
            operation.Description = "Signs out the current user. For token-based auth, invalidates the tokens by updating the security stamp (requires token in Authorization header). For cookie-based auth, clears the authentication cookie.";
            operation.Tags = new List<OpenApiTag> { new() { Name = "Additional Identity Endpoints" } };
            return operation;
        });

        // GDPR Data Export endpoint (for authenticated users to export their own data)
        app.MapGet("/gdpr/export-personal-data", async (
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            AppDbContext dbContext) =>
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user == null)
            {
                return Results.Unauthorized();
            }

            // Create a container for all personal data
            var personalData = new PersonalDataExport
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LockoutEnd = user.LockoutEnd,
                LockoutEnabled = user.LockoutEnabled,
                AccessFailedCount = user.AccessFailedCount,
                Roles = await userManager.GetRolesAsync(user)
            };

            // Collect any user-specific data from other tables
            // Example: Get user's todos (if you're tracking user ownership)
            try
            {
                var todos = await dbContext.Todos
                    .Where(t => t.OwnerId == user.Id)  // Assuming you have an OwnerId property
                    .Select(t => new { t.Id, t.Title, t.Description, t.IsComplete, t.CreatedAt })
                    .ToListAsync();

                if (todos.Any())
                {
                    personalData.AdditionalData["Todos"] = todos;
                }
            }
            catch
            {
                // If the Todos table doesn't have an OwnerId field or other issue
                // Just continue without adding todos data
            }

            // Add login history if you track it
            try
            {
                var logins = await userManager.GetLoginsAsync(user);
                if (logins.Any())
                {
                    personalData.AdditionalData["ExternalLogins"] = logins.Select(l =>
                        new { l.LoginProvider, l.ProviderKey }).ToList();
                }
            }
            catch
            {
                // Continue if unable to get logins
            }

            // Export all collected data as JSON
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            // Generate a filename for the download
            var fileName = $"personal-data-export-{DateTime.UtcNow:yyyy-MM-dd}.json";

            return Results.File(
                JsonSerializer.SerializeToUtf8Bytes(personalData, options),
                "application/json",
                fileName
            );
        })
        .RequireAuthorization()
        .ValidateSecurityStamp()
        .WithName("ExportPersonalData")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Export personal data (GDPR)";
            operation.Description = "Exports all personal data associated with the current user in compliance with GDPR right to data portability.";
            operation.Tags = new List<OpenApiTag> { new() { Name = "GDPR Compliance" } };
            return operation;
        });

        // GDPR Delete Account endpoint (for authenticated users to delete their own account)
        app.MapDelete("/gdpr/delete-account", async (
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AppDbContext dbContext) =>
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user == null)
            {
                return Results.Unauthorized();
            }

            // Begin a transaction to ensure all data is deleted consistently
            using var transaction = await dbContext.Database.BeginTransactionAsync();

            try
            {
                // Delete user-related data from other tables first
                // Example: Delete user's todos
                try
                {
                    var userTodos = await dbContext.Todos
                        .Where(t => t.OwnerId == user.Id)  // Assuming you have an OwnerId property
                        .ToListAsync();

                    if (userTodos.Any())
                    {
                        dbContext.Todos.RemoveRange(userTodos);
                        await dbContext.SaveChangesAsync();
                    }
                }
                catch
                {
                    // If the Todos table doesn't have an OwnerId field or other issue
                    // Just continue with account deletion
                }

                // Remove external logins
                var logins = await userManager.GetLoginsAsync(user);
                foreach (var login in logins)
                {
                    await userManager.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);
                }

                // Remove user's roles
                var roles = await userManager.GetRolesAsync(user);
                if (roles.Any())
                {
                    await userManager.RemoveFromRolesAsync(user, roles);
                }

                // Remove user tokens and claims
                var claims = await userManager.GetClaimsAsync(user);
                foreach (var claim in claims)
                {
                    await userManager.RemoveClaimAsync(user, claim);
                }

                // Delete the user account
                var result = await userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    // If deletion fails, roll back transaction
                    await transaction.RollbackAsync();

                    return Results.ValidationProblem(result.Errors.ToDictionary(
                        e => e.Code,
                        e => new[] { e.Description }
                    ));
                }

                // Sign out the user
                await signInManager.SignOutAsync();

                // Commit the transaction
                await transaction.CommitAsync();

                return Results.Ok(new
                {
                    Message = "Your account and all personal data have been successfully deleted."
                });
            }
            catch
            {
                // If any exception occurs, roll back transaction
                await transaction.RollbackAsync();
                return Results.Problem(
                    title: "Account deletion failed",
                    detail: "An error occurred while deleting your account. Please try again later.",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        })
        .RequireAuthorization()
        .ValidateSecurityStamp()
        .WithName("DeletePersonalAccount")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Delete account and personal data (GDPR)";
            operation.Description = "Permanently deletes the user's account and all associated personal data in compliance with GDPR right to erasure ('right to be forgotten').";
            operation.Tags = new List<OpenApiTag> { new() { Name = "GDPR Compliance" } };
            return operation;
        });

        // Delete user endpoint (Admin only)
        app.MapDelete("/users/delete", async (
            UserManager<ApplicationUser> userManager,
            [FromBody] UserDeleteRequest request) =>
        {
            // Find the user
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Results.NotFound($"User with email {request.Email} not found.");
            }

            // Delete the user
            // ASP.NET Core Identity automatically removes related records in AspNetUserRoles table
            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return Results.ValidationProblem(result.Errors.ToDictionary(
                    e => e.Code,
                    e => new[] { e.Description }
                ));
            }

            return Results.Ok($"User {request.Email} has been successfully deleted.");
        })
        .RequireAuthorization("RequireAdminRole")
        .ValidateSecurityStamp()
        .WithName("DeleteUser")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Delete user";
            operation.Description = "Deletes a user and all associated roles. Only accessible by administrators.";
            operation.Tags = new List<OpenApiTag> { new() { Name = "Additional Identity Endpoints" } };
            return operation;
        });

        // Add role to user endpoint (Admin only)
        app.MapPost("/users/add-role", async (
            UserManager<ApplicationUser> userManager,
            [FromBody] UserRoleRequest request) =>
        {
            // Validate role
            if (request.Role != "Member" && request.Role != "Admin")
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { "Role", new[] { "Invalid role specified. Role must be either 'Member' or 'Admin'" } }
                });
            }

            // Find the user
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Results.NotFound($"User with email {request.Email} not found.");
            }

            // Check if user already has this role
            if (await userManager.IsInRoleAsync(user, request.Role))
            {
                return Results.BadRequest($"User already has the role '{request.Role}'.");
            }

            // Add the role to the user
            var result = await userManager.AddToRoleAsync(user, request.Role);
            if (!result.Succeeded)
            {
                return Results.ValidationProblem(result.Errors.ToDictionary(
                    e => e.Code,
                    e => new[] { e.Description }
                ));
            }

            // Update security stamp to invalidate existing tokens
            await userManager.UpdateSecurityStampAsync(user);

            return Results.Ok($"Role '{request.Role}' added to user {request.Email} successfully.");
        })
        .RequireAuthorization("RequireAdminRole")
        .ValidateSecurityStamp()
        .WithName("AddRoleToUser")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Add role to user";
            operation.Description = "Adds a role to an existing user. Only accessible by administrators.";
            operation.Tags = new List<OpenApiTag> { new() { Name = "Additional Identity Endpoints" } };
            return operation;
        });

        // Remove role from user endpoint (Admin only)
        app.MapPost("/users/remove-role", async (
            UserManager<ApplicationUser> userManager,
            [FromBody] UserRoleRequest request) =>
        {
            // Validate role
            if (request.Role != "Member" && request.Role != "Admin")
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { "Role", new[] { "Invalid role specified. Role must be either 'Member' or 'Admin'" } }
                });
            }

            // Find the user
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Results.NotFound($"User with email {request.Email} not found.");
            }

            // Check if user has this role
            if (!await userManager.IsInRoleAsync(user, request.Role))
            {
                return Results.BadRequest($"User does not have the role '{request.Role}'.");
            }

            // Remove the role from the user
            var result = await userManager.RemoveFromRoleAsync(user, request.Role);
            if (!result.Succeeded)
            {
                return Results.ValidationProblem(result.Errors.ToDictionary(
                    e => e.Code,
                    e => new[] { e.Description }
                ));
            }

            // Update security stamp to invalidate existing tokens
            await userManager.UpdateSecurityStampAsync(user);

            return Results.Ok($"Role '{request.Role}' removed from user {request.Email} successfully.");
        })
        .RequireAuthorization("RequireAdminRole")
        .ValidateSecurityStamp()
        .WithName("RemoveRoleFromUser")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Remove role from user";
            operation.Description = "Removes a role from an existing user. Only accessible by administrators.";
            operation.Tags = new List<OpenApiTag> { new() { Name = "Additional Identity Endpoints" } };
            return operation;
        });

        // Custom registration with role (only for Admin)
        app.MapPost("/users/register-with-role", async (
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IOptions<IdentitySettings> identitySettings,
            HttpContext context,
            [FromBody] RegisterWithRoleRequest request) =>
        {
            // Validate role
            if (!string.IsNullOrEmpty(request.Role) && request.Role != "Member" && request.Role != "Admin")
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { "Role", new[] { "Invalid role specified. Role must be either 'Member' or 'Admin'" } }
                });
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = !identitySettings.Value.RequireEmailConfirmation
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(request.Role))
                {
                    var roleResult = await userManager.AddToRoleAsync(user, request.Role);
                    if (!roleResult.Succeeded)
                    {
                        return Results.ValidationProblem(roleResult.Errors.ToDictionary(
                            e => e.Code,
                            e => new[] { e.Description }
                        ));
                    }
                }

                if (identitySettings.Value.RequireEmailConfirmation)
                {
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = $"{context.Request.Scheme}://{context.Request.Host}/confirmEmail?userId={Uri.EscapeDataString(user.Id)}&code={Uri.EscapeDataString(token)}";
                    await emailService.SendEmailConfirmationAsync(request.Email, confirmationLink);

                    return Results.Ok(new
                    {
                        RequiresConfirmation = true,
                        Message = $"User created successfully{(request.Role != null ? $" with role {request.Role}" : "")}. Please check your email to confirm your account."
                    });
                }

                return Results.Ok(new
                {
                    RequiresConfirmation = false,
                    Message = $"User created successfully{(request.Role != null ? $" with role {request.Role}" : "")}."
                });
            }

            return Results.ValidationProblem(result.Errors.ToDictionary(
                e => e.Code,
                e => new[] { e.Description }
            ));
        })
        .RequireAuthorization("RequireAdminRole")
        .WithName("RegisterWithRole")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Register a new user with role";
            operation.Description = """
                Creates a new user account and assigns the specified role. Only accessible by administrators.
                The role must be either 'Member' or 'Admin'. If role is not provided no role is assigned to the user. If email confirmation is required, a confirmation email will be sent.
                Returns a validation problem if the registration fails or if the role is invalid.
                """;
            operation.Tags = new List<OpenApiTag> { new() { Name = "Additional Identity Endpoints" } };

            return operation;
        });

        // External login provider endpoint
        app.MapPost("/login/{provider}", (
            string provider,
            [FromQuery] string? returnUrl,
            SignInManager<ApplicationUser> signInManager) =>
        {
            // Configure external authentication properties
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, returnUrl);
            // Challenge the provider which triggers the OAuth flow
            return TypedResults.Challenge(properties, new[] { provider });
        })
        .WithName("ExternalLogin")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Login with external provider";
            operation.Description = """
                Initiates login flow with an external authentication provider (like Google, Facebook, etc.).
                The {provider} parameter specifies which authentication provider to use.
                The optional returnUrl query parameter specifies where to redirect after successful authentication.
                """;
            operation.Tags = new List<OpenApiTag> { new() { Name = "Additional Identity Endpoints" } };

            return operation;
        });

        // External login callback endpoint
        app.MapGet("/login/{provider}/callback", async (
            string provider,
            [FromQuery] string? returnUrl,
            [FromQuery] string? remoteError,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager) =>
        {
            if (remoteError != null)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { "Provider", new[] { $"Error from external provider: {remoteError}" } }
                });
            }

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { "Provider", new[] { "Error loading external login information." } }
                });
            }

            // Sign in the user with the external login provider
            var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                return Results.LocalRedirect(returnUrl ?? "/");
            }

            // If the user does not have an account, create one
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (email != null)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true // Email is verified by the external provider
                    };

                    var createResult = await userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        return Results.ValidationProblem(createResult.Errors.ToDictionary(
                            e => e.Code,
                            e => new[] { e.Description }
                        ));
                    }
                }

                // Add the external login to the user account
                var addLoginResult = await userManager.AddLoginAsync(user, info);
                if (!addLoginResult.Succeeded)
                {
                    return Results.ValidationProblem(addLoginResult.Errors.ToDictionary(
                        e => e.Code,
                        e => new[] { e.Description }
                    ));
                }

                // Sign in the user
                await signInManager.SignInAsync(user, isPersistent: false);
                return Results.LocalRedirect(returnUrl ?? "/");
            }

            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                { "Provider", new[] { "Error signing in with external provider." } }
            });
        })
        .WithName("ExternalLoginCallback")
        .WithOpenApi(operation =>
        {
            operation.Summary = "External login callback";
            operation.Description = """
                Handles the callback from external authentication providers.
                This endpoint is called by the external provider after the user has authenticated.
                If the user doesn't have an account, one will be created using their email from the external provider.
                After successful authentication, redirects to the specified returnUrl or the default page.
                """;
            operation.Tags = new List<OpenApiTag> { new() { Name = "Additional Identity Endpoints" } };

            return operation;
        });

        // Setup 2FA endpoint
        app.MapGet("/login/setup-2fa", async (
            HttpContext context,
            UserManager<ApplicationUser> userManager) =>
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user == null)
            {
                return Results.Unauthorized();
            }

            // Reset the authenticator key
            await userManager.ResetAuthenticatorKeyAsync(user);
            var unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);

            if (string.IsNullOrEmpty(unformattedKey))
            {
                return Results.BadRequest("Error generating 2FA key.");
            }

            // Generate QR code URI
            var email = await userManager.GetEmailAsync(user);
            if (email == null)
            {
                return Results.BadRequest("User email not found.");
            }

            var qrCodeUri = $"otpauth://totp/{Uri.EscapeDataString("ProtectedAPI")}:{Uri.EscapeDataString(email)}?secret={unformattedKey}&issuer={Uri.EscapeDataString("ProtectedAPI")}";

            return Results.Ok(new
            {
                SharedKey = unformattedKey,
                QrCodeUri = qrCodeUri,
                VerificationCodeLength = userManager.Options.Tokens.AuthenticatorTokenProvider.Length
            });
        })
        .RequireAuthorization()
        .WithName("Setup2FA")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Setup two-factor authentication";
            operation.Description = "Gets the information needed to setup two-factor authentication with an authenticator app.";
            operation.Tags = new List<OpenApiTag> { new() { Name = "Additional Identity Endpoints" } };
            return operation;
        });
    }
}
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

namespace ProtectedAPI.Endpoints;

public record RegisterWithRoleRequest(
    [Description("The user's email address")]
    [EmailAddress]
    string Email,

    [Description("The user's password")]
    string Password,

    [Description("The role to assign to the user ('Member' or 'Admin')")]
    string? Role);

public static class AdditionalIdentityEndpoints
{
    public static void MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        // Custom registration with role (only for Admin)
        app.MapPost("/register-with-role", async (
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
                The role must be either 'Member' or 'Admin'. If email confirmation is required, a confirmation email will be sent.
                Returns a validation problem if the registration fails or if the role is invalid.
                """;
            operation.Tags = new List<OpenApiTag> { new() { Name = "Identity" } };

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
            operation.Tags = new List<OpenApiTag> { new() { Name = "Identity" } };

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
            operation.Tags = new List<OpenApiTag> { new() { Name = "Identity" } };

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
            operation.Tags = new List<OpenApiTag> { new() { Name = "Identity" } };
            return operation;
        });
    }
}
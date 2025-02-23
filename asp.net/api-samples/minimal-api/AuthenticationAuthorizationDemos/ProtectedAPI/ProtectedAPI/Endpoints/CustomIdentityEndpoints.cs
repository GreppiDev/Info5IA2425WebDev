using Microsoft.AspNetCore.Identity;
using ProtectedAPI.Model;
using ProtectedAPI.Services;

namespace ProtectedAPI.Endpoints;

public static class CustomIdentityEndpoints
{
    public static void MapCustomIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        // Custom registration with role (only for Admin)
        app.MapPost("/register-with-role", async (
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            HttpContext context,
            string email,
            string password,
            string? role,
            bool confirmEmailAutomatically = true) =>
        {
            // Validate role
            if (!string.IsNullOrEmpty(role) && role != "Member" && role != "Admin")
            {
                return Results.BadRequest("Invalid role specified. Role must be either 'Member' or 'Admin'");
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = confirmEmailAutomatically // Se true, l'email viene confermata automaticamente
            };

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }

                if (!confirmEmailAutomatically)
                {
                    // Generate email confirmation token
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                    // Create confirmation link
                    var confirmationLink = $"{context.Request.Scheme}://{context.Request.Host}/confirm-email?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";

                    // Send confirmation email
                    await emailService.SendEmailConfirmationAsync(email, confirmationLink);

                    return Results.Ok($"User created successfully{(role != null ? $" with role {role}" : "")}. Please check your email to confirm your account.");
                }

                return Results.Ok($"User created successfully{(role != null ? $" with role {role}" : "")} with confirmed email.");
            }

            return Results.BadRequest(result.Errors);
        })
        .RequireAuthorization("RequireAdminRole")
        .WithOpenApi()
        .WithTags("Identity");
    }
}
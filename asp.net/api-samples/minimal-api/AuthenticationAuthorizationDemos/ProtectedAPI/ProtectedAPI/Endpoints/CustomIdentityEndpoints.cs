using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using ProtectedAPI.Model;
using ProtectedAPI.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ProtectedAPI.Endpoints;

public static class CustomIdentityEndpoints
{
    public static void MapCustomIdentityEndpoints(this IEndpointRouteBuilder app)
    {

        // Login endpoint with support for both token and cookie authentication
        app.MapPost("/login", async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> (
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            [FromBody] LoginRequest request,
            [FromQuery] bool? useCookies = false,
            [FromQuery] bool? useSessionCookies = false) =>
        {
            var user = await userManager.FindByNameAsync(request.Email);
            if (user == null)
            {
                return TypedResults.Problem("Invalid credentials", statusCode: StatusCodes.Status401Unauthorized);
            }

            Microsoft.AspNetCore.Identity.SignInResult result;
            if (!string.IsNullOrEmpty(request.TwoFactorCode))
            {
                result = await signInManager.TwoFactorAuthenticatorSignInAsync(request.TwoFactorCode, false, useSessionCookies ?? false);
            }
            else if (!string.IsNullOrEmpty(request.TwoFactorRecoveryCode))
            {
                result = await signInManager.TwoFactorRecoveryCodeSignInAsync(request.TwoFactorRecoveryCode);
            }
            else
            {
                result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            }

            if (!result.Succeeded)
            {
                if (result.RequiresTwoFactor)
                {
                    return TypedResults.Problem(
                        title: "Two-factor authentication required",
                        statusCode: StatusCodes.Status401Unauthorized,
                        extensions: new Dictionary<string, object?> { ["requiresTwoFactor"] = true }
                    );
                }
                return TypedResults.Problem("Invalid credentials", statusCode: StatusCodes.Status401Unauthorized);
            }

            // Handle cookie-based authentication if requested
            if (useCookies == true)
            {
                await signInManager.SignInAsync(user, !useSessionCookies ?? false);
                return TypedResults.Empty;
            }

            // Handle token-based authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!)
            };

            var roles = await userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Generate access token and refresh token
            var (accessToken, refreshToken) = await CreateTokensAsync(claims, user, configuration, userManager);

            // Use configured token duration instead of hardcoded value
            var accessTokenDuration = int.Parse(configuration["Jwt:AccessTokenDurationInMinutes"] ?? "15");

            return TypedResults.Ok(new AccessTokenResponse
            {
                TokenType = "Bearer",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = accessTokenDuration * 60  // Convert minutes to seconds
            });
        })
        .WithName("Login")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Log in with email and password";
            operation.Description = "Signs in a user using their email and password. Supports both cookie and token-based authentication.";
            operation.Tags = new List<OpenApiTag> { new() { Name = "Identity" } };
            return operation;
        });

        // Refresh token endpoint
        app.MapPost("/refresh", async (
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            [FromBody] RefreshTokenRequest request) =>
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return Results.BadRequest("Refresh token is required");
            }

            var parts = request.RefreshToken.Split(':');
            if (parts.Length != 3)
            {
                return Results.UnprocessableEntity("Invalid refresh token format");
            }

            var userId = parts[0];
            var securityStamp = parts[1];
            var actualToken = parts[2];

            // Recupera l'utente direttamente usando l'ID (una sola query al database)
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Results.UnprocessableEntity("Invalid refresh token");
            }

            // Verifica che il security stamp corrisponda (protegge da token revocati)
            if (await userManager.GetSecurityStampAsync(user) != securityStamp)
            {
                return Results.UnprocessableEntity("Token has been revoked");
            }

            // Verifica il token
            var tokenPurpose = $"RefreshToken:{userId}:{securityStamp}";
            if (!await userManager.VerifyUserTokenAsync(user, "RefreshTokenProvider", tokenPurpose, actualToken))
            {
                return Results.UnprocessableEntity("Invalid refresh token");
            }

            // Revoca il vecchio refresh token
            await userManager.RemoveAuthenticationTokenAsync(user, "RefreshTokenProvider", tokenPurpose);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!)
            };

            var roles = await userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Genera nuovi token
            var (accessToken, refreshToken) = await CreateTokensAsync(claims, user, configuration, userManager);

            // Usa la durata configurata anziché valore fisso
            var accessTokenDuration = int.Parse(configuration["Jwt:AccessTokenDurationInMinutes"] ?? "15");
            return Results.Ok(new AccessTokenResponse
            {
                TokenType = "Bearer",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = accessTokenDuration * 60 // Conversione da minuti a secondi
            });
        })
        .WithName("Refresh")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Refresh access token";
            operation.Description = "Exchanges a valid refresh token for a new access token and refresh token pair.";
            operation.Tags = new List<OpenApiTag> { new() { Name = "Identity" } };
            return operation;
        });

        // Register endpoint
        app.MapPost("/register", async (
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IOptions<IdentitySettings> identitySettings,
            HttpContext context,
            [FromBody] RegisterRequest request) =>
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = !identitySettings.Value.RequireEmailConfirmation
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return Results.ValidationProblem(result.Errors.ToDictionary(
                    e => e.Code,
                    e => new[] { e.Description }
                ));
            }

            // Only send confirmation email if required
            if (identitySettings.Value.RequireEmailConfirmation)
            {
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = $"{context.Request.Scheme}://{context.Request.Host}/confirmEmail?userId={Uri.EscapeDataString(user.Id)}&code={Uri.EscapeDataString(token)}";
                await emailService.SendEmailConfirmationAsync(request.Email, confirmationLink);
                return Results.Ok(new { RequiresConfirmation = true, Message = "Registration successful. Please check your email to confirm your account." });
            }

            return Results.Ok(new { RequiresConfirmation = false, Message = "Registration successful." });
        })
        .WithName("Register")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Register a new user";
            operation.Description = "Creates a new user account. May require email confirmation based on settings.";
            operation.Tags = new List<OpenApiTag> { new() { Name = "Identity" } };
            return operation;
        });

        // Email confirmation endpoint
        app.MapGet("/confirmEmail", async (
            UserManager<ApplicationUser> userManager,
            [FromQuery(Name = "userId")] string userId,
            [FromQuery(Name = "code")] string code,
            [FromQuery(Name = "changedEmail")] string? changedEmail = null) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Results.NotFound("User not found.");
            }

            var result = await userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
            {
                return Results.BadRequest("Failed to confirm email.");
            }

            if (changedEmail?.ToLowerInvariant() == "true")
            {
                // If this was a change of email, we need to update the user's email
                var changeEmailToken = await userManager.GenerateChangeEmailTokenAsync(user, user.Email!);
                await userManager.ChangeEmailAsync(user, user.Email!, changeEmailToken);
            }

            return Results.Ok("Email confirmed successfully.");
        })
        .WithName("ConfirmEmail")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Confirm email address";
            operation.Description = "Confirms a user's email address using the token sent via email.";
            operation.Tags = new List<OpenApiTag> { new() { Name = "Identity" } };
            operation.Parameters[0].Description = "The user ID";
            operation.Parameters[0].Example = new OpenApiString("123");
            operation.Parameters[1].Description = "The confirmation code sent via email";
            operation.Parameters[1].Example = new OpenApiString("string");
            operation.Parameters[2].Description = "Whether this is confirming a changed email address";
            operation.Parameters[2].Example = new OpenApiString("true");
            return operation;
        });

        // Resend email confirmation endpoint
        app.MapPost("/resendConfirmationEmail", async (
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            HttpContext context,
            [FromBody] ResendConfirmationEmailRequest request) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                return Results.Ok("If your email is registered, you will receive a confirmation email.");
            }

            if (user.EmailConfirmed)
            {
                return Results.BadRequest("Email is already confirmed.");
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"{context.Request.Scheme}://{context.Request.Host}/confirmEmail?userId={Uri.EscapeDataString(user.Id)}&code={Uri.EscapeDataString(token)}";
            await emailService.SendEmailConfirmationAsync(request.Email, confirmationLink);

            return Results.Ok("If your email is registered, you will receive a confirmation email.");
        })
        .WithName("ResendConfirmationEmail")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Resend email confirmation";
            operation.Description = "Resends the email confirmation link to the user's email address.";
            operation.Tags = new List<OpenApiTag> { new() { Name = "Identity" } };
            return operation;
        });

        // Password reset request endpoint
        app.MapPost("/forgotPassword", async (
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            HttpContext context,
            [FromBody] ForgotPasswordRequest request) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return Results.Ok("If your email is registered, you will receive a password reset link.");
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"{context.Request.Scheme}://{context.Request.Host}/resetPassword?email={Uri.EscapeDataString(request.Email)}&resetCode={Uri.EscapeDataString(token)}";
            await emailService.SendPasswordResetAsync(request.Email, resetLink);

            return Results.Ok("If your email is registered, you will receive a password reset link.");
        })
        .WithName("ForgotPassword")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Request password reset";
            operation.Description = "Sends a password reset link to the user's email address.";
            operation.Tags = new List<OpenApiTag> { new() { Name = "Identity" } };
            return operation;
        });

        // Reset password endpoint
        app.MapPost("/resetPassword", async (
            UserManager<ApplicationUser> userManager,
            [FromBody] ResetPasswordRequest request) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Results.BadRequest("Invalid request.");
            }

            var result = await userManager.ResetPasswordAsync(user, request.ResetCode, request.NewPassword);
            if (!result.Succeeded)
            {
                return Results.ValidationProblem(result.Errors.ToDictionary(
                    e => e.Code,
                    e => new[] { e.Description }
                ));
            }

            return Results.Ok("Password has been reset.");
        })
        .WithName("ResetPassword")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Reset password";
            operation.Description = "Resets a user's password using the reset code sent via email.";
            operation.Tags = new List<OpenApiTag> { new() { Name = "Identity" } };
            return operation;
        });

        // Get user info endpoint
        app.MapGet("/manage/info", async (
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager) =>
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user == null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new ManageInfoResponse
            {
                Email = user.Email,
                IsEmailConfirmed = user.EmailConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                HasAuthenticator = await userManager.GetAuthenticatorKeyAsync(user) != null,
            });
        })
        .RequireAuthorization()
        .ValidateSecurityStamp()
        .WithName("GetManageInfo")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get user information";
            operation.Description = "Gets the current user's profile information.";
            operation.Tags = new List<OpenApiTag> { new() { Name = "Identity" } };
            return operation;
        });

        // Update user info endpoint
        app.MapPost("/manage/info", async (
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            [FromBody] UpdateManageInfoRequest request) =>
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user == null)
            {
                return Results.Unauthorized();
            }

            if (!string.IsNullOrEmpty(request.NewEmail) && request.NewEmail != user.Email)
            {
                // Generate change email token
                var changeEmailToken = await userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);
                var confirmationLink = $"{context.Request.Scheme}://{context.Request.Host}/confirmEmail?userId={Uri.EscapeDataString(user.Id)}&code={Uri.EscapeDataString(changeEmailToken)}&changedEmail=true";
                await emailService.SendEmailConfirmationAsync(request.NewEmail, confirmationLink);
                return Results.Ok("Please check your new email to confirm the change.");
            }

            if (!string.IsNullOrEmpty(request.NewPassword))
            {
                if (string.IsNullOrEmpty(request.OldPassword))
                {
                    return Results.BadRequest("Current password is required to set a new password.");
                }

                var changePasswordResult = await userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    return Results.ValidationProblem(changePasswordResult.Errors.ToDictionary(
                        e => e.Code,
                        e => new[] { e.Description }
                    ));
                }

                return Results.Ok("Password updated successfully.");
            }

            return Results.Ok("No changes requested.");
        })
        .RequireAuthorization()
        .ValidateSecurityStamp()
        .WithName("UpdateManageInfo")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Update user information";
            operation.Description = "Updates the current user's profile information.";
            operation.Tags = new List<OpenApiTag> { new() { Name = "Identity" } };
            return operation;
        });

        app.MapPost("/manage/2fa", async (
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            [FromBody] Manage2FARequest request) =>
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user == null)
            {
                return Results.Unauthorized();
            }

            if (request.Enable)
            {
                if (!string.IsNullOrEmpty(request.TwoFactorCode))
                {
                    var verificationCode = request.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
                    var isTokenValid = await userManager.VerifyTwoFactorTokenAsync(
                        user, userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

                    if (!isTokenValid)
                    {
                        return Results.BadRequest("Verification code is invalid.");
                    }

                    await userManager.SetTwoFactorEnabledAsync(user, true);
                }

                if (request.ResetSharedKey)
                {
                    await userManager.ResetAuthenticatorKeyAsync(user);
                }

                if (request.ResetRecoveryCodes)
                {
                    var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                    return Results.Ok(new { RecoveryCodes = recoveryCodes });
                }
            }
            else
            {
                var result = await userManager.SetTwoFactorEnabledAsync(user, false);
                if (!result.Succeeded)
                {
                    return Results.BadRequest("Could not disable 2FA.");
                }
            }

            return Results.Ok();
        })
        .RequireAuthorization()
        .ValidateSecurityStamp()
        .WithName("Manage2FA")
        .WithOpenApi(operation =>
        {
            operation.Summary = "Manage two-factor authentication";
            operation.Description = "Enable, disable, or configure two-factor authentication settings.";
            operation.Tags = new List<OpenApiTag> { new() { Name = "Identity" } };
            return operation;
        });
    }

    private static async Task<(string AccessToken, string RefreshToken)> CreateTokensAsync(
        IEnumerable<Claim> claims,
        ApplicationUser user,
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager)
    {
        var secretKey = configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT secret key is not configured");
        var issuer = configuration["Jwt:Issuer"] ?? "ProtectedAPI";
        var audience = configuration["Jwt:Audience"] ?? "ProtectedAPI";
        var accessTokenDuration = int.Parse(configuration["Jwt:AccessTokenDurationInMinutes"] ?? "15");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Aggiungi il security stamp come claim usando il nome corretto che ASP.NET Identity si aspetta
        var securityStamp = await userManager.GetSecurityStampAsync(user);
        var claimsList = claims.ToList();

        // Usa la stessa chiave che utilizza ASP.NET Identity per il security stamp
        claimsList.Add(new Claim("AspNet.Identity.SecurityStamp", securityStamp));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claimsList,
            expires: DateTime.UtcNow.AddMinutes(accessTokenDuration),
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        // Include the user ID and security stamp in the token purpose
        var tokenPurpose = $"RefreshToken:{user.Id}:{securityStamp}";
        var refreshToken = await userManager.GenerateUserTokenAsync(user, "RefreshTokenProvider", tokenPurpose);
        await userManager.SetAuthenticationTokenAsync(user, "RefreshTokenProvider", tokenPurpose, refreshToken);

        return (accessToken, $"{user.Id}:{securityStamp}:{refreshToken}");
    }
}

// Request/Response models
public record LoginRequest(
    [Description("The user's email address")]
    string Email,

    [Description("The user's password")]
    string Password,

    [Description("Two-factor authentication code")]
    string? TwoFactorCode = null,

    [Description("Two-factor recovery code")]
    string? TwoFactorRecoveryCode = null);

public record RefreshTokenRequest(
    [Description("The refresh token")]
    string RefreshToken);

public record RegisterRequest(
    [Description("The user's email address")]
    [EmailAddress]
    string Email,

    [Description("The user's password")]
    string Password);

public record ForgotPasswordRequest(
    [Description("The user's email address")]
    [EmailAddress]
    string Email);

public record ResetPasswordRequest(
    [Description("The user's email address")]
    [EmailAddress]
    string Email,

    [Description("The password reset code from the email")]
    string ResetCode,

    [Description("The new password")]
    string NewPassword);

public record ResendConfirmationEmailRequest(
    [Description("The email address to resend confirmation to")]
    [EmailAddress]
    string Email);

public record AccessTokenResponse
{
    [Description("The type of token")]
    public string TokenType { get; set; } = "Bearer";

    [Description("The JWT access token")]
    public string AccessToken { get; set; } = string.Empty;

    [Description("The refresh token")]
    public string RefreshToken { get; set; } = string.Empty;

    [Description("Token expiration time in seconds")]
    public int ExpiresIn { get; set; }
}

public record ManageInfoResponse
{
    [Description("The user's email address")]
    public string? Email { get; init; }

    [Description("Whether the email is confirmed")]
    public bool IsEmailConfirmed { get; init; }

    [Description("Whether two-factor authentication is enabled")]
    public bool TwoFactorEnabled { get; init; }

    [Description("Whether the user has an authenticator configured")]
    public bool HasAuthenticator { get; init; }
}

public record UpdateManageInfoRequest
{
    [Description("The new email address")]
    [EmailAddress]
    public string? NewEmail { get; init; }

    [Description("The new password")]
    public string? NewPassword { get; init; }

    [Description("The current password")]
    public string? OldPassword { get; init; }
}

public record Manage2FARequest
{
    [Description("Whether to enable or disable 2FA")]
    public bool Enable { get; init; }

    [Description("The verification code from the authenticator app")]
    public string? TwoFactorCode { get; init; }

    [Description("Whether to reset the shared key")]
    public bool ResetSharedKey { get; init; }

    [Description("Whether to reset the recovery codes")]
    public bool ResetRecoveryCodes { get; init; }

    [Description("Whether to forget the current machine")]
    public bool ForgetMachine { get; init; }
}
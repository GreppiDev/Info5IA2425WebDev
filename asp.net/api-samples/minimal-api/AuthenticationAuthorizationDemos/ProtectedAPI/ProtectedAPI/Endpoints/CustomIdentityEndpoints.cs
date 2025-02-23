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

namespace ProtectedAPI.Endpoints;

public static class CustomIdentityEndpoints
{
    public static void MapCustomIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        // // 2FA endpoints
        // app.MapPost("/2fa", async (
        //     HttpContext context,
        //     SignInManager<ApplicationUser> signInManager,
        //     [FromBody] TwoFactorRequest request) =>
        // {
        //     var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
        //     if (user == null)
        //     {
        //         return Results.Unauthorized();
        //     }

        //     var result = await signInManager.TwoFactorAuthenticatorSignInAsync(request.Code, request.RememberClient, request.RememberMe);
        //     if (!result.Succeeded)
        //     {
        //         return Results.BadRequest("Invalid authenticator code.");
        //     }

        //     return Results.Ok();
        // }).AllowAnonymous().WithName("TwoFactorSignIn");

        // app.MapPost("/2fa/recovery-code", async (
        //     HttpContext context,
        //     SignInManager<ApplicationUser> signInManager,
        //     [FromBody] RecoveryCodeRequest request) =>
        // {
        //     var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
        //     if (user == null)
        //     {
        //         return Results.Unauthorized();
        //     }

        //     var result = await signInManager.TwoFactorRecoveryCodeSignInAsync(request.RecoveryCode);
        //     if (!result.Succeeded)
        //     {
        //         return Results.BadRequest("Invalid recovery code.");
        //     }

        //     return Results.Ok();
        // }).AllowAnonymous().WithName("RecoveryCodeSignIn");

        // app.MapPost("/manage/2fa/enable", async (
        //     HttpContext context,
        //     UserManager<ApplicationUser> userManager,
        //     [FromBody] EnableTwoFactorRequest request) =>
        // {
        //     var user = await userManager.GetUserAsync(context.User);
        //     if (user == null)
        //     {
        //         return Results.Unauthorized();
        //     }

        //     var verificationCode = request.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
        //     var isTokenValid = await userManager.VerifyTwoFactorTokenAsync(
        //         user, userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

        //     if (!isTokenValid)
        //     {
        //         return Results.BadRequest("Verification code is invalid.");
        //     }

        //     await userManager.SetTwoFactorEnabledAsync(user, true);
        //     var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        //     return Results.Ok(new
        //     {
        //         RecoveryCodes = recoveryCodes,
        //         RecoveryCodesLeft = recoveryCodes?.Count() ?? 0
        //     });
        // }).RequireAuthorization().WithName("EnableTwoFactor");

        // app.MapPost("/manage/2fa/disable", async (
        //     HttpContext context,
        //     UserManager<ApplicationUser> userManager) =>
        // {
        //     var user = await userManager.GetUserAsync(context.User);
        //     if (user == null)
        //     {
        //         return Results.Unauthorized();
        //     }

        //     var result = await userManager.SetTwoFactorEnabledAsync(user, false);
        //     if (!result.Succeeded)
        //     {
        //         return Results.BadRequest("Could not disable 2FA.");
        //     }

        //     return Results.Ok();
        // }).RequireAuthorization().WithName("DisableTwoFactor");

        // app.MapGet("/manage/2fa/info", async (
        //     HttpContext context,
        //     UserManager<ApplicationUser> userManager) =>
        // {
        //     var user = await userManager.GetUserAsync(context.User);
        //     if (user == null)
        //     {
        //         return Results.Unauthorized();
        //     }

        //     string? sharedKey = await userManager.GetAuthenticatorKeyAsync(user);
        //     if (string.IsNullOrEmpty(sharedKey))
        //     {
        //         await userManager.ResetAuthenticatorKeyAsync(user);
        //         sharedKey = await userManager.GetAuthenticatorKeyAsync(user);
        //     }

        //     var qrCodeUri = $"otpauth://totp/{Uri.EscapeDataString("ProtectedAPI")}:{Uri.EscapeDataString(user.Email)}?secret={sharedKey}&issuer={Uri.EscapeDataString("ProtectedAPI")}";

        //     return Results.Ok(new
        //     {
        //         SharedKey = sharedKey,
        //         QrCodeUri = qrCodeUri,
        //         IsTwoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user),
        //         RecoveryCodesLeft = await userManager.CountRecoveryCodesAsync(user),
        //         HasAuthenticator = !string.IsNullOrEmpty(sharedKey)
        //     });
        // }).RequireAuthorization().WithName("GetTwoFactorInfo");

        // app.MapPost("/manage/2fa/recovery-codes", async (
        //     HttpContext context,
        //     UserManager<ApplicationUser> userManager) =>
        // {
        //     var user = await userManager.GetUserAsync(context.User);
        //     if (user == null)
        //     {
        //         return Results.Unauthorized();
        //     }

        //     var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        //     return Results.Ok(new { RecoveryCodes = recoveryCodes });
        // }).RequireAuthorization().WithName("GenerateRecoveryCodes");

        // Login endpoint with support for both token and cookie authentication
        app.MapPost("/login", async Task<Results<Ok<AccessTokenResponse>, EmptyHttpResult, ProblemHttpResult>> (
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            [FromBody] LoginRequest request) =>
        {
            var user = await userManager.FindByNameAsync(request.Email);
            if (user == null)
            {
                return TypedResults.Problem("Invalid credentials", statusCode: StatusCodes.Status401Unauthorized);
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                return TypedResults.Problem("Invalid credentials", statusCode: StatusCodes.Status401Unauthorized);
            }

            // Handle cookie-based authentication if requested
            if (request.UseCookies)
            {
                await signInManager.SignInAsync(user, request.IsPersistent);
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

            return TypedResults.Ok(new AccessTokenResponse
            {
                TokenType = "Bearer",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 3600 // 1 hour
            });
        }).WithName("Login");

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

            var accessTokenDuration = int.Parse(configuration["Jwt:AccessTokenDurationInMinutes"] ?? "60");
            return Results.Ok(new AccessTokenResponse
            {
                TokenType = "Bearer",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = accessTokenDuration * 60
            });
        }).WithName("Refresh");

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
                var confirmationLink = $"{context.Request.Scheme}://{context.Request.Host}/confirm-email?email={Uri.EscapeDataString(request.Email)}&token={Uri.EscapeDataString(token)}";
                await emailService.SendEmailConfirmationAsync(request.Email, confirmationLink);
                return Results.Ok(new { RequiresConfirmation = true, Message = "Registration successful. Please check your email to confirm your account." });
            }

            return Results.Ok(new { RequiresConfirmation = false, Message = "Registration successful." });
        }).WithName("Register");

        // Email confirmation endpoint
        app.MapGet("/confirm-email", async (
            UserManager<ApplicationUser> userManager,
            string email,
            string token) =>
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Results.NotFound("User not found.");
            }

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return Results.BadRequest("Failed to confirm email.");
            }

            return Results.Ok("Email confirmed successfully.");
        }).WithName("ConfirmEmail");

        // Logout endpoint supporting both cookie and token authentication
        app.MapPost("/logout", async (
            HttpContext context,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            [FromBody] LogoutRequest? request) =>
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                if (request?.Token != null)
                {
                    // For token-based auth, invalidate the refresh token
                    var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (userId != null)
                    {
                        var user = await userManager.FindByIdAsync(userId);
                        if (user != null)
                        {
                            await userManager.RemoveAuthenticationTokenAsync(user, "RefreshTokenProvider", "RefreshToken");
                        }
                    }
                }
                else
                {
                    // For cookie-based auth
                    await signInManager.SignOutAsync();
                }
            }
            return Results.Ok();
        }).WithName("Logout");

        // Password reset request endpoint
        app.MapPost("/forgot-password", async (
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
            var resetLink = $"{context.Request.Scheme}://{context.Request.Host}/reset-password?email={Uri.EscapeDataString(request.Email)}&token={Uri.EscapeDataString(token)}";
            await emailService.SendPasswordResetAsync(request.Email, resetLink);

            return Results.Ok("If your email is registered, you will receive a password reset link.");
        }).WithName("ForgotPassword");

        // Reset password endpoint
        app.MapPost("/reset-password", async (
            UserManager<ApplicationUser> userManager,
            [FromBody] ResetPasswordRequest request) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Results.BadRequest("Invalid request.");
            }

            var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                return Results.ValidationProblem(result.Errors.ToDictionary(
                    e => e.Code,
                    e => new[] { e.Description }
                ));
            }

            return Results.Ok("Password has been reset.");
        }).WithName("ResetPassword");

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
                PhoneNumber = user.PhoneNumber,
                IsPhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                RecoveryCodesLeft = await userManager.CountRecoveryCodesAsync(user),
                HasAuthenticator = await userManager.GetAuthenticatorKeyAsync(user) != null,
                IsMachineRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user)
            });
        }).RequireAuthorization().WithName("GetManageInfo");

        // Update user info endpoint
        app.MapPost("/manage/info", async (
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            [FromBody] UpdateUserInfoRequest request) =>
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user == null)
            {
                return Results.Unauthorized();
            }

            if (!string.IsNullOrEmpty(request.NewEmail) && request.NewEmail != user.Email)
            {
                var emailToken = await userManager.GenerateChangeEmailTokenAsync(user, request.NewEmail);
                var confirmationLink = $"{context.Request.Scheme}://{context.Request.Host}/confirm-email?email={Uri.EscapeDataString(request.NewEmail)}&token={Uri.EscapeDataString(emailToken)}";
                await emailService.SendEmailConfirmationAsync(request.NewEmail, confirmationLink);

                return Results.Ok(new { RequiresConfirmation = true, Message = "Please check your new email to confirm the change." });
            }

            if (!string.IsNullOrEmpty(request.NewPhoneNumber) && request.NewPhoneNumber != user.PhoneNumber)
            {
                var phoneToken = await userManager.GenerateChangePhoneNumberTokenAsync(user, request.NewPhoneNumber);

                // In un'implementazione reale, qui invieresti un SMS con il codice
                // Per ora, restituiamo direttamente il token per testing
                return Results.Ok(new { RequiresConfirmation = true, Token = phoneToken, Message = "Please confirm your new phone number." });
            }

            return Results.Ok(new { Message = "No changes were requested." });
        }).RequireAuthorization().WithName("UpdateManageInfo");
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
        var accessTokenDuration = int.Parse(configuration["Jwt:AccessTokenDurationInMinutes"] ?? "60");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(accessTokenDuration),
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        // Include the user ID and security stamp in the token purpose
        var tokenPurpose = $"RefreshToken:{user.Id}:{await userManager.GetSecurityStampAsync(user)}";
        var refreshToken = await userManager.GenerateUserTokenAsync(user, "RefreshTokenProvider", tokenPurpose);
        await userManager.SetAuthenticationTokenAsync(user, "RefreshTokenProvider", tokenPurpose, refreshToken);

        return (accessToken, $"{user.Id}:{await userManager.GetSecurityStampAsync(user)}:{refreshToken}");
    }
}

// Request/Response models
public record LoginRequest(
    string Email,
    string Password,
    bool UseCookies = false,
    bool IsPersistent = false);

public record RefreshTokenRequest(string RefreshToken);
public record LogoutRequest(string? Token);
public record RegisterRequest(string Email, string Password);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);
public record AccessTokenResponse
{
    public string TokenType { get; set; } = "Bearer";
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}

public record ManageInfoResponse
{
    public string? Email { get; init; }
    public bool IsEmailConfirmed { get; init; }
    public string? PhoneNumber { get; init; }
    public bool IsPhoneNumberConfirmed { get; init; }
    public bool TwoFactorEnabled { get; init; }
    public int RecoveryCodesLeft { get; init; }
    public bool HasAuthenticator { get; init; }
    public bool IsMachineRemembered { get; init; }
}

public record UpdateUserInfoRequest
{
    public string? NewEmail { get; init; }
    public string? NewPhoneNumber { get; init; }
}

// Add new request models for 2FA
public record TwoFactorRequest(string Code, bool RememberClient = false, bool RememberMe = false);
public record RecoveryCodeRequest(string RecoveryCode);
public record EnableTwoFactorRequest(string Code);
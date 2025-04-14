using System;
using System.Security.Claims;
using EducationalGames.Utils;
using EducationalGames.Data;
using EducationalGames.ModelsDTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace EducationalGames.Endpoints;

public static class AccountEndpoints
{
    public static RouteGroupBuilder MapAccountEndpoints(this RouteGroupBuilder group)
    {
        // Endpoint di login
        group.MapPost("/login", async (AppDbContext db, HttpContext ctx, LoginModel model, [FromQuery] string? returnUrl) =>
        {
            bool success = false;
            string message = "";

            // Simulazione della validazione delle credenziali
            if (model.Username == "admin" && model.Password == "adminpass")
            {
                var claims = new List<Claim>
                {
            new(ClaimTypes.Name, model.Username),
            new(ClaimTypes.Role, "Admin"),
            new(ClaimTypes.Role, "Docente")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                message = "Login effettuato con successo come Admin+Docente";
                success = true;
            }
            else if (model.Username == "docente" && model.Password == "docentepass")
            {
                var claims = new List<Claim>
                {
            new(ClaimTypes.Name, model.Username),
            new(ClaimTypes.Role, "Docente")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                message = "Login effettuato con successo come Docente";
                success = true;
            }
            else if (model.Username == "studente" && model.Password == "pass")
            {
                var claims = new List<Claim>
                {
            new(ClaimTypes.Name, model.Username),
            new(ClaimTypes.Role, "Studente")
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                message = "Login effettuato con successo come Studente";
                success = true;
            }

            // Se il login è riuscito e c'è un returnUrl
            if (success && !string.IsNullOrEmpty(returnUrl))
            {
                // Verifica che l'URL sia sicuro prima di eseguire il redirect
                if (Uri.IsWellFormedUriString(returnUrl, UriKind.Relative) ||
                    returnUrl.StartsWith(ctx.Request.Scheme + "://" + ctx.Request.Host))
                {
                    return Results.Redirect(returnUrl);
                }
            }

            return success ? Results.Ok(message) : Results.Unauthorized();
        });


        // Endpoint protetto
        group.MapGet("/profile", (HttpContext ctx) =>
        {
            if (ctx.User.Identity != null && ctx.User.Identity.IsAuthenticated)
                return Results.Ok($"Benvenuto, {ctx.User.Identity.Name}");
            return Results.Unauthorized();
        }).RequireAuthorization();

        // Endpoint accessibile solo agli amministratori
        group.MapGet("/admin-area", (HttpContext ctx) =>
        {
            return Results.Ok($"Benvenuto nell'area amministrativa, {ctx.User.Identity?.Name}");
        }).RequireAuthorization("AdminOnly");

        // Endpoint accessibile a Docente o Admin (OR logico)
        group.MapGet("/power-area", (HttpContext ctx) =>
        {
            return Results.Ok($"Benvenuto nell'area power, {ctx.User.Identity?.Name}");
        }).RequireAuthorization("AdminOrDocente");


        // Endpoint per verificare i ruoli attuali dell'utente
        group.MapGet("/my-roles", (HttpContext ctx) =>
        {
            if (ctx.User.Identity?.IsAuthenticated != true)
                return Results.Unauthorized();

            var roles = ctx.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return Results.Ok(new
            {
                Username = ctx.User.Identity.Name,
                Roles = roles,
                IsAdmin = ctx.User.IsInRole("Admin"),
                IsDocente = ctx.User.IsInRole("Docente"),
                IsUser = ctx.User.IsInRole("Studente"),
                HasAllRoles = ctx.User.IsInRole("Admin") && ctx.User.IsInRole("Docente") && ctx.User.IsInRole("Studente")
            });
        });

        group.MapPost("/logout", async (HttpContext ctx, [FromQuery] string? returnUrl) =>
        {
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!string.IsNullOrEmpty(returnUrl) && HttpUtils.IsHtmlRequest(ctx.Request))
            {
                if (Uri.IsWellFormedUriString(returnUrl, UriKind.Relative) ||
                    returnUrl.StartsWith(ctx.Request.Scheme + "://" + ctx.Request.Host))
                {
                    return Results.Redirect(returnUrl);
                }
            }

            return Results.Ok("Logout effettuato con successo");
        });

        return group;
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EssenceMvp.Api.Infrastructure;
using EssenceMvp.Api.Infrastructure.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EssenceMvp.Api.Features;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request, EssenceDbContext db, IConfiguration config) =>
        {
            var email = request.Email.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
                return Results.BadRequest(new { message = "Email y contraseña son obligatorios." });

            var exists = await db.AppUsers.AnyAsync(u => u.Email == email);
            if (exists)
                return Results.Conflict(new { message = "Ya existe un usuario con ese email." });

            var hasher = new PasswordHasher<AppUser>();
            var user = new AppUser
            {
                Email = email,
                DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? null : request.DisplayName.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            user.PasswordHash = hasher.HashPassword(user, request.Password);

            db.AppUsers.Add(user);
            await db.SaveChangesAsync();

            var token = CreateToken(user, config);
            return Results.Ok(new AuthResponse(token, user.Email, user.DisplayName));
        });

        group.MapPost("/login", async (LoginRequest request, EssenceDbContext db, IConfiguration config) =>
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await db.AppUsers.SingleOrDefaultAsync(u => u.Email == email);
            if (user is null)
                return Results.Unauthorized();

            var hasher = new PasswordHasher<AppUser>();
            var verification = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (verification == PasswordVerificationResult.Failed)
                return Results.Unauthorized();

            var token = CreateToken(user, config);
            return Results.Ok(new AuthResponse(token, user.Email, user.DisplayName));
        });

        group.MapGet("/me", (ClaimsPrincipal user) =>
        {
            var email = user.FindFirstValue(ClaimTypes.Email);
            var displayName = user.FindFirstValue("display_name");
            return Results.Ok(new { email, displayName });
        }).RequireAuthorization();

        return group;
    }

    private static string CreateToken(AppUser user, IConfiguration config)
    {
        var issuer = config["Jwt:Issuer"] ?? "EssenceMvp";
        var audience = config["Jwt:Audience"] ?? "EssenceMvp";
        var key = config["Jwt:Key"] ?? throw new InvalidOperationException("Falta Jwt:Key en la configuración.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Email)
        };

        if (!string.IsNullOrWhiteSpace(user.DisplayName))
            claims.Add(new Claim("display_name", user.DisplayName));

        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}


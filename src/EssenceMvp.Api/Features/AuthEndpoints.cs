using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EssenceMvp.Api.Infrastructure;
using EssenceMvp.Api.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EssenceMvp.Api.Features;

public sealed record RegisterRequest(string Email, string Password, string? DisplayName);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthResponse(string Token, string RefreshToken, string Email, string? DisplayName);
public sealed record RefreshRequest(string RefreshToken);

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

            var session = await CreateSessionAsync(db, user, config);
            return Results.Ok(session);
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

            var session = await CreateSessionAsync(db, user, config);
            return Results.Ok(session);
        });

        group.MapPost("/refresh", async (RefreshRequest request, EssenceDbContext db, IConfiguration config) =>
        {
            var refreshHash = HashToken(request.RefreshToken);
            var session = await db.UserSessions
                .Include(s => s.AppUser)
                .SingleOrDefaultAsync(s => s.RefreshTokenHash == refreshHash);

            if (session is null || session.RevokedAt is not null || session.ExpiresAt <= DateTime.UtcNow)
                return Results.Unauthorized();

            var user = session.AppUser;
            session.RevokedAt = DateTime.UtcNow;
            var newSession = await CreateSessionAsync(db, user, config);
            session.ReplacedByTokenHash = HashToken(newSession.RefreshToken);
            await db.SaveChangesAsync();

            return Results.Ok(newSession);
        });

        group.MapPost("/logout", async (RefreshRequest request, EssenceDbContext db) =>
        {
            var refreshHash = HashToken(request.RefreshToken);
            var session = await db.UserSessions.SingleOrDefaultAsync(s => s.RefreshTokenHash == refreshHash);
            if (session is null)
                return Results.NoContent();

            session.RevokedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapGet("/me", (ClaimsPrincipal user) =>
        {
            var email = user.FindFirstValue(ClaimTypes.Email);
            var displayName = user.FindFirstValue("display_name");
            return Results.Ok(new { email, displayName });
        }).RequireAuthorization();

        return group;
    }

    private static async Task<AuthResponse> CreateSessionAsync(EssenceDbContext db, AppUser user, IConfiguration config)
    {
        var refreshToken = GenerateRefreshToken();
        var session = new UserSession
        {
            AppUserId = user.Id,
            RefreshTokenHash = HashToken(refreshToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        db.UserSessions.Add(session);
        await db.SaveChangesAsync();

        return new AuthResponse(CreateToken(user, config), refreshToken, user.Email, user.DisplayName);
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

    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}

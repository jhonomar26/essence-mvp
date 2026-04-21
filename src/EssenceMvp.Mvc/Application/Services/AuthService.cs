using EssenceMvp.Mvc.Infrastructure;
using EssenceMvp.Mvc.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Mvc.Application.Services;

public class AuthService : IAuthService
{
    private readonly EssenceDbContext _db;
    private readonly PasswordHasher<AppUser> _hasher = new();

    public AuthService(EssenceDbContext db) => _db = db;

    public async Task<AppUser?> AuthenticateAsync(string email, string password)
    {
        var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Email == email.ToLower());
        if (user == null) return null;
        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Success ? user : null;
    }

    public async Task<(AppUser? user, string? error)> RegisterAsync(string email, string password, string? displayName)
    {
        var exists = await _db.AppUsers.AnyAsync(u => u.Email == email.ToLower());
        if (exists) return (null, "Email already registered.");

        var user = new AppUser
        {
            Email = email.ToLower(),
            DisplayName = displayName,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _hasher.HashPassword(user, password);
        _db.AppUsers.Add(user);
        await _db.SaveChangesAsync();
        return (user, null);
    }
}

using EssenceMvp.Application.Abstractions;
using EssenceMvp.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EssenceMvp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IAppUserRepository _appUsers;
    private readonly IUserSessionRepository _sessions;
    private readonly ISessionTokenService _tokenService;
    private readonly PasswordHasher<AppUser> _hasher = new();

    public AuthService(
        IAppUserRepository appUsers,
        IUserSessionRepository sessions,
        ISessionTokenService tokenService)
    {
        _appUsers = appUsers;
        _sessions = sessions;
        _tokenService = tokenService;
    }

    public async Task<AppUser?> AuthenticateAsync(string email, string password)
    {
        var user = await _appUsers.GetByEmailAsync(email.ToLower());
        if (user == null) return null;
        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Success ? user : null;
    }

    public async Task<(AppUser? user, string? error)> RegisterAsync(string email, string password, string? displayName)
    {
        var exists = await _appUsers.ExistsByEmailAsync(email.ToLower());
        if (exists) return (null, "Email already registered.");

        var user = new AppUser
        {
            Email = email.ToLower(),
            DisplayName = displayName,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _hasher.HashPassword(user, password);
        user = await _appUsers.CreateAsync(user);
        return (user, null);
    }

    public async Task<(AppUser user, string token)> CreateSessionAsync(AppUser user)
    {
        var rawToken = _tokenService.GenerateToken();
        var hash = _tokenService.Hash(rawToken);

        await _sessions.CreateAsync(new UserSession
        {
            AppUserId = user.Id,
            RefreshTokenHash = hash,
            ExpiresAt = DateTime.UtcNow.AddHours(8),
            CreatedAt = DateTime.UtcNow
        });

        return (user, rawToken);
    }
}

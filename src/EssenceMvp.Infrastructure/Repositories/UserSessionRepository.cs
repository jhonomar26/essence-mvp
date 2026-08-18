using EssenceMvp.Application.Abstractions;
using EssenceMvp.Domain.Entities;
using EssenceMvp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Infrastructure.Repositories;

public class UserSessionRepository : IUserSessionRepository
{
    private readonly EssenceDbContext _db;

    public UserSessionRepository(EssenceDbContext db) => _db = db;

    public async Task<UserSession> CreateAsync(UserSession session)
    {
        _db.UserSessions.Add(session);
        await _db.SaveChangesAsync();
        return session;
    }

    public Task<UserSession?> GetByTokenHashAsync(string tokenHash) =>
        _db.UserSessions
            .Include(s => s.AppUser)
            .FirstOrDefaultAsync(s => s.RefreshTokenHash == tokenHash);

    public async Task RevokeAsync(UserSession session)
    {
        session.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
    /// <summary>
    /// Marks the current session as revoked and records the hash of the new token that replaces it.
    /// </summary>
    public async Task MarkRotatedAsync(
        UserSession session,
        string newTokenHash)
    {
        session.RevokedAt = DateTime.UtcNow;
        session.ReplacedByTokenHash = newTokenHash;

        await _db.SaveChangesAsync();
    }

    public async Task RevokeAllActiveForUserAsync(int appUserId)
    {
        var sesiones = await _db.UserSessions
            .Where(x => x.AppUserId == appUserId && x.RevokedAt == null)
            .ToListAsync();
        foreach (var sesion in sesiones)
        {
            sesion.RevokedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }
}

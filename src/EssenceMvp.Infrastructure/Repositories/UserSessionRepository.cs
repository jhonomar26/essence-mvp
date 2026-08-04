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
}

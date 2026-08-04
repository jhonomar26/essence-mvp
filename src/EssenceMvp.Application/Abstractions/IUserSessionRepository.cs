using EssenceMvp.Domain.Entities;

namespace EssenceMvp.Application.Abstractions;

public interface IUserSessionRepository
{
    Task<UserSession> CreateAsync(UserSession session);
    Task<UserSession?> GetByTokenHashAsync(string tokenHash);
    Task RevokeAsync(UserSession session);
}

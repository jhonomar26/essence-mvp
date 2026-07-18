using EssenceMvp.Domain.Entities;

namespace EssenceMvp.Application.Services;

public interface IAuthService
{
    Task<AppUser?> AuthenticateAsync(string email, string password);
    Task<(AppUser? user, string? error)> RegisterAsync(string email, string password, string? displayName);
}

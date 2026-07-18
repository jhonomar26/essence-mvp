using EssenceMvp.Domain.Entities;

namespace EssenceMvp.Application.Abstractions;

public interface IAppUserRepository
{
    Task<AppUser?> GetByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email);
    Task<AppUser> CreateAsync(AppUser user);
}

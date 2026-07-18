using EssenceMvp.Application.Abstractions;
using EssenceMvp.Domain.Entities;
using EssenceMvp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Infrastructure.Repositories;

public class AppUserRepository : IAppUserRepository
{
    private readonly EssenceDbContext _db;

    public AppUserRepository(EssenceDbContext db) => _db = db;

    public Task<AppUser?> GetByEmailAsync(string email) =>
        _db.AppUsers.FirstOrDefaultAsync(u => u.Email == email);

    public Task<bool> ExistsByEmailAsync(string email) =>
        _db.AppUsers.AnyAsync(u => u.Email == email);

    public async Task<AppUser> CreateAsync(AppUser user)
    {
        _db.AppUsers.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }
}

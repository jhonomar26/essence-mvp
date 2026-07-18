using EssenceMvp.Application.Abstractions;
using EssenceMvp.Domain.Entities;
using EssenceMvp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly EssenceDbContext _db;

    public ProjectRepository(EssenceDbContext db) => _db = db;

    public Task<List<Project>> GetByUserIdAsync(int userId) =>
        _db.Projects
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public Task<Project?> GetByIdAsync(int projectId) =>
        _db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId);

    public Task<Project?> GetByIdForUserAsync(int projectId, int userId) =>
        _db.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

    public Task<Project?> GetDetailByIdForUserAsync(int projectId, int userId) =>
        _db.Projects
            .Include(p => p.AlphaStatuses)
            .ThenInclude(s => s.Alpha)
            .ThenInclude(a => a.States)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

    public async Task<Project> CreateAsync(Project project)
    {
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();
        return project;
    }

    public async Task DeleteAsync(Project project)
    {
        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();
    }
}

using EssenceMvp.Mvc.Infrastructure;
using EssenceMvp.Mvc.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Mvc.Application.Services;

public class ProjectService : IProjectService
{
    private readonly EssenceDbContext _db;

    public ProjectService(EssenceDbContext db) => _db = db;

    public async Task<List<Project>> GetUserProjectsAsync(int userId) =>
        await _db.Projects
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<Project> CreateProjectAsync(int userId, string name, string? description, string? phase)
    {
        var project = new Project
        {
            UserId = userId,
            Name = name,
            Description = description,
            Phase = phase,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        var alphaIds = await _db.Alphas.Select(a => a.Id).ToListAsync();
        foreach (var alphaId in alphaIds)
        {
            _db.ProjectAlphaStatuses.Add(new ProjectAlphaStatus
            {
                ProjectId = project.Id,
                AlphaId = alphaId,
                CurrentStateNumber = 0,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }
        await _db.SaveChangesAsync();
        return project;
    }

    public async Task<Project?> GetProjectDetailAsync(int projectId, int userId) =>
        await _db.Projects
            .Include(p => p.AlphaStatuses)
                .ThenInclude(s => s.Alpha)
                    .ThenInclude(a => a.States)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

    public async Task<bool> DeleteProjectAsync(int projectId, int userId)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);
        if (project == null) return false;
        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();
        return true;
    }
}

using EssenceMvp.Application.Abstractions;
using EssenceMvp.Domain.Entities;
using EssenceMvp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Infrastructure.Repositories;

public class ProjectAlphaStatusRepository : IProjectAlphaStatusRepository
{
    private readonly EssenceDbContext _db;

    public ProjectAlphaStatusRepository(EssenceDbContext db) => _db = db;

    public Task<ProjectAlphaStatus?> GetAsync(int projectId, int alphaId) =>
        _db.ProjectAlphaStatuses
            .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.AlphaId == alphaId);

    public async Task UpdateCurrentStateAsync(int projectId, int alphaId, short newStateNumber)
    {
        var projAlpha = await _db.ProjectAlphaStatuses
            .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.AlphaId == alphaId);

        if (projAlpha != null)
        {
            projAlpha.CurrentStateNumber = newStateNumber;
            projAlpha.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    public async Task AddRangeAsync(List<ProjectAlphaStatus> statuses)
    {
        _db.ProjectAlphaStatuses.AddRange(statuses);
        await _db.SaveChangesAsync();
    }
}

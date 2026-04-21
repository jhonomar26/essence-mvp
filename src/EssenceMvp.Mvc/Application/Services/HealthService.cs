using EssenceMvp.Mvc.Infrastructure;
using EssenceMvp.Mvc.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Mvc.Application.Services;

public class HealthService : IHealthService
{
    private readonly EssenceDbContext _db;

    public HealthService(EssenceDbContext db) => _db = db;

    public async Task<HealthStatus> CalculateHealthAsync(int projectId, int userId)
    {
        var statuses = await _db.ProjectAlphaStatuses
            .Include(s => s.Alpha).ThenInclude(a => a.States)
            .Where(s => s.ProjectId == projectId && s.Project.UserId == userId)
            .ToListAsync();

        if (!statuses.Any()) return HealthStatus.Red;

        var avgScore = statuses
            .Select(s => s.Alpha.States.Count == 0 ? 0.0 : (double)s.CurrentStateNumber / s.Alpha.States.Count)
            .Average();

        return avgScore >= 0.7 ? HealthStatus.Green : avgScore >= 0.4 ? HealthStatus.Yellow : HealthStatus.Red;
    }
}

using EssenceMvp.Mvc.Infrastructure;
using EssenceMvp.Mvc.Infrastructure.Entities;
using EssenceMvp.Mvc.Models;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Mvc.Application.Services;

public class AlphaService : IAlphaService
{
    private readonly EssenceDbContext _db;

    public AlphaService(EssenceDbContext db) => _db = db;

    public async Task<List<AlphaProgressDto>> GetProjectAlphaProgressAsync(int projectId, int userId)
    {
        var project = await _db.Projects
            .Include(p => p.AlphaStatuses)
                .ThenInclude(s => s.Alpha)
                    .ThenInclude(a => a.States)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

        if (project == null) return new();

        return project.AlphaStatuses
            .OrderBy(s => s.AlphaId)
            .Select(s =>
            {
                var total = s.Alpha.States.Count;
                var completionPercent = total == 0 ? 0.0 : Math.Round((double)s.CurrentStateNumber / total * 100, 0);
                var currentStateName = s.Alpha.States
                    .FirstOrDefault(st => st.StateNumber == s.CurrentStateNumber)?.StateName ?? "Not started";
                var health = completionPercent >= 70 ? HealthStatus.Green
                    : completionPercent >= 40 ? HealthStatus.Yellow
                    : HealthStatus.Red;

                return new AlphaProgressDto
                {
                    AlphaId = s.AlphaId,
                    AlphaName = s.Alpha.Name,
                    AreaOfConcern = s.Alpha.AreaOfConcern,
                    CurrentStateNumber = s.CurrentStateNumber,
                    TotalStates = total,
                    CurrentStateName = currentStateName,
                    CompletionPercent = completionPercent,
                    AlphaHealth = health
                };
            })
            .ToList();
    }
}

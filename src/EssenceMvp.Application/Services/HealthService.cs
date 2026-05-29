using EssenceMvp.Application.Interfaces;
using EssenceMvp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Application.Services;

public class HealthService : IHealthService
{
    private readonly IEssenceDbContext _db;
    private readonly IHealthCalculationService _healthCalcService;

    public HealthService(IEssenceDbContext db, IHealthCalculationService healthCalcService)
    {
        _db = db;
        _healthCalcService = healthCalcService;
    }

    public async Task<HealthStatus> CalculateHealthAsync(int projectId, int userId)
    {
        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

        if (project == null) return HealthStatus.red;

        var healthScore = await _healthCalcService.CalculateProjectHealthAsync(projectId);

        return healthScore.HealthScore switch
        {
            >= 80 => HealthStatus.green,
            >= 60 => HealthStatus.yellow,
            >= 40 => HealthStatus.yellow,
            _ => HealthStatus.red
        };
    }
}

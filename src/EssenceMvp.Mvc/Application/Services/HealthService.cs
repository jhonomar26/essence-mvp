using EssenceMvp.Mvc.Infrastructure;
using EssenceMvp.Mvc.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Mvc.Application.Services;

public class HealthService : IHealthService
{
    private readonly EssenceDbContext _db;
    private readonly IHealthCalculationService _healthCalcService;

    public HealthService(EssenceDbContext db, IHealthCalculationService healthCalcService)
    {
        _db = db;
        _healthCalcService = healthCalcService;
    }

    // Maps Essence health score to traffic light status (for views/dashboards)
    public async Task<HealthStatus> CalculateHealthAsync(int projectId, int userId)
    {
        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

        if (project == null) return HealthStatus.Red;

        var healthScore = await _healthCalcService.CalculateProjectHealthAsync(projectId);

        return healthScore.HealthScore switch
        {
            >= 80 => HealthStatus.Green,      // SALUDABLE
            >= 60 => HealthStatus.Yellow,     // ACEPTABLE
            >= 40 => HealthStatus.Yellow,     // EN RIESGO
            _ => HealthStatus.Red             // CRÍTICO
        };
    }
}

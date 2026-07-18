using EssenceMvp.Application.Abstractions;
using EssenceMvp.Domain.Enums;

namespace EssenceMvp.Application.Services;

public class HealthService : IHealthService
{
    private readonly IProjectRepository _projects;
    private readonly IHealthCalculationService _healthCalcService;

    public HealthService(IProjectRepository projects, IHealthCalculationService healthCalcService)
    {
        _projects = projects;
        _healthCalcService = healthCalcService;
    }

    // Maps Essence health score to traffic light status (for views/dashboards)
    public async Task<HealthStatus> CalculateHealthAsync(int projectId, int userId)
    {
        var project = await _projects.GetByIdForUserAsync(projectId, userId);

        if (project == null) return HealthStatus.red;

        var healthScore = await _healthCalcService.CalculateProjectHealthAsync(projectId);

        return healthScore.HealthScore switch
        {
            >= 80 => HealthStatus.green,      // SALUDABLE
            >= 60 => HealthStatus.yellow,     // ACEPTABLE
            >= 40 => HealthStatus.yellow,     // EN RIESGO
            _ => HealthStatus.red             // CRÍTICO
        };
    }
}

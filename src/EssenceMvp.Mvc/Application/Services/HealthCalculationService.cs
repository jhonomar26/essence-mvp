using EssenceMvp.Mvc.Infrastructure;
using EssenceMvp.Mvc.Models;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Mvc.Application.Services;

public class HealthCalculationService : IHealthCalculationService
{
    private readonly EssenceDbContext _db;

    public HealthCalculationService(EssenceDbContext db)
    {
        _db = db;
    }

    // SEMAT Essence Health Calculation:
    // 1. Progress: (currentState / maxStates) * 100 per Alpha (each alpha)
    // 2. Average: mean of all Alpha progresses 
    // 3. Dispersion: max(
    // ) - min(progress)
    // 4. Penalty: dispersion * 0.2
    // 5. HealthScore: avg - penalty
    // 6. Classification: 80-100 SALUDABLE, 60-79 ACEPTABLE, 40-59 EN RIESGO, 0-39 CRÍTICO
    public async Task<ProjectHealthScore> CalculateProjectHealthAsync(int projectId)
    {
        var alphas = await _db.Alphas
            .Include(a => a.States)
            .Include(a => a.ProjectStatuses.Where(ps => ps.ProjectId == projectId))
            .ToListAsync();

        var alphaProgresses = new List<ProjectHealthScore.AlphaProgressDetail>();

        foreach (var alpha in alphas)
        {
            var status = alpha.ProjectStatuses.FirstOrDefault();
            short currentState = status?.CurrentStateNumber ?? 0;
            short maxStates = (short)alpha.States.Count;

            decimal progress = maxStates > 0 ? (currentState / (decimal)maxStates) * 100 : 0;

            alphaProgresses.Add(new ProjectHealthScore.AlphaProgressDetail
            {
                AlphaId = alpha.Id,
                AlphaName = alpha.Name,
                CurrentStateNumber = currentState,
                MaxStateNumber = maxStates,
                Progress = progress
            });
        }

        if (alphaProgresses.Count == 0)
            return new ProjectHealthScore { HealthScore = 0, Classification = "CRÍTICO", AverageProgress = 0, ProgressDispersion = 0 };

        decimal averageProgress = alphaProgresses.Average(p => p.Progress);
        decimal maxProgress = alphaProgresses.Max(p => p.Progress);
        decimal minProgress = alphaProgresses.Min(p => p.Progress);
        decimal dispersion = maxProgress - minProgress;
        decimal penalty = dispersion * 0.2m;
        decimal healthScore = averageProgress - penalty;

        // Clamp to 0-100
        healthScore = Math.Max(0, Math.Min(100, healthScore));

        string classification = healthScore switch
        {
            >= 80 => "SALUDABLE",
            >= 60 => "ACEPTABLE",
            >= 40 => "EN RIESGO",
            _ => "CRÍTICO"
        };

        return new ProjectHealthScore
        {
            HealthScore = Math.Round(healthScore, 2),
            Classification = classification,
            AverageProgress = Math.Round(averageProgress, 2),
            ProgressDispersion = Math.Round(dispersion, 2),
            AlphaProgresses = alphaProgresses
        };
    }
}
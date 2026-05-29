using EssenceMvp.Application.DTOs;
using EssenceMvp.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Application.Services;

public class HealthCalculationService : IHealthCalculationService
{
    private readonly IEssenceDbContext _db;

    public HealthCalculationService(IEssenceDbContext db) => _db = db;

    // SEMAT Essence Health: avg progress - (dispersion * 0.2 penalty)
    // 80-100: SALUDABLE, 60-79: ACEPTABLE, 40-59: EN RIESGO, 0-39: CRÍTICO
    public async Task<ProjectHealthScore> CalculateProjectHealthAsync(int projectId)
    {
        var alphas = await _db.Alphas
            .Include(a => a.States)
            .Include(a => a.ProjectStatuses.Where(ps => ps.ProjectId == projectId))
            .ToListAsync();

        var alphaProgresses = alphas.Select(alpha =>
        {
            var status = alpha.ProjectStatuses.FirstOrDefault();
            short currentState = status?.CurrentStateNumber ?? 0;
            short maxStates = (short)alpha.States.Count;
            decimal progress = maxStates > 0 ? (currentState / (decimal)maxStates) * 100 : 0;

            return new ProjectHealthScore.AlphaProgressDetail
            {
                AlphaId = alpha.Id,
                AlphaName = alpha.Name,
                CurrentStateNumber = currentState,
                MaxStateNumber = maxStates,
                Progress = progress
            };
        }).ToList();

        if (alphaProgresses.Count == 0)
            return new ProjectHealthScore { HealthScore = 0, Classification = "CRÍTICO" };

        decimal averageProgress = alphaProgresses.Average(p => p.Progress);
        decimal dispersion = alphaProgresses.Max(p => p.Progress) - alphaProgresses.Min(p => p.Progress);
        decimal healthScore = Math.Max(0, Math.Min(100, averageProgress - dispersion * 0.2m));

        return new ProjectHealthScore
        {
            HealthScore = Math.Round(healthScore, 2),
            Classification = healthScore switch
            {
                >= 80 => "SALUDABLE",
                >= 60 => "ACEPTABLE",
                >= 40 => "EN RIESGO",
                _ => "CRÍTICO"
            },
            AverageProgress = Math.Round(averageProgress, 2),
            ProgressDispersion = Math.Round(dispersion, 2),
            AlphaProgresses = alphaProgresses
        };
    }
}

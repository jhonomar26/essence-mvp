using System.Text.Json;
using EssenceMvp.Mvc.Infrastructure;
using EssenceMvp.Mvc.Infrastructure.Entities;
using EssenceMvp.Mvc.Models;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Mvc.Application.Services;

public class SnapshotService : ISnapshotService
{
    private readonly EssenceDbContext _db;
    private readonly IHealthCalculationService _healthCalcService;

    public SnapshotService(EssenceDbContext db, IHealthCalculationService healthCalcService)
    {
        _db = db;
        _healthCalcService = healthCalcService;
    }

    public async Task<SnapshotSummaryDto> CreateSnapshotAsync(int projectId, int userId)
    {
        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

        if (project == null)
            throw new UnauthorizedAccessException("Project not found or access denied");

        var healthScore = await _healthCalcService.CalculateProjectHealthAsync(projectId);

        var alphaDetails = healthScore.AlphaProgresses
            .Select(ap => new AlphaSnapshotDetailDto
            {
                AlphaId = ap.AlphaId,
                AlphaName = ap.AlphaName,
                CurrentStateNumber = ap.CurrentStateNumber,
                MaxStateNumber = ap.MaxStateNumber,
                Progress = ap.Progress
            })
            .ToList();

        var jsonPayload = new SnapshotJsonPayload
        {
            HealthScore = healthScore.HealthScore,
            Classification = healthScore.Classification,
            AverageProgress = healthScore.AverageProgress,
            ProgressDispersion = healthScore.ProgressDispersion,
            Alphas = alphaDetails
        };

        var globalStatus = healthScore.HealthScore switch
        {
            >= 80 => HealthStatus.Green,
            >= 60 => HealthStatus.Yellow,
            _ => HealthStatus.Red
        };

        var healthReport = new HealthReport
        {
            ProjectId = projectId,
            GlobalStatus = globalStatus,
            CreatedAt = DateTimeOffset.UtcNow,
            AlphaDetails = JsonSerializer.Serialize(jsonPayload)
        };

        _db.HealthReports.Add(healthReport);
        await _db.SaveChangesAsync();

        return new SnapshotSummaryDto
        {
            Id = healthReport.Id,
            CreatedAt = healthReport.CreatedAt,
            GlobalStatus = globalStatus,
            HealthScore = healthScore.HealthScore,
            Classification = healthScore.Classification,
            Alphas = alphaDetails
        };
    }

    public async Task<List<SnapshotSummaryDto>> GetHistoryAsync(int projectId, int userId)
    {
        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

        if (project == null)
            throw new UnauthorizedAccessException("Project not found or access denied");

        var reports = await _db.HealthReports
            .Where(r => r.ProjectId == projectId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(10)
            .ToListAsync();

        var result = new List<SnapshotSummaryDto>();

        foreach (var report in reports)
        {
            var dto = new SnapshotSummaryDto
            {
                Id = report.Id,
                CreatedAt = report.CreatedAt,
                GlobalStatus = report.GlobalStatus,
                Alphas = new List<AlphaSnapshotDetailDto>()
            };

            if (!string.IsNullOrEmpty(report.AlphaDetails))
            {
                try
                {
                    var payload = JsonSerializer.Deserialize<SnapshotJsonPayload>(report.AlphaDetails);
                    if (payload != null)
                    {
                        dto.HealthScore = payload.HealthScore;
                        dto.Classification = payload.Classification;
                        dto.Alphas = payload.Alphas;
                    }
                }
                catch
                {
                    // JSON corruption, skip
                }
            }

            result.Add(dto);
        }

        return result;
    }
}

using System.Text.Json;
using EssenceMvp.Application.Abstractions;
using EssenceMvp.Application.Dtos;
using EssenceMvp.Domain.Entities;
using EssenceMvp.Domain.Enums;

namespace EssenceMvp.Application.Services;

public class SnapshotService : ISnapshotService
{
    private readonly IProjectRepository _projects;
    private readonly IHealthReportRepository _healthReports;
    private readonly IHealthCalculationService _healthCalcService;

    public SnapshotService(IProjectRepository projects, IHealthReportRepository healthReports, IHealthCalculationService healthCalcService)
    {
        _projects = projects;
        _healthReports = healthReports;
        _healthCalcService = healthCalcService;
    }

    public async Task<SnapshotSummaryDto> CreateSnapshotAsync(int projectId, int userId)
    {
        var project = await _projects.GetByIdForUserAsync(projectId, userId);

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
            >= 80 => HealthStatus.green,
            >= 60 => HealthStatus.yellow,
            _ => HealthStatus.red
        };

        var healthReport = new HealthReport
        {
            ProjectId = projectId,
            GlobalStatus = globalStatus,
            CreatedAt = DateTimeOffset.UtcNow,
            AlphaDetails = JsonSerializer.Serialize(jsonPayload)
        };

        healthReport = await _healthReports.AddAsync(healthReport);

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
        var project = await _projects.GetByIdForUserAsync(projectId, userId);

        if (project == null)
            throw new UnauthorizedAccessException("Project not found or access denied");

        var reports = await _healthReports.GetRecentByProjectIdAsync(projectId, 10);

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

    public async Task DeleteSnapshotAsync(int snapshotId, int projectId, int userId)
    {
        var project = await _projects.GetByIdForUserAsync(projectId, userId);

        if (project == null)
            throw new UnauthorizedAccessException("Project not found or access denied");

        var snapshot = await _healthReports.GetAsync(snapshotId, projectId);

        if (snapshot == null)
            throw new InvalidOperationException("Snapshot not found");

        await _healthReports.DeleteAsync(snapshot);
    }
}

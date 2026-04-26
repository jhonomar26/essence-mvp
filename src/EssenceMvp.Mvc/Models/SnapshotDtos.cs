using EssenceMvp.Mvc.Infrastructure.Entities;

namespace EssenceMvp.Mvc.Models;

public class SnapshotSummaryDto
{
    public int Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public HealthStatus GlobalStatus { get; set; }
    public decimal HealthScore { get; set; }
    public string Classification { get; set; } = "";
    public List<AlphaSnapshotDetailDto> Alphas { get; set; } = new();
}

public class AlphaSnapshotDetailDto
{
    public int AlphaId { get; set; }
    public string AlphaName { get; set; } = "";
    public short CurrentStateNumber { get; set; }
    public short MaxStateNumber { get; set; }
    public decimal Progress { get; set; }
}

internal class SnapshotJsonPayload
{
    public decimal HealthScore { get; set; }
    public string Classification { get; set; } = "";
    public decimal AverageProgress { get; set; }
    public decimal ProgressDispersion { get; set; }
    public List<AlphaSnapshotDetailDto> Alphas { get; set; } = new();
}

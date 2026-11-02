using EssenceMvp.Domain.Enums;

namespace EssenceMvp.Application.Dtos;

public class SnapshotSummaryDto
{
    public int Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public HealthStatus GlobalStatus { get; set; }
    public decimal HealthScore { get; set; }
    public string Classification { get; set; } = "";
    public List<AlphaSnapshotDetailDto> Alphas { get; set; } = new();
}

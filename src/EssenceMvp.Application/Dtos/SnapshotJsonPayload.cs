namespace EssenceMvp.Application.Dtos;

internal class SnapshotJsonPayload
{
    public decimal HealthScore { get; set; }
    public string Classification { get; set; } = "";
    public decimal AverageProgress { get; set; }
    public decimal ProgressDispersion { get; set; }
    public List<AlphaSnapshotDetailDto> Alphas { get; set; } = new();
}

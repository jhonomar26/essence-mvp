using EssenceMvp.Domain.Enums;

namespace EssenceMvp.Application.Dtos;

public class ProjectDetailViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? Phase { get; set; }
    public HealthStatus OverallHealth { get; set; }
    public decimal HealthScore { get; set; }
    public string HealthClassification { get; set; } = "";
    public decimal AverageProgress { get; set; }
    public decimal ProgressDispersion { get; set; }
    public List<AlphaProgressDto> AlphaProgress { get; set; } = new();
}

namespace EssenceMvp.Api.Infrastructure.Entities;

public class HealthReport
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public HealthStatus GlobalStatus { get; set; }
    public string? AlphaDetails { get; set; }

    public Project Project { get; set; } = null!;
}

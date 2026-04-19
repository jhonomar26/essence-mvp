namespace EssenceMvp.Api.Infrastructure.Entities;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Phase { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<ProjectAlphaStatus> AlphaStatuses { get; set; } = new List<ProjectAlphaStatus>();
    public ICollection<ChecklistResponse> ChecklistResponses { get; set; } = new List<ChecklistResponse>();
    public ICollection<HealthReport> HealthReports { get; set; } = new List<HealthReport>();
}

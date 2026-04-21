namespace EssenceMvp.Mvc.Infrastructure.Entities;

public class Alpha
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string AreaOfConcern { get; set; } = null!;
    public string? Description { get; set; }

    public ICollection<AlphaState> States { get; set; } = new List<AlphaState>();
    public ICollection<ProjectAlphaStatus> ProjectStatuses { get; set; } = new List<ProjectAlphaStatus>();
}

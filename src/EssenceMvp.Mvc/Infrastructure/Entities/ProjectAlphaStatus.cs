namespace EssenceMvp.Mvc.Infrastructure.Entities;

public class ProjectAlphaStatus
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int AlphaId { get; set; }
    public short CurrentStateNumber { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Project Project { get; set; } = null!;
    public Alpha Alpha { get; set; } = null!;
}

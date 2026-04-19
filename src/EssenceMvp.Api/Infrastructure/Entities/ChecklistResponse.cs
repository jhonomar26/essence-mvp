namespace EssenceMvp.Api.Infrastructure.Entities;

public class ChecklistResponse
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int StateChecklistId { get; set; }
    public bool IsAchieved { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Project Project { get; set; } = null!;
    public StateChecklist StateChecklist { get; set; } = null!;
}

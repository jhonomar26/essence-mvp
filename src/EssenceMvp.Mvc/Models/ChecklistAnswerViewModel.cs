namespace EssenceMvp.Mvc.Models;

public class ChecklistAnswerViewModel
{
    public int StateChecklistId { get; set; }
    public bool IsAchieved { get; set; }
    public string? Notes { get; set; }
}
namespace EssenceMvp.Mvc.Models;

public class EvaluateChecklistViewModel
{
    public int ProjectId { get; set; }
    public int AlphaId { get; set; }
    public List<ChecklistAnswer> Answers { get; set; } = new();

    public class ChecklistAnswer
    {
        public int StateChecklistId { get; set; }
        public bool IsAchieved { get; set; }
        public string? Notes { get; set; }
    }
}

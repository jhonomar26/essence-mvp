namespace EssenceMvp.Mvc.Models;

public class EvaluateChecklistViewModel
{
    public int ProjectId { get; set; }
    public int AlphaId { get; set; }

    public List<ChecklistAnswerViewModel> Answers { get; set; } = new();
}


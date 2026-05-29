namespace EssenceMvp.Application.DTOs;

public class EvaluateChecklistDto
{
    public int ProjectId { get; set; }
    public int AlphaId { get; set; }
    public List<ChecklistAnswerDto> Answers { get; set; } = new();
}

public class ChecklistAnswerDto
{
    public int StateChecklistId { get; set; }
    public bool IsAchieved { get; set; }
    public string? Notes { get; set; }
}

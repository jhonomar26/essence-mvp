namespace EssenceMvp.Mvc.Models;

public class AlphaChecklistDto
{
    public short StateNumber { get; set; }
    public string StateName { get; set; } = "";
    public List<ChecklistCriterionDto> Criteria { get; set; } = new();
}

public class ChecklistCriterionDto
{
    public int Id { get; set; }
    public string CriterionText { get; set; } = "";
    public bool IsAchieved { get; set; }
}

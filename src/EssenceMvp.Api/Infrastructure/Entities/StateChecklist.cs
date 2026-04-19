namespace EssenceMvp.Api.Infrastructure.Entities;

public class StateChecklist
{
    public int Id { get; set; }
    public int AlphaStateId { get; set; }
    public string CriterionText { get; set; } = null!;
    public bool IsMandatory { get; set; } = true;

    public AlphaState AlphaState { get; set; } = null!;
    public ICollection<ChecklistResponse> Responses { get; set; } = new List<ChecklistResponse>();
}

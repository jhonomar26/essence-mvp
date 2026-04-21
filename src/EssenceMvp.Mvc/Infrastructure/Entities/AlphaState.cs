namespace EssenceMvp.Mvc.Infrastructure.Entities;

public class AlphaState
{
    public int Id { get; set; }
    public int AlphaId { get; set; }
    public short StateNumber { get; set; }
    public string StateName { get; set; } = null!;
    public string? Description { get; set; }

    public Alpha Alpha { get; set; } = null!;
    public ICollection<StateChecklist> Checklists { get; set; } = new List<StateChecklist>();
}

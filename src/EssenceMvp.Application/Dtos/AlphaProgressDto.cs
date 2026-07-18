namespace EssenceMvp.Application.Dtos;

public class AlphaProgressDto
{
    public int AlphaId { get; set; }
    public string AlphaName { get; set; } = "";
    public string AreaOfConcern { get; set; } = "";
    public short CurrentStateNumber { get; set; }
    public short MaxStateNumber { get; set; }
    public string CurrentStateName { get; set; } = "";
    public decimal Progress { get; set; }
}

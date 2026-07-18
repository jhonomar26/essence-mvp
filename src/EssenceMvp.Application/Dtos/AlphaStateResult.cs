namespace EssenceMvp.Application.Dtos;

public class AlphaStateResult
{
    public short CurrentStateNumber { get; set; }
    public string CurrentStateName { get; set; } = "";
    public short MaxStateNumber { get; set; }
}

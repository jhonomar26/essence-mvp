namespace EssenceMvp.Application.Dtos;

public class AlphaSnapshotDetailDto
{
    public int AlphaId { get; set; }
    public string AlphaName { get; set; } = "";
    public short CurrentStateNumber { get; set; }
    public short MaxStateNumber { get; set; }
    public decimal Progress { get; set; }
}

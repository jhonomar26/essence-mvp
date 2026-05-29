namespace EssenceMvp.Application.DTOs;

public class HealthResult
{
    public decimal HealthScore { get; set; }
    public string Classification { get; set; } = "";
    public decimal AverageProgress { get; set; }
    public decimal ProgressDispersion { get; set; }
    public List<AlphaDetail> AlphaDetails { get; set; } = new();

    public class AlphaDetail
    {
        public int AlphaId { get; set; }
        public string AlphaName { get; set; } = "";
        public short CurrentStateNumber { get; set; }
        public short MaxStateNumber { get; set; }
        public decimal Progress { get; set; }
    }
}

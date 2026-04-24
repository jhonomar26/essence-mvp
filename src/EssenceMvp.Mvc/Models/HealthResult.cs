namespace EssenceMvp.Mvc.Models;

public class HealthResult
{
    public string GlobalStatus { get; set; } = "";
    public List<AlphaDetail> AlphaDetails { get; set; } = new();

    public class AlphaDetail
    {
        public int AlphaId { get; set; }
        public string AlphaName { get; set; } = "";
        public short CurrentStateNumber { get; set; }
        public string CurrentStateName { get; set; } = "";
    }
}
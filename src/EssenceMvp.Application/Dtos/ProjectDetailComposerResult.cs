namespace EssenceMvp.Application.Dtos;

public class ProjectDetailComposerResult
{
    public ProjectDetailViewModel ViewModel { get; set; } = new();
    public Dictionary<int, List<AlphaChecklistDto>> AlphaChecklists { get; set; } = new();
    public List<SnapshotSummaryDto> RecentSnapshots { get; set; } = new();
}

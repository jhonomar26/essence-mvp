using EssenceMvp.Application.DTOs;

namespace EssenceMvp.Application.Interfaces;

public interface IProjectDetailComposerService
{
    Task<ProjectDetailComposerResult?> GetProjectDetailAsync(int projectId, int userId);
}

public class ProjectDetailComposerResult
{
    public ProjectDetailViewModel ViewModel { get; set; } = new();
    public Dictionary<int, List<AlphaChecklistDto>> AlphaChecklists { get; set; } = new();
    public List<SnapshotSummaryDto> RecentSnapshots { get; set; } = new();
}

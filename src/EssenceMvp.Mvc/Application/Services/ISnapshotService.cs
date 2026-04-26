using EssenceMvp.Mvc.Models;

namespace EssenceMvp.Mvc.Application.Services;

public interface ISnapshotService
{
    Task<SnapshotSummaryDto> CreateSnapshotAsync(int projectId, int userId);
    Task<List<SnapshotSummaryDto>> GetHistoryAsync(int projectId, int userId);
}

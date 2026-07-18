using EssenceMvp.Application.Dtos;

namespace EssenceMvp.Application.Services;

public interface ISnapshotService
{
    Task<SnapshotSummaryDto> CreateSnapshotAsync(int projectId, int userId);
    Task<List<SnapshotSummaryDto>> GetHistoryAsync(int projectId, int userId);
    Task DeleteSnapshotAsync(int snapshotId, int projectId, int userId);
}

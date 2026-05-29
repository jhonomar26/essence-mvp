using EssenceMvp.Application.DTOs;

namespace EssenceMvp.Application.Interfaces;

public interface ISnapshotService
{
    Task<SnapshotSummaryDto> CreateSnapshotAsync(int projectId, int userId);
    Task<List<SnapshotSummaryDto>> GetHistoryAsync(int projectId, int userId);
    Task DeleteSnapshotAsync(int snapshotId, int projectId, int userId);
}

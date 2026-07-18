using EssenceMvp.Domain.Entities;

namespace EssenceMvp.Application.Abstractions;

public interface IHealthReportRepository
{
    Task<HealthReport> AddAsync(HealthReport report);
    Task<List<HealthReport>> GetRecentByProjectIdAsync(int projectId, int take);
    Task<HealthReport?> GetAsync(int snapshotId, int projectId);
    Task DeleteAsync(HealthReport report);
}

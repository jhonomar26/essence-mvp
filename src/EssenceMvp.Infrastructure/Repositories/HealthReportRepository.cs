using EssenceMvp.Application.Abstractions;
using EssenceMvp.Domain.Entities;
using EssenceMvp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Infrastructure.Repositories;

public class HealthReportRepository : IHealthReportRepository
{
    private readonly EssenceDbContext _db;

    public HealthReportRepository(EssenceDbContext db) => _db = db;

    public async Task<HealthReport> AddAsync(HealthReport report)
    {
        _db.HealthReports.Add(report);
        await _db.SaveChangesAsync();
        return report;
    }

    public Task<List<HealthReport>> GetRecentByProjectIdAsync(int projectId, int take) =>
        _db.HealthReports
            .Where(r => r.ProjectId == projectId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(take)
            .ToListAsync();

    public Task<HealthReport?> GetAsync(int snapshotId, int projectId) =>
        _db.HealthReports.FirstOrDefaultAsync(r => r.Id == snapshotId && r.ProjectId == projectId);

    public async Task DeleteAsync(HealthReport report)
    {
        _db.HealthReports.Remove(report);
        await _db.SaveChangesAsync();
    }
}

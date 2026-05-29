using EssenceMvp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Application.Interfaces;

public interface IEssenceDbContext
{
    DbSet<Alpha> Alphas { get; }
    DbSet<AlphaState> AlphaStates { get; }
    DbSet<StateChecklist> StateChecklists { get; }
    DbSet<Project> Projects { get; }
    DbSet<ProjectAlphaStatus> ProjectAlphaStatuses { get; }
    DbSet<ChecklistResponse> ChecklistResponses { get; }
    DbSet<HealthReport> HealthReports { get; }
    DbSet<AppUser> AppUsers { get; }
    DbSet<UserSession> UserSessions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

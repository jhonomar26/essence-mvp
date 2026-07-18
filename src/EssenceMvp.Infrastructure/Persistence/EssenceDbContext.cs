using System.Reflection;
using EssenceMvp.Domain.Entities;
using EssenceMvp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Infrastructure.Persistence;

public class EssenceDbContext : DbContext
{
    public EssenceDbContext(DbContextOptions<EssenceDbContext> options) : base(options) { }

    public DbSet<Alpha> Alphas { get; set; }
    public DbSet<AlphaState> AlphaStates { get; set; }
    public DbSet<StateChecklist> StateChecklists { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectAlphaStatus> ProjectAlphaStatuses { get; set; }
    public DbSet<ChecklistResponse> ChecklistResponses { get; set; }
    public DbSet<HealthReport> HealthReports { get; set; }
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<HealthStatus>("health_status");
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}

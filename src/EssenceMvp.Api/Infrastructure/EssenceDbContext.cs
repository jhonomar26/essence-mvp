using EssenceMvp.Api.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Api.Infrastructure;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<HealthStatus>("health_status");

        modelBuilder.Entity<Alpha>(e =>
        {
            e.ToTable("alpha");
            e.Property(a => a.Id).HasColumnName("id");
            e.Property(a => a.Name).HasColumnName("name");
            e.Property(a => a.AreaOfConcern).HasColumnName("area_of_concern");
            e.Property(a => a.Description).HasColumnName("description");
        });

        modelBuilder.Entity<AlphaState>(e =>
        {
            e.ToTable("alpha_state");
            e.Property(s => s.Id).HasColumnName("id");
            e.Property(s => s.AlphaId).HasColumnName("alpha_id");
            e.Property(s => s.StateNumber).HasColumnName("state_number");
            e.Property(s => s.StateName).HasColumnName("state_name");
            e.Property(s => s.Description).HasColumnName("description");
            e.HasOne(s => s.Alpha).WithMany(a => a.States).HasForeignKey(s => s.AlphaId);
        });

        modelBuilder.Entity<StateChecklist>(e =>
        {
            e.ToTable("state_checklist");
            e.Property(c => c.Id).HasColumnName("id");
            e.Property(c => c.AlphaStateId).HasColumnName("alpha_state_id");
            e.Property(c => c.CriterionText).HasColumnName("criterion_text");
            e.Property(c => c.IsMandatory).HasColumnName("is_mandatory");
            e.HasOne(c => c.AlphaState).WithMany(s => s.Checklists).HasForeignKey(c => c.AlphaStateId);
        });

        modelBuilder.Entity<AppUser>(e =>
        {
            e.ToTable("app_user");
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Id).HasColumnName("id");
            e.Property(u => u.Email).HasColumnName("email");
            e.Property(u => u.PasswordHash).HasColumnName("password_hash");
            e.Property(u => u.DisplayName).HasColumnName("display_name");
            e.Property(u => u.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Project>(e =>
        {
            e.ToTable("project");
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.UserId).HasColumnName("user_id");
            e.Property(p => p.Name).HasColumnName("name").HasMaxLength(200);
            e.Property(p => p.Description).HasColumnName("description");
            e.Property(p => p.Phase).HasColumnName("phase").HasMaxLength(100);
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
            e.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectAlphaStatus>(e =>
        {
            e.ToTable("project_alpha_status");
            e.Property(s => s.Id).HasColumnName("id");
            e.Property(s => s.ProjectId).HasColumnName("project_id");
            e.Property(s => s.AlphaId).HasColumnName("alpha_id");
            e.Property(s => s.CurrentStateNumber).HasColumnName("current_state_number");
            e.Property(s => s.UpdatedAt).HasColumnName("updated_at");
            e.HasOne(s => s.Project).WithMany(p => p.AlphaStatuses).HasForeignKey(s => s.ProjectId);
            e.HasOne(s => s.Alpha).WithMany(a => a.ProjectStatuses).HasForeignKey(s => s.AlphaId);
        });

        modelBuilder.Entity<ChecklistResponse>(e =>
        {
            e.ToTable("checklist_response");
            e.Property(r => r.Id).HasColumnName("id");
            e.Property(r => r.ProjectId).HasColumnName("project_id");
            e.Property(r => r.StateChecklistId).HasColumnName("state_checklist_id");
            e.Property(r => r.IsAchieved).HasColumnName("is_achieved");
            e.Property(r => r.Notes).HasColumnName("notes");
            e.Property(r => r.UpdatedAt).HasColumnName("updated_at");
            e.HasOne(r => r.Project).WithMany(p => p.ChecklistResponses).HasForeignKey(r => r.ProjectId);
            e.HasOne(r => r.StateChecklist).WithMany(c => c.Responses).HasForeignKey(r => r.StateChecklistId);
        });

        modelBuilder.Entity<HealthReport>(e =>
        {
            e.ToTable("health_report");
            e.Property(r => r.Id).HasColumnName("id");
            e.Property(r => r.ProjectId).HasColumnName("project_id");
            e.Property(r => r.CreatedAt).HasColumnName("created_at");
            e.Property(r => r.GlobalStatus).HasColumnName("global_status");
            e.Property(r => r.AlphaDetails).HasColumnName("alpha_details").HasColumnType("jsonb");
            e.HasOne(r => r.Project).WithMany(p => p.HealthReports).HasForeignKey(r => r.ProjectId);
        });
    }
}

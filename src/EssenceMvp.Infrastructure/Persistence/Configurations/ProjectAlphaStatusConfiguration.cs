using EssenceMvp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EssenceMvp.Infrastructure.Persistence.Configurations;

public class ProjectAlphaStatusConfiguration : IEntityTypeConfiguration<ProjectAlphaStatus>
{
    public void Configure(EntityTypeBuilder<ProjectAlphaStatus> e)
    {
        e.ToTable("project_alpha_status");
        e.Property(s => s.Id).HasColumnName("id");
        e.Property(s => s.ProjectId).HasColumnName("project_id");
        e.Property(s => s.AlphaId).HasColumnName("alpha_id");
        e.Property(s => s.CurrentStateNumber).HasColumnName("current_state_number");
        e.Property(s => s.UpdatedAt).HasColumnName("updated_at");
        e.HasOne(s => s.Project).WithMany(p => p.AlphaStatuses).HasForeignKey(s => s.ProjectId);
        e.HasOne(s => s.Alpha).WithMany(a => a.ProjectStatuses).HasForeignKey(s => s.AlphaId);
    }
}

using EssenceMvp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EssenceMvp.Infrastructure.Persistence.Configurations;

public class HealthReportConfiguration : IEntityTypeConfiguration<HealthReport>
{
    public void Configure(EntityTypeBuilder<HealthReport> e)
    {
        e.ToTable("health_report");
        e.Property(r => r.Id).HasColumnName("id");
        e.Property(r => r.ProjectId).HasColumnName("project_id");
        e.Property(r => r.CreatedAt).HasColumnName("created_at");
        e.Property(r => r.GlobalStatus)
            .HasColumnName("global_status");
        e.Property(r => r.AlphaDetails).HasColumnName("alpha_details").HasColumnType("jsonb");
        e.HasOne(r => r.Project).WithMany(p => p.HealthReports).HasForeignKey(r => r.ProjectId);
    }
}

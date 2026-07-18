using EssenceMvp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EssenceMvp.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> e)
    {
        e.ToTable("project");
        e.Property(p => p.Id).HasColumnName("id");
        e.Property(p => p.UserId).HasColumnName("user_id");
        e.Property(p => p.Name).HasColumnName("name").HasMaxLength(200);
        e.Property(p => p.Description).HasColumnName("description");
        e.Property(p => p.Phase).HasColumnName("phase").HasMaxLength(100);
        e.Property(p => p.CreatedAt).HasColumnName("created_at");
        e.HasOne<AppUser>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

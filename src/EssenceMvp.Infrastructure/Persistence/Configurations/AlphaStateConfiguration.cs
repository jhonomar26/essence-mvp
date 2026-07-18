using EssenceMvp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EssenceMvp.Infrastructure.Persistence.Configurations;

public class AlphaStateConfiguration : IEntityTypeConfiguration<AlphaState>
{
    public void Configure(EntityTypeBuilder<AlphaState> e)
    {
        e.ToTable("alpha_state");
        e.Property(s => s.Id).HasColumnName("id");
        e.Property(s => s.AlphaId).HasColumnName("alpha_id");
        e.Property(s => s.StateNumber).HasColumnName("state_number");
        e.Property(s => s.StateName).HasColumnName("state_name");
        e.Property(s => s.Description).HasColumnName("description");
        e.HasOne(s => s.Alpha).WithMany(a => a.States).HasForeignKey(s => s.AlphaId);
    }
}

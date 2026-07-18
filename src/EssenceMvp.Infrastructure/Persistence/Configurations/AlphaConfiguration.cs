using EssenceMvp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EssenceMvp.Infrastructure.Persistence.Configurations;

public class AlphaConfiguration : IEntityTypeConfiguration<Alpha>
{
    public void Configure(EntityTypeBuilder<Alpha> e)
    {
        e.ToTable("alpha");
        e.Property(a => a.Id).HasColumnName("id");
        e.Property(a => a.Name).HasColumnName("name");
        e.Property(a => a.AreaOfConcern).HasColumnName("area_of_concern");
        e.Property(a => a.Description).HasColumnName("description");
    }
}

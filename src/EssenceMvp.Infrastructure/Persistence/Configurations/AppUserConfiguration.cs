using EssenceMvp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EssenceMvp.Infrastructure.Persistence.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> e)
    {
        e.ToTable("app_user");
        e.HasIndex(u => u.Email).IsUnique();
        e.Property(u => u.Id).HasColumnName("id");
        e.Property(u => u.Email).HasColumnName("email");
        e.Property(u => u.PasswordHash).HasColumnName("password_hash");
        e.Property(u => u.DisplayName).HasColumnName("display_name");
        e.Property(u => u.CreatedAt).HasColumnName("created_at");
    }
}

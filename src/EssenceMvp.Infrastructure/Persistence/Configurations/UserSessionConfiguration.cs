using EssenceMvp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EssenceMvp.Infrastructure.Persistence.Configurations;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> e)
    {
        e.ToTable("user_session");
        e.Property(s => s.Id).HasColumnName("id");
        e.Property(s => s.AppUserId).HasColumnName("app_user_id");
        e.Property(s => s.RefreshTokenHash).HasColumnName("refresh_token_hash");
        e.Property(s => s.ExpiresAt).HasColumnName("expires_at");
        e.Property(s => s.CreatedAt).HasColumnName("created_at");
        e.Property(s => s.RevokedAt).HasColumnName("revoked_at");
        e.Property(s => s.ReplacedByTokenHash).HasColumnName("replaced_by_token_hash");
        e.HasOne(s => s.AppUser).WithMany().HasForeignKey(s => s.AppUserId).OnDelete(DeleteBehavior.Cascade);
        e.HasIndex(s => s.RefreshTokenHash).IsUnique();
    }
}

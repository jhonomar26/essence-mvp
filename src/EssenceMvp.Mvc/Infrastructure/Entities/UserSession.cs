namespace EssenceMvp.Mvc.Infrastructure.Entities;

public class UserSession
{
    public int Id { get; set; }
    public int AppUserId { get; set; }
    public string RefreshTokenHash { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    public AppUser AppUser { get; set; } = null!;
}

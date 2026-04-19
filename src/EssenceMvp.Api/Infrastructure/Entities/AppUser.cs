namespace EssenceMvp.Api.Infrastructure.Entities;

public class AppUser
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? DisplayName { get; set; }
    public DateTime CreatedAt { get; set; }
}


namespace EssenceMvp.Api.Features;

public sealed class ProjectDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string? Phase { get; init; }
    public DateTime CreatedAt { get; init; }
}

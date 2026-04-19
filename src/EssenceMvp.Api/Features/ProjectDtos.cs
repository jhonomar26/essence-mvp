namespace EssenceMvp.Api.Features;

public sealed class ProjectDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string? Phase { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class CreateProjectRequest
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string? Phase { get; init; }
}

public sealed class UpdateProjectRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Phase { get; init; }
}

public sealed class ProjectDetailDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string? Phase { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<AlphaStatusDto> AlphaStatuses { get; init; } = new();
}

public sealed class AlphaStatusDto
{
    public int AlphaId { get; init; }
    public string AlphaName { get; init; } = null!;
    public string AreaOfConcern { get; init; } = null!;
    public int CurrentStateNumber { get; init; }
    public string? CurrentStateName { get; init; }
}

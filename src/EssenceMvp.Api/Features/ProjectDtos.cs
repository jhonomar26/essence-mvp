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

public sealed class HealthResponseDto
{
    public decimal HealthScore { get; init; }
    public string Classification { get; init; } = "";
    public decimal AverageProgress { get; init; }
    public decimal ProgressDispersion { get; init; }
    public List<AlphaHealthDetailDto> AlphaProgresses { get; init; } = new();
}

public sealed class AlphaHealthDetailDto
{
    public int AlphaId { get; init; }
    public string AlphaName { get; init; } = null!;
    public short CurrentStateNumber { get; init; }
    public short MaxStateNumber { get; init; }
    public decimal Progress { get; init; }
}

public sealed class ChecklistResponseRequest
{
    public int AlphaId { get; init; }
    public List<ChecklistItemRequest> Items { get; init; } = new();
}

public sealed class ChecklistItemRequest
{
    public int StateChecklistId { get; init; }
    public bool IsAchieved { get; init; }
    public string? Notes { get; init; }
}

public sealed class RecalculateAlphaStateDto
{
    public int AlphaId { get; init; }
    public short NewStateNumber { get; init; }
    public string StateName { get; init; } = null!;
}

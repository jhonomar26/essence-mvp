namespace EssenceMvp.Api.Features;

public sealed class AlphaDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string AreaOfConcern { get; init; } = null!;
    public string? Description { get; init; }
}

public sealed class AlphaStateDto
{
    public int Id { get; init; }
    public int AlphaId { get; init; }
    public int StateNumber { get; init; }
    public string StateName { get; init; } = null!;
    public string? Description { get; init; }
}

public sealed class StateChecklistDto
{
    public int Id { get; init; }
    public string CriterionText { get; init; } = null!;
    public bool IsMandatory { get; init; }
}

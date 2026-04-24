using System.ComponentModel.DataAnnotations;
using EssenceMvp.Mvc.Infrastructure.Entities;

namespace EssenceMvp.Mvc.Models;

public class ProjectSummary
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Phase { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public HealthStatus OverallHealth { get; set; }
}

public class CreateProjectViewModel
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = "";

    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Phase { get; set; }
}

public class ProjectDetailViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? Phase { get; set; }
    public HealthStatus OverallHealth { get; set; }
    public List<AlphaProgressDto> AlphaProgress { get; set; } = new();
}

public class AlphaProgressDto
{
    public int AlphaId { get; set; }
    public string AlphaName { get; set; } = "";
    public string AreaOfConcern { get; set; } = "";
    public short CurrentStateNumber { get; set; }
    public int TotalStates { get; set; }
    public string CurrentStateName { get; set; } = "";
    public double CompletionPercent { get; set; }
    public HealthStatus AlphaHealth { get; set; }
}

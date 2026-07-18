using System.ComponentModel.DataAnnotations;
using EssenceMvp.Domain.Enums;

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

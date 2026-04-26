using EssenceMvp.Mvc.Models;

namespace EssenceMvp.Mvc.Application.Services;

public interface IHealthCalculationService
{
    Task<ProjectHealthScore> CalculateProjectHealthAsync(int projectId);
}
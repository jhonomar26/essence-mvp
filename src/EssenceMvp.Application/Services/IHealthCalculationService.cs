using EssenceMvp.Application.Dtos;

namespace EssenceMvp.Application.Services;

public interface IHealthCalculationService
{
    Task<ProjectHealthScore> CalculateProjectHealthAsync(int projectId);
}

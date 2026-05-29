using EssenceMvp.Application.DTOs;

namespace EssenceMvp.Application.Interfaces;

public interface IHealthCalculationService
{
    Task<ProjectHealthScore> CalculateProjectHealthAsync(int projectId);
}

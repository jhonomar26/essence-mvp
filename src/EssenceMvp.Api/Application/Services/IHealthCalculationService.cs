using EssenceMvp.Api.Features;

namespace EssenceMvp.Api.Application.Services;

public interface IHealthCalculationService
{
    Task<ProjectHealthScoreDto> CalculateProjectHealthAsync(int projectId);
}

using EssenceMvp.Mvc.Infrastructure.Entities;

namespace EssenceMvp.Mvc.Application.Services;

public interface IHealthService
{
    Task<HealthStatus> CalculateHealthAsync(int projectId, int userId);
}

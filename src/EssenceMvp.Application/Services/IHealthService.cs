using EssenceMvp.Domain.Enums;

namespace EssenceMvp.Application.Services;

public interface IHealthService
{
    Task<HealthStatus> CalculateHealthAsync(int projectId, int userId);
}

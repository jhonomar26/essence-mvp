using EssenceMvp.Domain.Entities;

namespace EssenceMvp.Application.Interfaces;

public interface IHealthService
{
    Task<HealthStatus> CalculateHealthAsync(int projectId, int userId);
}

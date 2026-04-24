namespace EssenceMvp.Api.Application.Services;

public interface IHealthCalculationService
{
    Task<string> CalculateAsync(int projectId);
}

namespace EssenceMvp.Mvc.Application.Services;

public interface IHealthCalculationService
{
    Task<string> CalculateAsync(int projectId);
}
using EssenceMvp.Application.Dtos;

namespace EssenceMvp.Application.Services;

public interface IAlphaEvaluationService
{
    Task<AlphaStateResult> CalculateAsync(int projectId, int alphaId);
}

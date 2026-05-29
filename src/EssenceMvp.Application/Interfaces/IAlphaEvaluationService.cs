using EssenceMvp.Application.DTOs;

namespace EssenceMvp.Application.Interfaces;

public interface IAlphaEvaluationService
{
    Task<AlphaStateResult> CalculateAsync(int projectId, int alphaId);
}

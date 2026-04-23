using EssenceMvp.Mvc.Models;

namespace EssenceMvp.Mvc.Application.Services;

public interface IAlphaEvaluationService
{
    Task<AlphaStateResult> CalculateAsync(int projectId, int alphaId);

}
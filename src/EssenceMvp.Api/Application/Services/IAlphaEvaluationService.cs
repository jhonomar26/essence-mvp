namespace EssenceMvp.Api.Application.Services;

public record AlphaStateResult(short CurrentStateNumber, string CurrentStateName, short MaxStateNumber);

public interface IAlphaEvaluationService
{
    Task<AlphaStateResult> CalculateAsync(int projectId, int alphaId);
}

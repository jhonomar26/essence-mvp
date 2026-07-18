using EssenceMvp.Application.Abstractions;
using EssenceMvp.Application.Dtos;

namespace EssenceMvp.Application.Services;

public class AlphaEvaluationService : IAlphaEvaluationService
{
    private readonly IAlphaRepository _alphas;
    private readonly IChecklistResponseRepository _checklistResponses;

    public AlphaEvaluationService(IAlphaRepository alphas, IChecklistResponseRepository checklistResponses)
    {
        _alphas = alphas;
        _checklistResponses = checklistResponses;
    }

    // SEMAT Essence: Alpha in state N only if ALL checklists of states 1..N are satisfied.
    // Iterates ordered states. Stops at first incomplete state (binary sequential logic).
    public async Task<AlphaStateResult> CalculateAsync(int projectId, int alphaId)
    {
        var alpha = await _alphas.GetWithStatesAndChecklistsAsync(alphaId);

        if (alpha == null)
            return new AlphaStateResult { CurrentStateNumber = 0, CurrentStateName = "Not Found", MaxStateNumber = 0 };

        var states = alpha.States.OrderBy(s => s.StateNumber).ToList();
        short maxStateNumber = (short)states.Count;

        short current = 0;
        string name = "Not Started";

        foreach (var state in states)
        {
            var checklistIds = state.Checklists.Select(c => c.Id).ToList();

            var responses = await _checklistResponses.GetByProjectAndChecklistIdsAsync(projectId, checklistIds);

            bool completed = state.Checklists.All(c =>
                responses.Any(r => r.StateChecklistId == c.Id && r.IsAchieved));

            if (!completed)
                break;

            current = state.StateNumber;
            name = state.StateName;
        }

        return new AlphaStateResult
        {
            CurrentStateNumber = current,
            CurrentStateName = name,
            MaxStateNumber = maxStateNumber
        };
    }
}

using EssenceMvp.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Api.Application.Services;

public class AlphaEvaluationService : IAlphaEvaluationService
{
    private readonly EssenceDbContext _db;

    public AlphaEvaluationService(EssenceDbContext db)
    {
        _db = db;
    }

    // SEMAT Essence: Alpha in state N only if ALL checklists of states 1..N are satisfied.
    // Iterates ordered states. Stops at first incomplete state (binary sequential logic).
    public async Task<AlphaStateResult> CalculateAsync(int projectId, int alphaId)
    {
        var alpha = await _db.Alphas
            .Include(a => a.States)
            .ThenInclude(s => s.Checklists)
            .FirstOrDefaultAsync(a => a.Id == alphaId);

        if (alpha == null)
            return new AlphaStateResult(0, "Not Found", 0);

        var states = alpha.States.OrderBy(s => s.StateNumber).ToList();
        short maxStateNumber = (short)states.Count;

        short current = 0;
        string name = "Not Started";

        foreach (var state in states)
        {
            var checklistIds = state.Checklists.Select(c => c.Id).ToList();

            var responses = await _db.ChecklistResponses
                .Where(r => r.ProjectId == projectId &&
                            checklistIds.Contains(r.StateChecklistId))
                .ToListAsync();

            bool completed = state.Checklists.All(c =>
                responses.Any(r => r.StateChecklistId == c.Id && r.IsAchieved));

            if (!completed)
                break;

            current = state.StateNumber;
            name = state.StateName;
        }

        return new AlphaStateResult(current, name, maxStateNumber);
    }
}

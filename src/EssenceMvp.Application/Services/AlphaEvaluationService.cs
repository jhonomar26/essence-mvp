using EssenceMvp.Application.DTOs;
using EssenceMvp.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Application.Services;

public class AlphaEvaluationService : IAlphaEvaluationService
{
    private readonly IEssenceDbContext _db;

    public AlphaEvaluationService(IEssenceDbContext db) => _db = db;

    // SEMAT Essence: Alpha in state N only if ALL checklists of states 1..N are satisfied.
    public async Task<AlphaStateResult> CalculateAsync(int projectId, int alphaId)
    {
        var alpha = await _db.Alphas
            .Include(a => a.States)
            .ThenInclude(s => s.Checklists)
            .FirstOrDefaultAsync(a => a.Id == alphaId);

        if (alpha == null)
            return new AlphaStateResult { CurrentStateNumber = 0, CurrentStateName = "Not Found", MaxStateNumber = 0 };

        var states = alpha.States.OrderBy(s => s.StateNumber).ToList();
        short maxStateNumber = (short)states.Count;

        short current = 0;
        string name = "Not Started";

        foreach (var state in states)
        {
            var checklistIds = state.Checklists.Select(c => c.Id).ToList();

            var responses = await _db.ChecklistResponses
                .Where(r => r.ProjectId == projectId && checklistIds.Contains(r.StateChecklistId))
                .ToListAsync();

            bool completed = state.Checklists.All(c =>
                responses.Any(r => r.StateChecklistId == c.Id && r.IsAchieved));

            if (!completed) break;

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

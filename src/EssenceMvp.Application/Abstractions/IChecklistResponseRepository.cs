using EssenceMvp.Domain.Entities;

namespace EssenceMvp.Application.Abstractions;

public interface IChecklistResponseRepository
{
    Task<List<ChecklistResponse>> GetByProjectIdAsync(int projectId);
    Task<List<ChecklistResponse>> GetByProjectAndChecklistIdsAsync(int projectId, List<int> checklistIds);
    Task UpsertRangeAsync(int projectId, List<(int StateChecklistId, bool IsAchieved, string? Notes)> responses);
}

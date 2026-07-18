using EssenceMvp.Application.Abstractions;
using EssenceMvp.Domain.Entities;
using EssenceMvp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Infrastructure.Repositories;

public class ChecklistResponseRepository : IChecklistResponseRepository
{
    private readonly EssenceDbContext _db;

    public ChecklistResponseRepository(EssenceDbContext db) => _db = db;

    public Task<List<ChecklistResponse>> GetByProjectIdAsync(int projectId) =>
        _db.ChecklistResponses
            .Where(r => r.ProjectId == projectId)
            .ToListAsync();

    public Task<List<ChecklistResponse>> GetByProjectAndChecklistIdsAsync(int projectId, List<int> checklistIds) =>
        _db.ChecklistResponses
            .Where(r => r.ProjectId == projectId && checklistIds.Contains(r.StateChecklistId))
            .ToListAsync();

    // Guarda/actualiza respuestas de evaluación de checklist del usuario (upsert)
    public async Task UpsertRangeAsync(int projectId, List<(int StateChecklistId, bool IsAchieved, string? Notes)> responses)
    {
        foreach (var (checklistId, isAchieved, notes) in responses)
        {
            var existing = await _db.ChecklistResponses
                .FirstOrDefaultAsync(r => r.ProjectId == projectId && r.StateChecklistId == checklistId);

            if (existing == null)
            {
                _db.ChecklistResponses.Add(new ChecklistResponse
                {
                    ProjectId = projectId,
                    StateChecklistId = checklistId,
                    IsAchieved = isAchieved,
                    Notes = notes,
                    UpdatedAt = DateTimeOffset.UtcNow
                });
            }
            else
            {
                existing.IsAchieved = isAchieved;
                existing.Notes = notes;
                existing.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
    }
}

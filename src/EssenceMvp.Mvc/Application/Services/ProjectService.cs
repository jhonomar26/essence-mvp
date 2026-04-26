using EssenceMvp.Mvc.Infrastructure;
using EssenceMvp.Mvc.Infrastructure.Entities;
using EssenceMvp.Mvc.Models;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Mvc.Application.Services;

public class ProjectService : IProjectService
{
    private readonly EssenceDbContext _db;

    public ProjectService(EssenceDbContext db) => _db = db;

    public async Task<List<Project>> GetUserProjectsAsync(int userId) =>
        await _db.Projects
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<Project> CreateProjectAsync(int userId, string name, string? description, string? phase)
    {
        var project = new Project
        {
            UserId = userId,
            Name = name,
            Description = description,
            Phase = phase,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        var alphaIds = await _db.Alphas.Select(a => a.Id).ToListAsync();
        foreach (var alphaId in alphaIds)
        {
            _db.ProjectAlphaStatuses.Add(new ProjectAlphaStatus
            {
                ProjectId = project.Id,
                AlphaId = alphaId,
                CurrentStateNumber = 0,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }

        await _db.SaveChangesAsync();
        return project;
    }

    public async Task<Project?> GetProjectDetailAsync(int projectId, int userId) =>
        await _db.Projects
            .Include(p => p.AlphaStatuses)
            .ThenInclude(s => s.Alpha)
            .ThenInclude(a => a.States)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

    public async Task<bool> DeleteProjectAsync(int projectId, int userId)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);
        if (project == null) return false;
        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();
        return true;
    }

    // Guarda/actualiza respuestas de evaluación de checklist del usuario (upsert)
    public async Task SaveChecklistResponsesAsync(int projectId,
        List<(int stateChecklistId, bool isAchieved, string? notes)> responses)
    {
        foreach (var (checklistId, isAchieved, notes) in responses)
        {
            var existing = await _db.ChecklistResponses
                .FirstOrDefaultAsync(r => r.ProjectId == projectId && r.StateChecklistId == checklistId);

            if (existing == null)
            {
                // Nueva respuesta
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
                // Actualiza respuesta existente
                existing.IsAchieved = isAchieved;
                existing.Notes = notes;
                existing.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
    }

    // Devuelve todos los checklists de un Alpha, combinando la estructura (preguntas)
    // con las respuestas del proyecto para indicar cuáles están cumplidos (true/false)
    public async Task<List<(int Id, string CriterionText, bool IsAchieved)>> GetAlphaChecklistsAsync(int projectId,
        int alphaId)
    {
        var alpha = await _db.Alphas
            .Include(a => a.States)
            .ThenInclude(s => s.Checklists)
            .FirstOrDefaultAsync(a => a.Id == alphaId);

        if (alpha == null) return new();

        // Obtiene respuestas previas del proyecto
        var responses = await _db.ChecklistResponses
            .Where(r => r.ProjectId == projectId)
            .ToListAsync();

        var result = new List<(int Id, string CriterionText, bool IsAchieved)>();

        // Itera estados ordenados y sus checklists
        foreach (var state in alpha.States.OrderBy(s => s.StateNumber))
        {
            foreach (var checklist in state.Checklists)
            {
                var response = responses.FirstOrDefault(r => r.StateChecklistId == checklist.Id);
                // IsAchieved: true si existe respuesta anterior y está marcada, false si no existe
                result.Add((checklist.Id, checklist.CriterionText, response?.IsAchieved ?? false));
            }
        }

        return result;
    }

    // Returns current state of Alpha using SEMAT Essence sequential logic.
    // Current state = highest state where ALL checklists (1..N) are satisfied.
    public async Task<AlphaStateResult> GetAlphaCurrentStateAsync(int projectId, int alphaId)
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

        return new AlphaStateResult
        {
            CurrentStateNumber = current,
            CurrentStateName = name,
            MaxStateNumber = maxStateNumber
        };
    }
}
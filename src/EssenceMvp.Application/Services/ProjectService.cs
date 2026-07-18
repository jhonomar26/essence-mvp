using EssenceMvp.Application.Abstractions;
using EssenceMvp.Application.Dtos;
using EssenceMvp.Domain.Entities;

namespace EssenceMvp.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projects;
    private readonly IAlphaRepository _alphas;
    private readonly IProjectAlphaStatusRepository _projectAlphaStatuses;
    private readonly IChecklistResponseRepository _checklistResponses;

    public ProjectService(
        IProjectRepository projects,
        IAlphaRepository alphas,
        IProjectAlphaStatusRepository projectAlphaStatuses,
        IChecklistResponseRepository checklistResponses)
    {
        _projects = projects;
        _alphas = alphas;
        _projectAlphaStatuses = projectAlphaStatuses;
        _checklistResponses = checklistResponses;
    }

    public Task<List<Project>> GetUserProjectsAsync(int userId) =>
        _projects.GetByUserIdAsync(userId);

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
        project = await _projects.CreateAsync(project);

        var alphaIds = await _alphas.GetAllIdsAsync();
        var statuses = alphaIds.Select(alphaId => new ProjectAlphaStatus
        {
            ProjectId = project.Id,
            AlphaId = alphaId,
            CurrentStateNumber = 0,
            UpdatedAt = DateTimeOffset.UtcNow
        }).ToList();

        await _projectAlphaStatuses.AddRangeAsync(statuses);
        return project;
    }

    public Task<Project?> GetProjectDetailAsync(int projectId, int userId) =>
        _projects.GetDetailByIdForUserAsync(projectId, userId);

    public async Task<bool> DeleteProjectAsync(int projectId, int userId)
    {
        var project = await _projects.GetByIdForUserAsync(projectId, userId);
        if (project == null) return false;
        await _projects.DeleteAsync(project);
        return true;
    }

    // Guarda/actualiza respuestas de evaluación de checklist del usuario (upsert)
    public Task SaveChecklistResponsesAsync(int projectId,
        List<(int stateChecklistId, bool isAchieved, string? notes)> responses) =>
        _checklistResponses.UpsertRangeAsync(projectId, responses);

    // Devuelve todos los checklists de un Alpha, combinando la estructura (preguntas)
    // con las respuestas del proyecto para indicar cuáles están cumplidos (true/false)
    public async Task<List<AlphaChecklistDto>> GetAlphaChecklistsAsync(int projectId,
        int alphaId)
    {
        var alpha = await _alphas.GetWithStatesAndChecklistsAsync(alphaId);
        if (alpha == null) return new();

        // Obtiene respuestas previas del proyecto
        var responses = await _checklistResponses.GetByProjectIdAsync(projectId);

        var result = new List<AlphaChecklistDto>();

        // Itera estados ordenados y agrupa checklists por estado
        foreach (var state in alpha.States.OrderBy(s => s.StateNumber))
        {
            var stateCriteria = new List<ChecklistCriterionDto>();

            foreach (var checklist in state.Checklists)
            {
                var response = responses.FirstOrDefault(r => r.StateChecklistId == checklist.Id);
                stateCriteria.Add(new ChecklistCriterionDto
                {
                    Id = checklist.Id,
                    CriterionText = checklist.CriterionText,
                    IsAchieved = response?.IsAchieved ?? false
                });
            }

            if (stateCriteria.Count > 0)
            {
                result.Add(new AlphaChecklistDto
                {
                    StateNumber = state.StateNumber,
                    StateName = state.StateName,
                    Criteria = stateCriteria
                });
            }
        }

        return result;
    }

    // Returns current state of Alpha using SEMAT Essence sequential logic.
    // Current state = highest state where ALL checklists (1..N) are satisfied.
    public async Task<AlphaStateResult> GetAlphaCurrentStateAsync(int projectId, int alphaId)
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

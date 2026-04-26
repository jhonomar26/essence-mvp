using EssenceMvp.Mvc.Infrastructure.Entities;
using EssenceMvp.Mvc.Models;

namespace EssenceMvp.Mvc.Application.Services;

public interface IProjectService
{
    Task<List<Project>> GetUserProjectsAsync(int userId);
    Task<Project> CreateProjectAsync(int userId, string name, string? description, string? phase);
    Task<Project?> GetProjectDetailAsync(int projectId, int userId);
    Task<bool> DeleteProjectAsync(int projectId, int userId);
    Task SaveChecklistResponsesAsync(int projectId, List<(int stateChecklistId, bool isAchieved, string? notes)> responses);
    Task<List<AlphaChecklistDto>> GetAlphaChecklistsAsync(int projectId, int alphaId);
    Task<AlphaStateResult> GetAlphaCurrentStateAsync(int projectId, int alphaId);
}

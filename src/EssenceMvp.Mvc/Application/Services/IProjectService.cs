using EssenceMvp.Mvc.Infrastructure.Entities;

namespace EssenceMvp.Mvc.Application.Services;

public interface IProjectService
{
    Task<List<Project>> GetUserProjectsAsync(int userId);
    Task<Project> CreateProjectAsync(int userId, string name, string? description, string? phase);
    Task<Project?> GetProjectDetailAsync(int projectId, int userId);
    Task<bool> DeleteProjectAsync(int projectId, int userId);
}

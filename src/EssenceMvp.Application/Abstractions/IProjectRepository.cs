using EssenceMvp.Domain.Entities;

namespace EssenceMvp.Application.Abstractions;

public interface IProjectRepository
{
    Task<List<Project>> GetByUserIdAsync(int userId);
    Task<Project?> GetByIdAsync(int projectId);
    Task<Project?> GetByIdForUserAsync(int projectId, int userId);
    Task<Project?> GetDetailByIdForUserAsync(int projectId, int userId);
    Task<Project> CreateAsync(Project project);
    Task DeleteAsync(Project project);
}

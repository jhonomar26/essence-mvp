using EssenceMvp.Application.Dtos;

namespace EssenceMvp.Application.Services;

public interface IProjectDetailComposerService
{
    Task<ProjectDetailComposerResult?> GetProjectDetailAsync(int projectId, int userId);
}

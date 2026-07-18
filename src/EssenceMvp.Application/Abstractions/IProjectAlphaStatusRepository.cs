using EssenceMvp.Domain.Entities;

namespace EssenceMvp.Application.Abstractions;

public interface IProjectAlphaStatusRepository
{
    Task<ProjectAlphaStatus?> GetAsync(int projectId, int alphaId);
    Task UpdateCurrentStateAsync(int projectId, int alphaId, short newStateNumber);
    Task AddRangeAsync(List<ProjectAlphaStatus> statuses);
}

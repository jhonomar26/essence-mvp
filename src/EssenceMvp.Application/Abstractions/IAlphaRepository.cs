using EssenceMvp.Domain.Entities;

namespace EssenceMvp.Application.Abstractions;

public interface IAlphaRepository
{
    Task<List<int>> GetAllIdsAsync();
    Task<Alpha?> GetWithStatesAndChecklistsAsync(int alphaId);
    Task<List<Alpha>> GetAllWithStatesAndProjectStatusAsync(int projectId);
}

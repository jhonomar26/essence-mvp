using EssenceMvp.Application.DTOs;

namespace EssenceMvp.Application.Interfaces;

public interface IAlphaService
{
    Task<List<AlphaProgressDto>> GetProjectAlphaProgressAsync(int projectId, int userId);
}

using EssenceMvp.Mvc.Models;

namespace EssenceMvp.Mvc.Application.Services;

public interface IAlphaService
{
    Task<List<AlphaProgressDto>> GetProjectAlphaProgressAsync(int projectId, int userId);
}

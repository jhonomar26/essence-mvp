using EssenceMvp.Domain.Entities;

namespace EssenceMvp.Application.Services;

public interface IAccessTokenService
{
    string GenerateAccessToken(AppUser user);
}
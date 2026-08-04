namespace EssenceMvp.Application.Services;

public interface ISessionTokenService
{
    string GenerateToken();
    string Hash(string rawToken);
}

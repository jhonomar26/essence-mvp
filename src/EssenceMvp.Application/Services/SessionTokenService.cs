using System.Security.Cryptography;
using System.Text;

namespace EssenceMvp.Application.Services;

public class SessionTokenService : ISessionTokenService
{
    public string GenerateToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    public string Hash(string rawToken) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}

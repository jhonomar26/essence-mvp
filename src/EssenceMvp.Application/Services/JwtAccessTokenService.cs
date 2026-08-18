using System.Security.Claims;
using System.Text;
using EssenceMvp.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace EssenceMvp.Application.Services;

public class JwtAccessTokenService : IAccessTokenService
{
    private readonly JwtOptions _options;

    public JwtAccessTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateAccessToken(AppUser user)
    {
        // Herramienta encargada de generar el JWT
        var handler = new JsonWebTokenHandler();
        // Clave secreta utilizada para firmar el JWT.
        // Se convierte a bytes porque el algoritmo de firma trabaja con bytes.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        // Configuración que tendrá el JWT
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Claims = new Dictionary<string, object>
            {
                // Identifica al usuario dentro del token
                [ClaimTypes.NameIdentifier] = user.Id.ToString()
            },
            // Tiempo de expiración del access token
            Expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes),
            // Define cómo se firmará el JWT

            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };
        //Genera en JWT
        return handler.CreateToken(descriptor);
    }
}

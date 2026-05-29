using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EssenceMvp.Application.Interfaces;
using EssenceMvp.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace EssenceMvp.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly IConfiguration _config;

    public AuthController(IAuthService auth, IConfiguration config)
    {
        _auth = auth;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest model)
    {
        var user = await _auth.AuthenticateAsync(model.Email, model.Password);
        if (user == null)
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(new
        {
            token = GenerateToken(user.Id, user.Email, user.DisplayName ?? user.Email),
            user = new { user.Id, user.Email, displayName = user.DisplayName ?? user.Email }
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest model)
    {
        var (user, error) = await _auth.RegisterAsync(model.Email, model.Password, model.DisplayName);
        if (error != null)
            return BadRequest(new { message = error });

        return Ok(new
        {
            token = GenerateToken(user!.Id, user.Email, user.DisplayName ?? user.Email),
            user = new { user!.Id, user.Email, displayName = user.DisplayName ?? user.Email }
        });
    }

    private string GenerateToken(int userId, string email, string displayName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, displayName)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

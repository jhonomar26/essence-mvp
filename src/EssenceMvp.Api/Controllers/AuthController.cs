using EssenceMvp.Application.Interfaces;
using EssenceMvp.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace EssenceMvp.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly IJwtTokenService _tokenService;

    public AuthController(IAuthService auth, IJwtTokenService tokenService)
    {
        _auth = auth;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest model)
    {
        var user = await _auth.AuthenticateAsync(model.Email, model.Password);
        if (user == null)
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(new
        {
            token = _tokenService.GenerateToken(user.Id, user.Email, user.DisplayName ?? user.Email),
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
            token = _tokenService.GenerateToken(user!.Id, user.Email, user.DisplayName ?? user.Email),
            user = new { user!.Id, user.Email, displayName = user.DisplayName ?? user.Email }
        });
    }
}

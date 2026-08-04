using System.Security.Claims;
using EssenceMvp.Application.Abstractions;
using EssenceMvp.Application.Dtos;
using EssenceMvp.Application.Services;
using EssenceMvp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EssenceMvp.Mvc.Controllers;

[Route("auth")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAppUserRepository _appUsers;
    private readonly IUserSessionRepository _sessions;
    private readonly ISessionTokenService _tokenService;

    public AccountController(
        IAuthService authService,
        IAppUserRepository appUsers,
        IUserSessionRepository sessions,
        ISessionTokenService tokenService)
    {
        _authService = authService;
        _appUsers = appUsers;
        _sessions = sessions;
        _tokenService = tokenService;
    }

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _authService.AuthenticateAsync(request.Email, request.Password);
        if (user == null) return Unauthorized();

        var (_, token) = await _authService.CreateSessionAsync(user);
        return Ok(new AuthResponse(token, ToUserResponse(user)));
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var (user, error) = await _authService.RegisterAsync(request.Email, request.Password, request.DisplayName);
        if (error != null) return Conflict(error);

        var (_, token) = await _authService.CreateSessionAsync(user!);
        return StatusCode(StatusCodes.Status201Created, new AuthResponse(token, ToUserResponse(user!)));
    }

    [Authorize(AuthenticationSchemes = "SessionToken")]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        var rawToken = authHeader["Bearer ".Length..].Trim();
        var hash = _tokenService.Hash(rawToken);

        var session = await _sessions.GetByTokenHashAsync(hash);
        if (session != null) await _sessions.RevokeAsync(session);

        return Ok();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await _appUsers.GetByIdAsync(UserId);
        if (user == null) return NotFound();

        return Ok(ToUserResponse(user));
    }

    private static UserResponse ToUserResponse(AppUser user) =>
        new(user.Id, user.Email, user.DisplayName);
}

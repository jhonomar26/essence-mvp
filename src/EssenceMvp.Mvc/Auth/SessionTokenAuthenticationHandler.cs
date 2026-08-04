using System.Security.Claims;
using System.Text.Encodings.Web;
using EssenceMvp.Application.Abstractions;
using EssenceMvp.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EssenceMvp.Mvc.Auth;

public class SessionTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IUserSessionRepository _sessions;
    private readonly ISessionTokenService _tokenService;

    public SessionTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IUserSessionRepository sessions,
        ISessionTokenService tokenService)
        : base(options, logger, encoder)
    {
        _sessions = sessions;
        _tokenService = tokenService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader))
            return AuthenticateResult.NoResult();

        var value = authHeader.ToString();
        if (!value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.NoResult();

        var rawToken = value["Bearer ".Length..].Trim();
        if (string.IsNullOrEmpty(rawToken))
            return AuthenticateResult.NoResult();

        var hash = _tokenService.Hash(rawToken);
        var session = await _sessions.GetByTokenHashAsync(hash);

        if (session == null || session.RevokedAt != null || session.ExpiresAt <= DateTime.UtcNow)
            return AuthenticateResult.Fail("Invalid or expired session token.");

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, session.AppUserId.ToString()) };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}

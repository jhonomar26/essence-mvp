using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace EssenceMvp.Web.Services;

public sealed class SessionAuthenticationStateProvider(AuthState auth) : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = auth.IsAuthenticated
            ? new ClaimsIdentity(BuildClaims(), authenticationType: "EssenceSession")
            : new ClaimsIdentity();

        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }

    public void NotifySessionChanged() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    private IEnumerable<Claim> BuildClaims()
    {
        if (!string.IsNullOrWhiteSpace(auth.Email))
            yield return new Claim(ClaimTypes.Email, auth.Email);

        if (!string.IsNullOrWhiteSpace(auth.DisplayName))
            yield return new Claim(ClaimTypes.Name, auth.DisplayName);
        else if (!string.IsNullOrWhiteSpace(auth.Email))
            yield return new Claim(ClaimTypes.Name, auth.Email);
    }
}


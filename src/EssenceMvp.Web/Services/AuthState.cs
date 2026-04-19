namespace EssenceMvp.Web.Services;

public sealed class AuthState
{
    public string? Token { get; private set; }
    public string? Email { get; private set; }
    public string? DisplayName { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);

    public void SetSession(AuthResponse response)
    {
        Token = response.Token;
        Email = response.Email;
        DisplayName = response.DisplayName;
    }

    public void Clear()
    {
        Token = null;
        Email = null;
        DisplayName = null;
    }
}


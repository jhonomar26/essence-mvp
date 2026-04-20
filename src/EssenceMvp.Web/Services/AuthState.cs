namespace EssenceMvp.Web.Services;

public sealed class AuthState
{
    public string? Token { get; private set; }
    public string? RefreshToken { get; private set; }
    public string? Email { get; private set; }
    public string? DisplayName { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(Token);

    public event Action? OnChange;

    public void SetSession(AuthResponse response)
    {
        Token = response.Token;
        RefreshToken = response.RefreshToken;
        Email = response.Email;
        DisplayName = response.DisplayName;
        OnChange?.Invoke();
    }

    public void Clear()
    {
        Token = null;
        RefreshToken = null;
        Email = null;
        DisplayName = null;
        OnChange?.Invoke();
    }
}

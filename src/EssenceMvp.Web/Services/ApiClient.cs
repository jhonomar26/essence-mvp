using System.Net.Http.Headers;

namespace EssenceMvp.Web.Services;

public sealed class ApiClient
{
    private readonly HttpClient _http;
    private readonly AuthState _auth;

    public ApiClient(HttpClient http, AuthState auth)
    {
        _http = http;
        _auth = auth;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var response = await _http.PostAsJsonAsync("auth/register", request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Register failed ({(int)response.StatusCode}): {body}");

        return await response.Content.ReadFromJsonAsync<AuthResponse>();
    }

    public sealed record LoginDebugResult(
        AuthResponse? Auth,
        bool IsSuccess,
        int StatusCode,
        string? RawBody,
        string? ErrorMessage,
        string RequestPath);

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var result = await LoginWithDebugAsync(request);
        return result.Auth;
    }

    public async Task<LoginDebugResult> LoginWithDebugAsync(LoginRequest request)
    {
        const string requestPath = "/auth/login";

        try
        {
            var response = await _http.PostAsJsonAsync(requestPath, request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new LoginDebugResult(
                    Auth: null,
                    IsSuccess: false,
                    StatusCode: (int)response.StatusCode,
                    RawBody: body,
                    ErrorMessage: $"Login failed with status {(int)response.StatusCode}",
                    RequestPath: requestPath);
            }

            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
            return new LoginDebugResult(
                Auth: auth,
                IsSuccess: auth is not null,
                StatusCode: (int)response.StatusCode,
                RawBody: body,
                ErrorMessage: auth is null ? "Login response body could not be deserialized" : null,
                RequestPath: requestPath);
        }
        catch (Exception ex)
        {
            return new LoginDebugResult(
                Auth: null,
                IsSuccess: false,
                StatusCode: 0,
                RawBody: null,
                ErrorMessage: ex.Message,
                RequestPath: requestPath);
        }
    }

    public async Task<List<ProjectDto>> GetMyProjectsAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/projects/mine");
        if (!string.IsNullOrWhiteSpace(_auth.Token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _auth.Token);

        var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode) return [];
        return await response.Content.ReadFromJsonAsync<List<ProjectDto>>() ?? [];
    }
}

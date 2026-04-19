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

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("auth/login", request);
        var body = await response.Content.ReadAsStringAsync();

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return null;

        if (!response.IsSuccessStatusCode)
            throw new Exception($"HTTP {(int)response.StatusCode}: {body}");

        return await response.Content.ReadFromJsonAsync<AuthResponse>();
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

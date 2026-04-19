using System.Net.Http.Headers;
using System.Net.Http.Json;

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

    // ── Auth ──────────────────────────────────────────────────────────────

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
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) return null;
        if (!response.IsSuccessStatusCode)
            throw new Exception($"HTTP {(int)response.StatusCode}: {body}");
        return await response.Content.ReadFromJsonAsync<AuthResponse>();
    }

    // ── Projects ──────────────────────────────────────────────────────────

    public async Task<List<ProjectDto>> GetProjectsAsync()
    {
        using var req = AuthRequest(HttpMethod.Get, "projects");
        var response = await _http.SendAsync(req);
        if (!response.IsSuccessStatusCode) return [];
        return await response.Content.ReadFromJsonAsync<List<ProjectDto>>() ?? [];
    }

    public async Task<ProjectDetailDto?> GetProjectAsync(int id)
    {
        using var req = AuthRequest(HttpMethod.Get, $"projects/{id}");
        var response = await _http.SendAsync(req);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ProjectDetailDto>();
    }

    public async Task<ProjectDto?> CreateProjectAsync(CreateProjectRequest request)
    {
        using var req = AuthRequest(HttpMethod.Post, "projects");
        req.Content = JsonContent.Create(request);
        var response = await _http.SendAsync(req);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Create project failed ({(int)response.StatusCode}): {body}");
        return await System.Text.Json.JsonSerializer.DeserializeAsync<ProjectDto>(
            new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(body)),
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task<ProjectDto?> UpdateProjectAsync(int id, UpdateProjectRequest request)
    {
        using var req = AuthRequest(HttpMethod.Put, $"projects/{id}");
        req.Content = JsonContent.Create(request);
        var response = await _http.SendAsync(req);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<ProjectDto>();
    }

    public async Task<bool> DeleteProjectAsync(int id)
    {
        using var req = AuthRequest(HttpMethod.Delete, $"projects/{id}");
        var response = await _http.SendAsync(req);
        return response.IsSuccessStatusCode;
    }

    // ── Alphas ────────────────────────────────────────────────────────────

    public async Task<List<AlphaDto>> GetAlphasAsync()
    {
        var response = await _http.GetAsync("alphas");
        if (!response.IsSuccessStatusCode) return [];
        return await response.Content.ReadFromJsonAsync<List<AlphaDto>>() ?? [];
    }

    public async Task<List<AlphaStateDto>> GetAlphaStatesAsync(int alphaId)
    {
        var response = await _http.GetAsync($"alphas/{alphaId}/states");
        if (!response.IsSuccessStatusCode) return [];
        return await response.Content.ReadFromJsonAsync<List<AlphaStateDto>>() ?? [];
    }

    public async Task<List<StateChecklistDto>> GetChecklistAsync(int alphaId, int stateNumber)
    {
        var response = await _http.GetAsync($"alphas/{alphaId}/states/{stateNumber}/checklist");
        if (!response.IsSuccessStatusCode) return [];
        return await response.Content.ReadFromJsonAsync<List<StateChecklistDto>>() ?? [];
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private HttpRequestMessage AuthRequest(HttpMethod method, string url)
    {
        var req = new HttpRequestMessage(method, url);
        if (!string.IsNullOrWhiteSpace(_auth.Token))
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _auth.Token);
        return req;
    }
}

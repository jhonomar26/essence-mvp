using System.Net;
using System.Net.Http.Json;

namespace EssenceMvp.Tests.Integration;

// Spec: autorizacion-endpoints. Salud y evaluación exigen sesión y dueño del proyecto.
public class ProjectAccessTests : IClassFixture<EssenceWebFactory>
{

    private readonly EssenceWebFactory _factory;

    public ProjectAccessTests(EssenceWebFactory factory) => _factory = factory;

    private static Task<HttpResponseMessage> Health(HttpClient c, int projectId) =>
        c.GetAsync($"/evaluation/health/{projectId}");

    private static Task<HttpResponseMessage> Evaluate(HttpClient c, int projectId) =>
        c.PostAsJsonAsync("/evaluation/alpha", new
        {
            projectId, alphaId = 1,
            answers = new[] { new { stateChecklistId = 1, isAchieved = true } }
        });

    private async Task<(int ProjectId, HttpClient Owner, HttpClient Other)> ArrangeAsync()
    {
        var id = Guid.NewGuid().ToString("N");
        var owner = await _factory.RegisterAsync($"owner-{id}@test.com");
        var other = await _factory.RegisterAsync($"other-{id}@test.com");
        var projectId = await _factory.CreateProjectAsync(owner.User.Id);
        return (projectId, _factory.CreateClientFor(owner), _factory.CreateClientFor(other));
    }

    [Fact]
    public async Task Health_without_session_is_401()
    {
        var (projectId, _, _) = await ArrangeAsync();
        var response = await Health(_factory.CreateClientFor(null), projectId);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Evaluate_without_session_is_401()
    {
        var (projectId, _, _) = await ArrangeAsync();
        var response = await Evaluate(_factory.CreateClientFor(null), projectId);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Health_of_another_users_project_is_404()
    {
        var (projectId, _, other) = await ArrangeAsync();
        var response = await Health(other, projectId);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Evaluate_of_another_users_project_is_404()
    {
        var (projectId, _, other) = await ArrangeAsync();
        var response = await Evaluate(other, projectId);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Another_users_project_and_a_missing_one_look_the_same()
    {
        var (projectId, _, other) = await ArrangeAsync();
        var foreign = await Health(other, projectId);
        var missing = await Health(other, 999_999);
        Assert.Equal(missing.StatusCode, foreign.StatusCode);
    }

    [Fact]
    public async Task Owner_can_read_health()
    {
        var (projectId, owner, _) = await ArrangeAsync();
        var response = await Health(owner, projectId);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Owner_can_evaluate()
    {
        var (projectId, owner, _) = await ArrangeAsync();
        var response = await Evaluate(owner, projectId);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

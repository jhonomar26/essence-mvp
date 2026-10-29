using System.Net;
using System.Net.Http.Json;
using EssenceMvp.Application.Dtos;

namespace EssenceMvp.Tests.Integration;

// Spec: ADR-0001. Proyecto ajeno o inexistente y snapshot inexistente responden 404.
public class SnapshotAccessTests : IClassFixture<EssenceWebFactory>
{
    private readonly EssenceWebFactory _factory;

    public SnapshotAccessTests(EssenceWebFactory factory) => _factory = factory;

    private async Task<(int ProjectId, HttpClient Owner, HttpClient Other)> ArrangeAsync()
    {
        var id = Guid.NewGuid().ToString("N");
        var owner = await _factory.RegisterAsync($"owner-{id}@test.com");
        var other = await _factory.RegisterAsync($"other-{id}@test.com");
        var projectId = await _factory.CreateProjectAsync(owner.User.Id);
        return (projectId, _factory.CreateClientFor(owner), _factory.CreateClientFor(other));
    }

    private static string Url(int projectId) => $"/projects/{projectId}/snapshots";

    [Fact]
    public async Task Create_on_another_users_project_is_404()
    {
        var (projectId, _, other) = await ArrangeAsync();
        Assert.Equal(HttpStatusCode.NotFound, (await other.PostAsync(Url(projectId), null)).StatusCode);
    }

    [Fact]
    public async Task History_of_another_users_project_is_404()
    {
        var (projectId, _, other) = await ArrangeAsync();
        Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync(Url(projectId))).StatusCode);
    }

    [Fact]
    public async Task Delete_on_another_users_project_is_404()
    {
        var (projectId, _, other) = await ArrangeAsync();
        Assert.Equal(HttpStatusCode.NotFound, (await other.DeleteAsync($"{Url(projectId)}/1")).StatusCode);
    }

    [Fact]
    public async Task Another_users_project_and_a_missing_one_look_the_same()
    {
        var (projectId, _, other) = await ArrangeAsync();
        var foreign = await other.GetAsync(Url(projectId));
        var missing = await other.GetAsync(Url(999_999));
        Assert.Equal(missing.StatusCode, foreign.StatusCode);
    }

    [Fact]
    public async Task Delete_of_a_missing_snapshot_is_404()
    {
        var (projectId, owner, _) = await ArrangeAsync();
        Assert.Equal(HttpStatusCode.NotFound, (await owner.DeleteAsync($"{Url(projectId)}/999999")).StatusCode);
    }

    [Fact]
    public async Task Owner_can_create_list_and_delete_a_snapshot()
    {
        var (projectId, owner, _) = await ArrangeAsync();

        var created = await owner.PostAsync(Url(projectId), null);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var snapshot = (await created.Content.ReadFromJsonAsync<SnapshotSummaryDto>())!;

        var history = await owner.GetFromJsonAsync<List<SnapshotSummaryDto>>(Url(projectId));
        Assert.Contains(history!, s => s.Id == snapshot.Id);

        Assert.Equal(HttpStatusCode.OK, (await owner.DeleteAsync($"{Url(projectId)}/{snapshot.Id}")).StatusCode);
    }
}

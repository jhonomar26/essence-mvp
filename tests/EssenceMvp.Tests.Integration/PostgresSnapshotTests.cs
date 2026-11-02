using System.Net;
using System.Net.Http.Json;
using EssenceMvp.Application.Dtos;

namespace EssenceMvp.Tests.Integration;

// Ejercita contra PostgreSQL real lo que la base en memoria no puede: el enum health_status,
// la columna jsonb y las fechas. Spec: migracion-net10 (H1).
public class PostgresSnapshotTests : IClassFixture<PostgresWebFactory>
{
    private readonly PostgresWebFactory _factory;

    public PostgresSnapshotTests(PostgresWebFactory factory) => _factory = factory;

    [PostgresFact]
    public async Task Snapshot_round_trips_enum_jsonb_and_date_through_postgres()
    {
        var session = await _factory.RegisterAsync($"pg-{Guid.NewGuid():N}@test.com");
        var projectId = await _factory.CreateProjectAsync(session.User.Id);
        using var client = _factory.CreateClientFor(session);
        var before = DateTimeOffset.UtcNow;

        var created = await client.PostAsync($"/projects/{projectId}/snapshots", null);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var snapshot = (await created.Content.ReadFromJsonAsync<SnapshotSummaryDto>())!;

        var history = await client.GetFromJsonAsync<List<SnapshotSummaryDto>>($"/projects/{projectId}/snapshots");
        var stored = Assert.Single(history!, s => s.Id == snapshot.Id);

        Assert.Equal(snapshot.GlobalStatus, stored.GlobalStatus);                // enum
        Assert.Equal(snapshot.HealthScore, stored.HealthScore);                  // viene del jsonb
        Assert.Equal(snapshot.Classification, stored.Classification);            // viene del jsonb
        Assert.InRange(stored.CreatedAt, before.AddSeconds(-5), DateTimeOffset.UtcNow.AddSeconds(5)); // fecha
    }
}

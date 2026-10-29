using System.Net.Http.Json;
using EssenceMvp.Application.Dtos;
using EssenceMvp.Domain.Entities;
using EssenceMvp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EssenceMvp.Tests.Integration;

// Arranca la API real con una base en memoria en lugar de PostgreSQL.
public class EssenceWebFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            var options = services.Single(d => d.ServiceType == typeof(DbContextOptions<EssenceDbContext>));
            services.Remove(options);
            services.AddDbContext<EssenceDbContext>(o => o.UseInMemoryDatabase(_dbName));
        });
    }

    // Registra un usuario por la API y devuelve su sesión.
    public async Task<AuthResponse> RegisterAsync(string email)
    {
        using var client = CreateClient();
        var response = await client.PostAsJsonAsync("/auth/register", new RegisterRequest(email, "Password123", "Test"));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    // Crea un proyecto directo en la base (no hay endpoint para crearlo).
    public async Task<int> CreateProjectAsync(int userId)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EssenceDbContext>();
        var project = new Project { UserId = userId, Name = "Proyecto de prueba", CreatedAt = DateTimeOffset.UtcNow };
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        return project.Id;
    }

    public HttpClient CreateClientFor(AuthResponse? session)
    {
        var client = CreateClient();
        if (session != null)
            client.DefaultRequestHeaders.Authorization = new("Bearer", session.Token);
        return client;
    }
}

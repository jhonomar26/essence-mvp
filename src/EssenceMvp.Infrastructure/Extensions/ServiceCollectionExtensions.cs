using EssenceMvp.Application.Interfaces;
using EssenceMvp.Domain.Entities;
using EssenceMvp.Infrastructure.Persistence;
using EssenceMvp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace EssenceMvp.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection")!;
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.MapEnum<HealthStatus>("health_status");
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<EssenceDbContext>(options => options.UseNpgsql(dataSource));
        services.AddScoped<IEssenceDbContext>(sp => sp.GetRequiredService<EssenceDbContext>());
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}

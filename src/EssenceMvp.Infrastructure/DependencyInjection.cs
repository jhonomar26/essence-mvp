using EssenceMvp.Application.Abstractions;
using EssenceMvp.Domain.Enums;
using EssenceMvp.Infrastructure.Persistence;
using EssenceMvp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace EssenceMvp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.MapEnum<HealthStatus>("health_status");
        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<EssenceDbContext>(options => options.UseNpgsql(dataSource));

        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IAlphaRepository, AlphaRepository>();
        services.AddScoped<IChecklistResponseRepository, ChecklistResponseRepository>();
        services.AddScoped<IProjectAlphaStatusRepository, ProjectAlphaStatusRepository>();
        services.AddScoped<IHealthReportRepository, HealthReportRepository>();
        services.AddScoped<IAppUserRepository, AppUserRepository>();

        return services;
    }
}

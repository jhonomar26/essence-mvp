using EssenceMvp.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EssenceMvp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISessionTokenService, SessionTokenService>();
        services.AddScoped<IAccessTokenService, JwtAccessTokenService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IAlphaService, AlphaService>();
        services.AddScoped<IHealthService, HealthService>();
        services.AddScoped<IAlphaEvaluationService, AlphaEvaluationService>();
        services.AddScoped<IHealthCalculationService, HealthCalculationService>();
        services.AddScoped<IProjectDetailComposerService, ProjectDetailComposerService>();
        services.AddScoped<ISnapshotService, SnapshotService>();

        return services;
    }
}

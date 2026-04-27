using EssenceMvp.Mvc.Infrastructure.Entities;

namespace EssenceMvp.Mvc.Helpers;

public static class HealthStatusHelper
{
    public static string GetLabel(HealthStatus status) => status switch
    {
        HealthStatus.green => "Saludable",
        HealthStatus.yellow => "En Alerta",
        HealthStatus.red => "Crítico",
        _ => "Desconocido"
    };

    public static string GetBadgeClass(HealthStatus status) => status switch
    {
        HealthStatus.green => "badge-health badge-health-green",
        HealthStatus.yellow => "badge-health badge-health-yellow",
        HealthStatus.red => "badge-health badge-health-red",
        _ => "badge-secondary"
    };

    public static string GetBgClass(HealthStatus status) => status switch
    {
        HealthStatus.green => "bg-success",
        HealthStatus.yellow => "bg-warning text-dark",
        HealthStatus.red => "bg-danger",
        _ => "bg-secondary"
    };
}

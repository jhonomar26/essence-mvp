using EssenceMvp.Mvc.Models;

namespace EssenceMvp.Mvc.Application.Services;

public class ProjectDetailComposerService : IProjectDetailComposerService
{
    private readonly IProjectService _projects;
    private readonly IAlphaService _alphas;
    private readonly IHealthService _health;
    private readonly IHealthCalculationService _healthCalc;

    public ProjectDetailComposerService(
        IProjectService projects,
        IAlphaService alphas,
        IHealthService health,
        IHealthCalculationService healthCalc)
    {
        _projects = projects;
        _alphas = alphas;
        _health = health;
        _healthCalc = healthCalc;
    }

    public async Task<ProjectDetailComposerResult?> GetProjectDetailAsync(int projectId, int userId)
    {
        // 1. VALIDACIÓN: obtiene proyecto + verifica permisos
        var project = await _projects.GetProjectDetailAsync(projectId, userId);
        if (project == null) return null;
        var alphaProgress = await _alphas.GetProjectAlphaProgressAsync(projectId, userId);
        var healthStatus = await _health.CalculateHealthAsync(projectId, userId);
        var healthScore = await _healthCalc.CalculateProjectHealthAsync(projectId);
        // 3. CARGAR CHECKLISTS DINÁMICAMENTE
        var alphaChecklists = new Dictionary<int, List<AlphaChecklistDto>>();
        foreach (var alpha in alphaProgress)
        {
            alphaChecklists[alpha.AlphaId] = await _projects.GetAlphaChecklistsAsync(projectId, alpha.AlphaId);
        }
        // 4. CONSTRUIR VIEWMODEL (mapea datos a estructura para vista)

        var viewModel = new ProjectDetailViewModel
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Phase = project.Phase,
            OverallHealth = healthStatus,
            HealthScore = healthScore.HealthScore,
            HealthClassification = healthScore.Classification,
            AverageProgress = healthScore.AverageProgress,
            ProgressDispersion = healthScore.ProgressDispersion,
            AlphaProgress = alphaProgress
        };

        return new ProjectDetailComposerResult
        {
            ViewModel = viewModel,
            AlphaChecklists = alphaChecklists
        };
    }
}
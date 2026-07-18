using EssenceMvp.Application.Abstractions;
using EssenceMvp.Application.Services;
using EssenceMvp.Mvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace EssenceMvp.Mvc.Controllers;

[Route("evaluation/health")]
public class ProjectHealthController : Controller
{
    private readonly IHealthCalculationService _healthCalcService;
    private readonly IProjectRepository _projects;

    public ProjectHealthController(IHealthCalculationService healthCalcService, IProjectRepository projects)
    {
        _healthCalcService = healthCalcService;
        _projects = projects;
    }

    // GET /evaluation/health/{projectId}
    // Retorna: SEMAT Essence health score + alpha progress details
    [HttpGet("{projectId}")]
    public async Task<IActionResult> Get(int projectId)
    {
        var project = await _projects.GetByIdAsync(projectId);

        if (project == null) return NotFound();

        var healthScore = await _healthCalcService.CalculateProjectHealthAsync(projectId);

        var alphaDetails = healthScore.AlphaProgresses
            .Select(ap => new HealthResult.AlphaDetail
            {
                AlphaId = ap.AlphaId,
                AlphaName = ap.AlphaName,
                CurrentStateNumber = ap.CurrentStateNumber,
                MaxStateNumber = ap.MaxStateNumber,
                Progress = ap.Progress
            })
            .ToList();

        return Json(new HealthResult
        {
            HealthScore = healthScore.HealthScore,
            Classification = healthScore.Classification,
            AverageProgress = healthScore.AverageProgress,
            ProgressDispersion = healthScore.ProgressDispersion,
            AlphaDetails = alphaDetails
        });
    }
}

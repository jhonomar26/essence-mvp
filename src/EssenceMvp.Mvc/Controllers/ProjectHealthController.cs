using System.Security.Claims;
using EssenceMvp.Application.Services;
using EssenceMvp.Mvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EssenceMvp.Mvc.Controllers;

[Authorize]
[Route("evaluation/health")]
public class ProjectHealthController : Controller
{
    private readonly IHealthCalculationService _healthCalcService;
    private readonly IProjectService _projectService;

    public ProjectHealthController(IHealthCalculationService healthCalcService, IProjectService projectService)
    {
        _healthCalcService = healthCalcService;
        _projectService = projectService;
    }

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET /evaluation/health/{projectId}
    // Retorna: SEMAT Essence health score + alpha progress details
    [HttpGet("{projectId}")]
    public async Task<IActionResult> Get(int projectId)
    {
        await _projectService.EnsureOwnedAsync(projectId, UserId);

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

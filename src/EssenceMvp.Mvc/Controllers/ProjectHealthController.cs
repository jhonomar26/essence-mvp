using EssenceMvp.Mvc.Application.Services;
using EssenceMvp.Mvc.Infrastructure;
using EssenceMvp.Mvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Mvc.Controllers;

[ApiController]
[Authorize]
[Route("api/evaluation/health")]
public class ProjectHealthController : ControllerBase
{
    private readonly IHealthCalculationService _healthCalcService;
    private readonly EssenceDbContext _db;

    public ProjectHealthController(IHealthCalculationService healthCalcService, EssenceDbContext db)
    {
        _healthCalcService = healthCalcService;
        _db = db;
    }

    [HttpGet("{projectId}")]
    public async Task<IActionResult> Get(int projectId)
    {
        var project = await _db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == projectId);

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

        return Ok(new HealthResult
        {
            HealthScore = healthScore.HealthScore,
            Classification = healthScore.Classification,
            AverageProgress = healthScore.AverageProgress,
            ProgressDispersion = healthScore.ProgressDispersion,
            AlphaDetails = alphaDetails
        });
    }
}

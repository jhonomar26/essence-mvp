using EssenceMvp.Mvc.Application.Services;
using EssenceMvp.Mvc.Infrastructure;
using EssenceMvp.Mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Mvc.Controllers;

[Route("evaluation/health")]
public class ProjectHealthController : Controller
{
    private readonly IHealthCalculationService _healthCalcService;
    private readonly IAlphaEvaluationService _alphaEvalService;
    private readonly EssenceDbContext _db;

    public ProjectHealthController(IHealthCalculationService healthCalcService, IAlphaEvaluationService alphaEvalService, EssenceDbContext db)
    {
        _healthCalcService = healthCalcService;
        _alphaEvalService = alphaEvalService;
        _db = db;
    }

    // GET /evaluation/health/{projectId}
    // Retorna: semáforo global + detalles de estado de cada Alpha
    [HttpGet("{projectId}")]
    public async Task<IActionResult> Get(int projectId)
    {
        var project = await _db.Projects
            .AsNoTracking()
            .Include(p => p.AlphaStatuses)
            .ThenInclude(s => s.Alpha)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null) return NotFound();

        // Calcula semáforo global (Red/Yellow/Green) basado en atrasos de Alphas
        var globalStatus = await _healthCalcService.CalculateAsync(projectId);

        // Para cada Alpha, calcula estado actual
        var alphaDetails = new List<HealthResult.AlphaDetail>();
        foreach (var status in project.AlphaStatuses.OrderBy(s => s.Alpha.AreaOfConcern))
        {
            var stateResult = await _alphaEvalService.CalculateAsync(projectId, status.AlphaId);
            alphaDetails.Add(new HealthResult.AlphaDetail
            {
                AlphaId = status.AlphaId,
                AlphaName = status.Alpha.Name,
                CurrentStateNumber = stateResult.CurrentStateNumber,
                CurrentStateName = stateResult.CurrentStateName
            });
        }

        return Json(new HealthResult
        {
            GlobalStatus = globalStatus,
            AlphaDetails = alphaDetails
        });
    }
}
namespace EssenceMvp.Mvc.Controllers;

using EssenceMvp.Mvc.Application.Services;
using EssenceMvp.Mvc.Infrastructure;
using EssenceMvp.Mvc.Infrastructure.Entities;
using EssenceMvp.Mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("evaluation/alpha")]
public class AlphaEvaluationController : Controller
{
    private readonly IAlphaEvaluationService _alphaEvalService;
    private readonly IProjectService _projectService;
    private readonly EssenceDbContext _db;

    public AlphaEvaluationController(IAlphaEvaluationService alphaEvalService, IProjectService projectService, EssenceDbContext db)
    {
        _alphaEvalService = alphaEvalService;
        _projectService = projectService;
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Evaluate([FromBody] EvaluateChecklistViewModel model)
    {
        // Paso 1: Persiste respuestas de checklist (upsert por StateChecklistId)
        var responses = model.Answers.Select(a => (a.StateChecklistId, a.IsAchieved, a.Notes)).ToList();
        await _projectService.SaveChecklistResponsesAsync(model.ProjectId, responses);

        // Paso 2: Recalcula estado del Alpha usando lógica acumulativa
        var stateResult = await _alphaEvalService.CalculateAsync(model.ProjectId, model.AlphaId);

        // Paso 3: Actualiza ProjectAlphaStatus con nuevo estado
        var projAlpha = await _db.ProjectAlphaStatuses
            .FirstOrDefaultAsync(p => p.ProjectId == model.ProjectId && p.AlphaId == model.AlphaId);

        if (projAlpha != null)
        {
            projAlpha.CurrentStateNumber = stateResult.CurrentStateNumber;
            projAlpha.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
        }

        // Paso 4: Retorna nuevo estado al cliente (modal recarga página)
        return Json(stateResult);
    }
}
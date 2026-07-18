namespace EssenceMvp.Mvc.Controllers;

using EssenceMvp.Application.Abstractions;
using EssenceMvp.Application.Services;
using EssenceMvp.Mvc.Models;
using Microsoft.AspNetCore.Mvc;

[Route("evaluation/alpha")]
public class AlphaEvaluationController : Controller
{
    private readonly IAlphaEvaluationService _alphaEvalService;
    private readonly IProjectService _projectService;
    private readonly IProjectAlphaStatusRepository _projectAlphaStatuses;

    public AlphaEvaluationController(IAlphaEvaluationService alphaEvalService, IProjectService projectService, IProjectAlphaStatusRepository projectAlphaStatuses)
    {
        _alphaEvalService = alphaEvalService;
        _projectService = projectService;
        _projectAlphaStatuses = projectAlphaStatuses;
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
        await _projectAlphaStatuses.UpdateCurrentStateAsync(model.ProjectId, model.AlphaId, stateResult.CurrentStateNumber);

        // Paso 4: Retorna nuevo estado al cliente (modal recarga página)
        return Json(stateResult);
    }
}

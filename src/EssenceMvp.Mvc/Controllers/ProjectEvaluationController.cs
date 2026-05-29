using EssenceMvp.Mvc.Application.Services;
using EssenceMvp.Mvc.Infrastructure;
using EssenceMvp.Mvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Mvc.Controllers;

[ApiController]
[Authorize]
[Route("api/evaluation/alpha")]
public class AlphaEvaluationController : ControllerBase
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
        var responses = model.Answers.Select(a => (a.StateChecklistId, a.IsAchieved, a.Notes)).ToList();
        await _projectService.SaveChecklistResponsesAsync(model.ProjectId, responses);

        var stateResult = await _alphaEvalService.CalculateAsync(model.ProjectId, model.AlphaId);

        var projAlpha = await _db.ProjectAlphaStatuses
            .FirstOrDefaultAsync(p => p.ProjectId == model.ProjectId && p.AlphaId == model.AlphaId);

        if (projAlpha != null)
        {
            projAlpha.CurrentStateNumber = stateResult.CurrentStateNumber;
            projAlpha.UpdatedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync();
        }

        return Ok(stateResult);
    }
}

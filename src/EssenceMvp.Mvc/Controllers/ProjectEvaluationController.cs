namespace EssenceMvp.Mvc.Controllers;

using EssenceMvp.Mvc.Application.Services;
using EssenceMvp.Mvc.Models;
using Microsoft.AspNetCore.Mvc;

[Route("evaluation/alpha")]
public class AlphaEvaluationController : Controller
{
    private readonly IAlphaEvaluationService _service;

    public AlphaEvaluationController(IAlphaEvaluationService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Evaluate([FromBody] EvaluateChecklistViewModel model)
    {
        var result = await _service.CalculateAsync(model.ProjectId, model.AlphaId);
        return Json(result);
    }
}
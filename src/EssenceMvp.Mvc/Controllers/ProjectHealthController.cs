using EssenceMvp.Mvc.Application.Services;
using EssenceMvp.Mvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace EssenceMvp.Mvc.Controllers;

[Route("evaluation/health")]
public class ProjectHealthController : Controller
{
    private readonly IHealthCalculationService _service;

    public ProjectHealthController(IHealthCalculationService service)
    {
        _service = service;
    }

    [HttpGet("{projectId}")]
    public async Task<IActionResult> Get(int projectId)
    {
        var result = await _service.CalculateAsync(projectId);

        return Json(new HealthResult
        {
            GlobalStatus = result
        });
    }
}
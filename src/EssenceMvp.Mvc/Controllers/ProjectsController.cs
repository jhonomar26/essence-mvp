using System.Security.Claims;
using EssenceMvp.Mvc.Application.Services;
using EssenceMvp.Mvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EssenceMvp.Mvc.Controllers;

[ApiController]
[Authorize]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projects;
    private readonly IHealthService _health;
    private readonly IProjectDetailComposerService _detailComposer;

    public ProjectsController(IProjectService projects, IHealthService health, IProjectDetailComposerService detailComposer)
    {
        _projects = projects;
        _health = health;
        _detailComposer = detailComposer;
    }

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var projects = await _projects.GetUserProjectsAsync(UserId);
        var summaries = new List<ProjectSummary>();
        foreach (var p in projects)
        {
            var health = await _health.CalculateHealthAsync(p.Id, UserId);
            summaries.Add(new ProjectSummary
            {
                Id = p.Id,
                Name = p.Name,
                Phase = p.Phase,
                CreatedAt = p.CreatedAt,
                OverallHealth = health
            });
        }
        return Ok(summaries);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProjectViewModel model)
    {
        var project = await _projects.CreateProjectAsync(UserId, model.Name, model.Description, model.Phase);
        return Ok(new { project.Id, project.Name, project.Phase });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Detail(int id)
    {
        var result = await _detailComposer.GetProjectDetailAsync(id, UserId);
        if (result == null) return NotFound();

        return Ok(new
        {
            project = result.ViewModel,
            alphaChecklists = result.AlphaChecklists,
            recentSnapshots = result.RecentSnapshots
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _projects.DeleteProjectAsync(id, UserId);
        return Ok();
    }
}

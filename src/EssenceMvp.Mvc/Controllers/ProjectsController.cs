using System.Security.Claims;
using EssenceMvp.Mvc.Application.Services;
using EssenceMvp.Mvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EssenceMvp.Mvc.Controllers;

[Authorize]
public class ProjectsController : Controller
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
        return View(summaries);
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateProjectViewModel());

    [HttpPost]
    public async Task<IActionResult> Create(CreateProjectViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var project = await _projects.CreateProjectAsync(UserId, model.Name, model.Description, model.Phase);
        return RedirectToAction("Detail", new { id = project.Id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var result = await _detailComposer.GetProjectDetailAsync(id, UserId);
        if (result == null) return NotFound();

        ViewBag.AlphaChecklists = result.AlphaChecklists;
        return View(result.ViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _projects.DeleteProjectAsync(id, UserId);
        return RedirectToAction("Index");
    }
}

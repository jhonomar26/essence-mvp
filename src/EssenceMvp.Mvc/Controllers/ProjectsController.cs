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
    private readonly IAlphaService _alphas;
    private readonly IHealthService _health;

    public ProjectsController(IProjectService projects, IAlphaService alphas, IHealthService health)
    {
        _projects = projects;
        _alphas = alphas;
        _health = health;
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
        var project = await _projects.GetProjectDetailAsync(id, UserId);
        if (project == null) return NotFound();

        var alphaProgress = await _alphas.GetProjectAlphaProgressAsync(id, UserId);
        var health = await _health.CalculateHealthAsync(id, UserId);

        // Load checklists for each alpha to pass to modals
        var alphaChecklists = new Dictionary<int, List<(int Id, string CriterionText, bool IsAchieved)>>();
        foreach (var alpha in alphaProgress)
        {
            alphaChecklists[alpha.AlphaId] = await _projects.GetAlphaChecklistsAsync(id, alpha.AlphaId);
        }

        ViewBag.AlphaChecklists = alphaChecklists;

        var vm = new ProjectDetailViewModel
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Phase = project.Phase,
            OverallHealth = health,
            AlphaProgress = alphaProgress
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _projects.DeleteProjectAsync(id, UserId);
        return RedirectToAction("Index");
    }
}

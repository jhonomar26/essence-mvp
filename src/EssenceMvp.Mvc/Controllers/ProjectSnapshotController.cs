using System.Security.Claims;
using EssenceMvp.Mvc.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EssenceMvp.Mvc.Controllers;

[Authorize]
[Route("projects/{projectId}/snapshots")]
[ApiController]
public class ProjectSnapshotController : ControllerBase
{
    private readonly ISnapshotService _snapshotService;

    public ProjectSnapshotController(ISnapshotService snapshotService)
    {
        _snapshotService = snapshotService;
    }

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("")]
    public async Task<IActionResult> Create(int projectId)
    {
        try
        {
            var snapshot = await _snapshotService.CreateSnapshotAsync(projectId, UserId);
            return Ok(snapshot);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("")]
    public async Task<IActionResult> GetHistory(int projectId)
    {
        try
        {
            var history = await _snapshotService.GetHistoryAsync(projectId, UserId);
            return Ok(history);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}

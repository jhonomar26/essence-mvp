using System.Security.Claims;
using EssenceMvp.Application.Services;
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
    public async Task<IActionResult> Create(int projectId) =>
        Ok(await _snapshotService.CreateSnapshotAsync(projectId, UserId));

    [HttpGet("")]
    public async Task<IActionResult> GetHistory(int projectId) =>
        Ok(await _snapshotService.GetHistoryAsync(projectId, UserId));

    [HttpDelete("{snapshotId}")]
    public async Task<IActionResult> Delete(int projectId, int snapshotId)
    {
        await _snapshotService.DeleteSnapshotAsync(snapshotId, projectId, UserId);
        return Ok();
    }
}

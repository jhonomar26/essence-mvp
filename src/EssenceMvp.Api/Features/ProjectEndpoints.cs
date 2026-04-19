using System.Security.Claims;
using EssenceMvp.Api.Infrastructure;
using EssenceMvp.Api.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Api.Features;

public static class ProjectEndpoints
{
    public static RouteGroupBuilder MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/projects").WithTags("Projects").RequireAuthorization();

        // GET /projects — list user's projects
        group.MapGet("/", async (ClaimsPrincipal user, EssenceDbContext db) =>
        {
            if (!TryGetUserId(user, out var userId)) return Results.Unauthorized();

            var projects = await db.Projects
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Phase = p.Phase,
                    CreatedAt = p.CreatedAt.DateTime
                })
                .ToListAsync();

            return Results.Ok(projects);
        });

        // POST /projects — create project + init 7 alpha statuses
        group.MapPost("/", async (CreateProjectRequest req, ClaimsPrincipal user, EssenceDbContext db) =>
        {
            if (!TryGetUserId(user, out var userId)) return Results.Unauthorized();

            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.BadRequest(new { error = "Name is required." });

            var project = new Project
            {
                UserId = userId,
                Name = req.Name.Trim(),
                Description = req.Description?.Trim(),
                Phase = req.Phase?.Trim(),
                CreatedAt = DateTimeOffset.UtcNow
            };

            db.Projects.Add(project);
            await db.SaveChangesAsync();

            var alphaIds = await db.Alphas.Select(a => a.Id).ToListAsync();
            var now = DateTimeOffset.UtcNow;
            db.ProjectAlphaStatuses.AddRange(alphaIds.Select(alphaId => new ProjectAlphaStatus
            {
                ProjectId = project.Id,
                AlphaId = alphaId,
                CurrentStateNumber = 0,
                UpdatedAt = now
            }));
            await db.SaveChangesAsync();

            var dto = new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Phase = project.Phase,
                CreatedAt = project.CreatedAt.DateTime
            };

            return Results.Created($"/projects/{project.Id}", dto);
        });

        // GET /projects/{id} — detail with 7 alpha statuses
        group.MapGet("/{id:int}", async (int id, ClaimsPrincipal user, EssenceDbContext db) =>
        {
            if (!TryGetUserId(user, out var userId)) return Results.Unauthorized();

            var project = await db.Projects
                .AsNoTracking()
                .Include(p => p.AlphaStatuses)
                    .ThenInclude(s => s.Alpha)
                        .ThenInclude(a => a.States)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (project is null) return Results.NotFound();

            var dto = new ProjectDetailDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Phase = project.Phase,
                CreatedAt = project.CreatedAt.DateTime,
                AlphaStatuses = project.AlphaStatuses
                    .OrderBy(s => s.Alpha.AreaOfConcern)
                    .ThenBy(s => s.AlphaId)
                    .Select(s => new AlphaStatusDto
                    {
                        AlphaId = s.AlphaId,
                        AlphaName = s.Alpha.Name,
                        AreaOfConcern = s.Alpha.AreaOfConcern,
                        CurrentStateNumber = s.CurrentStateNumber,
                        CurrentStateName = s.CurrentStateNumber > 0
                            ? s.Alpha.States.FirstOrDefault(st => st.StateNumber == s.CurrentStateNumber)?.StateName
                            : null
                    })
                    .ToList()
            };

            return Results.Ok(dto);
        });

        // PUT /projects/{id} — update name/description/phase
        group.MapPut("/{id:int}", async (int id, UpdateProjectRequest req, ClaimsPrincipal user, EssenceDbContext db) =>
        {
            if (!TryGetUserId(user, out var userId)) return Results.Unauthorized();

            var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
            if (project is null) return Results.NotFound();

            if (req.Name is not null)
            {
                if (string.IsNullOrWhiteSpace(req.Name))
                    return Results.BadRequest(new { error = "Name cannot be empty." });
                project.Name = req.Name.Trim();
            }

            if (req.Description is not null) project.Description = req.Description.Trim();
            if (req.Phase is not null) project.Phase = req.Phase.Trim();

            await db.SaveChangesAsync();

            return Results.Ok(new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Phase = project.Phase,
                CreatedAt = project.CreatedAt.DateTime
            });
        });

        // DELETE /projects/{id}
        group.MapDelete("/{id:int}", async (int id, ClaimsPrincipal user, EssenceDbContext db) =>
        {
            if (!TryGetUserId(user, out var userId)) return Results.Unauthorized();

            var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
            if (project is null) return Results.NotFound();

            db.Projects.Remove(project);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        return group;
    }

    private static bool TryGetUserId(ClaimsPrincipal user, out int userId)
    {
        userId = 0;
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out userId);
    }
}

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

        group.MapGet("/mine", async (ClaimsPrincipal user, EssenceDbContext db) =>
        {
            var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId))
                return Results.Unauthorized();

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
                .ToListAsync<ProjectDto>();

            return Results.Ok(projects);
        });

        return group;
    }
}

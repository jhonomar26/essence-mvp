using EssenceMvp.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Api.Features;

public static class AlphaEndpoints
{
    public static RouteGroupBuilder MapAlphaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/alphas").WithTags("Alphas");

        // GET /alphas — list all 7 alphas
        group.MapGet("/", async (EssenceDbContext db) =>
        {
            var alphas = await db.Alphas
                .AsNoTracking()
                .OrderBy(a => a.AreaOfConcern)
                .ThenBy(a => a.Id)
                .Select(a => new AlphaDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    AreaOfConcern = a.AreaOfConcern,
                    Description = a.Description
                })
                .ToListAsync();

            return Results.Ok(alphas);
        });

        // GET /alphas/{id}/states — states ordered by stateNumber
        group.MapGet("/{id:int}/states", async (int id, EssenceDbContext db) =>
        {
            var exists = await db.Alphas.AnyAsync(a => a.Id == id);
            if (!exists) return Results.NotFound();

            var states = await db.AlphaStates
                .AsNoTracking()
                .Where(s => s.AlphaId == id)
                .OrderBy(s => s.StateNumber)
                .Select(s => new AlphaStateDto
                {
                    Id = s.Id,
                    AlphaId = s.AlphaId,
                    StateNumber = s.StateNumber,
                    StateName = s.StateName,
                    Description = s.Description
                })
                .ToListAsync();

            return Results.Ok(states);
        });

        // GET /alphas/{id}/states/{stateNumber}/checklist — criteria for a state
        group.MapGet("/{id:int}/states/{stateNumber:int}/checklist", async (int id, int stateNumber, EssenceDbContext db) =>
        {
            var state = await db.AlphaStates
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.AlphaId == id && s.StateNumber == stateNumber);

            if (state is null) return Results.NotFound();

            var items = await db.StateChecklists
                .AsNoTracking()
                .Where(c => c.AlphaStateId == state.Id)
                .OrderBy(c => c.Id)
                .Select(c => new StateChecklistDto
                {
                    Id = c.Id,
                    CriterionText = c.CriterionText,
                    IsMandatory = c.IsMandatory
                })
                .ToListAsync();

            return Results.Ok(items);
        });

        return group;
    }
}

using EssenceMvp.Application.DTOs;
using EssenceMvp.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Application.Services;

public class AlphaService : IAlphaService
{
    private readonly IEssenceDbContext _db;

    public AlphaService(IEssenceDbContext db) => _db = db;

    public async Task<List<AlphaProgressDto>> GetProjectAlphaProgressAsync(int projectId, int userId)
    {
        var project = await _db.Projects
            .Include(p => p.AlphaStatuses)
                .ThenInclude(s => s.Alpha)
                    .ThenInclude(a => a.States)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserId == userId);

        if (project == null) return new();

        return project.AlphaStatuses
            .OrderBy(s => s.AlphaId)
            .Select(s =>
            {
                var maxStates = (short)s.Alpha.States.Count;
                var progress = maxStates == 0 ? 0m : Math.Round(((decimal)s.CurrentStateNumber / maxStates) * 100, 2);
                var currentStateName = s.Alpha.States
                    .FirstOrDefault(st => st.StateNumber == s.CurrentStateNumber)?.StateName ?? "No iniciado";

                return new AlphaProgressDto
                {
                    AlphaId = s.AlphaId,
                    AlphaName = s.Alpha.Name,
                    AreaOfConcern = s.Alpha.AreaOfConcern,
                    CurrentStateNumber = s.CurrentStateNumber,
                    MaxStateNumber = maxStates,
                    CurrentStateName = currentStateName,
                    Progress = progress
                };
            })
            .ToList();
    }
}

using EssenceMvp.Application.Abstractions;
using EssenceMvp.Domain.Entities;
using EssenceMvp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EssenceMvp.Infrastructure.Repositories;

public class AlphaRepository : IAlphaRepository
{
    private readonly EssenceDbContext _db;

    public AlphaRepository(EssenceDbContext db) => _db = db;

    public Task<List<int>> GetAllIdsAsync() =>
        _db.Alphas.Select(a => a.Id).ToListAsync();

    public Task<Alpha?> GetWithStatesAndChecklistsAsync(int alphaId) =>
        _db.Alphas
            .Include(a => a.States)
            .ThenInclude(s => s.Checklists)
            .FirstOrDefaultAsync(a => a.Id == alphaId);

    public Task<List<Alpha>> GetAllWithStatesAndProjectStatusAsync(int projectId) =>
        _db.Alphas
            .Include(a => a.States)
            .Include(a => a.ProjectStatuses.Where(ps => ps.ProjectId == projectId))
            .ToListAsync();
}

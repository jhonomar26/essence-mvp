namespace EssenceMvp.Mvc.Application.Services;

using EssenceMvp.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

public class HealthCalculationService : IHealthCalculationService
{
    private readonly EssenceDbContext _db;

    public HealthCalculationService(EssenceDbContext db)
    {
        _db = db;
    }

    public async Task<string> CalculateAsync(int projectId)
    {
        var statuses = await _db.ProjectAlphaStatuses
            .Where(x => x.ProjectId == projectId)
            .ToListAsync();

        int red = 0;
        int yellow = 0;

        foreach (var s in statuses)
        {
            int diff = 3 - s.CurrentStateNumber;

            if (diff >= 2) red++;
            else if (diff == 1) yellow++;
        }

        if (red > 0 || yellow >= 3)
            return "Red";

        if (yellow > 0)
            return "Yellow";

        return "Green";
    }
}
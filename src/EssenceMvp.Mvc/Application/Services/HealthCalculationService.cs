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

    // Semáforo: Red/Yellow/Green según atrasos de Alphas
    // Asume que Alpha debe estar en estado ≥3 para considerar "en tiempo"
    public async Task<string> CalculateAsync(int projectId)
    {
        var statuses = await _db.ProjectAlphaStatuses
            .Where(x => x.ProjectId == projectId)
            .ToListAsync();

        int red = 0;
        int yellow = 0;

        // Calcula cuántos Alphas están en rojo/amarillo
        foreach (var s in statuses)
        {
            int diff = 3 - s.CurrentStateNumber;  // Estados que faltan para llegar a 3

            if (diff >= 2) red++;              // Falta 2+ estados → Red
            else if (diff == 1) yellow++;      // Falta 1 estado → Yellow
        }

        // Lógica de semáforo global
        if (red > 0 || yellow >= 3)
            return "Red";      // 1+ Alphas muy atrasados OR 3+ levemente atrasados

        if (yellow > 0)
            return "Yellow";   // 1-2 Alphas levemente atrasados

        return "Green";        // Todos en tiempo
    }
}
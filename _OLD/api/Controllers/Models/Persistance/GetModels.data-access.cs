using Core.Models.Models;
using Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers.Models.Persistance;

public interface IGetModelsDataAccess
{
    Task<(IEnumerable<Model> Models, int TotalCount)> GetModelsAsync(int page, int take);
}

public class GetModelsDataAccess : IGetModelsDataAccess
{
    private readonly Context _context;

    public GetModelsDataAccess(Context context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Model> Models, int TotalCount)> GetModelsAsync(int page, int take)
    {
        var query = _context.Models
            .Include(m => m.Files)
            .AsNoTracking();

        var totalCount = await query.CountAsync();
        var models = await query
            .Skip((page - 1) * take)
            .Take(take)
            .ToListAsync();

        return (models, totalCount);
    }
} 
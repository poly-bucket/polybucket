using Core.Models.Models;
using Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers.Models.GetModelById.Persistance;

public interface IGetModelByIdDataAccess
{
    Task<Model> GetModelByIdAsync(Guid id);
}

public class GetModelByIdDataAccess : IGetModelByIdDataAccess
{
    private readonly Context _context;

    public GetModelByIdDataAccess(Context context)
    {
        _context = context;
    }

    public async Task<Model> GetModelByIdAsync(Guid id)
    {
        return await _context.Models
            .Include(m => m.Files)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
    }
}
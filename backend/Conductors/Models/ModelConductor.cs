using Database;
using Microsoft.EntityFrameworkCore;
using Core.Models.Models;  // Make sure this is the correct Model type

namespace Conductors.Models;

public interface IModelConductor
{
    Task<IEnumerable<Model>> GetModelsAsync();
    Task<Model?> GetModelByIdAsync(int id);
}

public class ModelConductor : IModelConductor
{
    private readonly Context _context;

    public ModelConductor(Context context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Model>> GetModelsAsync()
    {
        return await _context.Models.ToListAsync();
    }

    public async Task<Model?> GetModelByIdAsync(int id)
    {
        return await _context.Models.FindAsync(id);
    }
} 
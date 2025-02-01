using Core.Models;
using Database;
using Microsoft.EntityFrameworkCore;

namespace Conductors.Models;

public class ModelConductor
{
    private readonly Context _context;

    public ModelConductor(Context context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Model>> GetModelsAsync()
    {
        return await _context.Models
            .Include(m => m.Author)
            .Include(m => m.Files)
            .Include(m => m.PrintSettings)
            .ToListAsync();
    }

    public async Task<Model?> GetModelByIdAsync(string id)
    {
        return await _context.Models
            .Include(m => m.Author)
            .Include(m => m.Files)
            .Include(m => m.PrintSettings)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Model>> GetModelsByAuthorIdAsync(string authorId)
    {
        return await _context.Models
            .Include(m => m.Author)
            .Include(m => m.Files)
            .Include(m => m.PrintSettings)
            .Where(m => m.AuthorId == authorId)
            .ToListAsync();
    }

    public async Task<Model> CreateModelAsync(Model model)
    {
        _context.Models.Add(model);
        await _context.SaveChangesAsync();
        return model;
    }

    public async Task<Model> UpdateModelAsync(Model model)
    {
        _context.Models.Update(model);
        await _context.SaveChangesAsync();
        return model;
    }

    public async Task DeleteModelAsync(string id)
    {
        var model = await _context.Models.FindAsync(id);
        if (model != null)
        {
            _context.Models.Remove(model);
            await _context.SaveChangesAsync();
        }
    }
}
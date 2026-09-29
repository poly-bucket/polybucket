using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;
using PolyBucket.Api.Features.Models.AddTagToModel.Domain;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.ModeratorEditModel.Repository;

public class ModeratorEditModelRepository(PolyBucketDbContext context) : IModeratorEditModelRepository
{
    private readonly PolyBucketDbContext _context = context;

    public async Task<Model?> GetModelForEditAsync(Guid modelId, CancellationToken cancellationToken = default)
    {
        return await _context.Models
            .Include(m => m.Tags)
            .Include(m => m.Categories)
            .Include(m => m.Author)
            .Include(m => m.Files)
            .FirstOrDefaultAsync(m => m.Id == modelId && m.DeletedAt == null, cancellationToken);
    }

    public async Task<Tag?> FindTagByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Tags.FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
    }

    public async Task<Category?> FindCategoryByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Categories.FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
    }

    public void AddTag(Tag tag)
    {
        _context.Tags.Add(tag);
    }

    public void AddCategory(Category category)
    {
        _context.Categories.Add(category);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}

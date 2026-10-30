using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Authentication.ExportAccount.Repository;

public class ExportAccountRepository(PolyBucketDbContext context) : IExportAccountRepository
{
    private readonly PolyBucketDbContext _context = context;

    public Task<PolyBucket.Database.Entities.Models.User?> GetUserForExportAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _context.Users
            .Include(u => u.Settings)
            .Include(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public Task<int> GetModelCountAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _context.Models.CountAsync(m => m.CreatedById == userId, cancellationToken);
    }

    public Task<int> GetCollectionCountAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _context.Collections.CountAsync(c => c.OwnerId == userId, cancellationToken);
    }

    public Task<int> GetFavoriteCollectionCountAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _context.Collections.CountAsync(c => c.OwnerId == userId && c.Favorite, cancellationToken);
    }

    public async Task<IReadOnlyList<ExportAccountModelSummary>> GetRecentModelsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Models
            .Where(m => m.CreatedById == userId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new ExportAccountModelSummary
            {
                ModelId = m.Id,
                Name = m.Name,
                CreatedAt = m.CreatedAt,
                Privacy = m.Privacy.ToString(),
            })
            .Take(10)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExportAccountCollectionSummary>> GetRecentCollectionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Collections
            .Where(c => c.OwnerId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ExportAccountCollectionSummary
            {
                CollectionId = c.Id,
                Name = c.Name,
                Visibility = c.Visibility.ToString(),
                CreatedAt = c.CreatedAt,
            })
            .Take(10)
            .ToListAsync(cancellationToken);
    }
}

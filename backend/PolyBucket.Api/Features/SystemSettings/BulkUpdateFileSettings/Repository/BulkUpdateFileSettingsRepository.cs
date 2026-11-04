using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.BulkUpdateFileSettings.Repository;

public class BulkUpdateFileSettingsRepository(PolyBucketDbContext context) : IBulkUpdateFileSettingsRepository
{
    private readonly PolyBucketDbContext _context = context;

    public async Task<IReadOnlyList<FileTypeSettings>> GetFileTypesByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken)
    {
        var idList = ids.Distinct().ToList();
        return await _context.FileTypeSettings
            .Where(ft => idList.Contains(ft.Id) && ft.DeletedAt == null)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _context.SaveChangesAsync(cancellationToken);
}

using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.SystemSettings.Domain;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.Services;

public class AllowedFileTypesSyncService(PolyBucketDbContext context) : IAllowedFileTypesSyncService
{
    private readonly PolyBucketDbContext _context = context;

    public async Task<string> GetDerivedAllowedFileTypesAsync(CancellationToken cancellationToken = default)
    {
        var enabled = await _context.FileTypeSettings
            .AsNoTracking()
            .Where(ft => ft.DeletedAt == null && ft.Enabled)
            .ToListAsync(cancellationToken);

        return AllowedFileTypesAlignment.BuildDerivedAllowedFileTypes(enabled);
    }

    public async Task<string> SyncFromFileTypeSettingsAsync(CancellationToken cancellationToken = default)
    {
        var derived = await GetDerivedAllowedFileTypesAsync(cancellationToken);
        var systemSetup = await _context.SystemSetups.FirstOrDefaultAsync(cancellationToken);

        if (systemSetup == null)
        {
            throw new InvalidOperationException("System setup not found.");
        }

        systemSetup.AllowedFileTypes = derived;
        await _context.SaveChangesAsync(cancellationToken);

        return derived;
    }
}

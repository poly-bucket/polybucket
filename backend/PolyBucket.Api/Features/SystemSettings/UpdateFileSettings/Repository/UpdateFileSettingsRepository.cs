using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.UpdateFileSettings.Repository;

public class UpdateFileSettingsRepository(PolyBucketDbContext context) : IUpdateFileSettingsRepository
{
    private readonly PolyBucketDbContext _context = context;

    public Task<FileTypeSettings?> GetFileTypeAsync(Guid id, CancellationToken cancellationToken) =>
        _context.FileTypeSettings.FirstOrDefaultAsync(ft => ft.Id == id && ft.DeletedAt == null, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _context.SaveChangesAsync(cancellationToken);
}

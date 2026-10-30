using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.UpdateThemeSettings.Repository;

public class UpdateThemeSettingsRepository(PolyBucketDbContext context) : IUpdateThemeSettingsRepository
{
    private readonly PolyBucketDbContext _context = context;

    public Task<SystemSetup?> GetSystemSetupAsync(CancellationToken cancellationToken) =>
        _context.SystemSetups.FirstOrDefaultAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _context.SaveChangesAsync(cancellationToken);
}

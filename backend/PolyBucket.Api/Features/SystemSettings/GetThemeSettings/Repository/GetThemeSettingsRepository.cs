using Microsoft.EntityFrameworkCore;
using PolyBucket.Database.Entities.SystemSettings;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.GetThemeSettings.Repository;

public class GetThemeSettingsRepository(PolyBucketDbContext context) : IGetThemeSettingsRepository
{
    private readonly PolyBucketDbContext _context = context;

    public Task<SystemSetup?> GetSystemSetupAsync(CancellationToken cancellationToken) =>
        _context.SystemSetups.FirstOrDefaultAsync(cancellationToken);
}

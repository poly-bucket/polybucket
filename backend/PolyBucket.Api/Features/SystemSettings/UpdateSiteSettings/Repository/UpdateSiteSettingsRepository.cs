using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.UpdateSiteSettings.Repository;

public class UpdateSiteSettingsRepository(PolyBucketDbContext context) : IUpdateSiteSettingsRepository
{
    public PolyBucketDbContext Context { get; } = context;
}

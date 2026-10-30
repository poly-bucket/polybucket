using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.GetSiteSettings.Repository;

public class GetSiteSettingsRepository(PolyBucketDbContext context) : IGetSiteSettingsRepository
{
    public PolyBucketDbContext Context { get; } = context;
}

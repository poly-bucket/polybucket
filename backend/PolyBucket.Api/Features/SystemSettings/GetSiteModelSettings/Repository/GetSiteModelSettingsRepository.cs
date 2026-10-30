using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.GetSiteModelSettings.Repository;

public class GetSiteModelSettingsRepository(PolyBucketDbContext context) : IGetSiteModelSettingsRepository
{
    public PolyBucketDbContext Context { get; } = context;
}

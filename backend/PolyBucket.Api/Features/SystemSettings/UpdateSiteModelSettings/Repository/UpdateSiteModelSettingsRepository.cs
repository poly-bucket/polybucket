using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.UpdateSiteModelSettings.Repository;

public class UpdateSiteModelSettingsRepository(PolyBucketDbContext context) : IUpdateSiteModelSettingsRepository
{
    public PolyBucketDbContext Context { get; } = context;
}

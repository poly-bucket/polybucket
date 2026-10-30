using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.UpdateSiteModelSettings.Repository;

public interface IUpdateSiteModelSettingsRepository
{
    PolyBucketDbContext Context { get; }
}

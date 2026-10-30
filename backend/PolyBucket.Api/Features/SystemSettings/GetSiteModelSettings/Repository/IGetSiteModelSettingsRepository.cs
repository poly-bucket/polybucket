using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.GetSiteModelSettings.Repository;

public interface IGetSiteModelSettingsRepository
{
    PolyBucketDbContext Context { get; }
}

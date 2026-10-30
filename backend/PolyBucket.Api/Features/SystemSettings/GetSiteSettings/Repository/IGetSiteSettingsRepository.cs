using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.GetSiteSettings.Repository;

public interface IGetSiteSettingsRepository
{
    PolyBucketDbContext Context { get; }
}

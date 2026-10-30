using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.UpdateSiteSettings.Repository;

public interface IUpdateSiteSettingsRepository
{
    PolyBucketDbContext Context { get; }
}

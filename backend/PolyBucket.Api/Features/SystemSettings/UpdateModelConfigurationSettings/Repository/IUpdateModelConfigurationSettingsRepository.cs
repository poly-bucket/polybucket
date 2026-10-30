using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.UpdateModelConfigurationSettings.Repository;

public interface IUpdateModelConfigurationSettingsRepository
{
    PolyBucketDbContext Context { get; }
}

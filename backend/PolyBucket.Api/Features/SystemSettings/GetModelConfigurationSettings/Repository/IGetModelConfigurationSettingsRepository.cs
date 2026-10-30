using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.GetModelConfigurationSettings.Repository;

public interface IGetModelConfigurationSettingsRepository
{
    PolyBucketDbContext Context { get; }
}

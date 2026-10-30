using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.GetModelConfigurationSettings.Repository;

public class GetModelConfigurationSettingsRepository(PolyBucketDbContext context) : IGetModelConfigurationSettingsRepository
{
    public PolyBucketDbContext Context { get; } = context;
}

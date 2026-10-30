using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.UpdateModelConfigurationSettings.Repository;

public class UpdateModelConfigurationSettingsRepository(PolyBucketDbContext context) : IUpdateModelConfigurationSettingsRepository
{
    public PolyBucketDbContext Context { get; } = context;
}

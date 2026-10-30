using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.SkipEmailSetup.Repository;

public class SkipEmailSetupRepository(PolyBucketDbContext context) : ISkipEmailSetupRepository
{
    public PolyBucketDbContext Context { get; } = context;
}

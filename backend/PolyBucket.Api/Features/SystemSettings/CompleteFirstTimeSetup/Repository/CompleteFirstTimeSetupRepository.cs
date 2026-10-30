using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.CompleteFirstTimeSetup.Repository;

public class CompleteFirstTimeSetupRepository(PolyBucketDbContext context) : ICompleteFirstTimeSetupRepository
{
    public PolyBucketDbContext Context { get; } = context;
}

using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.CheckFirstTimeSetup.Repository;

public class CheckFirstTimeSetupRepository(PolyBucketDbContext context) : ICheckFirstTimeSetupRepository
{
    public PolyBucketDbContext Context { get; } = context;
}

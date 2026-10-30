using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.ApplyStorageSetup.Repository;

public class ApplyStorageSetupRepository(PolyBucketDbContext context) : IApplyStorageSetupRepository
{
    public PolyBucketDbContext Context { get; } = context;
}

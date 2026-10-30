using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.CompleteFirstTimeSetup.Repository;

public interface ICompleteFirstTimeSetupRepository
{
    PolyBucketDbContext Context { get; }
}

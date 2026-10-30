using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.SkipEmailSetup.Repository;

public interface ISkipEmailSetupRepository
{
    PolyBucketDbContext Context { get; }
}

using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.CheckFirstTimeSetup.Repository;

public interface ICheckFirstTimeSetupRepository
{
    PolyBucketDbContext Context { get; }
}

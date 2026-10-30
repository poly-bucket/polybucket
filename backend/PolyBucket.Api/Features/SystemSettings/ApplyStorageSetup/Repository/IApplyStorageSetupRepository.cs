using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.SystemSettings.ApplyStorageSetup.Repository;

public interface IApplyStorageSetupRepository
{
    PolyBucketDbContext Context { get; }
}

namespace PolyBucket.Api.Features.Federation.InitiateHandshake.Repository;

public interface IInitiateHandshakeRepository
{
    PolyBucketDbContext Context { get; }
}

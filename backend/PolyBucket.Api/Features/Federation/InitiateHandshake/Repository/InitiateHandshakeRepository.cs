namespace PolyBucket.Api.Features.Federation.InitiateHandshake.Repository;

public class InitiateHandshakeRepository(PolyBucketDbContext context) : IInitiateHandshakeRepository
{
    public PolyBucketDbContext Context { get; } = context;
}

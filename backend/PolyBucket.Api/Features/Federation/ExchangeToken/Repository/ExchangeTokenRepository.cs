namespace PolyBucket.Api.Features.Federation.ExchangeToken.Repository;

public class ExchangeTokenRepository(PolyBucketDbContext context) : IExchangeTokenRepository
{
    public PolyBucketDbContext Context { get; } = context;
}

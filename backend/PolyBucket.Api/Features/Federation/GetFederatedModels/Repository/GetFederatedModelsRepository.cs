namespace PolyBucket.Api.Features.Federation.GetFederatedModels.Repository;

public class GetFederatedModelsRepository(PolyBucketDbContext context) : IGetFederatedModelsRepository
{
    public PolyBucketDbContext Context { get; } = context;
}

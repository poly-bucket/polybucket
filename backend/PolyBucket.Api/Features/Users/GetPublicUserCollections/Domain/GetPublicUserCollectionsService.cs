using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetPublicUserCollections.Repository;

namespace PolyBucket.Api.Features.Users.GetPublicUserCollections.Domain;

public class GetPublicUserCollectionsService(IGetPublicUserCollectionsRepository repository) : IGetPublicUserCollectionsService
{
    public Task<GetPublicUserCollectionsResult> GetPublicUserCollectionsAsync(GetPublicUserCollectionsQuery query, CancellationToken cancellationToken = default)
    {
        return repository.GetPublicUserCollectionsAsync(query, cancellationToken);
    }
}

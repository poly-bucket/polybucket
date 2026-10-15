using PolyBucket.Api.Features.Collections.GetUserCollections.Http;
using PolyBucket.Api.Features.Collections.GetUserCollections.Repository;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.GetUserCollections.Domain;

public class GetUserCollectionsService(ICollectionRepository repository) : IGetUserCollectionsService
{
    public Task<GetUserCollectionsResponseDto> GetUserCollectionsAsync(GetUserCollectionsQuery query, CancellationToken cancellationToken = default)
    {
        return repository.GetUserCollectionsAsync(query, cancellationToken);
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetPublicUserCollections.Domain;

namespace PolyBucket.Api.Features.Users.GetPublicUserCollections.Repository;

public class GetPublicUserCollectionsRepository : IGetPublicUserCollectionsRepository
{
    public Task<GetPublicUserCollectionsResult> GetPublicUserCollectionsAsync(GetPublicUserCollectionsQuery query, CancellationToken cancellationToken = default)
    {
        var collections = new List<PublicUserCollectionListItemDto>();
        const int totalCount = 0;

        return Task.FromResult(new GetPublicUserCollectionsResult
        {
            Collections = collections,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
        });
    }
}

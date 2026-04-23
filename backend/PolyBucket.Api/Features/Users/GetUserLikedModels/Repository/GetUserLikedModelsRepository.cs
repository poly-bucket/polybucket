using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetUserLikedModels.Domain;

namespace PolyBucket.Api.Features.Users.GetUserLikedModels.Repository;

public class GetUserLikedModelsRepository : IGetUserLikedModelsRepository
{
    public Task<GetUserLikedModelsResult> GetUserLikedModelsAsync(GetUserLikedModelsQuery query, CancellationToken cancellationToken = default)
    {
        var models = new List<UserLikedModelListItemDto>();
        const int totalCount = 0;

        return Task.FromResult(new GetUserLikedModelsResult
        {
            Models = models,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
        });
    }
}

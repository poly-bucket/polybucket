using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetUserComments.Domain;

namespace PolyBucket.Api.Features.Users.GetUserComments.Repository;

public class GetUserCommentsRepository : IGetUserCommentsRepository
{
    public Task<GetUserCommentsResult> GetUserCommentsAsync(GetUserCommentsQuery query, CancellationToken cancellationToken = default)
    {
        var comments = new List<UserCommentListItemDto>();
        const int totalCount = 0;

        return Task.FromResult(new GetUserCommentsResult
        {
            Comments = comments,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
        });
    }
}

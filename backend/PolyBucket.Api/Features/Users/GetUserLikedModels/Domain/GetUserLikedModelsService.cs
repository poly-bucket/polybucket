using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Users.GetUserLikedModels.Repository;

namespace PolyBucket.Api.Features.Users.GetUserLikedModels.Domain;

public class GetUserLikedModelsService(
    IGetUserLikedModelsRepository repository,
    PolyBucketDbContext dbContext) : IGetUserLikedModelsService
{
    public async Task<GetUserLikedModelsResult> GetUserLikedModelsAsync(GetUserLikedModelsQuery query, CancellationToken cancellationToken = default)
    {
        if (query.UserId == Guid.Empty && !string.IsNullOrWhiteSpace(query.Username))
        {
            var user = await dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == query.Username, cancellationToken);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with username {query.Username} not found");
            }

            query.UserId = user.Id;
        }

        if (query.UserId == Guid.Empty)
        {
            throw new ArgumentException("UserId or Username must be provided");
        }

        return await repository.GetUserLikedModelsAsync(query, cancellationToken);
    }
}

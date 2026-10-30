using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.FollowUser.Domain;

public interface IFollowUserService
{
    Task<FollowUserOutcome> FollowAsync(Guid followerUserId, Guid followedUserId, CancellationToken cancellationToken = default);
}

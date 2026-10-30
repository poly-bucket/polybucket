using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.Follow.Repository;
using PolyBucket.Api.Features.Users.UnfollowUser.Domain;

namespace PolyBucket.Api.Features.Users.UnfollowUser.Domain;

public class UnfollowUserService(IUserFollowRepository followRepository, TimeProvider timeProvider) : IUnfollowUserService
{
    public async Task UnfollowAsync(Guid followerUserId, Guid followedUserId, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        await followRepository.SoftDeleteActiveFollowAsync(followerUserId, followedUserId, followerUserId, now, cancellationToken);
    }
}

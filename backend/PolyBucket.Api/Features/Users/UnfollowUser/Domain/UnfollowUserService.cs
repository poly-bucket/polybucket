using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.UnfollowUser.Repository;

namespace PolyBucket.Api.Features.Users.UnfollowUser.Domain;

public class UnfollowUserService(IUnfollowUserRepository repository, TimeProvider timeProvider) : IUnfollowUserService
{
    public async Task UnfollowAsync(Guid followerUserId, Guid followedUserId, CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        await repository.SoftDeleteActiveFollowAsync(followerUserId, followedUserId, followerUserId, now, cancellationToken);
    }
}

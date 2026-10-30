using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.UnfollowUser.Domain;

public interface IUnfollowUserService
{
    Task UnfollowAsync(Guid followerUserId, Guid followedUserId, CancellationToken cancellationToken = default);
}

using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.Follow.Repository;
using PolyBucket.Database.Entities.Users;

namespace PolyBucket.Api.Features.Users.UnfollowUser.Repository;

public interface IUnfollowUserRepository
{
    Task<bool> SoftDeleteActiveFollowAsync(Guid followerUserId, Guid followedUserId, Guid actorUserId, DateTime now, CancellationToken cancellationToken = default);
}

public class UnfollowUserRepository(IUserFollowRepository followRepository) : IUnfollowUserRepository
{
    public Task<bool> SoftDeleteActiveFollowAsync(Guid followerUserId, Guid followedUserId, Guid actorUserId, DateTime now, CancellationToken cancellationToken = default) =>
        followRepository.SoftDeleteActiveFollowAsync(followerUserId, followedUserId, actorUserId, now, cancellationToken);
}

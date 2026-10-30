using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Features.Users.Follow.Repository;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.Users;
using PolyBucket.Database.Persistence;

namespace PolyBucket.Api.Features.Users.FollowUser.Repository;

public interface IFollowUserRepository
{
    Task<User?> GetActiveTargetUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserFollow?> GetActiveFollowAsync(Guid followerUserId, Guid followedUserId, CancellationToken cancellationToken = default);
    Task<UserFollow> AddFollowAsync(Guid followerUserId, Guid followedUserId, Guid actorUserId, DateTime now, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class FollowUserRepository(PolyBucketDbContext dbContext, IUserFollowRepository followRepository) : IFollowUserRepository
{
    public Task<User?> GetActiveTargetUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, cancellationToken);

    public Task<UserFollow?> GetActiveFollowAsync(Guid followerUserId, Guid followedUserId, CancellationToken cancellationToken = default) =>
        followRepository.GetActiveFollowAsync(followerUserId, followedUserId, cancellationToken);

    public Task<UserFollow> AddFollowAsync(Guid followerUserId, Guid followedUserId, Guid actorUserId, DateTime now, CancellationToken cancellationToken = default) =>
        followRepository.AddFollowAsync(followerUserId, followedUserId, actorUserId, now, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        followRepository.SaveChangesAsync(cancellationToken);
}

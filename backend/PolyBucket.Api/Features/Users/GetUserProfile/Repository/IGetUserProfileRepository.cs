using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Models;

namespace PolyBucket.Api.Features.Users.GetUserProfile.Repository;

public interface IGetUserProfileRepository
{
    Task<User?> GetByIdWithRoleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameWithRoleAsync(string username, CancellationToken cancellationToken = default);
    Task<GetUserProfileStatisticsRow> GetProfileStatisticsAsync(Guid userId, bool includePrivate, CancellationToken cancellationToken = default);
}

public class GetUserProfileStatisticsRow
{
    public int TotalModels { get; set; }
    public int TotalCollections { get; set; }
    public int TotalLikes { get; set; }
    public int TotalDownloads { get; set; }
    public int TotalFollowers { get; set; }
    public int TotalFollowing { get; set; }
}

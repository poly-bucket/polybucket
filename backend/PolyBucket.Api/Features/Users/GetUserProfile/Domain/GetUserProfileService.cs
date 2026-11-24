using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Users.GetUserProfile.Repository;
using PolyBucket.Database.Entities.Models;

namespace PolyBucket.Api.Features.Users.GetUserProfile.Domain;

public class GetUserProfileService(
    IGetUserProfileRepository repository,
    Follow.Repository.IUserFollowRepository followRepository) : IGetUserProfileService
{
    public async Task<object> GetUserProfileAsync(GetUserProfileQuery query, CancellationToken cancellationToken = default)
    {
        var user = !string.IsNullOrEmpty(query.Username)
            ? await repository.GetByUsernameWithRoleAsync(query.Username, cancellationToken)
            : await repository.GetByIdWithRoleAsync(query.Id, cancellationToken);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        if (!user.IsProfilePublic)
        {
            var canViewPrivateProfile = query.IsRequestingUserAdmin || (query.RequestingUserId.HasValue && query.RequestingUserId.Value == user.Id);
            if (canViewPrivateProfile)
            {
                var privateProfileStatistics = await repository.GetProfileStatisticsAsync(user.Id, includePrivate: true, cancellationToken);
                return await BuildResponseAsync(user, privateProfileStatistics, query, cancellationToken);
            }

            return new PrivateProfileResponse
            {
                Id = user.Id,
                Username = user.Username,
                IsProfilePublic = false,
                Message = "This profile is private"
            };
        }

        var includePrivate = query.IsRequestingUserAdmin || (query.RequestingUserId.HasValue && query.RequestingUserId.Value == user.Id);
        var statistics = await repository.GetProfileStatisticsAsync(user.Id, includePrivate, cancellationToken);

        return await BuildResponseAsync(user, statistics, query, cancellationToken);
    }

    private async Task<GetUserProfileResponse> BuildResponseAsync(
        User user,
        GetUserProfileStatisticsRow statistics,
        GetUserProfileQuery query,
        CancellationToken cancellationToken)
    {
        var isFollowedByCurrentUser = false;
        if (query.RequestingUserId.HasValue
            && query.RequestingUserId.Value != user.Id)
        {
            isFollowedByCurrentUser = await followRepository.IsActiveFollowerAsync(
                query.RequestingUserId.Value,
                user.Id,
                cancellationToken);
        }

        var canViewSensitive = CanViewSensitive(query, user.Id);
        var showEmail = canViewSensitive || user.ShowEmail;
        var showLastLogin = canViewSensitive || user.ShowLastLogin;
        var showStatistics = canViewSensitive || user.ShowStatistics;

        return new GetUserProfileResponse
        {
            Id = user.Id,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Bio = user.Bio,
            Avatar = user.Avatar,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Country = user.Country,
            Email = showEmail ? user.Email : null,
            RoleName = user.Role?.Name ?? "Unknown",
            CreatedAt = user.CreatedAt,
            LastLoginAt = showLastLogin ? user.LastLoginAt : null,
            IsBanned = user.IsBanned,
            BannedAt = user.BannedAt,
            BanReason = user.BanReason,
            BanExpiresAt = user.BanExpiresAt,
            TotalModels = showStatistics ? statistics.TotalModels : null,
            TotalCollections = showStatistics ? statistics.TotalCollections : null,
            TotalLikes = showStatistics ? statistics.TotalLikes : null,
            TotalDownloads = showStatistics ? statistics.TotalDownloads : null,
            TotalFollowers = showStatistics ? statistics.TotalFollowers : null,
            TotalFollowing = showStatistics ? statistics.TotalFollowing : null,
            IsProfilePublic = user.IsProfilePublic,
            IsFollowedByCurrentUser = isFollowedByCurrentUser,
            ShowEmail = canViewSensitive ? user.ShowEmail : null,
            ShowLastLogin = canViewSensitive ? user.ShowLastLogin : null,
            ShowStatistics = canViewSensitive ? user.ShowStatistics : null,
            WebsiteUrl = user.WebsiteUrl,
            TwitterUrl = user.TwitterUrl,
            InstagramUrl = user.InstagramUrl,
            YouTubeUrl = user.YouTubeUrl
        };
    }

    private static bool CanViewSensitive(GetUserProfileQuery query, Guid userId) =>
        query.IsRequestingUserAdmin
        || (query.RequestingUserId.HasValue && query.RequestingUserId.Value == userId);
}

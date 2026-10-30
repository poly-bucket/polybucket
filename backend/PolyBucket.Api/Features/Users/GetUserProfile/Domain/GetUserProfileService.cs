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
            RoleName = user.Role?.Name ?? "Unknown",
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            IsBanned = user.IsBanned,
            BannedAt = user.BannedAt,
            BanReason = user.BanReason,
            BanExpiresAt = user.BanExpiresAt,
            TotalModels = statistics.TotalModels,
            TotalCollections = statistics.TotalCollections,
            TotalLikes = statistics.TotalLikes,
            TotalDownloads = statistics.TotalDownloads,
            TotalFollowers = statistics.TotalFollowers,
            TotalFollowing = statistics.TotalFollowing,
            IsProfilePublic = user.IsProfilePublic,
            IsFollowedByCurrentUser = isFollowedByCurrentUser,
            ShowEmail = user.ShowEmail,
            ShowLastLogin = user.ShowLastLogin,
            ShowStatistics = user.ShowStatistics,
            WebsiteUrl = user.WebsiteUrl,
            TwitterUrl = user.TwitterUrl,
            InstagramUrl = user.InstagramUrl,
            YouTubeUrl = user.YouTubeUrl
        };
    }
}

using System;

namespace PolyBucket.Api.Features.Users.GetUserProfile.Domain;

public class GetUserProfileQuery
{
    public Guid Id { get; set; }
    public string? Username { get; set; }
    public Guid? RequestingUserId { get; set; }
    public bool IsRequestingUserAdmin { get; set; }
}

public class GetUserProfileResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Bio { get; set; }
    public string? Avatar { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Country { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsBanned { get; set; }
    public DateTime? BannedAt { get; set; }
    public string? BanReason { get; set; }
    public DateTime? BanExpiresAt { get; set; }
    public int TotalModels { get; set; }
    public int TotalCollections { get; set; }
    public int TotalLikes { get; set; }
    public int TotalDownloads { get; set; }
    public int TotalFollowers { get; set; }
    public int TotalFollowing { get; set; }
    public bool IsProfilePublic { get; set; }
    public bool ShowEmail { get; set; }
    public bool ShowLastLogin { get; set; }
    public bool ShowStatistics { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? YouTubeUrl { get; set; }
}

public class PrivateProfileResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool IsProfilePublic { get; set; } = false;
    public string Message { get; set; } = "This profile is private";
}

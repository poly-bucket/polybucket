using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Users.GetUserFollowing.Domain;

public class GetUserFollowingQuery
{
    public Guid UserId { get; set; }
    public string? Username { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? RequestingUserId { get; set; }
    public bool IsRequestingUserAdmin { get; set; }
}

public class UserFollowingListItemDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public DateTime FollowedAt { get; set; }
}

public class GetUserFollowingResult
{
    public IReadOnlyList<UserFollowingListItemDto> Users { get; set; } = Array.Empty<UserFollowingListItemDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public enum GetUserFollowingOutcome
{
    Success,
    UserNotFound,
    ProfilePrivate
}

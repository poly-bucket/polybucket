using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Users.GetUserFollowers.Domain;

public class GetUserFollowListQuery
{
    public Guid UserId { get; set; }
    public string? Username { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? RequestingUserId { get; set; }
    public bool IsRequestingUserAdmin { get; set; }
}

public class UserFollowListItemDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public DateTime FollowedAt { get; set; }
}

public class GetUserFollowListResult
{
    public IReadOnlyList<UserFollowListItemDto> Users { get; set; } = Array.Empty<UserFollowListItemDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public enum GetUserFollowListOutcome
{
    Success,
    UserNotFound,
    ProfilePrivate
}

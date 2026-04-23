using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Users.GetUsers.Domain;

public class GetUsersQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchQuery { get; set; }
    public string? RoleFilter { get; set; }
    public string? StatusFilter { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class GetUsersResult
{
    public IEnumerable<UserListItemDto> Users { get; set; } = new List<UserListItemDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class UserListItemDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Country { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public Guid? RoleId { get; set; }
    public bool IsBanned { get; set; }
    public DateTime? BannedAt { get; set; }
    public string? BanReason { get; set; }
    public DateTime? BanExpiresAt { get; set; }
    public bool HasCompletedFirstTimeSetup { get; set; }
    public bool RequiresPasswordChange { get; set; }
    public string? Avatar { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

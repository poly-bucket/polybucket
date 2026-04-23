using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Users.GetBannedUsers.Http;

public class BannedUserListItemDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? BannedAt { get; set; }
    public string? BannedByUsername { get; set; }
    public string? BanReason { get; set; }
    public DateTime? BanExpiresAt { get; set; }
    public string RoleName { get; set; } = string.Empty;
}

public class BannedUsersListResponse
{
    public IEnumerable<BannedUserListItemDto> Users { get; set; } = new List<BannedUserListItemDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

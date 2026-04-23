using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Users.GetUserComments.Domain;

public class GetUserCommentsQuery
{
    public Guid UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchQuery { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class GetUserCommentsResult
{
    public IEnumerable<UserCommentListItemDto> Comments { get; set; } = new List<UserCommentListItemDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class UserCommentListItemDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsEdited { get; set; }
    public bool IsDeleted { get; set; }
    public UserCommentModelDto Model { get; set; } = new();
    public UserCommentUserDto User { get; set; } = new();
}

public class UserCommentModelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
}

public class UserCommentUserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Avatar { get; set; }
}

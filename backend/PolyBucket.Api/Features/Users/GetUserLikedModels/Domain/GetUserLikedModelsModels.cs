using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Users.GetUserLikedModels.Domain;

public class GetUserLikedModelsQuery
{
    public Guid UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchQuery { get; set; }
    public string? SortBy { get; set; } = "LikedAt";
    public bool SortDescending { get; set; } = true;
}

public class GetUserLikedModelsResult
{
    public IEnumerable<UserLikedModelListItemDto> Models { get; set; } = new List<UserLikedModelListItemDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class UserLikedModelListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int Downloads { get; set; }
    public int Likes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime LikedAt { get; set; }
    public string? License { get; set; }
    public bool AIGenerated { get; set; }
    public bool WIP { get; set; }
    public bool NSFW { get; set; }
    public LikedModelAuthorDto Author { get; set; } = new();
}

public class LikedModelAuthorDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Avatar { get; set; }
}

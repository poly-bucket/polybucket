using System;
using System.Collections.Generic;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;

namespace PolyBucket.Api.Features.Users.GetUserModels.Domain;

public class GetUserModelsQuery
{
    public string Username { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool IncludePrivate { get; set; } = false;
}

public class GetUserModelsResult
{
    public IEnumerable<UserModelListItemDto> Models { get; set; } = new List<UserModelListItemDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class UserModelListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int Downloads { get; set; }
    public int Likes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? License { get; set; }
    public bool AIGenerated { get; set; }
    public bool WIP { get; set; }
    public bool NSFW { get; set; }
    public bool IsRemix { get; set; }
    public PrivacySettings Privacy { get; set; }
}

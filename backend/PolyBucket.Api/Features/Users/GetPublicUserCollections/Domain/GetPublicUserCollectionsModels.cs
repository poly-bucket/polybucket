using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Users.GetPublicUserCollections.Domain;

public class GetPublicUserCollectionsQuery
{
    public Guid UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchQuery { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class GetPublicUserCollectionsResult
{
    public IEnumerable<PublicUserCollectionListItemDto> Collections { get; set; } = new List<PublicUserCollectionListItemDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class PublicUserCollectionListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Avatar { get; set; }
    public string Visibility { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int ModelCount { get; set; }
    public bool IsPasswordProtected { get; set; }
}

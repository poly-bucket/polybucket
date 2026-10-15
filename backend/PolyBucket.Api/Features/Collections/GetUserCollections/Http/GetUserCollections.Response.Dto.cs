using PolyBucket.Api.Features.Collections.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PolyBucket.Api.Features.Collections.GetUserCollections.Http;

[DisplayName("GetUserCollectionsResponseDto")]
public class GetUserCollectionsResponseDto
{
    public List<GetUserCollectionsListItemDto> Collections { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class GetUserCollectionsListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CollectionVisibility Visibility { get; set; }
    public Guid OwnerId { get; set; }
    public GetUserCollectionsOwnerDto? Owner { get; set; }
    public string? Avatar { get; set; }
    public bool Favorite { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPasswordProtected { get; set; }
    public int ModelCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class GetUserCollectionsOwnerDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

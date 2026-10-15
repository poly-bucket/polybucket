using PolyBucket.Api.Features.Collections.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PolyBucket.Api.Features.Collections.GetCollectionById.Http;

[DisplayName("GetCollectionByIdResponseDto")]
public class GetCollectionByIdResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CollectionVisibility Visibility { get; set; }
    public Guid OwnerId { get; set; }
    public GetCollectionByIdOwnerDto? Owner { get; set; }
    public string? Avatar { get; set; }
    public bool Favorite { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPasswordProtected { get; set; }
    public List<GetCollectionByIdCollectionModelDto> CollectionModels { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class GetCollectionByIdOwnerDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

public class GetCollectionByIdCollectionModelDto
{
    public Guid CollectionId { get; set; }
    public Guid ModelId { get; set; }
    public DateTime AddedAt { get; set; }
    public GetCollectionByIdModelSummaryDto? Model { get; set; }
}

public class GetCollectionByIdModelSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
}

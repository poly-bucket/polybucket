using PolyBucket.Api.Features.Collections.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace PolyBucket.Api.Features.Collections.AccessCollection.Http;

[DisplayName("AccessCollectionCollectionDto")]
public class AccessCollectionCollectionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CollectionVisibility Visibility { get; set; }
    public Guid OwnerId { get; set; }
    public AccessCollectionOwnerDto? Owner { get; set; }
    public string? Avatar { get; set; }
    public bool Favorite { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPasswordProtected { get; set; }
    public List<AccessCollectionCollectionModelDto> CollectionModels { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AccessCollectionOwnerDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

public class AccessCollectionCollectionModelDto
{
    public Guid CollectionId { get; set; }
    public Guid ModelId { get; set; }
    public DateTime AddedAt { get; set; }
    public AccessCollectionModelSummaryDto? Model { get; set; }
}

public class AccessCollectionModelSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
}

using PolyBucket.Api.Features.Collections.Domain.Enums;
using System;
using System.ComponentModel;

namespace PolyBucket.Api.Features.Collections.CreateCollection.Http;

[DisplayName("CreateCollectionResponseDto")]
public class CreateCollectionResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CollectionVisibility Visibility { get; set; }
    public Guid OwnerId { get; set; }
    public string? Avatar { get; set; }
    public bool Favorite { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPasswordProtected { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

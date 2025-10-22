using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using System;

namespace PolyBucket.Api.Features.Collections.GetFavoriteCollections.Domain
{
    public class GetFavoriteCollectionsResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public CollectionVisibility Visibility { get; set; }
        public string? Avatar { get; set; }
        public bool Favorite { get; set; }
        public int DisplayOrder { get; set; }
        public int ModelCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

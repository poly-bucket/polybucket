using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.UpdateCollection.Http;

namespace PolyBucket.Api.Features.Collections.UpdateCollection.Domain;

public static class UpdateCollectionMapper
{
    public static UpdateCollectionResponseDto ToResponseDto(Collection collection) => new()
    {
        Id = collection.Id,
        Name = collection.Name,
        Description = collection.Description,
        Visibility = collection.Visibility,
        OwnerId = collection.OwnerId,
        Avatar = collection.Avatar,
        Favorite = collection.Favorite,
        DisplayOrder = collection.DisplayOrder,
        IsPasswordProtected = !string.IsNullOrEmpty(collection.PasswordHash),
        CreatedAt = collection.CreatedAt,
        UpdatedAt = collection.UpdatedAt
    };
}

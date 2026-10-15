using PolyBucket.Api.Features.Collections.CreateCollection.Http;
using PolyBucket.Api.Features.Collections.Domain;

namespace PolyBucket.Api.Features.Collections.CreateCollection.Domain;

public static class CreateCollectionMapper
{
    public static CreateCollectionResponseDto ToResponseDto(Collection collection) => new()
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

using PolyBucket.Api.Features.Collections.Domain;
using PolyBucket.Api.Features.Collections.GetCollectionById.Http;
using System.Linq;

namespace PolyBucket.Api.Features.Collections.GetCollectionById.Domain;

public static class GetCollectionByIdMapper
{
    public static GetCollectionByIdResponseDto ToDto(Collection collection)
    {
        return new GetCollectionByIdResponseDto
        {
            Id = collection.Id,
            Name = collection.Name,
            Description = collection.Description,
            Visibility = collection.Visibility,
            OwnerId = collection.OwnerId,
            Owner = collection.Owner == null
                ? null
                : new GetCollectionByIdOwnerDto
                {
                    Id = collection.Owner.Id,
                    Username = collection.Owner.Username,
                    Avatar = collection.Owner.Avatar,
                    ProfilePictureUrl = collection.Owner.ProfilePictureUrl
                },
            Avatar = collection.Avatar,
            Favorite = collection.Favorite,
            DisplayOrder = collection.DisplayOrder,
            IsPasswordProtected = !string.IsNullOrEmpty(collection.PasswordHash),
            CreatedAt = collection.CreatedAt,
            UpdatedAt = collection.UpdatedAt,
            CollectionModels = collection.CollectionModels
                .Select(cm => new GetCollectionByIdCollectionModelDto
                {
                    CollectionId = cm.CollectionId,
                    ModelId = cm.ModelId,
                    AddedAt = cm.AddedAt,
                    Model = cm.Model == null
                        ? null
                        : new GetCollectionByIdModelSummaryDto
                        {
                            Id = cm.Model.Id,
                            Name = cm.Model.Name,
                            Description = cm.Model.Description,
                            ThumbnailUrl = cm.Model.ThumbnailUrl
                        }
                })
                .ToList()
        };
    }
}

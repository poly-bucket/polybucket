using PolyBucket.Api.Features.Collections.AccessCollection.Http;
using PolyBucket.Api.Features.Collections.Domain;
using System.Linq;

namespace PolyBucket.Api.Features.Collections.AccessCollection.Domain;

public static class AccessCollectionMapper
{
    public static AccessCollectionCollectionDto ToDto(Collection collection)
    {
        return new AccessCollectionCollectionDto
        {
            Id = collection.Id,
            Name = collection.Name,
            Description = collection.Description,
            Visibility = collection.Visibility,
            OwnerId = collection.OwnerId,
            Owner = collection.Owner == null
                ? null
                : new AccessCollectionOwnerDto
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
                .Select(cm => new AccessCollectionCollectionModelDto
                {
                    CollectionId = cm.CollectionId,
                    ModelId = cm.ModelId,
                    AddedAt = cm.AddedAt,
                    Model = cm.Model == null
                        ? null
                        : new AccessCollectionModelSummaryDto
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

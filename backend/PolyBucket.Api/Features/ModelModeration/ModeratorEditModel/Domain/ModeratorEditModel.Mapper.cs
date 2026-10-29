using PolyBucket.Database.Entities.Models;
using PolyBucket.Api.Features.ModelModeration.ModeratorEditModel.Http;
using System.Linq;

namespace PolyBucket.Api.Features.ModelModeration.ModeratorEditModel.Domain;

public static class ModeratorEditModelMapper
{
    public static ModeratorEditModelResponseDto ToResponseDto(Model model)
    {
        return new ModeratorEditModelResponseDto
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            ThumbnailUrl = model.ThumbnailUrl,
            License = model.License,
            Privacy = model.Privacy,
            AIGenerated = model.AIGenerated,
            WIP = model.WIP,
            NSFW = model.NSFW,
            IsRemix = model.IsRemix,
            RemixUrl = model.RemixUrl,
            IsPublic = model.IsPublic,
            IsFeatured = model.IsFeatured,
            AuthorId = model.AuthorId,
            Author = model.Author == null
                ? null
                : new ModeratorEditModelAuthorDto
                {
                    Id = model.Author.Id,
                    Username = model.Author.Username,
                    Avatar = model.Author.Avatar,
                    ProfilePictureUrl = model.Author.ProfilePictureUrl
                },
            Tags = model.Tags.Select(t => t.Name).ToList(),
            Categories = model.Categories.Select(c => c.Name).ToList(),
            Files = model.Files.Select(f => new ModeratorEditModelFileDto
            {
                Id = f.Id,
                Name = f.Name,
                Size = f.Size,
                MimeType = f.MimeType
            }).ToList(),
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt
        };
    }
}

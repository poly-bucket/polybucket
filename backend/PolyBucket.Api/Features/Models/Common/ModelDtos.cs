using System;
using System.Collections.Generic;
using System.Linq;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;
using PolyBucket.Api.Features.Models.AddTagToModel.Domain;
using PolyBucket.Api.Features.Models.CreateModel.Domain;
using PolyBucket.Api.Features.Models.CreateModelVersion.Domain;
using PolyBucket.Api.Features.Models.LikeModel.Domain;

namespace PolyBucket.Api.Features.Models.Common
{
    /// <summary>
    /// Public-safe projection of a model author. Excludes credentials and other
    /// internal account data that must never be returned in model responses.
    /// </summary>
    public class ModelAuthorDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Bio { get; set; }
        public string? Avatar { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Country { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? YouTubeUrl { get; set; }
        public bool IsProfilePublic { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ModelFileDto
    {
        public Guid Id { get; set; }
        public Guid ModelId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public long Size { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ModelCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class ModelTagDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    public class ModelVersionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string? FileUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int VersionNumber { get; set; }
        public Guid ModelId { get; set; }
        public List<ModelFileDto> Files { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ModelCommentDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public ModelAuthorDto? Author { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ModelLikeDto
    {
        public Guid Id { get; set; }
        public Guid ModelId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Public-safe projection of a model. Used by all model read/write endpoints so
    /// that nested entities (author, comment authors, likers) never leak sensitive
    /// account data such as password hashes, salts, logins, permissions, or 2FA.
    /// </summary>
    public class ModelDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string? FileUrl { get; set; }
        public int Downloads { get; set; }
        public int Likes { get; set; }
        public LicenseTypes? License { get; set; }
        public PrivacySettings Privacy { get; set; }
        public bool AIGenerated { get; set; }
        public bool WIP { get; set; }
        public bool NSFW { get; set; }
        public bool IsRemix { get; set; }
        public string? RemixUrl { get; set; }
        public bool IsPublic { get; set; }
        public bool IsFeatured { get; set; }
        public Guid AuthorId { get; set; }
        public ModelAuthorDto? Author { get; set; }
        public List<ModelFileDto> Files { get; set; } = new();
        public List<ModelCategoryDto> Categories { get; set; } = new();
        public List<ModelTagDto> Tags { get; set; } = new();
        public List<ModelVersionDto> Versions { get; set; } = new();
        public List<ModelCommentDto> Comments { get; set; } = new();
        public List<ModelLikeDto> LikeCollection { get; set; } = new();
        public string? RemoteInstanceId { get; set; }
        public string? RemoteModelId { get; set; }
        public Guid? RemoteAuthorId { get; set; }
        public bool IsFederated { get; set; }
        public DateTime? LastFederationSync { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }

    public static class ModelDtoMapper
    {
        public static ModelDto ToDto(Model model)
        {
            return new ModelDto
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                ThumbnailUrl = model.ThumbnailUrl,
                FileUrl = model.FileUrl,
                Downloads = model.Downloads,
                Likes = model.Likes,
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
                Author = ToAuthorDto(model.Author),
                Files = model.Files?.Select(ToFileDto).ToList() ?? new List<ModelFileDto>(),
                Categories = model.Categories?.Select(ToCategoryDto).ToList() ?? new List<ModelCategoryDto>(),
                Tags = model.Tags?.Select(ToTagDto).ToList() ?? new List<ModelTagDto>(),
                Versions = model.Versions?.Select(ToVersionDto).ToList() ?? new List<ModelVersionDto>(),
                Comments = model.Comments?.Select(ToCommentDto).ToList() ?? new List<ModelCommentDto>(),
                LikeCollection = model.LikeCollection?.Select(ToLikeDto).ToList() ?? new List<ModelLikeDto>(),
                RemoteInstanceId = model.RemoteInstanceId,
                RemoteModelId = model.RemoteModelId,
                RemoteAuthorId = model.RemoteAuthorId,
                IsFederated = model.IsFederated,
                LastFederationSync = model.LastFederationSync,
                CreatedAt = model.CreatedAt,
                UpdatedAt = model.UpdatedAt,
                IsDeleted = model.IsDeleted
            };
        }

        public static ModelAuthorDto? ToAuthorDto(User? author)
        {
            if (author == null)
            {
                return null;
            }

            return new ModelAuthorDto
            {
                Id = author.Id,
                Username = author.Username,
                FirstName = author.FirstName,
                LastName = author.LastName,
                Bio = author.Bio,
                Avatar = author.Avatar,
                ProfilePictureUrl = author.ProfilePictureUrl,
                Country = author.Country,
                WebsiteUrl = author.WebsiteUrl,
                TwitterUrl = author.TwitterUrl,
                InstagramUrl = author.InstagramUrl,
                YouTubeUrl = author.YouTubeUrl,
                IsProfilePublic = author.IsProfilePublic,
                CreatedAt = author.CreatedAt
            };
        }

        public static ModelVersionDto ToVersionDto(ModelVersion version)
        {
            return new ModelVersionDto
            {
                Id = version.Id,
                Name = version.Name,
                Notes = version.Notes,
                FileUrl = version.FileUrl,
                ThumbnailUrl = version.ThumbnailUrl,
                VersionNumber = version.VersionNumber,
                ModelId = version.ModelId,
                Files = version.Files?.Select(ToFileDto).ToList() ?? new List<ModelFileDto>(),
                CreatedAt = version.CreatedAt,
                UpdatedAt = version.UpdatedAt
            };
        }

        private static ModelFileDto ToFileDto(ModelFile file)
        {
            return new ModelFileDto
            {
                Id = file.Id,
                ModelId = file.ModelId,
                Name = file.Name,
                Path = file.Path,
                Size = file.Size,
                MimeType = file.MimeType,
                CreatedAt = file.CreatedAt,
                UpdatedAt = file.UpdatedAt
            };
        }

        private static ModelCategoryDto ToCategoryDto(Category category)
        {
            return new ModelCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Icon = category.Icon,
                Color = category.Color
            };
        }

        private static ModelTagDto ToTagDto(Tag tag)
        {
            return new ModelTagDto
            {
                Id = tag.Id,
                Name = tag.Name,
                Color = tag.Color
            };
        }

        private static ModelCommentDto ToCommentDto(Comment comment)
        {
            return new ModelCommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                Likes = comment.Likes,
                Dislikes = comment.Dislikes,
                Author = ToAuthorDto(comment.Author),
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt
            };
        }

        private static ModelLikeDto ToLikeDto(Like like)
        {
            return new ModelLikeDto
            {
                Id = like.Id,
                ModelId = like.ModelId,
                UserId = like.UserId,
                CreatedAt = like.CreatedAt
            };
        }
    }
}

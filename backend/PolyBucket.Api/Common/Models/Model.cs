using PolyBucket.Api.Common.Models.Enums;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;
using PolyBucket.Api.Features.Models.AddTagToModel.Domain;
using PolyBucket.Api.Features.Models.CreateModel.Domain;
using PolyBucket.Api.Features.Models.CreateModelVersion.Domain;
using PolyBucket.Api.Features.Comments.Domain;
using PolyBucket.Api.Features.Models.LikeModel.Domain;
using System;
using System.Collections.Generic;
using PolyBucket.Api.Common.Entities;

namespace PolyBucket.Api.Common.Models;

public class Model : Auditable
{
    public new Guid Id { get; set; }
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
    public User Author { get; set; } = null!;

    public ICollection<ModelFile> Files { get; set; } = new List<ModelFile>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<ModelVersion> Versions { get; set; } = new List<ModelVersion>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Like> LikeCollection { get; set; } = new List<Like>();

    public string? RemoteInstanceId { get; set; }
    public string? RemoteModelId { get; set; }
    public Guid? RemoteAuthorId { get; set; }
    public bool IsFederated { get; set; } = false;
    public DateTime? LastFederationSync { get; set; }
}

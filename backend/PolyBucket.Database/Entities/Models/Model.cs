using PolyBucket.Database.Entities.Models.Enums;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.Comments;
using PolyBucket.Database.Entities.Models;
using System;
using System.Collections.Generic;
using PolyBucket.Database.Common;

namespace PolyBucket.Database.Entities.Models;

public class Model : Auditable
{
    public new Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? FileUrl { get; set; }
    public int Downloads { get; set; }
    public int Views { get; set; }
    public int Likes { get; set; }
    public int Dislikes { get; set; }
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

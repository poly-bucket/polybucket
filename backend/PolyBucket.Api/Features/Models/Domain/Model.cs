using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.Domain.Enums;
using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Models.Domain
{
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
    }
} 
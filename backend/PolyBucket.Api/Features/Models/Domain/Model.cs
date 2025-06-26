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
        public required string Name { get; set; }
        public required string Description { get; set; }
        public Guid UserId { get; set; }
        public LicenseTypes? License { get; set; }
        public PrivacySettings Privacy { get; set; }
        public List<ModelCategories> Categories { get; set; } = new();
        public bool AIGenerated { get; set; }
        public bool WIP { get; set; }
        public bool NSFW { get; set; }
        public bool IsRemix { get; set; }
        public string? RemixUrl { get; set; }
        public required User Author { get; set; }
        public virtual ICollection<ModelFile> Files { get; set; } = new List<ModelFile>();
        public bool IsPublic { get; set; }
        public bool IsFeatured { get; set; }
        public ICollection<Category> CategoryCollection { get; set; } = new List<Category>();
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public ICollection<ModelVersion> Versions { get; set; } = new List<ModelVersion>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public Guid AuthorId { get; set; }
    }
} 
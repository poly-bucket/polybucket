using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Models.Domain.Enums;
using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Models.Domain
{
    public class Model : Auditable
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid UserId { get; set; }
        public LicenseTypes? License { get; set; }
        public PrivacySettings Privacy { get; set; }
        public List<ModelCategories> Categories { get; set; }
        public bool AIGenerated { get; set; }
        public bool WIP { get; set; }
        public bool NSFW { get; set; }
        public bool IsRemix { get; set; }
        public string? RemixUrl { get; set; }
        public virtual User Author { get; set; }
        public virtual ICollection<ModelFile> Files { get; set; } = new List<ModelFile>();
    }
} 
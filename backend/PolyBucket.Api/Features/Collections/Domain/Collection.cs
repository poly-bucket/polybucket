using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using PolyBucket.Api.Features.Models.Shared.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Api.Features.Collections.Domain
{
    public class Collection : Auditable
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public CollectionVisibility Visibility { get; set; } = CollectionVisibility.Private;
        public string? PasswordHash { get; set; }
        public string? Avatar { get; set; }
        public bool Favorite { get; set; } = false;
        
        [Range(0, 999, ErrorMessage = "Display order must be between 0 and 999")]
        public int DisplayOrder { get; set; } = 0;

        public Guid OwnerId { get; set; }
        public virtual User Owner { get; set; } = null!;

        public virtual ICollection<CollectionModel> CollectionModels { get; set; } = new List<CollectionModel>();
    }

    public class CollectionModel
    {
        public Guid CollectionId { get; set; }
        public virtual Collection Collection { get; set; } = null!;

        public Guid ModelId { get; set; }
        public virtual Model Model { get; set; } = null!;
        
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
} 
using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Features.Collections.Domain.Enums;
using PolyBucket.Api.Features.Models.Domain;
using System;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Collections.Domain
{
    public class Collection : Auditable
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public CollectionVisibility Visibility { get; set; } = CollectionVisibility.Private;

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
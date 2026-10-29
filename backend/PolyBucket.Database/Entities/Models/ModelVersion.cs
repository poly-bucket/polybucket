using PolyBucket.Database.Common;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.Models;
using System;
using System.Collections.Generic;

namespace PolyBucket.Database.Entities.Models
{
    public class ModelVersion : Auditable
    {
        public new Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string? FileUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int VersionNumber { get; set; }
        public Guid ModelId { get; set; }
        public Model Model { get; set; } = null!;
        public ICollection<ModelFile> Files { get; set; } = new List<ModelFile>();
    }
}

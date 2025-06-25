using PolyBucket.Api.Common.Entities;
using System;

namespace PolyBucket.Api.Features.Models.Domain
{
    public class ModelFile : Auditable
    {
        public Guid ModelId { get; set; }
        public virtual Model Model { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public string MimeType { get; set; }
    }
} 
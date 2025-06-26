using PolyBucket.Api.Common.Entities;
using System;

namespace PolyBucket.Api.Features.Models.Domain
{
    public class ModelFile : Auditable
    {
        public Guid ModelId { get; set; }
        public required Model Model { get; set; }
        public required string Name { get; set; }
        public required string Path { get; set; }
        public long Size { get; set; }
        public required string MimeType { get; set; }
    }
} 
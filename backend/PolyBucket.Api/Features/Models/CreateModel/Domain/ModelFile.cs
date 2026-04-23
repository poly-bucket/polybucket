using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using System;

namespace PolyBucket.Api.Features.Models.CreateModel.Domain
{
    public class ModelFile : Auditable
    {
        public new Guid Id { get; set; }
        public Model Model { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public long Size { get; set; }
        public string MimeType { get; set; } = string.Empty;
    }
}

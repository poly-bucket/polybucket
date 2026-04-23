using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Models;
using System;

namespace PolyBucket.Api.Features.Models.GenerateModelPreview.Domain
{
    public class ModelPreview : Auditable
    {
        public new Guid Id { get; set; }
        public Guid ModelId { get; set; }
        public Model Model { get; set; } = null!;
        public string Size { get; set; } = string.Empty; // e.g., "thumbnail", "medium", "large"
        public string PreviewUrl { get; set; } = string.Empty;
        public string StorageKey { get; set; } = string.Empty;
        public PreviewStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? GeneratedAt { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public long FileSizeBytes { get; set; }
    }

    public enum PreviewStatus
    {
        Pending,
        Generating,
        Completed,
        Failed
    }
}

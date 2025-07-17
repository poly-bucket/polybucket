using PolyBucket.Api.Features.Models.Domain;
using System;

namespace PolyBucket.Api.Features.Models.GetModelPreview.Domain
{
    public class GetModelPreviewResponse
    {
        public Guid ModelId { get; set; }
        public string Size { get; set; } = string.Empty;
        public string? PreviewUrl { get; set; }
        public PreviewStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? GeneratedAt { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public long FileSizeBytes { get; set; }
        public bool IsAvailable => Status == PreviewStatus.Completed && !string.IsNullOrEmpty(PreviewUrl);
    }
} 
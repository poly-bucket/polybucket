using PolyBucket.Api.Features.Models.Shared.Domain;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using System;

namespace PolyBucket.Api.Features.Models.GenerateModelPreview.Domain
{
    public class GenerateModelPreviewResponse
    {
        public Guid ModelId { get; set; }
        public string Size { get; set; } = string.Empty;
        public PreviewStatus Status { get; set; }
        public string? Message { get; set; }
        public bool IsQueued { get; set; }
        public DateTime QueuedAt { get; set; }
    }
} 
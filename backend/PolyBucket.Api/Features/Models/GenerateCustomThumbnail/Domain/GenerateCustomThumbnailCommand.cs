using MediatR;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Services;
using System;

namespace PolyBucket.Api.Features.Models.GenerateCustomThumbnail.Domain
{
    public class GenerateCustomThumbnailCommand : IRequest<GenerateCustomThumbnailResponse>
    {
        public Guid ModelId { get; set; }
        public string ModelFileUrl { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string Size { get; set; } = "thumbnail";
        public PreviewGenerationSettings Settings { get; set; } = new();
        public bool ForceRegenerate { get; set; } = false;
    }

    public class GenerateCustomThumbnailResponse
    {
        public Guid ModelId { get; set; }
        public string Size { get; set; } = string.Empty;
        public string PreviewUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsQueued { get; set; }
        public DateTime QueuedAt { get; set; }
    }
} 
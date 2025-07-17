using MediatR;
using System;

namespace PolyBucket.Api.Features.Models.GenerateModelPreview.Domain
{
    public class GenerateModelPreviewCommand : IRequest<GenerateModelPreviewResponse>
    {
        public Guid ModelId { get; set; }
        public string Size { get; set; } = "thumbnail";
        public bool ForceRegenerate { get; set; } = false;
    }
} 
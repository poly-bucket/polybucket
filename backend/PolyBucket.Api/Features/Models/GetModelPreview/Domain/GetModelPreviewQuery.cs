using MediatR;
using System;

namespace PolyBucket.Api.Features.Models.GetModelPreview.Domain
{
    public class GetModelPreviewQuery : IRequest<GetModelPreviewResponse>
    {
        public Guid ModelId { get; set; }
        public string Size { get; set; } = "thumbnail"; // Default to thumbnail size
    }
} 
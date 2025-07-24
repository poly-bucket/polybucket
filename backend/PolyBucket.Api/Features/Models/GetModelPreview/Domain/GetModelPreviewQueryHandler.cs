using MediatR;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.GetModelPreview.Repository;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GetModelPreview.Domain
{
    public class GetModelPreviewQueryHandler(
        IModelPreviewRepository previewRepository,
        ILogger<GetModelPreviewQueryHandler> logger) : IRequestHandler<GetModelPreviewQuery, GetModelPreviewResponse>
    {
        private readonly IModelPreviewRepository _previewRepository = previewRepository;
        private readonly ILogger<GetModelPreviewQueryHandler> _logger = logger;

        public async Task<GetModelPreviewResponse> Handle(GetModelPreviewQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting preview for model {ModelId} with size {Size}", request.ModelId, request.Size);

            var preview = await _previewRepository.GetPreviewAsync(request.ModelId, request.Size);

            if (preview == null)
            {
                _logger.LogWarning("No preview found for model {ModelId} with size {Size}", request.ModelId, request.Size);
                return new GetModelPreviewResponse
                {
                    ModelId = request.ModelId,
                    Size = request.Size,
                    Status = PreviewStatus.Pending
                };
            }

            return new GetModelPreviewResponse
            {
                ModelId = preview.ModelId,
                Size = preview.Size,
                PreviewUrl = preview.PreviewUrl,
                Status = preview.Status,
                ErrorMessage = preview.ErrorMessage,
                GeneratedAt = preview.GeneratedAt,
                Width = preview.Width,
                Height = preview.Height,
                FileSizeBytes = preview.FileSizeBytes
            };
        }
    }
} 
using MediatR;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Repository;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GenerateModelPreview.Domain
{
    public class GenerateModelPreviewCommandHandler : IRequestHandler<GenerateModelPreviewCommand, GenerateModelPreviewResponse>
    {
        private readonly IGenerateModelPreviewRepository _previewRepository;
        private readonly ILogger<GenerateModelPreviewCommandHandler> _logger;

        public GenerateModelPreviewCommandHandler(
            IGenerateModelPreviewRepository previewRepository,
            ILogger<GenerateModelPreviewCommandHandler> logger)
        {
            _previewRepository = previewRepository;
            _logger = logger;
        }

        public async Task<GenerateModelPreviewResponse> Handle(GenerateModelPreviewCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Generating preview for model {ModelId} with size {Size}", request.ModelId, request.Size);

            // Check if preview already exists and is completed
            var existingPreview = await _previewRepository.GetPreviewAsync(request.ModelId, request.Size);
            
            if (existingPreview != null && existingPreview.Status == PreviewStatus.Completed && !request.ForceRegenerate)
            {
                _logger.LogInformation("Preview already exists and is completed for model {ModelId} with size {Size}", request.ModelId, request.Size);
                return new GenerateModelPreviewResponse
                {
                    ModelId = request.ModelId,
                    Size = request.Size,
                    Status = PreviewStatus.Completed,
                    Message = "Preview already exists",
                    IsQueued = false,
                    QueuedAt = DateTime.UtcNow
                };
            }

            // Create or update preview record
            var preview = existingPreview ?? new ModelPreview
            {
                Id = Guid.NewGuid(),
                ModelId = request.ModelId,
                Size = request.Size,
                Status = PreviewStatus.Pending
            };

            if (request.ForceRegenerate || existingPreview == null)
            {
                preview.Status = PreviewStatus.Pending;
                preview.ErrorMessage = null;
                preview.GeneratedAt = null;
            }

            if (existingPreview == null)
            {
                await _previewRepository.CreatePreviewAsync(preview);
            }
            else
            {
                await _previewRepository.UpdatePreviewAsync(preview);
            }

            // TODO: Queue background job for actual preview generation
            // This would integrate with Hangfire or similar background job system
            _logger.LogInformation("Preview generation queued for model {ModelId} with size {Size}", request.ModelId, request.Size);

            return new GenerateModelPreviewResponse
            {
                ModelId = request.ModelId,
                Size = request.Size,
                Status = preview.Status,
                Message = "Preview generation queued",
                IsQueued = true,
                QueuedAt = DateTime.UtcNow
            };
        }
    }
} 
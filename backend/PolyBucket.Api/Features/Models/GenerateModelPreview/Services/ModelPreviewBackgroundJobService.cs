using Hangfire;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Repository;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.GenerateModelPreview.Services
{
    public class ModelPreviewBackgroundJobService
    {
        private readonly IModelPreviewGenerationService _previewGenerationService;
        private readonly IGenerateModelPreviewRepository _previewRepository;
        private readonly ILogger<ModelPreviewBackgroundJobService> _logger;

        public ModelPreviewBackgroundJobService(
            IModelPreviewGenerationService previewGenerationService,
            IGenerateModelPreviewRepository previewRepository,
            ILogger<ModelPreviewBackgroundJobService> logger)
        {
            _previewGenerationService = previewGenerationService;
            _previewRepository = previewRepository;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task GenerateModelPreviewAsync(
            Guid modelId,
            string modelFileUrl,
            string fileType,
            string size,
            PreviewGenerationSettings settings)
        {
            _logger.LogInformation("Starting background preview generation for model {ModelId} with size {Size}", modelId, size);

            try
            {
                // Update status to generating
                var preview = await _previewRepository.GetPreviewAsync(modelId, size);
                if (preview != null)
                {
                    preview.Status = PreviewStatus.Generating;
                    await _previewRepository.UpdatePreviewAsync(preview);
                }

                // Generate the preview
                var result = await _previewGenerationService.GeneratePreviewAsync(
                    modelId, modelFileUrl, fileType, size, settings);

                // Update the preview record with results
                if (preview != null)
                {
                    preview.Status = result.Status;
                    preview.PreviewUrl = result.PreviewUrl;
                    preview.StorageKey = result.StorageKey;
                    preview.ErrorMessage = result.ErrorMessage;
                    preview.GeneratedAt = result.GeneratedAt;
                    preview.Width = result.Width;
                    preview.Height = result.Height;
                    preview.FileSizeBytes = result.FileSizeBytes;

                    await _previewRepository.UpdatePreviewAsync(preview);
                }

                _logger.LogInformation("Background preview generation completed for model {ModelId} with size {Size}", modelId, size);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background preview generation for model {ModelId} with size {Size}", modelId, size);
                
                // Update status to failed
                var preview = await _previewRepository.GetPreviewAsync(modelId, size);
                if (preview != null)
                {
                    preview.Status = PreviewStatus.Failed;
                    preview.ErrorMessage = ex.Message;
                    await _previewRepository.UpdatePreviewAsync(preview);
                }
                
                throw;
            }
        }
    }
} 
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Features.Models.Shared.Domain;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.CreateModel.Services
{
    public class ModelUploadService
    {
        private readonly IStorageService _storageService;
        private readonly IModelPreviewGenerationService _previewGenerationService;
        private readonly ILogger<ModelUploadService> _logger;

        public ModelUploadService(
            IStorageService storageService,
            IModelPreviewGenerationService previewGenerationService,
            ILogger<ModelUploadService> logger)
        {
            _storageService = storageService;
            _previewGenerationService = previewGenerationService;
            _logger = logger;
        }

        public async Task<Model> ProcessModelUploadAsync(
            Model model,
            CancellationToken cancellationToken = default)
        {
            // Check if model has a thumbnail
            if (string.IsNullOrEmpty(model.ThumbnailUrl))
            {
                await GenerateAutomaticThumbnailAsync(model, cancellationToken);
            }

            return model;
        }

        private async Task GenerateAutomaticThumbnailAsync(Model model, CancellationToken cancellationToken)
        {
            try
            {
                // Find the first 3D model file
                var modelFile = model.Files?.FirstOrDefault(f => Is3DModelFile(f.Name));
                if (modelFile == null)
                {
                    _logger.LogWarning("No 3D model file found for automatic thumbnail generation for model {ModelId}", model.Id);
                    return;
                }

                // Check if file type is supported
                var fileType = GetFileExtension(modelFile.Name);
                if (!await _previewGenerationService.IsSupportedFileTypeAsync(fileType))
                {
                    _logger.LogWarning("File type {FileType} is not supported for automatic thumbnail generation for model {ModelId}", fileType, model.Id);
                    return;
                }

                // Get presigned URL for the model file
                var modelFileUrl = await _storageService.GetPresignedUrlAsync(modelFile.Path, TimeSpan.FromHours(1), cancellationToken);

                // Generate thumbnail with default settings
                var settings = new PreviewGenerationSettings
                {
                    Width = 800,
                    Height = 600,
                    BackgroundColor = "#1a1a1a",
                    ModelColor = "#888888",
                    Metalness = 0.5,
                    Roughness = 0.5,
                    AutoRotate = false,
                    CameraDistance = 2.5,
                    Lighting = "studio",
                    ViewMode = "solid",
                    LightIntensity = 1.0,
                    LightColor = "#ffffff"
                };

                var preview = await _previewGenerationService.GeneratePreviewAsync(
                    model.Id,
                    modelFileUrl,
                    fileType,
                    "thumbnail",
                    settings,
                    cancellationToken);

                if (preview.Status == PreviewStatus.Completed)
                {
                    model.ThumbnailUrl = preview.PreviewUrl;
                    _logger.LogInformation("Automatic thumbnail generated successfully for model {ModelId}", model.Id);
                }
                else
                {
                    _logger.LogWarning("Failed to generate automatic thumbnail for model {ModelId}: {Error}", model.Id, preview.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating automatic thumbnail for model {ModelId}", model.Id);
            }
        }

        private bool Is3DModelFile(string fileName)
        {
            var extension = GetFileExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".stl" or ".obj" or ".fbx" or ".gltf" or ".glb" or ".ply" or ".3mf" or ".step" or ".stp" => true,
                _ => false
            };
        }

        private string GetFileExtension(string fileName)
        {
            var lastDotIndex = fileName.LastIndexOf('.');
            return lastDotIndex >= 0 ? fileName.Substring(lastDotIndex) : string.Empty;
        }
    }
} 
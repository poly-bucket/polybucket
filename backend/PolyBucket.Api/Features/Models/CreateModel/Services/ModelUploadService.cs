using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Models;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Services;

namespace PolyBucket.Api.Features.Models.CreateModel.Services;

public class ModelUploadService(
    IStorageService storageService,
    IStorageObjectKeyResolver objectKeyResolver,
    IModelPreviewGenerationService previewGenerationService,
    ILogger<ModelUploadService> logger) : IModelUploadService
{
    public async Task<Model> ProcessModelUploadAsync(
        Model model,
        CancellationToken cancellationToken = default)
    {
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
            var modelFile = model.Files?.FirstOrDefault(f => Is3DModelFile(f.Name));
            if (modelFile == null)
            {
                logger.LogWarning("No 3D model file found for automatic thumbnail generation for model {ModelId}", model.Id);
                return;
            }

            var fileType = GetFileExtension(modelFile.Name);
            if (!await previewGenerationService.IsSupportedFileTypeAsync(fileType))
            {
                logger.LogWarning(
                    "File type {FileType} is not supported for automatic thumbnail generation for model {ModelId}",
                    fileType,
                    model.Id);
                return;
            }

            var objectKey = objectKeyResolver.Resolve(modelFile.Path);
            if (string.IsNullOrEmpty(objectKey))
            {
                logger.LogWarning("Could not resolve object key for model file path on model {ModelId}", model.Id);
                return;
            }

            var modelFileUrl = await storageService.GetPresignedUrlAsync(
                objectKey,
                StorageAccessDurations.ThumbnailSourceUrl,
                cancellationToken);

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

            var preview = await previewGenerationService.GeneratePreviewAsync(
                model.Id,
                modelFileUrl,
                fileType,
                "thumbnail",
                settings,
                cancellationToken);

            if (preview.Status == PreviewStatus.Completed)
            {
                model.ThumbnailUrl = preview.PreviewUrl;
                logger.LogInformation("Automatic thumbnail generated successfully for model {ModelId}", model.Id);
            }
            else
            {
                logger.LogWarning(
                    "Failed to generate automatic thumbnail for model {ModelId}: {Error}",
                    model.Id,
                    preview.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating automatic thumbnail for model {ModelId}", model.Id);
        }
    }

    private static bool Is3DModelFile(string fileName)
    {
        var extension = GetFileExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".stl" or ".obj" or ".fbx" or ".gltf" or ".glb" or ".ply" or ".3mf" or ".step" or ".stp" => true,
            _ => false
        };
    }

    private static string GetFileExtension(string fileName)
    {
        var lastDotIndex = fileName.LastIndexOf('.');
        return lastDotIndex >= 0 ? fileName[lastDotIndex..] : string.Empty;
    }
}

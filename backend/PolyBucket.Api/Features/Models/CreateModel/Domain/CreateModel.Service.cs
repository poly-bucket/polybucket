using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Features.Models.CreateModel.Http;
using PolyBucket.Api.Features.Models.CreateModel.Repository;
using PolyBucket.Api.Features.Models.Shared.Domain;
using PolyBucket.Api.Features.Models.Shared.Domain.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.CreateModel.Domain
{
    public class CreateModelService
    {
        private readonly ICreateModelRepository _repository;
        private readonly IStorageService _storage;
        private readonly ILogger<CreateModelService> _logger;

        private static readonly string[] Supported3DFormats = { ".stl", ".obj", ".fbx", ".gltf", ".glb", ".3mf", ".step", ".stp" };
        private static readonly string[] SupportedImageFormats = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
        private static readonly string[] SupportedDocumentFormats = { ".pdf", ".md", ".markdown", ".txt" };
        private static readonly string[] AllSupportedFormats = Supported3DFormats.Concat(SupportedImageFormats).Concat(SupportedDocumentFormats).ToArray();

        private const long MaxFileSize = 100 * 1024 * 1024; // 100MB
        private const long Max3DModelSize = 500 * 1024 * 1024; // 500MB for 3D models

        public CreateModelService(ICreateModelRepository repository, IStorageService storage, ILogger<CreateModelService> logger)
        {
            _repository = repository;
            _storage = storage;
            _logger = logger;
        }

        public async Task<CreateModelResponse> CreateModelAsync(CreateModelRequest request, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            ValidateRequest(request);
            
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var authorId))
            {
                throw new ValidationException("Invalid authentication token");
            }

            var modelId = Guid.NewGuid();
            var modelFiles = new List<ModelFile>();
            string? thumbnailObjectKey = null;

            try
            {
                // Upload all files
                foreach (var file in request.Files)
                {
                    if (file.Length == 0) continue;

                    var objectKey = $"models/{modelId}/{file.FileName}";
                    await using var stream = file.OpenReadStream();
                    await _storage.UploadAsync(objectKey, stream, file.ContentType, cancellationToken);

                    var modelFile = new ModelFile
                    {
                        Id = Guid.NewGuid(),
                        Name = file.FileName,
                        Path = objectKey, // Store the object key, not the presigned URL
                        Size = file.Length,
                        MimeType = file.ContentType,
                        CreatedAt = DateTime.UtcNow,
                        CreatedById = authorId,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedById = authorId
                    };

                    modelFiles.Add(modelFile);

                    // Check if this file should be the thumbnail - store object key, not presigned URL
                    if (request.ThumbnailFileId != null && file.FileName.Contains(request.ThumbnailFileId))
                    {
                        thumbnailObjectKey = objectKey;
                    }
                }

                // If no thumbnail was specified, prefer image files, then fallback to first 3D model
                if (thumbnailObjectKey == null)
                {
                    var imageFile = modelFiles.FirstOrDefault(f => IsImageFile(f.Name));
                    if (imageFile != null)
                    {
                        thumbnailObjectKey = imageFile.Path;
                    }
                    else
                    {
                        var first3DModel = modelFiles.FirstOrDefault(f => Is3DModelFile(f.Name));
                        if (first3DModel != null)
                        {
                            thumbnailObjectKey = first3DModel.Path;
                        }
                    }
                }

                // Create model entity - store object keys, not presigned URLs
                var model = new Model
                {
                    Id = modelId,
                    Name = request.Name,
                    Description = request.Description ?? string.Empty,
                    FileUrl = modelFiles.First().Path, // Store object key
                    ThumbnailUrl = thumbnailObjectKey, // Store object key
                    Privacy = request.Privacy == "private" ? PrivacySettings.Private : PrivacySettings.Public,
                    License = ParseLicense(request.License),
                    AIGenerated = request.AIGenerated,
                    WIP = request.WorkInProgress,
                    NSFW = request.NSFW,
                    IsRemix = request.Remix,
                    IsPublic = request.Privacy != "private",
                    AuthorId = authorId,
                    Files = modelFiles,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = authorId,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = authorId
                };

                // Save model
                await _repository.CreateModelAsync(model, cancellationToken);

                _logger.LogInformation("Successfully created model {ModelId} with {FileCount} files for user {UserId}", 
                    modelId, modelFiles.Count, authorId);

                return new CreateModelResponse { Model = model };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create model for user {UserId}", authorId);
                
                // Clean up uploaded files on error
                foreach (var modelFile in modelFiles)
                {
                    try
                    {
                        await _storage.DeleteAsync(modelFile.Path, cancellationToken);
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogWarning(deleteEx, "Failed to clean up file {FilePath} after upload error", modelFile.Path);
                    }
                }

                throw;
            }
        }

        private void ValidateRequest(CreateModelRequest request)
        {
            if (request.Files == null || request.Files.Length == 0)
            {
                throw new ValidationException("At least one file is required");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ValidationException("Model name is required");
            }

            var validationError = ValidateFiles(request.Files);
            if (validationError != null)
            {
                throw new ValidationException(validationError);
            }
        }

        private string? ValidateFiles(IFormFile[] files)
        {
            if (files.Length > 20)
            {
                return "Too many files. Maximum of 20 files allowed per upload.";
            }

            var has3DModel = false;
            var totalSize = 0L;

            foreach (var file in files)
            {
                if (file.Length == 0)
                    continue;

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllSupportedFormats.Contains(extension))
                {
                    return $"Unsupported file type: {extension}. Supported formats: {string.Join(", ", AllSupportedFormats)}";
                }

                var maxSize = Is3DModelFile(file.FileName) ? Max3DModelSize : MaxFileSize;
                if (file.Length > maxSize)
                {
                    var maxSizeMB = maxSize / (1024 * 1024);
                    return $"File {file.FileName} is too large. Maximum size: {maxSizeMB}MB";
                }

                if (Is3DModelFile(file.FileName))
                {
                    has3DModel = true;
                }

                totalSize += file.Length;
            }

            if (!has3DModel)
            {
                return "At least one 3D model file is required (.stl, .obj, .fbx, .gltf, .glb, .3mf, .step, .stp)";
            }

            if (totalSize > 1024 * 1024 * 1024)
            {
                return "Total upload size exceeds 1GB limit";
            }

            return null;
        }

        private static bool Is3DModelFile(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return Supported3DFormats.Contains(extension);
        }

        private static bool IsImageFile(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return SupportedImageFormats.Contains(extension);
        }

        private static LicenseTypes? ParseLicense(string? license)
        {
            return license?.ToLower() switch
            {
                "mit" => LicenseTypes.MIT,
                "gpl" => LicenseTypes.GPLv3,
                "creative commons" => LicenseTypes.CCBy4,
                "commercial" => LicenseTypes.MIT,
                "custom" => LicenseTypes.MIT,
                _ => null
            };
        }
    }

    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
} 
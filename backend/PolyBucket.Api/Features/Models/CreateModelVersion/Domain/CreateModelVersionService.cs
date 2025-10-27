using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Models.CreateModelVersion.Http;
using PolyBucket.Api.Features.Models.CreateModelVersion.Repository;
using PolyBucket.Api.Features.Models.Shared.Domain;
using PolyBucket.Api.Features.Models.CreateModel.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.CreateModelVersion.Domain
{
    public class CreateModelVersionService : ICreateModelVersionService
    {
        private readonly ICreateModelVersionRepository _repository;
        private readonly IStorageService _storage;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<CreateModelVersionService> _logger;

        private static readonly string[] Supported3DFormats = { ".stl", ".obj", ".fbx", ".gltf", ".glb", ".3mf", ".step", ".stp" };
        private static readonly string[] SupportedImageFormats = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
        private static readonly string[] SupportedDocumentFormats = { ".pdf", ".md", ".markdown", ".txt" };
        private static readonly string[] AllSupportedFormats = Supported3DFormats.Concat(SupportedImageFormats).Concat(SupportedDocumentFormats).ToArray();

        private const long MaxFileSize = 100 * 1024 * 1024; // 100MB
        private const long Max3DModelSize = 500 * 1024 * 1024; // 500MB for 3D models

        public CreateModelVersionService(ICreateModelVersionRepository repository, IStorageService storage, IPermissionService permissionService, ILogger<CreateModelVersionService> logger)
        {
            _repository = repository;
            _storage = storage;
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task<CreateModelVersionResponse> CreateModelVersionAsync(Guid modelId, CreateModelVersionRequest request, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            ValidateRequest(request);
            
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new ValidationException("Invalid authentication token");
            }

            var model = await _repository.GetModelByIdAsync(modelId, cancellationToken);
            if (model == null)
            {
                throw new ModelNotFoundException($"Model with ID {modelId} not found");
            }

            if (model.IsDeleted)
            {
                throw new ValidationException("Cannot create version for a deleted model");
            }

            // Check ownership - user can only create versions for their own models unless they have MODEL_EDIT_ANY permission
            var hasAnyPermission = await _permissionService.HasPermissionAsync(userId, PermissionConstants.MODEL_EDIT_ANY);
            if (!hasAnyPermission && model.AuthorId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to create versions for this model");
            }

            var versionId = Guid.NewGuid();
            var versionFiles = new List<ModelFile>();
            string? thumbnailObjectKey = null;

            try
            {
                // Upload all files
                foreach (var file in request.Files)
                {
                    if (file.Length == 0) continue;

                    var objectKey = $"models/{model.Id}/versions/{versionId}/{Guid.NewGuid()}_{file.FileName}";
                    await using var stream = file.OpenReadStream();
                    await _storage.UploadAsync(objectKey, stream, file.ContentType, cancellationToken);

                    var modelFile = new ModelFile
                    {
                        Id = Guid.NewGuid(),
                        Name = file.FileName,
                        Path = objectKey, // Store the object key, not the presigned URL
                        Size = file.Length,
                        MimeType = file.ContentType
                    };

                    versionFiles.Add(modelFile);

                    // Check if this file should be the thumbnail - store object key, not presigned URL
                    if (request.ThumbnailFileId != null && file.FileName.Contains(request.ThumbnailFileId))
                    {
                        thumbnailObjectKey = objectKey;
                    }
                }

                // If no thumbnail was specified, prefer image files, then fallback to first 3D model
                if (thumbnailObjectKey == null)
                {
                    var imageFile = versionFiles.FirstOrDefault(f => IsImageFile(f.Name));
                    if (imageFile != null)
                    {
                        thumbnailObjectKey = imageFile.Path;
                    }
                    else
                    {
                        var first3DModel = versionFiles.FirstOrDefault(f => Is3DModelFile(f.Name));
                        thumbnailObjectKey = first3DModel?.Path;
                    }
                }

                // Get next version number
                var nextVersionNumber = await _repository.GetNextVersionNumberAsync(modelId, cancellationToken);

                // Create version entity - store object keys, not presigned URLs
                var version = new ModelVersion
                {
                    Id = versionId,
                    Name = request.Name,
                    Notes = request.Notes ?? string.Empty,
                    FileUrl = versionFiles.First().Path,
                    ThumbnailUrl = thumbnailObjectKey,
                    VersionNumber = nextVersionNumber,
                    ModelId = model.Id,
                    Model = model,
                    Files = versionFiles,
                    CreatedById = userId
                };

                // Save version
                await _repository.CreateModelVersionAsync(version, cancellationToken);

                _logger.LogInformation("Successfully created version {VersionId} for model {ModelId} with {FileCount} files by user {UserId}", 
                    versionId, model.Id, versionFiles.Count, userId);

                return new CreateModelVersionResponse { ModelVersion = version };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create version for model {ModelId} by user {UserId}", model.Id, userId);
                
                // Clean up uploaded files on error
                foreach (var modelFile in versionFiles)
                {
                    try
                    {
                        await _storage.DeleteAsync(modelFile.Path, cancellationToken);
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogWarning(deleteEx, "Failed to clean up file {FilePath} after version creation error", modelFile.Path);
                    }
                }

                throw;
            }
        }

        private void ValidateRequest(CreateModelVersionRequest request)
        {
            if (request.Files == null || request.Files.Length == 0)
            {
                throw new ValidationException("At least one file is required");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ValidationException("Version name is required");
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
                return "Too many files. Maximum of 20 files allowed per version.";
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
    }

    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }

    public class ModelNotFoundException : Exception
    {
        public ModelNotFoundException(string message) : base(message) { }
    }
} 
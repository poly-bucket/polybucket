using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Models.CreateModelVersion.Http;
using PolyBucket.Api.Features.Models.CreateModelVersion.Repository;
using PolyBucket.Api.Features.Models.Common;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Api.Features.Models.CreateModel.Domain;
using PolyBucket.Api.Features.Models.UploadValidation.Domain;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.CreateModelVersion.Domain
{
    public class CreateModelVersionService : ICreateModelVersionService
    {
        private readonly ICreateModelVersionRepository _repository;
        private readonly IStorageService _storage;
        private readonly IPermissionService _permissionService;
        private readonly IModelPreviewQueue _previewQueue;
        private readonly IModelUploadValidator _uploadValidator;
        private readonly ISiteUploadCompressionPolicy _compressionPolicy;
        private readonly ILogger<CreateModelVersionService> _logger;

        private static readonly string[] SupportedImageFormats = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };

        public CreateModelVersionService(
            ICreateModelVersionRepository repository,
            IStorageService storage,
            IPermissionService permissionService,
            IModelPreviewQueue previewQueue,
            IModelUploadValidator uploadValidator,
            ISiteUploadCompressionPolicy compressionPolicy,
            ILogger<CreateModelVersionService> logger)
        {
            _repository = repository;
            _storage = storage;
            _permissionService = permissionService;
            _previewQueue = previewQueue;
            _uploadValidator = uploadValidator;
            _compressionPolicy = compressionPolicy;
            _logger = logger;
        }

        public async Task<CreateModelVersionResponse> CreateModelVersionAsync(Guid modelId, CreateModelVersionRequest request, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            var userIdClaim = user.FindUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
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

            var hasAnyPermission = await _permissionService.HasPermissionAsync(userId, PermissionConstants.MODEL_EDIT_ANY);
            if (!hasAnyPermission && model.AuthorId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to create versions for this model");
            }

            await ValidateRequestAsync(request, cancellationToken);

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

                    string storedPath;
                    long storedSize;
                    if (await _compressionPolicy.ShouldCompressAsync(file.FileName, cancellationToken))
                    {
                        (storedPath, storedSize) = await ModelStorageContentHelper.CompressAndUploadAsync(
                            _storage,
                            objectKey,
                            stream,
                            file.ContentType,
                            cancellationToken);
                    }
                    else
                    {
                        await _storage.UploadAsync(objectKey, stream, file.ContentType, cancellationToken);
                        storedPath = objectKey;
                        storedSize = file.Length;
                    }

                    var modelFile = new ModelFile
                    {
                        Id = Guid.NewGuid(),
                        ModelId = model.Id,
                        Name = file.FileName,
                        Path = storedPath,
                        Size = storedSize,
                        MimeType = file.ContentType
                    };

                    versionFiles.Add(modelFile);

                    // Check if this file should be the thumbnail - only images are valid thumbnails
                    if (request.ThumbnailFileId != null && file.FileName.Contains(request.ThumbnailFileId) && IsImageFile(file.FileName))
                    {
                        thumbnailObjectKey = storedPath;
                    }
                }

                // If no thumbnail was specified, fall back to the first image file.
                // 3D mesh files are never used as thumbnails; the client renders the model instead.
                if (thumbnailObjectKey == null)
                {
                    var imageFile = versionFiles.FirstOrDefault(f => IsImageFile(f.Name));
                    if (imageFile != null)
                    {
                        thumbnailObjectKey = imageFile.Path;
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
                await _previewQueue.EnqueueForNewContentAsync(model.Id, CancellationToken.None);

                _logger.LogInformation("Successfully created version {VersionId} for model {ModelId} with {FileCount} files by user {UserId}", 
                    versionId, model.Id, versionFiles.Count, userId);

                return new CreateModelVersionResponse { ModelVersion = ModelDtoMapper.ToVersionDto(version) };
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

        private async Task ValidateRequestAsync(CreateModelVersionRequest request, CancellationToken cancellationToken)
        {
            if (request.Files == null || request.Files.Length == 0)
            {
                throw new ValidationException("At least one file is required");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ValidationException("Version name is required");
            }

            var validationError = await _uploadValidator.ValidateAsync(request.Files, cancellationToken);
            if (validationError != null)
            {
                throw new ValidationException(validationError);
            }
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
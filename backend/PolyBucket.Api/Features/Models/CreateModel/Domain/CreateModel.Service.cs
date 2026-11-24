using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Services;
using PolyBucket.Api.Features.Models.CreateModel.Http;
using PolyBucket.Api.Features.Models.CreateModel.Repository;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.Models.GenerateModelPreview.Domain;
using PolyBucket.Api.Features.Models.UpdateModel.Domain;
using PolyBucket.Api.Features.Models.Common;
using PolyBucket.Api.Features.Models.UploadValidation.Domain;
using PolyBucket.Api.Features.Models.Upload;
using PolyBucket.Api.Features.SystemSettings.Services;
using PolyBucket.Database.Entities.Models;
using PolyBucket.Database.Entities.Models.Enums;
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
        private readonly IModelModerationEnqueueService _moderationEnqueueService;
        private readonly IModelPreviewQueue _previewQueue;
        private readonly IModelUploadValidator _uploadValidator;
        private readonly ISiteModelUploadPolicy _uploadPolicy;
        private readonly IAnonymousUploadAuthorProvider _anonymousUploadAuthorProvider;
        private readonly ILogger<CreateModelService> _logger;

        private static readonly string[] SupportedImageFormats = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };

        public CreateModelService(
            ICreateModelRepository repository,
            IStorageService storage,
            IModelModerationEnqueueService moderationEnqueueService,
            IModelPreviewQueue previewQueue,
            IModelUploadValidator uploadValidator,
            ISiteModelUploadPolicy uploadPolicy,
            IAnonymousUploadAuthorProvider anonymousUploadAuthorProvider,
            ILogger<CreateModelService> logger)
        {
            _repository = repository;
            _storage = storage;
            _moderationEnqueueService = moderationEnqueueService;
            _previewQueue = previewQueue;
            _uploadValidator = uploadValidator;
            _uploadPolicy = uploadPolicy;
            _anonymousUploadAuthorProvider = anonymousUploadAuthorProvider;
            _logger = logger;
        }

        public async Task<CreateModelResponse> CreateModelAsync(CreateModelRequest request, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            await ValidateRequestAsync(request, cancellationToken);

            var authorId = await ResolveAuthorIdAsync(user, cancellationToken);
            var defaultPrivacy = await _uploadPolicy.GetDefaultModelPrivacyAsync(cancellationToken);
            var resolvedPrivacy = ModelPrivacyResolver.Resolve(request.Privacy, defaultPrivacy);
            var isPublic = ModelPrivacyResolver.ComputeIsPublic(resolvedPrivacy);

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
                        ModelId = modelId,
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

                    // Check if this file should be the thumbnail - only images are valid thumbnails
                    if (request.ThumbnailFileId != null && file.FileName.Contains(request.ThumbnailFileId) && IsImageFile(file.FileName))
                    {
                        thumbnailObjectKey = objectKey;
                    }
                }

                // If no thumbnail was specified, fall back to the first image file.
                // 3D mesh files are never used as thumbnails; the client renders the model instead.
                if (thumbnailObjectKey == null)
                {
                    var imageFile = modelFiles.FirstOrDefault(f => IsImageFile(f.Name));
                    if (imageFile != null)
                    {
                        thumbnailObjectKey = imageFile.Path;
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
                    Privacy = resolvedPrivacy,
                    License = ParseLicense(request.License),
                    AIGenerated = request.AIGenerated,
                    WIP = request.WorkInProgress,
                    NSFW = request.NSFW,
                    IsRemix = request.Remix,
                    IsPublic = isPublic,
                    AuthorId = authorId,
                    Files = modelFiles,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = authorId,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedById = authorId
                };

                // Save model
                await _repository.CreateModelAsync(model, cancellationToken);
                await _moderationEnqueueService.ApplyModerationStateForNewModelAsync(model, authorId, cancellationToken);
                await _previewQueue.EnqueueForNewContentAsync(modelId, CancellationToken.None);

                _logger.LogInformation("Successfully created model {ModelId} with {FileCount} files for user {UserId}", 
                    modelId, modelFiles.Count, authorId);

                return new CreateModelResponse { Model = ModelDtoMapper.ToDto(model) };
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

        private async Task ValidateRequestAsync(CreateModelRequest request, CancellationToken cancellationToken)
        {
            if (request.Files == null || request.Files.Length == 0)
            {
                throw new ValidationException("At least one file is required");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ValidationException("Model name is required");
            }

            var validationError = await _uploadValidator.ValidateAsync(request.Files, cancellationToken);
            if (validationError != null)
            {
                throw new ValidationException(validationError);
            }
        }

        private async Task<Guid> ResolveAuthorIdAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            var userIdClaim = user.FindUserIdClaim();
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var authorId))
            {
                return authorId;
            }

            if (await _uploadPolicy.IsAnonymousModelUploadAllowedAsync(cancellationToken))
            {
                return await _anonymousUploadAuthorProvider.GetAuthorIdAsync(cancellationToken);
            }

            throw new ValidationException("Authentication required to upload models");
        }

        private static bool IsImageFile(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return SupportedImageFormats.Contains(extension);
        }

        private static LicenseTypes? ParseLicense(string? license)
        {
            return license?.Trim().ToLowerInvariant() switch
            {
                "mit" => LicenseTypes.MIT,
                "gpl" => LicenseTypes.GPLv3,
                "creative commons" or "ccby4" or "cc-by-4.0" => LicenseTypes.CCBy4,
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
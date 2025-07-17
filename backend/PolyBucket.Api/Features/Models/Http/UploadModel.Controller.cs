using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.Domain.Enums;
using System.Linq;
using System.Threading;
using System.Security.Claims;
using System.IO;

namespace PolyBucket.Api.Features.Models.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    public class UploadModelController : ControllerBase
    {
        private readonly IStorageService _storage;
        private readonly PolyBucketDbContext _db;
        private readonly ILogger<UploadModelController> _logger;

        // Supported file extensions
        private static readonly string[] Supported3DFormats = { ".stl", ".obj", ".fbx", ".gltf", ".glb" };
        private static readonly string[] SupportedImageFormats = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
        private static readonly string[] AllSupportedFormats = Supported3DFormats.Concat(SupportedImageFormats).ToArray();

        // File size limits (in bytes)
        private const long MaxFileSize = 100 * 1024 * 1024; // 100MB
        private const long Max3DModelSize = 500 * 1024 * 1024; // 500MB for 3D models

        public UploadModelController(IStorageService storage, PolyBucketDbContext db, ILogger<UploadModelController> logger)
        {
            _storage = storage;
            _db = db;
            _logger = logger;
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(Model), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Model>> UploadModel([FromForm] ModelUploadRequest request, CancellationToken cancellationToken)
        {
            // Validate basic request
            if (request.Files == null || request.Files.Length == 0)
            {
                return BadRequest("At least one file is required");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Model name is required");
            }

            // Get user ID from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var authorId))
            {
                _logger.LogWarning("Unable to extract valid user ID from JWT claims");
                return Unauthorized("Invalid authentication token");
            }

            // Validate files
            var validationError = ValidateFiles(request.Files);
            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            var modelId = Guid.NewGuid();
            var modelFiles = new List<ModelFile>();
            string? thumbnailUrl = null;

            try
            {
                // Upload all files
                foreach (var file in request.Files)
                {
                    if (file.Length == 0) continue;

                    var objectKey = $"models/{modelId}/{Guid.NewGuid()}_{file.FileName}";
                    await using var stream = file.OpenReadStream();
                    var url = await _storage.UploadAsync(objectKey, stream, file.ContentType, cancellationToken);

                    var modelFile = new ModelFile
                    {
                        Id = Guid.NewGuid(),
                        Name = file.FileName,
                        Path = url,
                        Size = file.Length,
                        MimeType = file.ContentType
                    };

                    modelFiles.Add(modelFile);

                    // Check if this file should be the thumbnail
                    if (request.ThumbnailFileId != null && file.FileName.Contains(request.ThumbnailFileId))
                    {
                        thumbnailUrl = url;
                    }
                }

                // If no thumbnail was specified, prefer image files, then fallback to first 3D model
                if (thumbnailUrl == null)
                {
                    var imageFile = modelFiles.FirstOrDefault(f => IsImageFile(f.Name));
                    if (imageFile != null)
                    {
                        thumbnailUrl = imageFile.Path;
                    }
                    else
                    {
                        var first3DModel = modelFiles.FirstOrDefault(f => Is3DModelFile(f.Name));
                        thumbnailUrl = first3DModel?.Path;
                    }
                }

                // Create model entity
                var model = new Model
                {
                    Id = modelId,
                    Name = request.Name,
                    Description = request.Description ?? string.Empty,
                    FileUrl = modelFiles.First().Path, // Primary file URL
                    ThumbnailUrl = thumbnailUrl,
                    Privacy = request.Privacy == "private" ? PrivacySettings.Private : PrivacySettings.Public,
                    License = ParseLicense(request.License),
                    AIGenerated = request.AIGenerated,
                    WIP = request.WorkInProgress,
                    NSFW = request.NSFW,
                    IsRemix = request.Remix,
                    IsPublic = request.Privacy != "private",
                    AuthorId = authorId,
                    Files = modelFiles
                };

                // Save model and files
                _db.Models.Add(model);
                await _db.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully uploaded model {ModelId} with {FileCount} files for user {UserId}", 
                    modelId, modelFiles.Count, authorId);

                return CreatedAtAction(nameof(UploadModel), new { id = model.Id }, model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload model for user {UserId}", authorId);
                
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

                return StatusCode(500, "An error occurred while uploading the model");
            }
        }

        private string? ValidateFiles(IFormFile[] files)
        {
            if (files.Length > 20) // Reasonable limit
            {
                return "Too many files. Maximum of 20 files allowed per upload.";
            }

            var has3DModel = false;
            var totalSize = 0L;

            foreach (var file in files)
            {
                if (file.Length == 0)
                    continue;

                // Check file extension
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllSupportedFormats.Contains(extension))
                {
                    return $"Unsupported file type: {extension}. Supported formats: {string.Join(", ", AllSupportedFormats)}";
                }

                // Check file size
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

            // Ensure at least one 3D model file
            if (!has3DModel)
            {
                return "At least one 3D model file is required (.stl, .obj, .fbx, .gltf, .glb)";
            }

            // Check total upload size
            if (totalSize > 1024 * 1024 * 1024) // 1GB total limit
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
                "commercial" => LicenseTypes.MIT, // Default to MIT for commercial
                "custom" => LicenseTypes.MIT, // Default to MIT for custom
                _ => null
            };
        }
    }
} 
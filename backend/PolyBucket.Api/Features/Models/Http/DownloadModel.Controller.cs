using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.Domain.Enums;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Settings;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Models.Http
{
    [ApiController]
    [Route("api/models")]
    public class DownloadModelController : ControllerBase
    {
        private readonly PolyBucketDbContext _context;
        private readonly IPermissionService _permissionService;
        private readonly IStorageService _storageService;
        private readonly ILogger<DownloadModelController> _logger;
        private readonly StorageSettings _storageSettings;

        public DownloadModelController(
            PolyBucketDbContext context,
            IPermissionService permissionService,
            IStorageService storageService,
            ILogger<DownloadModelController> logger,
            IOptions<StorageSettings> storageOptions)
        {
            _context = context;
            _permissionService = permissionService;
            _storageService = storageService;
            _logger = logger;
            _storageSettings = storageOptions.Value;
        }

        [HttpGet("{id}/download")]
        [Authorize]
        [RequirePermission(PermissionConstants.MODEL_DOWNLOAD)]
        public async Task<IActionResult> DownloadModel(Guid id)
        {
            try
            {
                // Get the model with all its files
                var model = await _context.Models
                    .Include(m => m.Files)
                    .Include(m => m.Author)
                    .FirstOrDefaultAsync(m => m.Id == id);

                // Also get model previews/thumbnails
                var modelPreviews = await _context.Set<ModelPreview>()
                    .Where(p => p.ModelId == id && p.Status == PreviewStatus.Completed)
                    .ToListAsync();

                if (model == null)
                {
                    return NotFound("Model not found");
                }

                // Check privacy-based authorization
                if (!await CanUserAccessModel(model))
                {
                    return Forbid("You do not have permission to download this model");
                }

                // Get current user ID for download tracking
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    // Increment download count
                    model.Downloads++;
                    await _context.SaveChangesAsync();
                }

                // This section has been moved above for better logic flow

                // If there are multiple files or we need to include previews, create a zip archive
                var needsZip = model.Files.Count > 1 || modelPreviews.Any();
                
                if (!needsZip && model.Files.Count == 1)
                {
                    // Single file download without previews
                    var file = model.Files.First();
                    
                    var objectKey = ExtractObjectKey(file.Path);
                    if (string.IsNullOrEmpty(objectKey))
                    {
                        _logger.LogError("Could not extract object key from path: {FilePath} for single file download: {FileName}", file.Path, file.Name);
                        return StatusCode(500, "Invalid file path format");
                    }
                    
                    try
                    {
                        var fileStream = await _storageService.DownloadAsync(objectKey);
                        _logger.LogInformation("Successfully downloaded single file {FileName} for model {ModelId}", file.Name, model.Id);
                        return File(fileStream, file.MimeType, file.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to download single file {FileName} for model {ModelId}: {ErrorMessage}", file.Name, model.Id, ex.Message);
                        return StatusCode(500, "Failed to download the file");
                    }
                }
                
                // Create a zip archive that includes model files and previews
                var zipFileName = $"{model.Name.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMdd}.zip";
                var failedFiles = new List<string>();
                
                // Use Microsoft documentation pattern: Create ZIP in memory, return as byte array
                using (var memoryStream = new MemoryStream())
                {
                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        var totalFilesAdded = 0;
                        
                        // Add model files
                        foreach (var file in model.Files)
                        {
                            try
                            {
                                var objectKey = ExtractObjectKey(file.Path);
                                if (string.IsNullOrEmpty(objectKey))
                                {
                                    _logger.LogWarning("Could not extract object key from path: {FilePath} for file: {FileName}", file.Path, file.Name);
                                    failedFiles.Add(file.Name);
                                    continue;
                                }
                                
                                using var fileStream = await _storageService.DownloadAsync(objectKey);
                                if (fileStream == null || fileStream.Length == 0)
                                {
                                    _logger.LogWarning("File stream is null or empty for file: {FileName}", file.Name);
                                    failedFiles.Add(file.Name);
                                    continue;
                                }
                                
                                var entry = archive.CreateEntry($"files/{file.Name}", CompressionLevel.Optimal);
                                
                                using var entryStream = entry.Open();
                                await fileStream.CopyToAsync(entryStream);
                                await entryStream.FlushAsync(); // Ensure data is written
                                
                                totalFilesAdded++;
                                _logger.LogDebug("Successfully added file {FileName} ({FileSize} bytes) to archive for model {ModelId}", 
                                    file.Name, fileStream.Length, model.Id);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to download file {FileName} (ID: {FileId}) for model {ModelId}: {ErrorMessage}", 
                                    file.Name, file.Id, model.Id, ex.Message);
                                failedFiles.Add(file.Name);
                            }
                        }
                        
                        // Add model previews/thumbnails
                        foreach (var preview in modelPreviews)
                        {
                            try
                            {
                                var objectKey = ExtractObjectKey(preview.StorageKey);
                                if (string.IsNullOrEmpty(objectKey))
                                {
                                    _logger.LogWarning("Could not extract object key from storage key: {StorageKey} for preview: {PreviewId}", preview.StorageKey, preview.Id);
                                    failedFiles.Add($"preview_{preview.Size}");
                                    continue;
                                }
                                
                                using var previewStream = await _storageService.DownloadAsync(objectKey);
                                if (previewStream == null || previewStream.Length == 0)
                                {
                                    _logger.LogWarning("Preview stream is null or empty for preview: {PreviewId}", preview.Id);
                                    failedFiles.Add($"preview_{preview.Size}");
                                    continue;
                                }
                                
                                var fileName = $"preview_{preview.Size}_{preview.Width}x{preview.Height}.jpg";
                                var entry = archive.CreateEntry($"previews/{fileName}", CompressionLevel.Optimal);
                                
                                using var entryStream = entry.Open();
                                await previewStream.CopyToAsync(entryStream);
                                await entryStream.FlushAsync(); // Ensure data is written
                                
                                totalFilesAdded++;
                                _logger.LogDebug("Successfully added preview {PreviewSize} ({FileSize} bytes) to archive for model {ModelId}", 
                                    preview.Size, previewStream.Length, model.Id);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to download preview {PreviewId} for model {ModelId}: {ErrorMessage}", 
                                    preview.Id, model.Id, ex.Message);
                                failedFiles.Add($"preview_{preview.Size}");
                            }
                        }
                        
                        // Add model thumbnail if available and not already included as preview
                        if (!string.IsNullOrEmpty(model.ThumbnailUrl) && !modelPreviews.Any())
                        {
                            try
                            {
                                var objectKey = ExtractObjectKey(model.ThumbnailUrl);
                                if (!string.IsNullOrEmpty(objectKey))
                                {
                                    using var thumbnailStream = await _storageService.DownloadAsync(objectKey);
                                    if (thumbnailStream != null && thumbnailStream.Length > 0)
                                    {
                                        var entry = archive.CreateEntry("thumbnail.jpg", CompressionLevel.Optimal);
                                        
                                        using var entryStream = entry.Open();
                                        await thumbnailStream.CopyToAsync(entryStream);
                                        await entryStream.FlushAsync(); // Ensure data is written
                                        
                                        totalFilesAdded++;
                                        _logger.LogDebug("Successfully added thumbnail ({FileSize} bytes) to archive for model {ModelId}", 
                                            thumbnailStream.Length, model.Id);
                                    }
                                    else
                                    {
                                        _logger.LogWarning("Thumbnail stream is null or empty for model {ModelId}", model.Id);
                                        failedFiles.Add("thumbnail");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to download thumbnail for model {ModelId}: {ErrorMessage}", model.Id, ex.Message);
                                failedFiles.Add("thumbnail");
                            }
                        }
                        
                        // If no files were added, create a simple text file to ensure the ZIP is valid
                        if (totalFilesAdded == 0)
                        {
                            var entry = archive.CreateEntry("README.txt", CompressionLevel.Optimal);
                            using var entryStream = entry.Open();
                            using var writer = new StreamWriter(entryStream);
                            await writer.WriteLineAsync($"Model: {model.Name}");
                            await writer.WriteLineAsync($"Download Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                            await writer.WriteLineAsync("Note: No files were available for download.");
                            await writer.FlushAsync();
                            await entryStream.FlushAsync();
                            
                            totalFilesAdded++;
                            _logger.LogInformation("Created README.txt as no files were available for model {ModelId}", model.Id);
                        }
                        
                        _logger.LogInformation("ZIP archive creation completed for model {ModelId}: {TotalFiles} files added, {FailedFiles} failures", 
                            model.Id, totalFilesAdded, failedFiles.Count);
                    }
                    
                    // Verify the ZIP was created successfully
                    if (memoryStream.Length == 0)
                    {
                        _logger.LogError("ZIP archive is empty after creation for model {ModelId}", model.Id);
                        return StatusCode(500, "Failed to create download archive - archive is empty");
                    }
                    
                    // Log summary of failed files
                    if (failedFiles.Any())
                    {
                        _logger.LogWarning("Download completed for model {ModelId} with {FailedCount} failed files: {FailedFiles}", 
                            model.Id, failedFiles.Count, string.Join(", ", failedFiles));
                    }
                    else
                    {
                        _logger.LogInformation("Successfully created download archive for model {ModelId} with {FileCount} files and {PreviewCount} previews", 
                            model.Id, model.Files.Count, modelPreviews.Count);
                    }

                    _logger.LogInformation("Returning ZIP file for model {ModelId}: {FileName} ({FileSize} bytes)", 
                        model.Id, zipFileName, memoryStream.Length);
                    
                    // CRITICAL: Use ToArray() to create a proper byte array - this is the Microsoft recommended pattern
                    return File(memoryStream.ToArray(), "application/zip", zipFileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error occurred while downloading model {ModelId}: {ErrorMessage}", id, ex.Message);
                return StatusCode(500, "An error occurred while downloading the model");
            }
        }

        private async Task<bool> CanUserAccessModel(Model model)
        {
            // Public models are accessible to everyone
            if (model.Privacy == PrivacySettings.Public)
            {
                return true;
            }

            // Get current user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
            {
                // No authenticated user - can only access public models
                return false;
            }

            // Model owner can always access their own models
            if (model.AuthorId == currentUserId)
            {
                return true;
            }

            // Check if user has admin or moderator privileges
            var isAdmin = await _permissionService.IsAdminAsync(currentUserId);
            var userRole = await _permissionService.GetUserRoleAsync(currentUserId);
            var isModerator = userRole?.Name.Equals("Moderator", StringComparison.OrdinalIgnoreCase) == true;
            
            if (isAdmin || isModerator)
            {
                return true;
            }

            // For Private models, only owner and admins/moderators can access
            if (model.Privacy == PrivacySettings.Private)
            {
                return false;
            }

            // For Unlisted models, anyone with the link can access
            return model.Privacy == PrivacySettings.Unlisted;
        }

        /// <summary>
        /// Extracts the object key from either a presigned URL or direct object key path.
        /// </summary>
        /// <param name="filePath">The file path, which could be a presigned URL or object key</param>
        /// <returns>The extracted object key, or null if extraction fails</returns>
        private string? ExtractObjectKey(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

            // If it's not a URL, assume it's already an object key
            if (!filePath.StartsWith("http"))
            {
                return filePath;
            }

            try
            {
                // This is a presigned URL, extract the object key
                var uri = new Uri(filePath);
                var pathSegments = uri.AbsolutePath.Split('/');
                
                // Find the bucket name and extract everything after it
                var bucketIndex = Array.IndexOf(pathSegments, _storageSettings.BucketName);
                if (bucketIndex >= 0 && bucketIndex + 1 < pathSegments.Length)
                {
                    var objectKey = string.Join("/", pathSegments.Skip(bucketIndex + 1));
                    // URL decode the object key to handle spaces and other encoded characters
                    return Uri.UnescapeDataString(objectKey);
                }
                
                _logger.LogWarning("Could not find bucket name '{BucketName}' in URL path: {FilePath}", _storageSettings.BucketName, filePath);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse URL for object key extraction: {FilePath}", filePath);
                return null;
            }
        }
    }
} 
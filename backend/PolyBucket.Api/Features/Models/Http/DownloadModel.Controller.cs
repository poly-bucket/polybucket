using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Models.Domain;
using PolyBucket.Api.Features.Models.Domain.Enums;
using PolyBucket.Api.Features.ACL.Services;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Models.Http
{
    [ApiController]
    [Route("api/models")]
    public class DownloadModelController : ControllerBase
    {
        private readonly PolyBucketDbContext _context;
        private readonly IPermissionService _permissionService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IStorageService _storageService;

        public DownloadModelController(
            PolyBucketDbContext context,
            IPermissionService permissionService,
            IHttpClientFactory httpClientFactory,
            IStorageService storageService)
        {
            _context = context;
            _permissionService = permissionService;
            _httpClientFactory = httpClientFactory;
            _storageService = storageService;
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

                // If there's only one file, return it directly
                if (model.Files.Count == 1)
                {
                    var file = model.Files.First();
                    
                    // Extract object key from path - handle both object keys and presigned URLs
                    string objectKey;
                    if (file.Path.StartsWith("http"))
                    {
                        // This is a presigned URL, extract the object key
                        var uri = new Uri(file.Path);
                        var pathSegments = uri.AbsolutePath.Split('/');
                        // Find the bucket name and extract everything after it
                        var bucketIndex = Array.IndexOf(pathSegments, "polybucket-uploads");
                        if (bucketIndex >= 0 && bucketIndex + 1 < pathSegments.Length)
                        {
                            objectKey = string.Join("/", pathSegments.Skip(bucketIndex + 1));
                        }
                        else
                        {
                            return StatusCode(500, "Invalid file path format");
                        }
                    }
                    else
                    {
                        // This is already an object key
                        objectKey = file.Path;
                    }
                    
                    var fileStream = await _storageService.DownloadAsync(objectKey);
                    return File(fileStream, file.MimeType, file.Name);
                }

                // If there are multiple files, create a zip archive
                var zipFileName = $"{model.Name.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMdd}.zip";
                
                using var memoryStream = new MemoryStream();
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in model.Files)
                    {
                        try
                        {
                            // Extract object key from path - handle both object keys and presigned URLs
                            string objectKey;
                            if (file.Path.StartsWith("http"))
                            {
                                // This is a presigned URL, extract the object key
                                var uri = new Uri(file.Path);
                                var pathSegments = uri.AbsolutePath.Split('/');
                                // Find the bucket name and extract everything after it
                                var bucketIndex = Array.IndexOf(pathSegments, "polybucket-uploads");
                                if (bucketIndex >= 0 && bucketIndex + 1 < pathSegments.Length)
                                {
                                    objectKey = string.Join("/", pathSegments.Skip(bucketIndex + 1));
                                }
                                else
                                {
                                    continue; // Skip this file if we can't parse the path
                                }
                            }
                            else
                            {
                                // This is already an object key
                                objectKey = file.Path;
                            }
                            
                            var fileStream = await _storageService.DownloadAsync(objectKey);
                            var entry = archive.CreateEntry(file.Name, CompressionLevel.Optimal);
                            
                            using var entryStream = entry.Open();
                            await fileStream.CopyToAsync(entryStream);
                        }
                        catch (Exception ex)
                        {
                            // Log the error but continue with other files
                            // You might want to add proper logging here
                            continue;
                        }
                    }
                }

                memoryStream.Position = 0;
                return File(memoryStream, "application/zip", zipFileName);
            }
            catch (Exception ex)
            {
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
            if (model.Privacy == PrivacySettings.Unlisted)
            {
                return true;
            }

            return false;
        }
    }
} 
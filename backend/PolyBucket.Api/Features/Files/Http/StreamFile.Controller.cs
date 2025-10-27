using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common.Storage;
using PolyBucket.Api.Common.Entities;
using PolyBucket.Api.Common.Enums;
using PolyBucket.Api.Features.Models.Shared.Domain;
using PolyBucket.Api.Features.Models.Shared.Domain.Enums;
using PolyBucket.Api.Features.Models.GetModels.Repository;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Services;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Data;
using PolyBucket.Api.Settings;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Files.Http
{
    [ApiController]
    [Route("api/files")]
    public class StreamFileController : ControllerBase
    {
        private readonly PolyBucketDbContext _context;
        private readonly IPermissionService _permissionService;
        private readonly IStorageService _storageService;
        private readonly StorageSettings _storageSettings;

        public StreamFileController(
            PolyBucketDbContext context,
            IPermissionService permissionService,
            IStorageService storageService,
            IOptions<StorageSettings> storageOptions)
        {
            _context = context;
            _permissionService = permissionService;
            _storageService = storageService;
            _storageSettings = storageOptions.Value;
        }

        [HttpGet("stream/{fileId}")]
        [Authorize]
        [RequirePermission(PermissionConstants.MODEL_DOWNLOAD)]
        public async Task<IActionResult> StreamFile(Guid fileId)
        {
            try
            {
                // Get the file with its associated model
                var file = await _context.ModelFiles
                    .Include(f => f.Model)
                    .ThenInclude(m => m.Author)
                    .FirstOrDefaultAsync(f => f.Id == fileId);

                if (file == null)
                {
                    return NotFound("File not found");
                }

                // Check if user has permission to access the model
                if (!await CanUserAccessModel(file.Model))
                {
                    return Forbid("You do not have permission to access this file");
                }

                // Get the file stream from storage
                var fileStream = await _storageService.DownloadAsync(file.Path);
                if (fileStream == null)
                {
                    return NotFound("File not found in storage");
                }

                // Set appropriate headers
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{file.Name}\"");
                Response.Headers.Add("Cache-Control", "public, max-age=3600"); // Cache for 1 hour

                // Return the file stream
                return File(fileStream, file.MimeType ?? "application/octet-stream");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while streaming the file");
            }
        }

        [HttpGet("stream/model/{modelId}/{fileName}")]
        [ServiceFilter(typeof(PublicModelAuthorizationFilter))]
        public async Task<IActionResult> StreamModelFile(Guid modelId, string fileName)
        {
            try
            {
                // URL decode the filename - this is the only place we need to decode
                var decodedFileName = Uri.UnescapeDataString(fileName);
                
                // Get the model with its files
                var model = await _context.Models
                    .Include(m => m.Files)
                    .Include(m => m.Author)
                    .FirstOrDefaultAsync(m => m.Id == modelId);

                if (model == null)
                {
                    return NotFound("Model not found");
                }

                // Check if user has permission to access the model
                if (!await CanUserAccessModel(model))
                {
                    return Forbid("You do not have permission to access this model");
                }

                // Find the specific file by name (case-insensitive)
                var file = model.Files.FirstOrDefault(f => 
                    string.Equals(f.Name, decodedFileName, StringComparison.OrdinalIgnoreCase));
                
                if (file == null)
                {
                    // Log available files for debugging
                    var availableFiles = string.Join(", ", model.Files.Select(f => f.Name));
                    return NotFound($"File '{decodedFileName}' not found. Available files: {availableFiles}");
                }

                // Extract object key from path - handle both object keys and presigned URLs
                string objectKey;
                if (file.Path.StartsWith("http"))
                {
                    // This is a presigned URL, extract the object key
                    var uri = new Uri(file.Path);
                    var pathSegments = uri.AbsolutePath.Split('/');
                    // Find the bucket name and extract everything after it
                    var bucketIndex = Array.IndexOf(pathSegments, _storageSettings.BucketName);
                    if (bucketIndex >= 0 && bucketIndex + 1 < pathSegments.Length)
                    {
                        objectKey = string.Join("/", pathSegments.Skip(bucketIndex + 1));
                        // No need to URL decode here - the object key should be used as-is
                        // The storage service expects the raw object key
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

                // Get the file stream from storage
                var fileStream = await _storageService.DownloadAsync(objectKey);
                if (fileStream == null)
                {
                    return NotFound("File not found in storage");
                }

                // Set appropriate headers
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{file.Name}\"");
                Response.Headers.Add("Cache-Control", "public, max-age=3600"); // Cache for 1 hour

                // Return the file stream
                return File(fileStream, file.MimeType ?? "application/octet-stream");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while streaming the file");
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
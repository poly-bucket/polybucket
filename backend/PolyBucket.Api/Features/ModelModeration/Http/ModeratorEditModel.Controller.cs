using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.Models.Shared.Domain;
using PolyBucket.Api.Features.Models.AddTagToModel.Domain;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;
using PolyBucket.Api.Features.Models.Shared.Domain.Enums;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ACL.Services;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.Http
{
    [Authorize]
    [ApiController]
    [Route("api/moderation/models")]
    [RequirePermission(PermissionConstants.MODERATION_EDIT_MODELS)]
    public class ModeratorEditModelController(PolyBucketDbContext context, IPermissionService permissionService) : ControllerBase
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly IPermissionService _permissionService = permissionService;

        /// <summary>
        /// Allows moderators to edit model details and metadata
        /// </summary>
        /// <param name="id">Model ID to edit</param>
        /// <param name="request">Moderation edit request</param>
        /// <returns>Updated model</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Model), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Model>> EditModel(Guid id, [FromBody] ModeratorEditRequest request)
        {
            // Get current user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var moderatorId))
            {
                return Unauthorized("Invalid user token");
            }

            // Check if user has moderation privileges
            var user = await _context.Users.FindAsync(moderatorId);
            if (user == null || !HasModerationPrivileges(user))
            {
                return Forbid("Insufficient privileges for moderation actions");
            }

            // Get the model to edit
            var model = await _context.Models
                .Include(m => m.Tags)
                .Include(m => m.Categories)
                .Include(m => m.Author)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (model == null)
            {
                return NotFound("Model not found");
            }

            // Store previous values for audit log
            var previousValues = new
            {
                model.Name,
                model.Description,
                model.License,
                model.Privacy,
                model.AIGenerated,
                model.WIP,
                model.NSFW,
                model.IsRemix,
                model.RemixUrl,
                model.IsPublic,
                model.IsFeatured,
                Tags = model.Tags.Select(t => t.Name).ToList(),
                Categories = model.Categories.Select(c => c.Name).ToList()
            };

            // Update model properties
            model.Name = request.Name;
            model.Description = request.Description;
            model.License = request.License;
            model.Privacy = request.Privacy;
            model.AIGenerated = request.AIGenerated;
            model.WIP = request.WIP;
            model.NSFW = request.NSFW;
            model.IsRemix = request.IsRemix;
            model.RemixUrl = request.RemixUrl;
            model.IsPublic = request.IsPublic;
            model.IsFeatured = request.IsFeatured;
            model.UpdatedAt = DateTime.UtcNow;

            // Handle tags update
            if (request.Tags.Any())
            {
                // Remove existing tags
                model.Tags.Clear();
                
                // Add new tags
                foreach (var tagName in request.Tags.Where(t => !string.IsNullOrWhiteSpace(t)))
                {
                    var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName.Trim());
                    if (existingTag == null)
                    {
                        existingTag = new Tag { Name = tagName.Trim() };
                        _context.Tags.Add(existingTag);
                    }
                    model.Tags.Add(existingTag);
                }
            }

            // Handle categories update
            if (request.Categories.Any())
            {
                // Remove existing categories
                model.Categories.Clear();
                
                // Add new categories
                foreach (var categoryName in request.Categories.Where(c => !string.IsNullOrWhiteSpace(c)))
                {
                    var existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == categoryName.Trim());
                    if (existingCategory == null)
                    {
                        existingCategory = new Category { Name = categoryName.Trim() };
                        _context.Categories.Add(existingCategory);
                    }
                    model.Categories.Add(existingCategory);
                }
            }

            // Store new values for audit log
            var newValues = new
            {
                model.Name,
                model.Description,
                model.License,
                model.Privacy,
                model.AIGenerated,
                model.WIP,
                model.NSFW,
                model.IsRemix,
                model.RemixUrl,
                model.IsPublic,
                model.IsFeatured,
                Tags = request.Tags,
                Categories = request.Categories
            };

            // Create audit log entry
            var auditLog = new ModerationAuditLog
            {
                ModelId = model.Id,
                PerformedByUserId = moderatorId,
                Action = request.Action,
                PreviousValues = JsonSerializer.Serialize(previousValues),
                NewValues = JsonSerializer.Serialize(newValues),
                ModerationNotes = request.ModerationNotes,
                IPAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers["User-Agent"].ToString()
            };

            _context.ModerationAuditLogs.Add(auditLog);

            try
            {
                await _context.SaveChangesAsync();
                
                // Return updated model with related data
                var updatedModel = await _context.Models
                    .Include(m => m.Tags)
                    .Include(m => m.Categories)
                    .Include(m => m.Author)
                    .FirstOrDefaultAsync(m => m.Id == id);

                return Ok(updatedModel);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to update model: {ex.Message}");
            }
        }

        /// <summary>
        /// Get model details for moderation editing
        /// </summary>
        /// <param name="id">Model ID</param>
        /// <returns>Model details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Model), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Model>> GetModelForModeration(Guid id)
        {
            // Get current user ID
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var moderatorId))
            {
                return Unauthorized("Invalid user token");
            }

            // Check if user has moderation privileges
            var user = await _context.Users.FindAsync(moderatorId);
            if (user == null || !HasModerationPrivileges(user))
            {
                return Forbid("Insufficient privileges for moderation actions");
            }

            var model = await _context.Models
                .Include(m => m.Tags)
                .Include(m => m.Categories)
                .Include(m => m.Author)
                .Include(m => m.Files)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (model == null)
            {
                return NotFound("Model not found");
            }

            return Ok(model);
        }

        private static bool HasModerationPrivileges(PolyBucket.Api.Common.Models.User user)
        {
            // Check if user has moderation role
            // This would typically check against user roles or permissions
            // For now, we'll check if the user is an admin
            return user.Role != null && user.Role.Name == "Admin";
        }
    }
} 
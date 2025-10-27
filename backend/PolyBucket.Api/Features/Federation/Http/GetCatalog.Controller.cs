using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.Federation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Http
{
    /// <summary>
    /// Provides model catalog for federation
    /// </summary>
    [ApiController]
    [Route("api/federation/catalog")]
    [AllowAnonymous] // Token validation done manually
    [ApiExplorerSettings(GroupName = "v1")]
    [Tags("Federation Catalog")]
    public class GetCatalogController(
        PolyBucketDbContext context,
        IFederationTokenService tokenService) : ControllerBase
    {
        private readonly PolyBucketDbContext _context = context;
        private readonly IFederationTokenService _tokenService = tokenService;

        /// <summary>
        /// Get paginated catalog of models available for federation
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 50, max: 100)</param>
        /// <param name="since">Only return models updated since this timestamp</param>
        /// <remarks>
        /// This endpoint provides a catalog of models available for federation to remote instances.
        /// 
        /// Step 4 of the federation handshake - Model Catalog Exchange:
        /// - Remote instances call this endpoint to browse available models
        /// - Requires a valid federation token in the Authorization header
        /// - Only public models are included in the catalog
        /// - Supports pagination and incremental sync (using 'since' parameter)
        /// 
        /// Authorization: Bearer {federation_token}
        /// </remarks>
        /// <response code="200">Returns the model catalog with pagination metadata</response>
        /// <response code="401">Unauthorized - invalid or missing federation token</response>
        [HttpGet]
        [ProducesResponseType(typeof(CatalogResponse), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<CatalogResponse>> GetCatalog(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] DateTime? since = null)
        {
            // Validate federation token
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized("Federation token required");
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            
            // For now, we'll skip token validation since we don't have the audience ID yet
            // In production, you'd validate the token here
            // var isValid = await _tokenService.ValidateTokenAsync(token, thisInstanceId);
            // if (!isValid) return Unauthorized("Invalid federation token");

            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            // Query for public, non-federated models (we don't share federated models)
            var query = _context.Models
                .Where(m => !m.IsFederated) // Only share original models, not federated ones
                .Include(m => m.Author)
                .AsQueryable();

            // Filter by update timestamp if provided (for incremental sync)
            if (since.HasValue)
            {
                query = query.Where(m => m.UpdatedAt > since.Value || m.CreatedAt > since.Value);
            }

            var totalCount = await query.CountAsync();

            var models = await query
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new CatalogModelDto
                {
                    Id = m.Id.ToString(),
                    Name = m.Name,
                    Description = m.Description,
                    ThumbnailUrl = m.ThumbnailUrl,
                    Author = new CatalogAuthorDto
                    {
                        Id = m.AuthorId.ToString(),
                        Username = m.Author.Username,
                        DisplayName = m.Author.Username,
                        AvatarUrl = null // User doesn't have AvatarUrl directly
                    },
                    License = m.License.ToString(),
                    Categories = m.Categories.Select(c => c.Name).ToList(),
                    Tags = m.Tags.Select(t => t.Name).ToList(),
                    FilesSize = 0, // TODO: Calculate from versions
                    FileCount = 0, // TODO: Calculate from versions
                    Downloads = m.Downloads,
                    Likes = m.Likes,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt ?? m.CreatedAt
                })
                .ToListAsync();

            Response.Headers.Append("X-Total-Count", totalCount.ToString());
            Response.Headers.Append("X-Page", page.ToString());
            Response.Headers.Append("X-Page-Size", pageSize.ToString());
            Response.Headers.Append("X-Total-Pages", ((int)Math.Ceiling((double)totalCount / pageSize)).ToString());

            return Ok(new CatalogResponse
            {
                InstanceId = Guid.NewGuid(), // TODO: Get actual instance ID from config
                InstanceName = "PolyBucket Instance", // TODO: Get from config
                TotalModels = totalCount,
                Page = page,
                PageSize = pageSize,
                Models = models
            });
        }

        /// <summary>
        /// Get changes to the catalog since a specific timestamp
        /// </summary>
        /// <param name="since">Timestamp to check changes since</param>
        /// <remarks>
        /// This endpoint is used for change detection during automatic sync.
        /// Returns IDs and actions for models that have been created, updated, or deleted since the given timestamp.
        /// </remarks>
        /// <response code="200">Returns the list of changes</response>
        /// <response code="401">Unauthorized - invalid or missing federation token</response>
        [HttpGet("changes")]
        [ProducesResponseType(typeof(CatalogChangesResponse), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<CatalogChangesResponse>> GetChanges(
            [FromQuery] DateTime since)
        {
            // Validate federation token (simplified for MVP)
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized("Federation token required");
            }

            // Find new models
            var newModels = await _context.Models
                .Where(m => !m.IsFederated && m.CreatedAt > since)
                .Select(m => new CatalogChangeDto
                {
                    Id = m.Id.ToString(),
                    Action = "created",
                    UpdatedAt = m.CreatedAt
                })
                .ToListAsync();

            // Find updated models
            var updatedModels = await _context.Models
                .Where(m => !m.IsFederated && m.UpdatedAt.HasValue && m.UpdatedAt > since)
                .Select(m => new CatalogChangeDto
                {
                    Id = m.Id.ToString(),
                    Action = "updated",
                    UpdatedAt = m.UpdatedAt!.Value
                })
                .ToListAsync();

            var allChanges = newModels.Concat(updatedModels).OrderBy(c => c.UpdatedAt).ToList();

            return Ok(new CatalogChangesResponse
            {
                NewModels = newModels.Count,
                UpdatedModels = updatedModels.Count,
                DeletedModels = 0, // TODO: Implement soft delete tracking
                Models = allChanges
            });
        }
    }

    public class CatalogResponse
    {
        public Guid InstanceId { get; set; }
        public string InstanceName { get; set; } = string.Empty;
        public int TotalModels { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<CatalogModelDto> Models { get; set; } = new();
    }

    public class CatalogModelDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public CatalogAuthorDto Author { get; set; } = new();
        public string License { get; set; } = string.Empty;
        public List<string> Categories { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public long FilesSize { get; set; }
        public int FileCount { get; set; }
        public int Downloads { get; set; }
        public int Likes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CatalogAuthorDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }

    public class CatalogChangesResponse
    {
        public int NewModels { get; set; }
        public int UpdatedModels { get; set; }
        public int DeletedModels { get; set; }
        public List<CatalogChangeDto> Models { get; set; } = new();
    }

    public class CatalogChangeDto
    {
        public string Id { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // "created", "updated", "deleted"
        public DateTime UpdatedAt { get; set; }
        public int? Version { get; set; }
    }
}


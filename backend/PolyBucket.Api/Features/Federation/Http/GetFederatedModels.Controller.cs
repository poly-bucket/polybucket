using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Data;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Http
{
    /// <summary>
    /// Manages retrieval of federated models
    /// </summary>
    [ApiController]
    [Route("api/federation/models")]
    [Authorize]
    [ApiExplorerSettings(GroupName = "v1")]
    [Tags("Federation")]
    public class GetFederatedModelsController(PolyBucketDbContext context) : ControllerBase
    {
        private readonly PolyBucketDbContext _context = context;

        /// <summary>
        /// Get all federated models imported to this instance
        /// </summary>
        /// <param name="instanceId">Optional filter by remote instance ID</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 50, max: 100)</param>
        /// <remarks>
        /// Retrieves a paginated list of all federated models imported to this instance.
        /// Can optionally filter by the source instance.
        /// Returns pagination metadata in response headers (X-Total-Count, X-Page, X-Page-Size).
        /// Requires admin permissions.
        /// </remarks>
        /// <response code="200">Returns the list of federated models with pagination metadata</response>
        /// <response code="401">Unauthorized - user not authenticated</response>
        /// <response code="403">Forbidden - user lacks admin permissions</response>
        [HttpGet]
        [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
        [ProducesResponseType(typeof(List<FederatedModelDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<List<FederatedModelDto>>> GetFederatedModels(
            [FromQuery] string? instanceId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            var query = _context.Models
                .Where(m => m.IsFederated)
                .Include(m => m.Author)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(instanceId))
            {
                query = query.Where(m => m.RemoteInstanceId == instanceId);
            }

            var totalCount = await query.CountAsync();
            
            var models = await query
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new FederatedModelDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    ThumbnailUrl = m.ThumbnailUrl,
                    RemoteInstanceId = m.RemoteInstanceId,
                    RemoteModelId = m.RemoteModelId,
                    RemoteAuthorId = m.RemoteAuthorId,
                    AuthorUsername = m.Author.Username,
                    Downloads = m.Downloads,
                    Likes = m.Likes,
                    LastFederationSync = m.LastFederationSync,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("X-Page", page.ToString());
            Response.Headers.Add("X-Page-Size", pageSize.ToString());

            return Ok(models);
        }
    }

    public class FederatedModelDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string? RemoteInstanceId { get; set; }
        public string? RemoteModelId { get; set; }
        public Guid? RemoteAuthorId { get; set; }
        public string AuthorUsername { get; set; } = string.Empty;
        public int Downloads { get; set; }
        public int Likes { get; set; }
        public DateTime? LastFederationSync { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}


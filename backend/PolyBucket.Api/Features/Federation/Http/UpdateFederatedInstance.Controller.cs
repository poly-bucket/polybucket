using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Federation.Repository;
using PolyBucket.Api.Features.Federation.Domain;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Http
{
    /// <summary>
    /// Manages updating of federated instances
    /// </summary>
    [ApiController]
    [Route("api/federation/instances")]
    [Authorize]
    [ApiExplorerSettings(GroupName = "v1")]
    [Tags("Federation")]
    public class UpdateFederatedInstanceController(
        IFederationRepository repository,
        IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly IFederationRepository _repository = repository;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        /// <summary>
        /// Update an existing federated instance
        /// </summary>
        /// <param name="id">The unique identifier of the federated instance</param>
        /// <param name="request">The update request containing fields to modify</param>
        /// <remarks>
        /// Updates an existing federated instance connection. Can modify name, description, enabled status, etc.
        /// Requires admin permissions.
        /// </remarks>
        /// <response code="200">Federated instance updated successfully</response>
        /// <response code="400">Bad request - validation failed</response>
        /// <response code="401">Unauthorized - user not authenticated</response>
        /// <response code="403">Forbidden - user lacks admin permissions</response>
        /// <response code="404">Federated instance not found</response>
        [HttpPut("{id}")]
        [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
        [ProducesResponseType(typeof(UpdateFederatedInstanceResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UpdateFederatedInstanceResponse>> UpdateFederatedInstance(
            Guid id,
            [FromBody] UpdateFederatedInstanceRequest request)
        {
            var existing = await _repository.GetFederatedInstanceAsync(id);
            
            if (existing == null)
            {
                return NotFound($"Federated instance with ID {id} not found");
            }

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                existing.Name = request.Name;
            }

            if (!string.IsNullOrWhiteSpace(request.BaseUrl))
            {
                if (!Uri.TryCreate(request.BaseUrl, UriKind.Absolute, out _))
                {
                    return BadRequest("Invalid base URL format");
                }
                existing.BaseUrl = request.BaseUrl.TrimEnd('/');
            }

            if (request.Description != null)
            {
                existing.Description = request.Description;
            }

            if (request.PublicKey != null)
            {
                existing.PublicKey = request.PublicKey;
            }

            if (request.SharedSecret != null)
            {
                existing.SharedSecret = request.SharedSecret;
            }

            if (request.IsEnabled.HasValue)
            {
                existing.IsEnabled = request.IsEnabled.Value;
            }

            if (request.Status != null && Enum.TryParse<FederationStatus>(request.Status, out var status))
            {
                existing.Status = status;
            }

            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized("User is not authenticated");
            }

            existing.UpdatedById = Guid.Parse(userIdClaim);
            existing.UpdatedAt = DateTime.UtcNow;

            var updated = await _repository.UpdateFederatedInstanceAsync(existing);

            return Ok(new UpdateFederatedInstanceResponse
            {
                Id = updated.Id,
                Name = updated.Name,
                BaseUrl = updated.BaseUrl,
                Status = updated.Status.ToString(),
                Description = updated.Description,
                IsEnabled = updated.IsEnabled,
                LastSyncAt = updated.LastSyncAt,
                UpdatedAt = updated.UpdatedAt
            });
        }
    }

    public class UpdateFederatedInstanceRequest
    {
        public string? Name { get; set; }
        public string? BaseUrl { get; set; }
        public string? Description { get; set; }
        public string? PublicKey { get; set; }
        public string? SharedSecret { get; set; }
        public bool? IsEnabled { get; set; }
        public string? Status { get; set; }
    }

    public class UpdateFederatedInstanceResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? LastSyncAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}


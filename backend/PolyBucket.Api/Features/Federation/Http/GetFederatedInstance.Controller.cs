using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Federation.Repository;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Http
{
    /// <summary>
    /// Manages retrieval of specific federated instance
    /// </summary>
    [ApiController]
    [Route("api/federation/instances")]
    [Authorize]
    [ApiExplorerSettings(GroupName = "v1")]
    [Tags("Federation")]
    public class GetFederatedInstanceController(IFederationRepository repository) : ControllerBase
    {
        private readonly IFederationRepository _repository = repository;

        /// <summary>
        /// Get a specific federated instance by ID
        /// </summary>
        /// <param name="id">The unique identifier of the federated instance</param>
        /// <remarks>
        /// Retrieves detailed information about a specific federated PolyBucket instance.
        /// Requires admin permissions.
        /// </remarks>
        /// <response code="200">Returns the federated instance details</response>
        /// <response code="401">Unauthorized - user not authenticated</response>
        /// <response code="403">Forbidden - user lacks admin permissions</response>
        /// <response code="404">Federated instance not found</response>
        [HttpGet("{id}")]
        [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
        [ProducesResponseType(typeof(FederatedInstanceDetailDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<FederatedInstanceDetailDto>> GetFederatedInstance(Guid id)
        {
            var instance = await _repository.GetFederatedInstanceAsync(id);
            
            if (instance == null)
            {
                return NotFound($"Federated instance with ID {id} not found");
            }

            var dto = new FederatedInstanceDetailDto
            {
                Id = instance.Id,
                Name = instance.Name,
                BaseUrl = instance.BaseUrl,
                Status = instance.Status.ToString(),
                Description = instance.Description,
                IsEnabled = instance.IsEnabled,
                LastSyncAt = instance.LastSyncAt,
                CreatedAt = instance.CreatedAt,
                UpdatedAt = instance.UpdatedAt
            };

            return Ok(dto);
        }
    }

    public class FederatedInstanceDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? LastSyncAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Federation.Repository;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Federation.Http
{
    /// <summary>
    /// Manages retrieval of federated instances
    /// </summary>
    [ApiController]
    [Route("api/federation/instances")]
    [Authorize]
    [ApiExplorerSettings(GroupName = "v1")]
    [Tags("Federation")]
    public class GetFederatedInstancesController(IFederationRepository repository) : ControllerBase
    {
        private readonly IFederationRepository _repository = repository;

        /// <summary>
        /// Get all federated instances
        /// </summary>
        /// <remarks>
        /// Retrieves a list of all federated PolyBucket instances connected to this instance.
        /// Requires admin permissions.
        /// </remarks>
        /// <response code="200">Returns the list of federated instances</response>
        /// <response code="401">Unauthorized - user not authenticated</response>
        /// <response code="403">Forbidden - user lacks admin permissions</response>
        [HttpGet]
        [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
        [ProducesResponseType(typeof(List<FederatedInstanceDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<List<FederatedInstanceDto>>> GetFederatedInstances()
        {
            var instances = await _repository.GetFederatedInstancesAsync();
            
            var dtos = instances.Select(i => new FederatedInstanceDto
            {
                Id = i.Id,
                Name = i.Name,
                BaseUrl = i.BaseUrl,
                Status = i.Status.ToString(),
                Description = i.Description,
                IsEnabled = i.IsEnabled,
                LastSyncAt = i.LastSyncAt,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            }).ToList();

            return Ok(dtos);
        }
    }

    public class FederatedInstanceDto
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


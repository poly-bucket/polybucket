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
    /// Manages deletion/revocation of federated instances
    /// </summary>
    [ApiController]
    [Route("api/federation/instances")]
    [Authorize]
    [ApiExplorerSettings(GroupName = "v1")]
    [Tags("Federation")]
    public class DeleteFederatedInstanceController(IFederationRepository repository) : ControllerBase
    {
        private readonly IFederationRepository _repository = repository;

        /// <summary>
        /// Delete or revoke a federated instance connection
        /// </summary>
        /// <param name="id">The unique identifier of the federated instance</param>
        /// <remarks>
        /// Revokes the federation connection with the specified instance. 
        /// All imported models remain on this instance, but syncing will stop.
        /// The federation record is soft-deleted and can be re-established later.
        /// Requires admin permissions.
        /// </remarks>
        /// <response code="204">Federated instance deleted/revoked successfully</response>
        /// <response code="401">Unauthorized - user not authenticated</response>
        /// <response code="403">Forbidden - user lacks admin permissions</response>
        /// <response code="404">Federated instance not found</response>
        [HttpDelete("{id}")]
        [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteFederatedInstance(Guid id)
        {
            var existing = await _repository.GetFederatedInstanceAsync(id);
            
            if (existing == null)
            {
                return NotFound($"Federated instance with ID {id} not found");
            }

            await _repository.DeleteFederatedInstanceAsync(id);

            return NoContent();
        }
    }
}


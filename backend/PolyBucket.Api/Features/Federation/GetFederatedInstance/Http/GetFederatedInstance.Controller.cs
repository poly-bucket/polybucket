using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.Federation.GetFederatedInstance.Domain;

namespace PolyBucket.Api.Features.Federation.GetFederatedInstance.Http;

[ApiController]
[Route("api/federation/instances")]
[Authorize]
[ApiExplorerSettings(GroupName = "v1")]
[Tags("Federation")]
public class GetFederatedInstanceController(IGetFederatedInstanceService service) : ControllerBase
{
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
    public async Task<ActionResult<FederatedInstanceDetailDto>> GetFederatedInstance(Guid id, CancellationToken cancellationToken = default)
    {
        var dto = await service.GetAsync(id, cancellationToken);
        if (dto == null)
        {
            return NotFound($"Federated instance with ID {id} not found");
        }

        return Ok(dto);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.Federation.GetFederatedInstances.Domain;

namespace PolyBucket.Api.Features.Federation.GetFederatedInstances.Http;

[ApiController]
[Route("api/federation/instances")]
[Authorize]
[ApiExplorerSettings(GroupName = "v1")]
[Tags("Federation")]
public class GetFederatedInstancesController(IGetFederatedInstancesService service) : ControllerBase
{
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
    public async Task<ActionResult<List<FederatedInstanceDto>>> GetFederatedInstances(CancellationToken cancellationToken = default)
    {
        var dtos = await service.GetAllAsync(cancellationToken);
        return Ok(dtos.ToList());
    }
}

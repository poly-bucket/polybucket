using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.Federation.DeleteFederatedInstance.Domain;

namespace PolyBucket.Api.Features.Federation.DeleteFederatedInstance.Http;

[ApiController]
[Route("api/federation/instances")]
[Authorize]
[ApiExplorerSettings(GroupName = "v1")]
[Tags("Federation")]
public class DeleteFederatedInstanceController(IDeleteFederatedInstanceService service) : ControllerBase
{
    /// <summary>
    /// Delete or revoke a federated instance connection
    /// </summary>
    [HttpDelete("{id}")]
    [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteFederatedInstance(Guid id, CancellationToken cancellationToken = default)
    {
        return await service.DeleteAsync(id, cancellationToken) switch
        {
            DeleteFederatedInstanceOutcome.Deleted => NoContent(),
            DeleteFederatedInstanceOutcome.NotFound => NotFound($"Federated instance with ID {id} not found"),
            _ => StatusCode(500)
        };
    }
}

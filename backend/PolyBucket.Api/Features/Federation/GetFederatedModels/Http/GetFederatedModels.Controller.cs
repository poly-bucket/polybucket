using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.Federation.GetFederatedModels.Domain;

namespace PolyBucket.Api.Features.Federation.GetFederatedModels.Http;

[ApiController]
[Route("api/federation/models")]
[Authorize]
[ApiExplorerSettings(GroupName = "v1")]
[Tags("Federation")]
public class GetFederatedModelsController(IGetFederatedModelsService service) : ControllerBase
{
    /// <summary>
    /// Get all federated models imported to this instance
    /// </summary>
    [HttpGet]
    [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
    [ProducesResponseType(typeof(List<FederatedModelDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<List<FederatedModelDto>>> GetFederatedModels(
        [FromQuery] string? instanceId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var (result, totalCount) = await service.GetFederatedModelsAsync(instanceId, page, pageSize, cancellationToken);
        Response.Headers.Append("X-Total-Count", totalCount.ToString());
        Response.Headers.Append("X-Page", page.ToString());
        Response.Headers.Append("X-Page-Size", pageSize.ToString());
        return result;
    }
}

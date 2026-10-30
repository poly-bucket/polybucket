using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Federation.GetFederationHealth.Domain;
using PolyBucket.Api.Features.Federation.GetFederationHealth.Http;

namespace PolyBucket.Api.Features.Federation.GetFederationHealth.Http;

[ApiController]
    [Route("api/federation/health")]
    [Authorize]
    [ApiExplorerSettings(GroupName = "v1")]
    [Tags("Federation")]
    public class GetFederationHealthController(IGetFederationHealthService service) : ControllerBase
{
    [HttpGet("{instanceId}")]
        [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
        [ProducesResponseType(typeof(FederationHealthResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<FederationHealthResponse>> GetFederationHealth(Guid instanceId, CancellationToken cancellationToken = default)
    {
        return await service.GetFederationHealthAsync(instanceId, cancellationToken);
    }
}

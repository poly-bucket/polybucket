using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Federation.UpdateFederatedInstance.Domain;
using PolyBucket.Api.Features.Federation.UpdateFederatedInstance.Http;

using PolyBucket.Api.Features.Federation.Http;

namespace PolyBucket.Api.Features.Federation.UpdateFederatedInstance.Http;

[ApiController]
[Route("api/federation/instances")]
[RequireFederationEnabled(FederationGateMode.Mutation)]
[Authorize]
    [ApiExplorerSettings(GroupName = "v1")]
    [Tags("Federation")]
    public class UpdateFederatedInstanceController(IUpdateFederatedInstanceService service) : ControllerBase
{
    [HttpPut("{id}")]
        [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
        [ProducesResponseType(typeof(UpdateFederatedInstanceResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UpdateFederatedInstanceResponse>> UpdateFederatedInstance(Guid id,
            [FromBody] UpdateFederatedInstanceRequest request, CancellationToken cancellationToken = default)
    {
        return await service.UpdateFederatedInstanceAsync(id, request, cancellationToken);
    }
}

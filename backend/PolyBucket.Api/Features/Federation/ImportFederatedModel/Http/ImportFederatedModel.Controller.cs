using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Federation.ImportFederatedModel.Domain;
using PolyBucket.Api.Features.Federation.ImportFederatedModel.Http;

namespace PolyBucket.Api.Features.Federation.ImportFederatedModel.Http;

[ApiController]
    [Route("api/federation/models")]
    [Authorize]
    [ApiExplorerSettings(GroupName = "v1")]
    [Tags("Federation")]
    public class ImportFederatedModelController(IImportFederatedModelService service) : ControllerBase
{
    [HttpPost("import")]
        [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
        [ProducesResponseType(typeof(ImportFederatedModelResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ImportFederatedModelResponse>> ImportFederatedModel([FromBody] ImportFederatedModelRequest request, CancellationToken cancellationToken = default)
    {
        return await service.ImportFederatedModelAsync(request, cancellationToken);
    }
}

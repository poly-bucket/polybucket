using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.GetMetadataEntityFields.Domain;
using PolyBucket.Api.Features.Plugins.ValidateMetadata.Domain;

namespace PolyBucket.Api.Features.Plugins.ValidateMetadata.Http;

[ApiController]
[Route("api/plugins/metadata")]
[Authorize]
public class ValidateMetadataController(IValidateMetadataService service) : ControllerBase
{
    [HttpPost("validate/{entityType}/{entityId}")]
    public async Task<ActionResult<MetadataValidationResult>> ValidateMetadata(string entityType, string entityId, [FromBody] MetadataValidationRequest request)
    {
        var outcome = await service.ValidateAsync(entityType, entityId, request);
        if (outcome.Status == MetadataRequestStatus.BadRequest) return BadRequest(outcome.ErrorBody);
        return outcome.Value!.IsValid ? Ok(outcome.Value) : BadRequest(outcome.Value);
    }
}

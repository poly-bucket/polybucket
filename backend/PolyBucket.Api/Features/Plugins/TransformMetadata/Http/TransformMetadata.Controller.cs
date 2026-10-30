using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.GetMetadataEntityFields.Domain;
using PolyBucket.Api.Features.Plugins.TransformMetadata.Domain;

namespace PolyBucket.Api.Features.Plugins.TransformMetadata.Http;

[ApiController]
[Route("api/plugins/metadata")]
[Authorize]
public class TransformMetadataController(ITransformMetadataService service) : ControllerBase
{
    [HttpPost("transform/{entityType}")]
    public async Task<ActionResult<Dictionary<string, object>>> TransformMetadata(string entityType, [FromBody] MetadataTransformRequest request)
    {
        var outcome = await service.TransformAsync(entityType, request);
        return outcome.Status == MetadataRequestStatus.BadRequest ? BadRequest(outcome.ErrorBody) : Ok(outcome.Value);
    }
}

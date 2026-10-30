using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.GetDefaultMetadata.Domain;
using PolyBucket.Api.Features.Plugins.GetMetadataEntityFields.Domain;

namespace PolyBucket.Api.Features.Plugins.GetDefaultMetadata.Http;

[ApiController]
[Route("api/plugins/metadata")]
[Authorize]
public class GetDefaultMetadataController(IGetDefaultMetadataService service) : ControllerBase
{
    [HttpGet("defaults/{entityType}")]
    public async Task<ActionResult<Dictionary<string, object>>> GetDefaultMetadata(string entityType)
    {
        var outcome = await service.GetDefaultsAsync(entityType);
        return outcome.Status == MetadataRequestStatus.BadRequest ? BadRequest(outcome.ErrorBody) : Ok(outcome.Value);
    }
}

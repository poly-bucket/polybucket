using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.GetMetadataEntityFields.Domain;

namespace PolyBucket.Api.Features.Plugins.GetMetadataEntityFields.Http;

[ApiController]
[Route("api/plugins/metadata")]
[Authorize]
public class GetMetadataEntityFieldsController(IGetMetadataEntityFieldsService service) : ControllerBase
{
    [HttpGet("fields/{entityType}")]
    public async Task<ActionResult<List<MetadataField>>> GetEntityFields(string entityType)
    {
        var outcome = await service.GetFieldsAsync(entityType);
        return outcome.Status == MetadataRequestStatus.BadRequest ? BadRequest(outcome.ErrorBody) : Ok(outcome.Value);
    }
}

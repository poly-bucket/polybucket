using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.GetMetadataEntityFields.Domain;
using PolyBucket.Api.Features.Plugins.GetMetadataSchema.Domain;

namespace PolyBucket.Api.Features.Plugins.GetMetadataSchema.Http;

[ApiController]
[Route("api/plugins/metadata")]
[Authorize]
public class GetMetadataSchemaController(IGetMetadataSchemaService service) : ControllerBase
{
    [HttpGet("schema/{entityType}")]
    public async Task<ActionResult<MetadataSchema>> GetMetadataSchema(string entityType)
    {
        var outcome = await service.GetSchemaAsync(entityType);
        return outcome.Status == MetadataRequestStatus.BadRequest ? BadRequest(outcome.ErrorBody) : Ok(outcome.Value);
    }
}

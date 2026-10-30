using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Federation.GetCatalogChanges.Domain;
using PolyBucket.Api.Features.Federation.GetCatalogChanges.Http;

namespace PolyBucket.Api.Features.Federation.GetCatalogChanges.Http;

[ApiController]
[Route("api/federation/catalog")]
[AllowAnonymous]
[ApiExplorerSettings(GroupName = "v1")]
[Tags("Federation Catalog")]
public class GetCatalogChangesController(IGetCatalogChangesService service) : ControllerBase
{
    [HttpGet("changes")]
    [ProducesResponseType(typeof(CatalogChangesResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<CatalogChangesResponse>> GetChanges([FromQuery] DateTime since, CancellationToken cancellationToken = default)
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        return await service.GetChangesAsync(since, authHeader, cancellationToken);
    }
}

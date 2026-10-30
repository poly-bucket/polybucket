using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Federation.GetCatalog.Domain;
using PolyBucket.Api.Features.Federation.GetCatalog.Http;

namespace PolyBucket.Api.Features.Federation.GetCatalog.Http;

[ApiController]
[Route("api/federation/catalog")]
[AllowAnonymous]
[ApiExplorerSettings(GroupName = "v1")]
[Tags("Federation Catalog")]
public class GetCatalogController(IGetCatalogService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(CatalogResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<CatalogResponse>> GetCatalog(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] DateTime? since = null,
        CancellationToken cancellationToken = default)
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        var result = await service.GetCatalogAsync(page, pageSize, since, authHeader, cancellationToken);
        if (result.Result is OkObjectResult ok && ok.Value is CatalogResponse catalog)
        {
            Response.Headers.Append("X-Total-Count", catalog.TotalModels.ToString());
            Response.Headers.Append("X-Page", catalog.Page.ToString());
            Response.Headers.Append("X-Page-Size", catalog.PageSize.ToString());
            Response.Headers.Append("X-Total-Pages", ((int)Math.Ceiling((double)catalog.TotalModels / catalog.PageSize)).ToString());
        }
        return result;
    }
}

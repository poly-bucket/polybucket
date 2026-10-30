using PolyBucket.Api.Features.Plugins.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.GetMetadataPlugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetMetadataPlugins.Http;

[ApiController]
[Route("api/plugins/metadata")]
[Authorize]
public class GetMetadataPluginsController(IGetMetadataPluginsService service) : ControllerBase
{
    [HttpGet("plugins")]
    [ProducesResponseType(typeof(List<MetadataPluginInfo>), 200)]
    public ActionResult<List<MetadataPluginInfo>> GetMetadataPlugins() => Ok(service.GetPlugins());
}

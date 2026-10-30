using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.GetPlugins.Domain;

namespace PolyBucket.Api.Features.Plugins.GetPlugins.Http;

[ApiController]
[Route("api/plugins")]
[Authorize]
public class GetPluginsController(IGetPluginsService getPluginsService) : ControllerBase
{
    private readonly IGetPluginsService _getPluginsService = getPluginsService;

    /// <summary>
    /// Get all loaded plugins.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GetPluginsResponseDto>), 200)]
    [ProducesResponseType(401)]
    public ActionResult<IEnumerable<GetPluginsResponseDto>> GetPlugins()
    {
        var plugins = _getPluginsService.GetPlugins();
        return Ok(GetPluginsMapper.ToDto(plugins));
    }
}

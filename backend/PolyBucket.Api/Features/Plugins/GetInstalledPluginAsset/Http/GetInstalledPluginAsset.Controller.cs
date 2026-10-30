using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.GetInstalledPluginAsset.Domain;

namespace PolyBucket.Api.Features.Plugins.GetInstalledPluginAsset.Http;

[Authorize]
[ApiController]
[Route("api/plugins/installed")]
public class GetInstalledPluginAssetController(IGetInstalledPluginAssetService service) : ControllerBase
{
    /// <summary>
    /// Stream a static asset file from an installed plugin directory.
    /// </summary>
    [HttpGet("{pluginId}/assets/{*assetPath}")]
    [ProducesResponseType(typeof(FileStreamResult), 200)]
    [ProducesResponseType(404)]
    public IActionResult GetInstalledPluginAsset(string pluginId, string assetPath)
    {
        var result = service.ResolveAsset(pluginId, assetPath);
        if (!result.Found)
        {
            return NotFound();
        }

        var stream = System.IO.File.OpenRead(result.FullPath);
        return File(stream, result.ContentType, enableRangeProcessing: true);
    }
}

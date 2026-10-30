using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.Services;

namespace PolyBucket.Api.Features.Plugins.Http;

[ApiController]
[Route("api/plugins/installed")]
public class InstalledPluginAssetsController(PluginInstallationService installationService) : ControllerBase
{
    /// <summary>
    /// Stream a static asset file from an installed plugin directory.
    /// </summary>
    [HttpGet("{pluginId}/assets/{*assetPath}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(FileStreamResult), 200)]
    [ProducesResponseType(404)]
    public IActionResult GetInstalledPluginAsset(string pluginId, string assetPath)
    {
        if (!installationService.TryResolveAssetPath(pluginId, assetPath, out var fullPath))
        {
            return NotFound();
        }

        var contentType = GetContentType(fullPath);
        var stream = System.IO.File.OpenRead(fullPath);
        return File(stream, contentType, enableRangeProcessing: true);
    }

    private static string GetContentType(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream"
        };
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.InstallPlugin.Domain;

namespace PolyBucket.Api.Features.Plugins.InstallPlugin.Http;

[ApiController]
[Route("api/plugins")]
[Authorize]
public class InstallPluginController(IInstallPluginService installPluginService) : ControllerBase
{
    private readonly IInstallPluginService _installPluginService = installPluginService;

    /// <summary>
    /// Install a plugin from URL
    /// </summary>
    [HttpPost("install")]
    [ProducesResponseType(typeof(PluginInstallationResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<PluginInstallationResult>> InstallPlugin(
        [FromBody] PluginInstallationRequest request,
        CancellationToken cancellationToken)
    {
        var outcome = await _installPluginService.InstallAsync(request, cancellationToken);

        return outcome.HttpStatus switch
        {
            InstallPluginHttpStatus.Ok => Ok(outcome.Result),
            InstallPluginHttpStatus.NotFound => NotFound(outcome.ErrorBody),
            _ when outcome.Result != null => BadRequest(outcome.Result),
            _ => BadRequest(outcome.ErrorBody)
        };
    }
}

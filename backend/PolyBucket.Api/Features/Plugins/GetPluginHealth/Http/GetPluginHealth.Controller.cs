using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Plugins.GetPluginHealth.Domain;

namespace PolyBucket.Api.Features.Plugins.GetPluginHealth.Http;

[ApiController]
[Route("api/plugins")]
[Authorize]
public class GetPluginHealthController(IGetPluginHealthService getPluginHealthService) : ControllerBase
{
    /// <summary>
    /// Get plugin health status
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(typeof(GetPluginHealthResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<GetPluginHealthResponse>> GetPluginHealth(CancellationToken cancellationToken)
    {
        var health = await getPluginHealthService.GetHealthAsync(cancellationToken);
        return Ok(health);
    }
}

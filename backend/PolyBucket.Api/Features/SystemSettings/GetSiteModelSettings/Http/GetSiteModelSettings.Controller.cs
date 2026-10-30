using PolyBucket.Api.Features.SystemSettings.GetSiteModelSettings.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PolyBucket.Api.Features.SystemSettings.GetSiteModelSettings.Http;

[ApiController]
[Route("api/system-settings/model-settings")]
public class GetSiteModelSettingsController(IGetSiteModelSettingsService service) : ControllerBase
{
    private readonly IGetSiteModelSettingsService _service = service;

    /// <summary>
    /// Get the current site model settings
    /// </summary>
    /// <returns>Current site model configuration</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(200, Type = typeof(GetSiteModelSettingsResponse))]
    [ProducesResponseType(400)]
    public async Task<ActionResult<GetSiteModelSettingsResponse>> GetSiteModelSettings(CancellationToken cancellationToken)
    {
        var result = await _service.GetSiteModelSettingsAsync(new GetSiteModelSettingsQuery(), cancellationToken);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }
}

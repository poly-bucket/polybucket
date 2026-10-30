using PolyBucket.Api.Features.SystemSettings.GetModelConfigurationSettings.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PolyBucket.Api.Features.SystemSettings.GetModelConfigurationSettings.Http;

[ApiController]
[Route("api/system-settings/model-configuration")]
public class GetModelConfigurationSettingsController(IGetModelConfigurationSettingsService service) : ControllerBase
{
    private readonly IGetModelConfigurationSettingsService _service = service;

    /// <summary>
    /// Get the current model configuration settings
    /// </summary>
    /// <returns>Current model configuration</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(200, Type = typeof(GetModelConfigurationSettingsResponse))]
    [ProducesResponseType(400)]
    public async Task<ActionResult<GetModelConfigurationSettingsResponse>> GetModelConfigurationSettings(CancellationToken cancellationToken)
    {
        var result = await _service.GetModelConfigurationSettingsAsync(new GetModelConfigurationSettingsQuery(), cancellationToken);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using PolyBucket.Api.Features.SystemSettings.GetModelConfigurationSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.Http;

[ApiController]
[Route("api/system-settings/model-configuration")]
public class GetModelConfigurationSettingsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Get the current model configuration settings
    /// </summary>
    /// <returns>Current model configuration</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(200, Type = typeof(GetModelConfigurationSettingsResponse))]
    [ProducesResponseType(400)]
    public async Task<ActionResult<GetModelConfigurationSettingsResponse>> GetModelConfigurationSettings()
    {
        var query = new GetModelConfigurationSettingsQuery();
        var result = await _mediator.Send(query);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }
}

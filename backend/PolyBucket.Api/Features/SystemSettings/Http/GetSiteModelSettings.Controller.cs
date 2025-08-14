using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using PolyBucket.Api.Features.SystemSettings.GetSiteModelSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.Http;

[ApiController]
[Route("api/system-settings/model-settings")]
public class GetSiteModelSettingsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Get the current site model settings
    /// </summary>
    /// <returns>Current site model configuration</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(200, Type = typeof(GetSiteModelSettingsResponse))]
    [ProducesResponseType(400)]
    public async Task<ActionResult<GetSiteModelSettingsResponse>> GetSiteModelSettings()
    {
        var query = new GetSiteModelSettingsQuery();
        var result = await _mediator.Send(query);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }
}

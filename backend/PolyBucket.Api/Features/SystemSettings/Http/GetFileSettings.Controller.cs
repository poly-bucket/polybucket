using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using PolyBucket.Api.Features.SystemSettings.GetFileSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.Http;

[ApiController]
[Route("api/system-settings/file-settings")]
public class GetFileSettingsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Get the current file type settings
    /// </summary>
    /// <returns>Current file type configuration</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(200, Type = typeof(GetFileSettingsResponse))]
    [ProducesResponseType(400)]
    public async Task<ActionResult<GetFileSettingsResponse>> GetFileSettings()
    {
        var query = new GetFileSettingsQuery();
        var result = await _mediator.Send(query);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }
}

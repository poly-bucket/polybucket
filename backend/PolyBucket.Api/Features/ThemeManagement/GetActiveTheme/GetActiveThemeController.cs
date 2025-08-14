using MediatR;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ThemeManagement.GetActiveTheme;
using PolyBucket.Api.Features.ThemeManagement.Domain;

namespace PolyBucket.Api.Features.ThemeManagement.GetActiveTheme;

[ApiController]
[Route("api/theme-management")]
public class GetActiveThemeController : ControllerBase
{
    private readonly IMediator _mediator;

    public GetActiveThemeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get the currently active theme (public endpoint)
    /// </summary>
    /// <returns>The active theme or null if none is active</returns>
    /// <response code="200">Returns the active theme</response>
    /// <response code="404">No active theme found</response>
    [HttpGet("active-theme")]
    [ProducesResponseType(typeof(ThemeDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ThemeDto>> GetActiveTheme()
    {
        var query = new GetActiveThemeQuery();
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound("No active theme found");
        }
        
        return Ok(result);
    }
}

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ThemeManagement.GetThemes;
using PolyBucket.Api.Features.ThemeManagement.Domain;

namespace PolyBucket.Api.Features.ThemeManagement.GetThemes;

[ApiController]
[Route("api/theme-management")]
[Authorize]
public class GetThemesController : ControllerBase
{
    private readonly IMediator _mediator;

    public GetThemesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all available themes and the currently active theme
    /// </summary>
    /// <returns>List of all themes and the active theme</returns>
    /// <response code="200">Returns the list of themes</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpGet("themes")]
    [ProducesResponseType(typeof(ThemeListResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<ThemeListResponse>> GetThemes()
    {
        var query = new GetThemesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

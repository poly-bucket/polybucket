using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ThemeManagement.SetActiveTheme;

namespace PolyBucket.Api.Features.ThemeManagement.SetActiveTheme;

[ApiController]
[Route("api/theme-management")]
[Authorize]
public class SetActiveThemeController : ControllerBase
{
    private readonly IMediator _mediator;

    public SetActiveThemeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Set a theme as active
    /// </summary>
    /// <param name="themeId">ID of the theme to activate</param>
    /// <returns>Success status and message</returns>
    /// <response code="200">Theme activated successfully</response>
    /// <response code="400">Invalid request or theme not found</response>
    /// <response code="401">If the user is not authenticated</response>
    [HttpPost("themes/{themeId}/activate")]
    [ProducesResponseType(typeof(SetActiveThemeResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<SetActiveThemeResponse>> SetActiveTheme(int themeId)
    {
        var command = new SetActiveThemeCommand { ThemeId = themeId };
        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

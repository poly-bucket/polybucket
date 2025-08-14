using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ThemeManagement.CreateTheme;
using PolyBucket.Api.Features.ThemeManagement.Domain;

namespace PolyBucket.Api.Features.ThemeManagement.CreateTheme;

[ApiController]
[Route("api/theme-management")]
[Authorize(Roles = "Admin")]
public class CreateThemeController : ControllerBase
{
    private readonly IMediator _mediator;

    public CreateThemeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new theme
    /// </summary>
    /// <param name="request">Theme creation request</param>
    /// <returns>Created theme information</returns>
    /// <response code="200">Theme created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">If the user is not authenticated</response>
    /// <response code="403">If the user is not authorized</response>
    [HttpPost("themes")]
    [ProducesResponseType(typeof(ThemeResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ThemeResponse>> CreateTheme([FromBody] CreateThemeRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new CreateThemeCommand
        {
            Name = request.Name,
            Description = request.Description,
            IsDefault = request.IsDefault,
            Colors = request.Colors
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

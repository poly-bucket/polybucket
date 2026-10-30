using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ThemeManagement.CreateTheme.Domain;
using PolyBucket.Api.Features.ThemeManagement.CreateTheme.Http;

namespace PolyBucket.Api.Features.ThemeManagement.CreateTheme.Http;

[ApiController]
[Route("api/theme-management")]
[Authorize(Roles = "Admin")]
public class CreateThemeController(ICreateThemeService service) : ControllerBase
{
    /// <summary>
    /// Create a new theme.
    /// </summary>
    [HttpPost("themes")]
    [ProducesResponseType(typeof(CreateThemeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateThemeResponse>> CreateTheme([FromBody] CreateThemeRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await service.CreateThemeAsync(request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

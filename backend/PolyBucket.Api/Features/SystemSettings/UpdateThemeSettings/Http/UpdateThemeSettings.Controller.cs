using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.SystemSettings.UpdateThemeSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.UpdateThemeSettings.Http;

[ApiController]
[Route("api/system-settings/theme")]
[Authorize(Roles = "Admin")]
public class UpdateThemeSettingsController(IUpdateThemeSettingsService updateThemeSettingsService) : ControllerBase
{
    /// <summary>
    /// Update the theme settings
    /// </summary>
    /// <param name="command">Theme settings configuration</param>
    /// <returns>Update result</returns>
    [HttpPut]
    [ProducesResponseType(200, Type = typeof(UpdateThemeSettingsResponse))]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<UpdateThemeSettingsResponse>> UpdateThemeSettings([FromBody] UpdateThemeSettingsCommand command, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!command.IsValid(out var validationResults))
        {
            foreach (var validationResult in validationResults)
            {
                ModelState.AddModelError("", validationResult.ErrorMessage ?? "Validation error");
            }
            return BadRequest(ModelState);
        }

        var result = await updateThemeSettingsService.UpdateThemeSettingsAsync(command, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}

using PolyBucket.Api.Features.SystemSettings.UpdateSiteModelSettings.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PolyBucket.Api.Features.SystemSettings.UpdateSiteModelSettings.Http;

[ApiController]
[Route("api/system-settings/model-settings")]
[Authorize(Roles = "Admin")]
public class UpdateSiteModelSettingsController(IUpdateSiteModelSettingsService service) : ControllerBase
{
    private readonly IUpdateSiteModelSettingsService _service = service;

    /// <summary>
    /// Update the site model settings
    /// </summary>
    /// <param name="command">Site model settings configuration</param>
    /// <returns>Update result</returns>
    [HttpPut]
    [ProducesResponseType(200, Type = typeof(UpdateSiteModelSettingsResponse))]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<UpdateSiteModelSettingsResponse>> UpdateSiteModelSettings([FromBody] UpdateSiteModelSettingsCommand command, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!command.IsValid(out var validationResults))
        {
            foreach (var validationResult in validationResults)
            {
                foreach (var memberName in validationResult.MemberNames)
                {
                    ModelState.AddModelError(memberName, validationResult.ErrorMessage ?? "Validation failed");
                }
            }
            return BadRequest(ModelState);
        }

        var result = await _service.UpdateSiteModelSettingsAsync(command, cancellationToken);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }
}

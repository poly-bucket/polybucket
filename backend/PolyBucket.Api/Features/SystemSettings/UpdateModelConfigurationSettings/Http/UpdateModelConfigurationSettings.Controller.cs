using PolyBucket.Api.Features.SystemSettings.UpdateModelConfigurationSettings.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PolyBucket.Api.Features.SystemSettings.UpdateModelConfigurationSettings.Http;

[ApiController]
[Route("api/system-settings/model-configuration")]
[Authorize(Roles = "Admin")]
public class UpdateModelConfigurationSettingsController(IUpdateModelConfigurationSettingsService service) : ControllerBase
{
    private readonly IUpdateModelConfigurationSettingsService _service = service;

    /// <summary>
    /// Update the model configuration settings
    /// </summary>
    /// <param name="command">Model configuration settings</param>
    /// <returns>Update result</returns>
    [HttpPut]
    [ProducesResponseType(200, Type = typeof(UpdateModelConfigurationSettingsResponse))]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<UpdateModelConfigurationSettingsResponse>> UpdateModelConfigurationSettings([FromBody] UpdateModelConfigurationSettingsCommand command, CancellationToken cancellationToken)
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

        var result = await _service.UpdateModelConfigurationSettingsAsync(command, cancellationToken);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }
}

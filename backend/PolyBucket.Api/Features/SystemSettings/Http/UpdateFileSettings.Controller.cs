using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using PolyBucket.Api.Features.SystemSettings.UpdateFileSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.Http;

[ApiController]
[Route("api/system-settings/file-settings")]
[Authorize(Roles = "Admin")]
public class UpdateFileSettingsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Update the file type settings
    /// </summary>
    /// <param name="command">File type settings configuration</param>
    /// <returns>Update result</returns>
    [HttpPut]
    [ProducesResponseType(200, Type = typeof(UpdateFileSettingsResponse))]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<UpdateFileSettingsResponse>> UpdateFileSettings([FromBody] UpdateFileSettingsCommand command)
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

        var result = await _mediator.Send(command);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }
}

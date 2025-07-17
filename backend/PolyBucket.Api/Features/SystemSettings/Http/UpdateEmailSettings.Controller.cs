using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.SystemSettings.Domain;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.SystemSettings.Http;

[ApiController]
[Route("api/system-settings/email")]
[Authorize(Roles = "Admin")]
public class UpdateEmailSettingsController : ControllerBase
{
    private readonly UpdateEmailSettingsCommandHandler _handler;
    private readonly ILogger<UpdateEmailSettingsController> _logger;

    public UpdateEmailSettingsController(
        UpdateEmailSettingsCommandHandler handler,
        ILogger<UpdateEmailSettingsController> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    /// <summary>
    /// Update email service configuration
    /// </summary>
    /// <param name="command">Email settings to update</param>
    /// <returns>Update result</returns>
    [HttpPut]
    [ProducesResponseType(200, Type = typeof(UpdateEmailSettingsResponse))]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> UpdateEmailSettings([FromBody] UpdateEmailSettingsCommand command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _handler.Handle(command);
            
            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email settings");
            return StatusCode(500, new { message = "An unexpected error occurred while updating email settings" });
        }
    }
} 
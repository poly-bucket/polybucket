using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.SystemSettings.Domain;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.SystemSettings.Http;

[ApiController]
[Route("api/system-settings/email")]
[Authorize(Roles = "Admin")]
public class TestEmailConfigurationController(
    TestEmailConfigurationCommandHandler handler,
    ILogger<TestEmailConfigurationController> logger) : ControllerBase
{
    private readonly TestEmailConfigurationCommandHandler _handler = handler;
    private readonly ILogger<TestEmailConfigurationController> _logger = logger;

    /// <summary>
    /// Test email configuration by sending a test email
    /// </summary>
    /// <param name="command">Test email command with email address and optional settings</param>
    /// <returns>Test result</returns>
    [HttpPost("test")]
    [ProducesResponseType(200, Type = typeof(TestEmailConfigurationResponse))]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> TestEmailConfiguration([FromBody] TestEmailConfigurationCommand command)
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
            _logger.LogError(ex, "Error testing email configuration");
            return StatusCode(500, new { message = "An unexpected error occurred while testing email configuration" });
        }
    }
} 
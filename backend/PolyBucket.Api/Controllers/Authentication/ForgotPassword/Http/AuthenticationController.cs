using Api.Controllers.Authentication.ForgotPassword.Domain;
using Api.Controllers.Authentication.Login.Domain;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Authentication.ForgotPassword.Http;

public partial class AuthenticationController(
    IAuthService authService,
    ILogger<AuthenticationController> logger) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly ILogger<AuthenticationController> _logger = logger;

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.ForgotPasswordAsync(request);

            // Always return success even if the email doesn't exist for security reasons
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password request");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}
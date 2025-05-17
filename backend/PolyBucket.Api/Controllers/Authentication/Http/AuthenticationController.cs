using Microsoft.AspNetCore.Mvc;
using Api.Controllers.Authentication.Login.Domain;

namespace Api.Controllers.Authentication.Http;

[ApiController]
[Route("authentication")]
public partial class AuthenticationController(
    CreateUserLoginService createUserLoginService,
    ILogger<AuthenticationController> logger) : ControllerBase
{
    private readonly CreateUserLoginService _createUserLoginService = createUserLoginService;
    private readonly ILogger<AuthenticationController> _logger = logger;

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateUserLoginResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] CreateUserLoginRequest userLoginRequest)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get IP address, with fallback options
            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            if (ipAddress == "::1") // localhost IPv6
                ipAddress = "127.0.0.1";

            var response = await _createUserLoginService.CreateUserLoginAsync(userLoginRequest, ipAddress);

            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("Failed login attempt for user {Email}", userLoginRequest.EmailOrUsername);
            return Unauthorized(new { message = "Invalid username or password" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Email}", userLoginRequest.EmailOrUsername);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Authentication.Domain;
using PolyBucket.Api.Features.Authentication.Http;
// using PolyBucket.Core.Features.Authentication; // This will be created later

namespace PolyBucket.Api.Features.Authentication.Http
{
    [ApiController]
    [Route("api/authentication")]
    public class LoginController : ControllerBase
    {
        // private readonly LoginService _loginService;
        private readonly ILogger<LoginController> _logger;

        public LoginController(ILogger<LoginController> logger) //, LoginService loginService)
        {
            // _loginService = loginService;
            _logger = logger;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                // Get IP address, with fallback options
                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
                if (ipAddress == "::1") // localhost IPv6
                    ipAddress = "127.0.0.1";

                // var response = await _loginService.LoginAsync(
                //     loginRequest.Email,
                //     loginRequest.Password,
                //     ipAddress,
                //     loginRequest.UserAgent
                // );
                
                // return Ok(response);
                await Task.CompletedTask;
                return Ok(new LoginResponse { Token = "dummy-token" });
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("Failed login attempt for user {Email}", loginRequest.Email);
                return Unauthorized(new { message = "Invalid username or password" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Email}", loginRequest.Email);
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }
    }
} 
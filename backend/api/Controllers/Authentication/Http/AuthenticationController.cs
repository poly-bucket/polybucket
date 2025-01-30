using Microsoft.AspNetCore.Mvc;
using Conductors.Login;
using Api.Controllers.Authentication.Domain;

namespace Api.Controllers.Authentication.Http;

[ApiController]
[Route("[controller]")]
public class AuthenticationController(ILoginConductor loginConductor, ILogger<AuthenticationController> logger) : ControllerBase
{
    private readonly ILoginConductor _loginConductor = loginConductor;
    private readonly ILogger<AuthenticationController> _logger = logger;

    [HttpPost("login")]
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

            var (token, user) = await _loginConductor.LoginAsync(userLoginRequest.Email, userLoginRequest.Password, ipAddress, userLoginRequest.UserAgent);

            var response = new LoginResponse
            {
                Token = token,
                User = new UserResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                }
            };

            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("Failed login attempt for user {Email}", userLoginRequest.Email);
            return Unauthorized(new { message = "Invalid username or password" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Email}", userLoginRequest.Email);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }
}
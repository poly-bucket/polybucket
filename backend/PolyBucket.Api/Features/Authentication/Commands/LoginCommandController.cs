using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace PolyBucket.Api.Features.Authentication.Commands
{
    [ApiController]
    [Route("api/auth")]
    public class LoginCommandController : ControllerBase
    {
        private readonly LoginCommandHandler _handler;
        private readonly ILogger<LoginCommandController> _logger;

        public LoginCommandController(LoginCommandHandler handler, ILogger<LoginCommandController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _handler.Handle(command, cancellationToken);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized login attempt for {Email}", command.Email);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", command.Email);
                return StatusCode(500, new { message = "An unexpected error occurred" });
            }
        }
    }
} 
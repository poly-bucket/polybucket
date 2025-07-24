using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Login.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.Login.Http
{
    [ApiController]
    [Route("api/auth")]
    public class LoginController(LoginCommandHandler handler, ILogger<LoginController> logger) : ControllerBase
    {
        private readonly LoginCommandHandler _handler = handler;
        private readonly ILogger<LoginController> _logger = logger;

        [HttpPost("login")]
        [ProducesResponseType(200, Type = typeof(LoginCommandResponse))]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
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
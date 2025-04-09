using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Authentication.Register.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.Register.Http
{
    [ApiController]
    [Route("api/auth")]
    public class RegisterController : ControllerBase
    {
        private readonly RegisterCommandHandler _handler;
        private readonly ILogger<RegisterController> _logger;

        public RegisterController(RegisterCommandHandler handler, ILogger<RegisterController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user account
        /// </summary>
        /// <param name="command">Registration details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Authentication response with tokens</returns>
        [HttpPost("register")]
        [ProducesResponseType(200, Type = typeof(RegisterCommandResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _handler.Handle(command, cancellationToken);
                return Ok(response);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already"))
            {
                _logger.LogWarning(ex, "Registration failed - duplicate data for {Email}", command.Email);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Email}", command.Email);
                return StatusCode(500, new { message = "An unexpected error occurred during registration" });
            }
        }
    }
} 
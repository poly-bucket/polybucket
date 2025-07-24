using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Users.CreateUser.Domain;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.CreateUser.Http
{
    [Authorize]
    [ApiController]
    [Route("api/admin/users")]
    [RequirePermission(PermissionConstants.ADMIN_MANAGE_USERS)]
    public class CreateUserController(CreateUserCommandHandler handler, ILogger<CreateUserController> logger) : ControllerBase
    {
        private readonly CreateUserCommandHandler _handler = handler;
        private readonly ILogger<CreateUserController> _logger = logger;

        /// <summary>
        /// Create a new user account with auto-generated password (Admin only)
        /// </summary>
        /// <param name="command">User creation details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created user details including generated password</returns>
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(CreateUserCommandResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Set internal command properties from request context
                command.UserAgent = Request.Headers["User-Agent"].ToString();
                command.CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                var response = await _handler.Handle(command, cancellationToken);

                // Log the admin action
                var adminUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("Admin user {AdminId} created new user {NewUserId} with email {Email} and role {Role}", 
                    adminUserId, response.UserId, response.Email, response.RoleName);

                return CreatedAtAction(
                    nameof(GetUser), 
                    "GetUser", 
                    new { id = response.UserId }, 
                    response
                );
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already"))
            {
                _logger.LogWarning(ex, "User creation failed - duplicate data for email {Email}", command.Email);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user creation for {Email}", command.Email);
                return StatusCode(500, new { message = "An unexpected error occurred during user creation" });
            }
        }

        // Placeholder for GetUser action - this would typically be in a separate GetUser controller
        private IActionResult GetUser(Guid id)
        {
            return Ok(); // This is just to satisfy the CreatedAtAction reference
        }
    }
} 
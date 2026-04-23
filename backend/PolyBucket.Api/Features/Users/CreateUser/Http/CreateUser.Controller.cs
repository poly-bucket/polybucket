using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Users.CreateUser.Domain;
using PolyBucket.Api.Features.Users.GetUserById.Http;

namespace PolyBucket.Api.Features.Users.CreateUser.Http;

[Authorize]
[ApiController]
[Route("api/admin/users")]
[RequirePermission(PermissionConstants.ADMIN_MANAGE_USERS)]
public class CreateUserController : ControllerBase
{
    private readonly ICreateUserService _createUserService;
    private readonly ILogger<CreateUserController> _logger;

    public CreateUserController(ICreateUserService createUserService, ILogger<CreateUserController> logger)
    {
        _createUserService = createUserService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new user account with auto-generated password (Admin only)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(201, Type = typeof(CreateUserCommandResponse))]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            command.UserAgent = Request.Headers["User-Agent"].ToString();
            command.CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var response = await _createUserService.CreateUserAsync(command, cancellationToken);

            var adminUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("Admin user {AdminId} created new user {NewUserId} with email {Email} and role {Role}",
                adminUserId, response.UserId, response.Email, response.RoleName);

            return CreatedAtAction(
                nameof(GetUserByIdController.GetUserById),
                "GetUserById",
                new { id = response.UserId },
                response);
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
}

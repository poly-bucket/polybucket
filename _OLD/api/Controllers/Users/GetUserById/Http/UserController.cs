using Api.Controllers.Users.GetUser.Domain;
using Api.Controllers.Users.GetUserById.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Users.GetUser.Http;

[ApiController]
[Route("Users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly GetUserByIdService _getUserByIdService;
    private readonly ILogger<UserController> _logger;

    public UserController(GetUserByIdService getUserByIdService, ILogger<UserController> logger)
    {
        _getUserByIdService = getUserByIdService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(200, Type = typeof(GetUserByIdResponse))]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    [Produces("application/json")]
    public async Task<ActionResult<GetUserByIdResponse>> GetUserById([FromRoute] Guid id)
    {
        try
        {
            var user = await _getUserByIdService.GetUserByIdAsync(id);
            return user;
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("User with ID {Id} not found", id);
            return NotFound(new { message = $"User with ID {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the user" });
        }
    }
}
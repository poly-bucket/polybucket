using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Users.GetUserById.Domain;
using Microsoft.Extensions.Logging;

namespace PolyBucket.Api.Features.Users.GetUserById.Http;

[ApiController]
[Route("api/users")]
[Authorize]
public class GetUserByIdController : ControllerBase
{
    private readonly IGetUserByIdService _getUserByIdService;
    private readonly ILogger<GetUserByIdController> _logger;

    public GetUserByIdController(IGetUserByIdService getUserByIdService, ILogger<GetUserByIdController> logger)
    {
        _getUserByIdService = getUserByIdService;
        _logger = logger;
    }

    /// <summary>
    /// Get a user by id
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(200, Type = typeof(GetUserByIdResult))]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    [Produces("application/json")]
    public async Task<ActionResult<GetUserByIdResult>> GetUserById([FromRoute] Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _getUserByIdService.GetUserByIdAsync(id, cancellationToken);
            return user;
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user with ID {Id}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the user" });
        }
    }
}

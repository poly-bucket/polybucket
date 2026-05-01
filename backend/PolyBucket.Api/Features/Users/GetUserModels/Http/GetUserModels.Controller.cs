using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Users.GetUserModels.Domain;

namespace PolyBucket.Api.Features.Users.GetUserModels.Http;

[ApiController]
[Route("api/users/models")]
public class GetUserModelsController : ControllerBase
{
    private readonly IGetUserModelsService _getUserModelsService;
    private readonly ILogger<GetUserModelsController> _logger;

    public GetUserModelsController(IGetUserModelsService getUserModelsService, ILogger<GetUserModelsController> logger)
    {
        _getUserModelsService = getUserModelsService;
        _logger = logger;
    }

    /// <summary>
    /// Get user models by username.
    /// Returns public models for regular viewers and includes private models for the owner and admins.
    /// </summary>
    [HttpGet("{username}/models/public")]
    [ProducesResponseType(200, Type = typeof(GetUserModelsResult))]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<GetUserModelsResult>> GetUserPublicModels(
        [FromRoute] string username,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? requestingUserId = Guid.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : null;
            var query = new GetUserModelsQuery
            {
                Username = username,
                Page = page,
                PageSize = pageSize,
                RequestingUserId = requestingUserId,
                IsRequestingUserAdmin = User.IsInRole("Admin"),
                IncludePrivate = User.IsInRole("Admin")
            };

            var response = await _getUserModelsService.GetUserPublicModelsAsync(query, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User models not found for username {Username}", username);
            return NotFound(new { message = "User models not found" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized models access for username {Username}", username);
            return StatusCode(403, new { message = "User profile is private" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user models for username {Username}", username);
            return StatusCode(500, new { message = "An error occurred while retrieving user models" });
        }
    }
}

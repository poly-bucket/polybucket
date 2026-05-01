using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Users.GetPublicUserCollections.Domain;

namespace PolyBucket.Api.Features.Users.GetPublicUserCollections.Http;

[ApiController]
[Route("api/users")]
public class GetPublicUserCollectionsController : ControllerBase
{
    private readonly IGetPublicUserCollectionsService _getPublicUserCollectionsService;
    private readonly ILogger<GetPublicUserCollectionsController> _logger;

    public GetPublicUserCollectionsController(
        IGetPublicUserCollectionsService getPublicUserCollectionsService,
        ILogger<GetPublicUserCollectionsController> logger)
    {
        _getPublicUserCollectionsService = getPublicUserCollectionsService;
        _logger = logger;
    }

    /// <summary>
    /// Get profile collections by user ID.
    /// Returns public collections for regular viewers and includes private collections for owner/admin viewers.
    /// </summary>
    [HttpGet("{userId}/collections/public")]
    [ProducesResponseType(200, Type = typeof(GetPublicUserCollectionsResult))]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<GetPublicUserCollectionsResult>> GetPublicUserCollections(
        [FromRoute] Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchQuery = null,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (page < 1)
            {
                return BadRequest("Page number must be greater than 0");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? requestingUserId = Guid.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : null;
            var query = new GetPublicUserCollectionsQuery
            {
                UserId = userId,
                Page = page,
                PageSize = pageSize,
                SearchQuery = searchQuery,
                SortBy = sortBy,
                SortDescending = sortDescending,
                RequestingUserId = requestingUserId,
                IsRequestingUserAdmin = User.IsInRole("Admin")
            };

            var response = await _getPublicUserCollectionsService.GetPublicUserCollectionsAsync(query, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found while retrieving collections for user {UserId}", userId);
            return NotFound(new { message = "User not found" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized collections access for user {UserId}", userId);
            return StatusCode(403, new { message = "User profile is private" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving public collections for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving the user collections" });
        }
    }

    /// <summary>
    /// Get profile collections by username.
    /// Returns public collections for regular viewers and includes private collections for owner/admin viewers.
    /// </summary>
    [HttpGet("profile/{username}/collections/public")]
    [ProducesResponseType(200, Type = typeof(GetPublicUserCollectionsResult))]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<GetPublicUserCollectionsResult>> GetPublicUserCollectionsByUsername(
        [FromRoute] string username,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchQuery = null,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (page < 1)
            {
                return BadRequest("Page number must be greater than 0");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? requestingUserId = Guid.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : null;
            var query = new GetPublicUserCollectionsQuery
            {
                Username = username,
                Page = page,
                PageSize = pageSize,
                SearchQuery = searchQuery,
                SortBy = sortBy,
                SortDescending = sortDescending,
                RequestingUserId = requestingUserId,
                IsRequestingUserAdmin = User.IsInRole("Admin")
            };

            var response = await _getPublicUserCollectionsService.GetPublicUserCollectionsAsync(query, cancellationToken);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "User not found while retrieving collections for username {Username}", username);
            return NotFound(new { message = "User not found" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized collections access for username {Username}", username);
            return StatusCode(403, new { message = "User profile is private" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving public collections for username {Username}", username);
            return StatusCode(500, new { message = "An error occurred while retrieving the user collections" });
        }
    }
}

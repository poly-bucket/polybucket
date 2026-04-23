using System;
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

    [HttpGet("{userId}/collections/public")]
    [ProducesResponseType(200, Type = typeof(GetPublicUserCollectionsResult))]
    [ProducesResponseType(400)]
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

            var query = new GetPublicUserCollectionsQuery
            {
                UserId = userId,
                Page = page,
                PageSize = pageSize,
                SearchQuery = searchQuery,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var response = await _getPublicUserCollectionsService.GetPublicUserCollectionsAsync(query, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving public collections for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving the user collections" });
        }
    }

    [HttpGet("profile/{username}/collections/public")]
    [ProducesResponseType(200, Type = typeof(GetPublicUserCollectionsResult))]
    [ProducesResponseType(400)]
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

            var query = new GetPublicUserCollectionsQuery
            {
                UserId = Guid.Empty,
                Page = page,
                PageSize = pageSize,
                SearchQuery = searchQuery,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var response = await _getPublicUserCollectionsService.GetPublicUserCollectionsAsync(query, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving public collections for username {Username}", username);
            return StatusCode(500, new { message = "An error occurred while retrieving the user collections" });
        }
    }
}

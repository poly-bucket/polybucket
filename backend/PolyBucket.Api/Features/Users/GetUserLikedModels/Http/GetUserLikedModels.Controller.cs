using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Users.GetUserLikedModels.Domain;

namespace PolyBucket.Api.Features.Users.GetUserLikedModels.Http;

[ApiController]
[Route("api/users")]
public class GetUserLikedModelsController : ControllerBase
{
    private readonly IGetUserLikedModelsService _getUserLikedModelsService;
    private readonly ILogger<GetUserLikedModelsController> _logger;

    public GetUserLikedModelsController(IGetUserLikedModelsService getUserLikedModelsService, ILogger<GetUserLikedModelsController> logger)
    {
        _getUserLikedModelsService = getUserLikedModelsService;
        _logger = logger;
    }

    [HttpGet("{userId}/liked-models")]
    [ProducesResponseType(200, Type = typeof(GetUserLikedModelsResult))]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<GetUserLikedModelsResult>> GetUserLikedModels(
        [FromRoute] Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchQuery = null,
        [FromQuery] string? sortBy = "LikedAt",
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

            var query = new GetUserLikedModelsQuery
            {
                UserId = userId,
                Page = page,
                PageSize = pageSize,
                SearchQuery = searchQuery,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var response = await _getUserLikedModelsService.GetUserLikedModelsAsync(query, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving liked models for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving the user liked models" });
        }
    }

    [HttpGet("profile/{username}/liked-models")]
    [ProducesResponseType(200, Type = typeof(GetUserLikedModelsResult))]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<GetUserLikedModelsResult>> GetUserLikedModelsByUsername(
        [FromRoute] string username,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchQuery = null,
        [FromQuery] string? sortBy = "LikedAt",
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

            var query = new GetUserLikedModelsQuery
            {
                UserId = Guid.Empty,
                Page = page,
                PageSize = pageSize,
                SearchQuery = searchQuery,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var response = await _getUserLikedModelsService.GetUserLikedModelsAsync(query, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving liked models for username {Username}", username);
            return StatusCode(500, new { message = "An error occurred while retrieving the user liked models" });
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Users.GetUserComments.Domain;

namespace PolyBucket.Api.Features.Users.GetUserComments.Http;

[ApiController]
[Route("api/users")]
public class GetUserCommentsController : ControllerBase
{
    private readonly IGetUserCommentsService _getUserCommentsService;
    private readonly ILogger<GetUserCommentsController> _logger;

    public GetUserCommentsController(IGetUserCommentsService getUserCommentsService, ILogger<GetUserCommentsController> logger)
    {
        _getUserCommentsService = getUserCommentsService;
        _logger = logger;
    }

    [HttpGet("{userId}/comments")]
    [ProducesResponseType(200, Type = typeof(GetUserCommentsResult))]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<GetUserCommentsResult>> GetUserComments(
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

            var query = new GetUserCommentsQuery
            {
                UserId = userId,
                Page = page,
                PageSize = pageSize,
                SearchQuery = searchQuery,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var response = await _getUserCommentsService.GetUserCommentsAsync(query, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving comments for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving the user comments" });
        }
    }

    [HttpGet("profile/{username}/comments")]
    [ProducesResponseType(200, Type = typeof(GetUserCommentsResult))]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<GetUserCommentsResult>> GetUserCommentsByUsername(
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

            var query = new GetUserCommentsQuery
            {
                UserId = Guid.Empty,
                Page = page,
                PageSize = pageSize,
                SearchQuery = searchQuery,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var response = await _getUserCommentsService.GetUserCommentsAsync(query, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving comments for username {Username}", username);
            return StatusCode(500, new { message = "An error occurred while retrieving the user comments" });
        }
    }
}

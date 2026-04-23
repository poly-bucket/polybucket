using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Users.GetUserPrinters.Domain;

namespace PolyBucket.Api.Features.Users.GetUserPrinters.Http;

[ApiController]
[Route("api/users")]
public class GetUserPrintersController : ControllerBase
{
    private readonly IGetUserPrintersService _getUserPrintersService;
    private readonly ILogger<GetUserPrintersController> _logger;

    public GetUserPrintersController(IGetUserPrintersService getUserPrintersService, ILogger<GetUserPrintersController> logger)
    {
        _getUserPrintersService = getUserPrintersService;
        _logger = logger;
    }

    /// <summary>
    /// Get user printers and filaments by user ID
    /// </summary>
    [HttpGet("{userId}/printers")]
    [ProducesResponseType(200, Type = typeof(GetUserPrintersResult))]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<GetUserPrintersResult>> GetUserPrinters(
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

            var query = new GetUserPrintersQuery
            {
                UserId = userId,
                Page = page,
                PageSize = pageSize,
                SearchQuery = searchQuery,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var response = await _getUserPrintersService.GetUserPrintersAsync(query, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving printers for user {UserId}", userId);
            return StatusCode(500, new { message = "An error occurred while retrieving the user printers" });
        }
    }

    /// <summary>
    /// Get user printers and filaments by username
    /// </summary>
    [HttpGet("profile/{username}/printers")]
    [ProducesResponseType(200, Type = typeof(GetUserPrintersResult))]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<GetUserPrintersResult>> GetUserPrintersByUsername(
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

            var query = new GetUserPrintersQuery
            {
                UserId = Guid.Empty,
                Page = page,
                PageSize = pageSize,
                SearchQuery = searchQuery,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var response = await _getUserPrintersService.GetUserPrintersAsync(query, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving printers for username {Username}", username);
            return StatusCode(500, new { message = "An error occurred while retrieving the user printers" });
        }
    }
}

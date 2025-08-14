using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Users.Queries.GetUserPrinters;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.Queries.GetUserPrinters
{
    [ApiController]
    [Route("api/users")]
    public class GetUserPrintersController : ControllerBase
    {
        private readonly GetUserPrintersQueryHandler _handler;
        private readonly ILogger<GetUserPrintersController> _logger;

        public GetUserPrintersController(GetUserPrintersQueryHandler handler, ILogger<GetUserPrintersController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        /// <summary>
        /// Get user printers and filaments by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <param name="searchQuery">Search term</param>
        /// <param name="sortBy">Sort field</param>
        /// <param name="sortDescending">Sort order</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User printers and filaments information</returns>
        [HttpGet("{userId}/printers")]
        [ProducesResponseType(200, Type = typeof(GetUserPrintersResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<GetUserPrintersResponse>> GetUserPrinters(
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

                var response = await _handler.Handle(query, cancellationToken);
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
        /// <param name="username">Username</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <param name="searchQuery">Search term</param>
        /// <param name="sortBy">Sort field</param>
        /// <param name="sortDescending">Sort order</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User printers and filaments information</returns>
        [HttpGet("profile/{username}/printers")]
        [ProducesResponseType(200, Type = typeof(GetUserPrintersResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<GetUserPrintersResponse>> GetUserPrintersByUsername(
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

                // TODO: Get user ID from username first
                var query = new GetUserPrintersQuery
                {
                    UserId = Guid.Empty, // This should be resolved from username
                    Page = page,
                    PageSize = pageSize,
                    SearchQuery = searchQuery,
                    SortBy = sortBy,
                    SortDescending = sortDescending
                };

                var response = await _handler.Handle(query, cancellationToken);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving printers for username {Username}", username);
                return StatusCode(500, new { message = "An error occurred while retrieving the user printers" });
            }
        }
    }
}

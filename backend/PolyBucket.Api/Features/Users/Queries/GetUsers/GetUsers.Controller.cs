using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Users.Queries.GetUsers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Users.Queries.GetUsers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize]
    [RequirePermission(PermissionConstants.ADMIN_MANAGE_USERS)]
    public class GetUsersController : ControllerBase
    {
        private readonly GetUsersQueryHandler _handler;
        private readonly ILogger<GetUsersController> _logger;

        public GetUsersController(GetUsersQueryHandler handler, ILogger<GetUsersController> logger)
        {
            _handler = handler;
            _logger = logger;
        }

        /// <summary>
        /// Get all users with filtering, sorting, and pagination (Admin only)
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 20, max: 100)</param>
        /// <param name="searchQuery">Search term for username, email, first name, or last name</param>
        /// <param name="roleFilter">Filter by role name</param>
        /// <param name="statusFilter">Filter by status (Active, Banned)</param>
        /// <param name="sortBy">Sort by field (username, email, role, lastlogin, createdat)</param>
        /// <param name="sortDescending">Sort in descending order (default: true)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated list of users with filtering and sorting</returns>
        [HttpGet]
        [ProducesResponseType(typeof(GetUsersResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<GetUsersResponse>> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchQuery = null,
            [FromQuery] string? roleFilter = null,
            [FromQuery] string? statusFilter = null,
            [FromQuery] string? sortBy = "CreatedAt",
            [FromQuery] bool sortDescending = true,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate pagination parameters
                if (page < 1)
                {
                    return BadRequest("Page number must be greater than 0");
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    return BadRequest("Page size must be between 1 and 100");
                }

                // Validate sort field
                var validSortFields = new[] { "username", "email", "role", "lastlogin", "createdat" };
                if (!string.IsNullOrEmpty(sortBy) && !validSortFields.Contains(sortBy.ToLower()))
                {
                    return BadRequest($"Invalid sort field. Valid options are: {string.Join(", ", validSortFields)}");
                }

                var query = new GetUsersQuery
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchQuery = searchQuery,
                    RoleFilter = roleFilter,
                    StatusFilter = statusFilter,
                    SortBy = sortBy,
                    SortDescending = sortDescending
                };

                var response = await _handler.Handle(query, cancellationToken);

                _logger.LogInformation("Admin user retrieved {UserCount} users (page {Page} of {TotalPages})", 
                    response.Users.Count(), response.Page, response.TotalPages);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving users");
                return StatusCode(500, new { message = "An error occurred while retrieving users" });
            }
        }
    }
}

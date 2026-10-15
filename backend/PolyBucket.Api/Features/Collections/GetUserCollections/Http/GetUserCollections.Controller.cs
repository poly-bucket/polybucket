using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.Collections.GetUserCollections.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.GetUserCollections.Http
{
    [ApiController]
    [Route("api/collections")]
    public class GetUserCollectionsController(IGetUserCollectionsService getUserCollectionsService) : ControllerBase
    {
        private readonly IGetUserCollectionsService _getUserCollectionsService = getUserCollectionsService;

        /// <summary>
        /// Lists collections owned by the signed-in user.
        /// </summary>
        [HttpGet("mine")]
        [Authorize]
        [ProducesResponseType(typeof(GetUserCollectionsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<GetUserCollectionsResponseDto>> GetCurrentUserCollections(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? searchQuery = null,
            CancellationToken cancellationToken = default)
        {
            var userIdClaim = User.FindUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid authentication token");
            }

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 12;

            var response = await _getUserCollectionsService.GetUserCollectionsAsync(new GetUserCollectionsQuery
            {
                TargetUserId = userId,
                RequestingUserId = userId,
                IsRequestingUserAdmin = User.IsInRole("Admin"),
                Page = page,
                PageSize = pageSize,
                SearchQuery = searchQuery
            }, cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Lists public collections for a user profile. Private collections are included only for the owner or an admin.
        /// </summary>
        [HttpGet("user/{userId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(GetUserCollectionsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetUserCollectionsResponseDto>> GetCollectionsByUserId(
            Guid userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? searchQuery = null,
            CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 12;

            Guid? requestingUserId = null;
            var userIdClaim = User.FindUserIdClaim();
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsed))
            {
                requestingUserId = parsed;
            }

            try
            {
                var response = await _getUserCollectionsService.GetUserCollectionsAsync(new GetUserCollectionsQuery
                {
                    TargetUserId = userId,
                    RequestingUserId = requestingUserId,
                    IsRequestingUserAdmin = User.IsInRole("Admin"),
                    Page = page,
                    PageSize = pageSize,
                    SearchQuery = searchQuery
                }, cancellationToken);

                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "User not found" });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, new { message = "User profile is private" });
            }
        }
    }
} 
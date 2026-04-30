using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.Collections.GetFavoriteCollections.Repository;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.GetFavoriteCollections.Http
{
    /// <summary>
    /// Controller for retrieving favorite collections
    /// </summary>
    [ApiController]
    [Route("api/collections")]
    public class GetFavoriteCollectionsController : ControllerBase
    {
        private readonly IGetFavoriteCollectionsRepository _repository;

        public GetFavoriteCollectionsController(IGetFavoriteCollectionsRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Get current user's favorite collections
        /// </summary>
        /// <returns>List of favorite collections ordered by display order and name</returns>
        /// <response code="200">Returns the list of favorite collections</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="500">If an error occurred while retrieving the collections</response>
        [HttpGet("favorites")]
        [Authorize]
        public async Task<IActionResult> GetFavoriteCollections()
        {
            try
            {
                var userIdClaim = User.FindUserIdClaim();
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("Invalid authentication token");
                }

                var collections = await _repository.GetFavoriteCollectionsByUserIdAsync(userId);
                
                return Ok(new { collections });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving favorite collections" });
            }
        }
    }
}

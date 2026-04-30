using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.Collections.GetUserCollections.Repository;
using System;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Collections.GetUserCollections.Http
{
    [ApiController]
    [Route("api/collections")]
    public class GetUserCollectionsController : ControllerBase
    {
        private readonly ICollectionRepository _collectionRepository;

        public GetUserCollectionsController(ICollectionRepository collectionRepository)
        {
            _collectionRepository = collectionRepository;
        }

        [HttpGet("mine")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserCollections(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? searchQuery = null)
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindUserIdClaim();
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("Invalid authentication token");
                }

                // Validate parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 12;
                
                var (collections, totalCount) = await _collectionRepository.GetCollectionsByUserIdAsync(userId, page, pageSize, searchQuery);
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                
                return Ok(new { 
                    collections = collections,
                    totalCount = totalCount,
                    page = page,
                    pageSize = pageSize,
                    totalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the collections" });
            }
        }

        [HttpGet("user/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCollectionsByUserId(
            Guid userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] string? searchQuery = null)
        {
            try
            {
                // Validate parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 12;
                
                var (collections, totalCount) = await _collectionRepository.GetCollectionsByUserIdAsync(userId, page, pageSize, searchQuery);
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                
                return Ok(new { 
                    collections = collections,
                    totalCount = totalCount,
                    page = page,
                    pageSize = pageSize,
                    totalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the collections" });
            }
        }
    }
} 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Models.GetModelByUserId.Domain;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace PolyBucket.Api.Features.Models.GetModelByUserId.Http
{
    [ApiController]
    [Route("api/models")]
    public class GetModelByUserIdController : ControllerBase
    {
        private readonly IGetModelByUserIdService _getModelByUserIdService;
        private readonly ILogger<GetModelByUserIdController> _logger;

        public GetModelByUserIdController(IGetModelByUserIdService getModelByUserIdService, ILogger<GetModelByUserIdController> logger)
        {
            _getModelByUserIdService = getModelByUserIdService;
            _logger = logger;
        }

        [HttpGet("user/{userId}")]
        [Authorize]
        [ProducesResponseType(typeof(GetModelByUserIdResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<GetModelByUserIdResponse>> GetModelsByUserId(Guid userId, [FromQuery] GetModelByUserIdRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _getModelByUserIdService.GetModelsByUserIdAsync(userId, request, User, cancellationToken);
                return Ok(response);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error getting models for user {UserId}: {Message}", userId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get models for user {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving the models");
            }
        }

        [HttpGet("user/{userId}/public")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPublicModelsByUserId(
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
                
                var request = new GetModelByUserIdRequest
                {
                    Page = page,
                    Take = pageSize,
                    IncludeDeleted = false,
                    IncludePrivate = false
                };
                
                var response = await _getModelByUserIdService.GetModelsByUserIdAsync(userId, request, null, CancellationToken.None);
                
                // Apply search filter if provided
                var filteredModels = response.Models;
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    filteredModels = filteredModels.Where(m => 
                        m.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) || 
                        (m.Description != null && m.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }
                
                var totalCount = filteredModels.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                
                // Apply pagination to filtered results
                var paginatedModels = filteredModels
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                return Ok(new { 
                    models = paginatedModels,
                    totalCount = totalCount,
                    page = page,
                    pageSize = pageSize,
                    totalPages = totalPages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get public models for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving the models" });
            }
        }
    }
} 
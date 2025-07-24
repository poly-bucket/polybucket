using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Models.GetModelByUserId.Domain;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace PolyBucket.Api.Features.Models.GetModelByUserId.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    public class GetModelByUserIdController(IGetModelByUserIdService getModelByUserIdService, ILogger<GetModelByUserIdController> logger) : ControllerBase
    {
        private readonly IGetModelByUserIdService _getModelByUserIdService = getModelByUserIdService;
        private readonly ILogger<GetModelByUserIdController> _logger = logger;

        [HttpGet("user/{userId}")]
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
    }
} 
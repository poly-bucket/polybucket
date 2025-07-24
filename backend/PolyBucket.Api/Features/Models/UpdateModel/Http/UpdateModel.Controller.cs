using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Models.UpdateModel.Domain;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace PolyBucket.Api.Features.Models.UpdateModel.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    [RequirePermission(PermissionRequirement.Any, PermissionConstants.MODEL_EDIT_OWN, PermissionConstants.MODEL_EDIT_ANY)]
    public class UpdateModelController(IUpdateModelService updateModelService, ILogger<UpdateModelController> logger) : ControllerBase
    {
        private readonly IUpdateModelService _updateModelService = updateModelService;
        private readonly ILogger<UpdateModelController> _logger = logger;

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UpdateModelResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UpdateModelResponse>> UpdateModel(Guid id, [FromBody] UpdateModelRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _updateModelService.UpdateModelAsync(id, request, User, cancellationToken);
                return Ok(response);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error updating model {ModelId}: {Message}", id, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (ModelNotFoundException)
            {
                return NotFound("Model not found");
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update model {ModelId}", id);
                return StatusCode(500, "An error occurred while updating the model");
            }
        }
    }
} 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Models.UpdateModelVersion.Domain;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace PolyBucket.Api.Features.Models.UpdateModelVersion.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    [RequirePermission(PermissionRequirement.Any, PermissionConstants.MODEL_EDIT_OWN, PermissionConstants.MODEL_EDIT_ANY)]
    public class UpdateModelVersionController(IUpdateModelVersionService updateModelVersionService, ILogger<UpdateModelVersionController> logger) : ControllerBase
    {
        private readonly IUpdateModelVersionService _updateModelVersionService = updateModelVersionService;
        private readonly ILogger<UpdateModelVersionController> _logger = logger;

        [HttpPut("{modelId}/versions/{versionId}")]
        [ProducesResponseType(typeof(UpdateModelVersionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UpdateModelVersionResponse>> UpdateModelVersion(Guid modelId, Guid versionId, [FromBody] UpdateModelVersionRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _updateModelVersionService.UpdateModelVersionAsync(modelId, versionId, request, User, cancellationToken);
                return Ok(response);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error updating model version {ModelId}/{VersionId}: {Message}", modelId, versionId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (ModelNotFoundException)
            {
                return NotFound("Model not found");
            }
            catch (ModelVersionNotFoundException)
            {
                return NotFound("Model version not found");
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update model version {ModelId}/{VersionId}", modelId, versionId);
                return StatusCode(500, "An error occurred while updating the model version");
            }
        }
    }
} 
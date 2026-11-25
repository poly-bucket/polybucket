using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Models.DeleteModelVersion.Domain;
using PolyBucket.Api.Features.Models.Http;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace PolyBucket.Api.Features.Models.DeleteModelVersion.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    [RequirePermission(PermissionRequirement.Any, PermissionConstants.MODEL_DELETE_OWN, PermissionConstants.MODEL_DELETE_ANY)]
    public class DeleteModelVersionController(IDeleteModelVersionService deleteModelVersionService, ILogger<DeleteModelVersionController> logger) : ControllerBase
    {
        private readonly IDeleteModelVersionService _deleteModelVersionService = deleteModelVersionService;
        private readonly ILogger<DeleteModelVersionController> _logger = logger;

        /// <summary>Deletes a model version.</summary>
        /// <response code="403">Model versioning is disabled on this site.</response>
        [HttpDelete("{modelId}/versions/{versionId}")]
        [ServiceFilter(typeof(RequireModelVersioningEnabledFilter))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteModelVersion(Guid modelId, Guid versionId, CancellationToken cancellationToken)
        {
            try
            {
                await _deleteModelVersionService.DeleteModelVersionAsync(modelId, versionId, User, cancellationToken);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error deleting model version {ModelId}/{VersionId}: {Message}", modelId, versionId, ex.Message);
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
                _logger.LogError(ex, "Failed to delete model version {ModelId}/{VersionId}", modelId, versionId);
                return StatusCode(500, "An error occurred while deleting the model version");
            }
        }
    }
} 
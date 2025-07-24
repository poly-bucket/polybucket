using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Models.DeleteModel.Domain;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace PolyBucket.Api.Features.Models.DeleteModel.Http
{
    [Authorize]
    [ApiController]
    [Route("api/models")]
    [RequirePermission(PermissionRequirement.Any, PermissionConstants.MODEL_DELETE_OWN, PermissionConstants.MODEL_DELETE_ANY)]
    public class DeleteModelController(IDeleteModelService deleteModelService, ILogger<DeleteModelController> logger) : ControllerBase
    {
        private readonly IDeleteModelService _deleteModelService = deleteModelService;
        private readonly ILogger<DeleteModelController> _logger = logger;

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteModel(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await _deleteModelService.DeleteModelAsync(id, User, cancellationToken);
                return NoContent();
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation error deleting model {ModelId}: {Message}", id, ex.Message);
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
                _logger.LogError(ex, "Failed to delete model {ModelId}", id);
                return StatusCode(500, "An error occurred while deleting the model");
            }
        }
    }
} 
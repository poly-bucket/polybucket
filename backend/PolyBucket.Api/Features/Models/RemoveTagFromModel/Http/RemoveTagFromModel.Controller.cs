using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Models.RemoveTagFromModel.Domain;

namespace PolyBucket.Api.Features.Models.RemoveTagFromModel.Http;

[Authorize]
[ApiController]
[Route("api/models")]
[RequirePermission(PermissionRequirement.Any, PermissionConstants.MODEL_EDIT_OWN, PermissionConstants.MODEL_EDIT_ANY)]
public class RemoveTagFromModelController(IRemoveTagFromModelService removeTagFromModelService, ILogger<RemoveTagFromModelController> logger) : ControllerBase
{
    private readonly IRemoveTagFromModelService _removeTagFromModelService = removeTagFromModelService;
    private readonly ILogger<RemoveTagFromModelController> _logger = logger;

    /// <summary>
    /// Unlinks a tag from a model. The tag record is kept for other models.
    /// </summary>
    /// <param name="id">Model ID</param>
    /// <param name="tagId">Tag ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpDelete("{id}/tags/{tagId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveTagFromModel(Guid id, Guid tagId, CancellationToken cancellationToken)
    {
        try
        {
            await _removeTagFromModelService.RemoveTagFromModelAsync(id, tagId, User, cancellationToken);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error removing tag from model {ModelId}: {Message}", id, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (ModelNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove tag from model {ModelId}", id);
            return StatusCode(500, "An error occurred while removing the tag");
        }
    }
}

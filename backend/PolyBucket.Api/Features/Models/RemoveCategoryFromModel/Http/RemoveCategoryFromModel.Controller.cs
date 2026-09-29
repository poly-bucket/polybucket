using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Models.RemoveCategoryFromModel.Domain;

namespace PolyBucket.Api.Features.Models.RemoveCategoryFromModel.Http;

[Authorize]
[ApiController]
[Route("api/models")]
[RequirePermission(PermissionRequirement.Any, PermissionConstants.MODEL_EDIT_OWN, PermissionConstants.MODEL_EDIT_ANY)]
public class RemoveCategoryFromModelController(IRemoveCategoryFromModelService removeCategoryFromModelService, ILogger<RemoveCategoryFromModelController> logger) : ControllerBase
{
    private readonly IRemoveCategoryFromModelService _removeCategoryFromModelService = removeCategoryFromModelService;
    private readonly ILogger<RemoveCategoryFromModelController> _logger = logger;

    /// <summary>
    /// Unlinks a category from a model. The category record is kept.
    /// </summary>
    /// <param name="id">Model ID</param>
    /// <param name="categoryId">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpDelete("{id}/categories/{categoryId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveCategoryFromModel(Guid id, Guid categoryId, CancellationToken cancellationToken)
    {
        try
        {
            await _removeCategoryFromModelService.RemoveCategoryFromModelAsync(id, categoryId, User, cancellationToken);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error removing category from model {ModelId}: {Message}", id, ex.Message);
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
            _logger.LogError(ex, "Failed to remove category from model {ModelId}", id);
            return StatusCode(500, "An error occurred while removing the category");
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Models.AddCategoryToModel.Domain;

namespace PolyBucket.Api.Features.Models.AddCategoryToModel.Http;

[Authorize]
[ApiController]
[Route("api/models")]
[RequirePermission(PermissionRequirement.Any, PermissionConstants.MODEL_EDIT_OWN, PermissionConstants.MODEL_EDIT_ANY)]
public class AddCategoryToModelController(IAddCategoryToModelService addCategoryToModelService, ILogger<AddCategoryToModelController> logger) : ControllerBase
{
    private readonly IAddCategoryToModelService _addCategoryToModelService = addCategoryToModelService;
    private readonly ILogger<AddCategoryToModelController> _logger = logger;

    /// <summary>
    /// Links an existing category to a model.
    /// </summary>
    /// <param name="id">Model ID</param>
    /// <param name="categoryId">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpPost("{id}/categories/{categoryId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AddCategoryToModel(Guid id, Guid categoryId, CancellationToken cancellationToken)
    {
        try
        {
            await _addCategoryToModelService.AddCategoryToModelAsync(id, categoryId, User, cancellationToken);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error adding category to model {ModelId}: {Message}", id, ex.Message);
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
            _logger.LogError(ex, "Failed to add category to model {ModelId}", id);
            return StatusCode(500, "An error occurred while adding the category");
        }
    }
}

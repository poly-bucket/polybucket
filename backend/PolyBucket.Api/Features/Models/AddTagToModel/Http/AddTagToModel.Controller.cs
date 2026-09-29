using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Models.AddTagToModel.Domain;

namespace PolyBucket.Api.Features.Models.AddTagToModel.Http;

[Authorize]
[ApiController]
[Route("api/models")]
[RequirePermission(PermissionRequirement.Any, PermissionConstants.MODEL_EDIT_OWN, PermissionConstants.MODEL_EDIT_ANY)]
public class AddTagToModelController(IAddTagToModelService addTagToModelService, ILogger<AddTagToModelController> logger) : ControllerBase
{
    private readonly IAddTagToModelService _addTagToModelService = addTagToModelService;
    private readonly ILogger<AddTagToModelController> _logger = logger;

    /// <summary>
    /// Links a tag to a model. Reuses an existing tag with the same name.
    /// </summary>
    /// <param name="id">Model ID</param>
    /// <param name="tagName">Tag name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpPost("{id}/tags")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AddTagToModel(Guid id, [FromBody] string tagName, CancellationToken cancellationToken)
    {
        try
        {
            await _addTagToModelService.AddTagToModelAsync(id, tagName, User, cancellationToken);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error adding tag to model {ModelId}: {Message}", id, ex.Message);
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
            _logger.LogError(ex, "Failed to add tag to model {ModelId}", id);
            return StatusCode(500, "An error occurred while adding the tag");
        }
    }
}

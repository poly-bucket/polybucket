using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ModelModeration.ModeratorEditModel.Domain;
using PolyBucket.Api.Features.ModelModeration.ModeratorEditModel.Http;

namespace PolyBucket.Api.Features.ModelModeration.GetModelForModeration.Http;

[Authorize]
[ApiController]
[Route("api/moderation/models")]
[RequirePermission(PermissionConstants.MODERATION_EDIT_MODELS)]
public class GetModelForModerationController(IModeratorEditModelService service) : ControllerBase
{
    /// <summary>
    /// Get model details for moderation editing.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ModeratorEditModelResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ModeratorEditModelResponseDto>> GetModelForModeration(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await service.GetModelForModerationAsync(id, cancellationToken);
        if (model == null)
        {
            return NotFound("Model not found");
        }

        return Ok(model);
    }
}

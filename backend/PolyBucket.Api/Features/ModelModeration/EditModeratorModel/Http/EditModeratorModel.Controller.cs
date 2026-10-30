using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ModelModeration.ModeratorEditModel.Domain;
using PolyBucket.Api.Features.ModelModeration.ModeratorEditModel.Http;
using System.Security.Claims;

namespace PolyBucket.Api.Features.ModelModeration.EditModeratorModel.Http;

[Authorize]
[ApiController]
[Route("api/moderation/models")]
[RequirePermission(PermissionConstants.MODERATION_EDIT_MODELS)]
public class EditModeratorModelController(IModeratorEditModelService service) : ControllerBase
{
    /// <summary>
    /// Allows moderators to edit model details and metadata.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ModeratorEditModelResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ModeratorEditModelResponseDto>> EditModel(
        Guid id,
        [FromBody] ModeratorEditModelRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var moderatorId))
        {
            return Unauthorized("Invalid user token");
        }

        try
        {
            var updatedModel = await service.EditModelAsync(
                id,
                moderatorId,
                request,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString(),
                cancellationToken);
            return Ok(updatedModel);
        }
        catch (NotFoundException)
        {
            return NotFound("Model not found");
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to update model: {ex.Message}");
        }
    }
}

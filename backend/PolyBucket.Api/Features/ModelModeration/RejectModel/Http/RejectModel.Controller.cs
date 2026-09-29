using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ModelModeration.RejectModel.Domain;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.RejectModel.Http;

[Authorize]
[ApiController]
[Route("api/moderation/models")]
[RequirePermission(PermissionConstants.MODERATION_REJECT_MODELS)]
public class RejectModelController(IRejectModelService service) : ControllerBase
{
    private readonly IRejectModelService _service = service;

    /// <summary>
    /// Rejects a model pending moderation.
    /// </summary>
    [HttpPost("{id}/reject")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> RejectModel(
        Guid id,
        [FromBody] RejectModelRequestDto? request,
        CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var moderatorId))
        {
            return Unauthorized();
        }

        try
        {
            await _service.RejectAsync(
                id,
                moderatorId,
                request?.Reason,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString(),
                cancellationToken);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound("Model not found");
        }
        catch (ConflictException)
        {
            return Conflict("Model is not pending moderation");
        }
    }
}

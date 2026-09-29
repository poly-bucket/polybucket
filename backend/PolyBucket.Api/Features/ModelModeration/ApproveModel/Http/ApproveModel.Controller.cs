using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ModelModeration.ApproveModel.Domain;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.ApproveModel.Http;

[Authorize]
[ApiController]
[Route("api/moderation/models")]
[RequirePermission(PermissionConstants.MODERATION_APPROVE_MODELS)]
public class ApproveModelController(IApproveModelService service) : ControllerBase
{
    private readonly IApproveModelService _service = service;

    /// <summary>
    /// Approves a model pending moderation and publishes it when privacy allows.
    /// </summary>
    [HttpPost("{id}/approve")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> ApproveModel(Guid id, CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var moderatorId))
        {
            return Unauthorized();
        }

        try
        {
            await _service.ApproveAsync(
                id,
                moderatorId,
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

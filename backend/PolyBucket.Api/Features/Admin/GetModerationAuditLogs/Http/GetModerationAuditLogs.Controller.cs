using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.Admin.GetModerationAuditLogs.Domain;
using PolyBucket.Api.Features.Admin.GetModerationAuditLogs.Http;

namespace PolyBucket.Api.Features.Admin.GetModerationAuditLogs.Http;

[Authorize]
[ApiController]
[Route("api/admin/moderation")]
public class GetModerationAuditLogsController(IGetModerationAuditLogsService service) : ControllerBase
{
    /// <summary>
    /// Get moderation audit logs for admin review
    /// </summary>
    [HttpGet("audit-logs")]
    [ProducesResponseType(typeof(ModerationAuditResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ModerationAuditResponseDto>> GetAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? action = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? modelId = null,
        CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindUserIdClaim();
        if (!Guid.TryParse(userIdClaim, out var currentUserId))
        {
            return Unauthorized();
        }

        var query = new GetModerationAuditLogsQuery
        {
            Page = page,
            PageSize = pageSize,
            Action = action,
            UserId = userId,
            ModelId = modelId
        };

        var response = await service.GetAuditLogsAsync(currentUserId, query, cancellationToken);
        if (response == null)
        {
            return Forbid();
        }

        return Ok(response);
    }
}

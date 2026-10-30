using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ModelModeration.GetModerationSettings.Domain;
using PolyBucket.Api.Features.ModelModeration.GetModerationSettings.Http;

namespace PolyBucket.Api.Features.ModelModeration.GetModerationSettings.Http;

[Authorize]
[ApiController]
[Route("api/moderation/settings")]
[RequirePermission(PermissionConstants.MODERATION_VIEW_QUEUE)]
public class GetModerationSettingsController(IGetModerationSettingsService service) : ControllerBase
{
    /// <summary>
    /// Get moderation workflow settings.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(GetModerationSettingsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetModerationSettingsResponseDto>> GetModerationSettings(CancellationToken cancellationToken = default)
    {
        return Ok(await service.GetAsync(cancellationToken));
    }
}

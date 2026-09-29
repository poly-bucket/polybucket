using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.ModelModeration.UpdateModerationSettings.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.UpdateModerationSettings.Http;

[Authorize]
[ApiController]
[Route("api/moderation/models")]
[RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
public class UpdateModerationSettingsController(IUpdateModerationSettingsService service) : ControllerBase
{
    private readonly IUpdateModerationSettingsService _service = service;

    /// <summary>
    /// Updates site model moderation settings.
    /// </summary>
    [HttpPut("settings")]
    [ProducesResponseType(typeof(ModerationSettingsDto), 200)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ModerationSettingsDto>> UpdateModerationSettings(
        [FromBody] ModerationSettingsDto settings,
        CancellationToken cancellationToken = default)
    {
        var updated = await _service.UpdateAsync(settings, cancellationToken);
        return Ok(updated);
    }
}

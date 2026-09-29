using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.ModelModeration.GetModerationSettings.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.GetModerationSettings.Http;

[Authorize]
[ApiController]
[Route("api/moderation/models")]
[RequirePermission(PermissionConstants.MODERATION_VIEW_QUEUE)]
public class GetModerationSettingsController(IGetModerationSettingsService service) : ControllerBase
{
    private readonly IGetModerationSettingsService _service = service;

    /// <summary>
    /// Returns site model moderation settings.
    /// </summary>
    [HttpGet("settings")]
    [ProducesResponseType(typeof(ModerationSettingsDto), 200)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ModerationSettingsDto>> GetModerationSettings(CancellationToken cancellationToken = default)
    {
        var settings = await _service.GetAsync(cancellationToken);
        return Ok(settings);
    }
}

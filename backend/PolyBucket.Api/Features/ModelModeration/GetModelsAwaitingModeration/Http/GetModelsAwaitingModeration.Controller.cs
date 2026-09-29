using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.ModelModeration.Domain;
using PolyBucket.Api.Features.ModelModeration.GetModelsAwaitingModeration.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.ModelModeration.GetModelsAwaitingModeration.Http;

[Authorize]
[ApiController]
[Route("api/moderation/models")]
[RequirePermission(PermissionConstants.MODERATION_VIEW_QUEUE)]
public class GetModelsAwaitingModerationController(IGetModelsAwaitingModerationService service) : ControllerBase
{
    private readonly IGetModelsAwaitingModerationService _service = service;

    /// <summary>
    /// Returns models awaiting moderator approval.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ModelsAwaitingModerationResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<ModelsAwaitingModerationResponse>> GetModelsAwaitingModeration(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest("Invalid pagination parameters");
        }

        var result = await _service.GetAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }
}

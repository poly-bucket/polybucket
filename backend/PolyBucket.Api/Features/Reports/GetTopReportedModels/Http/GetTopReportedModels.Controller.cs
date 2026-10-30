using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Reports.GetTopReportedModels.Domain;
using PolyBucket.Api.Features.Reports.GetTopReportedModels.Http;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;

namespace PolyBucket.Api.Features.Reports.GetTopReportedModels.Http;

[ApiController]
[Route("api/reports/analytics")]
[Authorize]
[RequirePermission(PermissionConstants.MODERATION_VIEW_REPORTS)]
public class GetTopReportedModelsController(IGetTopReportedModelsService service) : ControllerBase
{
    [HttpGet("top-models")]
    [ProducesResponseType(typeof(TopReportedModelsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTopReportedModels([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        if (limit < 1 || limit > 100)
        {
            return BadRequest("Limit must be between 1 and 100");
        }

        return Ok(await service.GetTopReportedModelsAsync(limit, cancellationToken));
    }
}

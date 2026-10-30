using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Reports.GetModeratorReportActivity.Domain;
using PolyBucket.Api.Features.Reports.GetModeratorReportActivity.Http;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
namespace PolyBucket.Api.Features.Reports.GetModeratorReportActivity.Http;
[ApiController]
[Route("api/reports/analytics")]
[Authorize]
[RequirePermission(PermissionConstants.MODERATION_VIEW_REPORTS)]
public class GetModeratorReportActivityController(IGetModeratorReportActivityService service) : ControllerBase
{
    [HttpGet("moderator-activity")]
    public async Task<IActionResult> GetModeratorActivity([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate) return BadRequest("From date cannot be after to date");
        return Ok(await service.GetModeratorActivityAsync(fromDate, toDate, cancellationToken));
    }
}

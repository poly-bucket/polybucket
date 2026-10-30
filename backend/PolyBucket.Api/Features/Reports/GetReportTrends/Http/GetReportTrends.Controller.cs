using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Reports.GetReportTrends.Domain;
using PolyBucket.Api.Features.Reports.GetReportTrends.Http;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
namespace PolyBucket.Api.Features.Reports.GetReportTrends.Http;
[ApiController]
[Route("api/reports/analytics")]
[Authorize]
[RequirePermission(PermissionConstants.MODERATION_VIEW_REPORTS)]
public class GetReportTrendsController(IGetReportTrendsService service) : ControllerBase
{
    [HttpGet("trends")]
    public async Task<IActionResult> GetReportTrends([FromQuery] string period = "daily", [FromQuery] int days = 30, CancellationToken cancellationToken = default)
    {
        if (days < 1 || days > 365) return BadRequest("Days must be between 1 and 365");
        if (!new[] { "daily", "weekly", "monthly" }.Contains(period.ToLower())) return BadRequest("Period must be daily, weekly, or monthly");
        return Ok(await service.GetReportTrendsAsync(period, days, cancellationToken));
    }
}

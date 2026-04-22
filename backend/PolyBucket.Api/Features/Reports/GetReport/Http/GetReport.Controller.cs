using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using System;
using PolyBucket.Api.Features.Reports.GetReport.Domain;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;

namespace PolyBucket.Api.Features.Reports.GetReport.Http
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    [RequirePermission(PermissionConstants.MODERATION_VIEW_REPORTS)]
    public class GetReportController(IGetReportService getReportService) : ControllerBase
    {
        private readonly IGetReportService _getReportService = getReportService;

        [HttpGet("{reportId}")]
        public async Task<IActionResult> GetReport(Guid reportId, CancellationToken cancellationToken = default)
        {
            var report = await _getReportService.GetReportByIdAsync(reportId, cancellationToken);
            if (report == null)
            {
                return NotFound();
            }
            return Ok(report);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using PolyBucket.Api.Features.Reports.Domain;

namespace PolyBucket.Api.Features.Reports.Queries
{
    [ApiController]
    [Route("api/reports")]
    [Authorize(Roles = "Admin")]
    public class GetReportController : ControllerBase
    {
        private readonly IReportingPlugin _reportingPlugin;

        public GetReportController(IReportingPlugin reportingPlugin)
        {
            _reportingPlugin = reportingPlugin;
        }

        [HttpGet("{reportId}")]
        public async Task<IActionResult> GetReport(Guid reportId)
        {
            var report = await _reportingPlugin.GetReportByIdAsync(reportId);
            if (report == null)
            {
                return NotFound();
            }
            return Ok(report);
        }
    }
} 
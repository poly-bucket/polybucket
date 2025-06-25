using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using PolyBucket.Api.Features.Reports.Domain;
using System.Collections.Generic;

namespace PolyBucket.Api.Features.Reports.Queries
{
    [ApiController]
    [Route("api/reports")]
    [Authorize(Roles = "Admin")]
    public class GetReportsForTargetQueryController : ControllerBase
    {
        private readonly IReportingPlugin _reportingPlugin;

        public GetReportsForTargetQueryController(IReportingPlugin reportingPlugin)
        {
            _reportingPlugin = reportingPlugin;
        }

        [HttpGet("target/{targetId}")]
        public async Task<IActionResult> GetReportsForTarget(Guid targetId, [FromQuery] ReportType type)
        {
            var reports = await _reportingPlugin.GetReportsForTargetAsync(type, targetId);
            return Ok(reports);
        }
    }
} 
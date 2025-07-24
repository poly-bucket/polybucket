using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using PolyBucket.Api.Features.Reports.Domain;
using System.Collections.Generic;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;

namespace PolyBucket.Api.Features.Reports.Queries
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    [RequirePermission(PermissionConstants.MODERATION_VIEW_REPORTS)]
    public class GetReportsForTargetController(IReportingPlugin reportingPlugin) : ControllerBase
    {
        private readonly IReportingPlugin _reportingPlugin = reportingPlugin;

        [HttpGet("target/{targetId}")]
        public async Task<IActionResult> GetReportsForTarget(Guid targetId, [FromQuery] ReportType type)
        {
            var reports = await _reportingPlugin.GetReportsForTargetAsync(type, targetId);
            return Ok(reports);
        }
    }
} 
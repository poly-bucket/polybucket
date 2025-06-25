using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using PolyBucket.Api.Features.Reports.Domain;

namespace PolyBucket.Api.Features.Reports.Commands
{
    [ApiController]
    [Route("api/reports")]
    [Authorize(Roles = "Admin")]
    public class ResolveReportCommandController : ControllerBase
    {
        private readonly IReportingPlugin _reportingPlugin;

        public ResolveReportCommandController(IReportingPlugin reportingPlugin)
        {
            _reportingPlugin = reportingPlugin;
        }

        [HttpPost("{reportId}/resolve")]
        public async Task<IActionResult> ResolveReport(Guid reportId, [FromBody] ResolveReportRequest request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                throw new UnauthorizedAccessException("User ID not found in token"));

            await _reportingPlugin.ResolveReportAsync(reportId, userId, request.Resolution);
            return Ok();
        }
    }

    public class ResolveReportRequest
    {
        public string Resolution { get; set; }
    }
} 
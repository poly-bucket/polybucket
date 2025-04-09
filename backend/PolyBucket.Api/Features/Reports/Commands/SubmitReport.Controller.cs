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
    [Authorize]
    public class SubmitReportController : ControllerBase
    {
        private readonly IReportingPlugin _reportingPlugin;

        public SubmitReportController(IReportingPlugin reportingPlugin)
        {
            _reportingPlugin = reportingPlugin;
        }
        
        [HttpPost]
        public async Task<IActionResult> SubmitReport([FromBody] SubmitReportRequest request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                throw new UnauthorizedAccessException("User ID not found in token"));

            var report = await _reportingPlugin.SubmitReportAsync(
                request.Type,
                request.TargetId,
                userId,
                request.Reason,
                request.Description
            );

            return Ok(report);
        }
    }

    public class SubmitReportRequest
    {
        public ReportType Type { get; set; }
        public Guid TargetId { get; set; }
        public ReportReason Reason { get; set; }
        public string Description { get; set; }
    }
} 
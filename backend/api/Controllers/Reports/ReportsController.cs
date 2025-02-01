using Core.Plugins.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers.Reports
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportingPlugin _reportingPlugin;

        public ReportsController(IReportingPlugin reportingPlugin)
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

        [HttpGet("target/{targetId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetReportsForTarget(Guid targetId, [FromQuery] ReportType type)
        {
            var reports = await _reportingPlugin.GetReportsForTargetAsync(type, targetId);
            return Ok(reports);
        }

        [HttpGet("unresolved")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUnresolvedReports()
        {
            var reports = await _reportingPlugin.GetUnresolvedReportsAsync();
            return Ok(reports);
        }

        [HttpPost("{reportId}/resolve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResolveReport(Guid reportId, [FromBody] ResolveReportRequest request)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                throw new UnauthorizedAccessException("User ID not found in token"));

            await _reportingPlugin.ResolveReportAsync(reportId, userId, request.Resolution);
            return Ok();
        }

        [HttpGet("{reportId}")]
        [Authorize(Roles = "Admin")]
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

    public class SubmitReportRequest
    {
        public ReportType Type { get; set; }
        public Guid TargetId { get; set; }
        public ReportReason Reason { get; set; }
        public string Description { get; set; }
    }

    public class ResolveReportRequest
    {
        public string Resolution { get; set; }
    }
} 
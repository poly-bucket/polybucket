using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;

namespace PolyBucket.Api.Features.Reports.Commands
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    [RequirePermission(PermissionConstants.MODERATION_HANDLE_REPORTS)]
    public class ResolveReportController(IReportingPlugin reportingPlugin) : ControllerBase
    {
        private readonly IReportingPlugin _reportingPlugin = reportingPlugin;

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
        public required string Resolution { get; set; }
    }
} 
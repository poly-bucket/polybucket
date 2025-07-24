using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;

namespace PolyBucket.Api.Features.Reports.Queries
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    [RequirePermission(PermissionConstants.MODERATION_VIEW_REPORTS)]
    public class GetUnresolvedReportsController(IReportingPlugin reportingPlugin) : ControllerBase
    {
        private readonly IReportingPlugin _reportingPlugin = reportingPlugin;

        [HttpGet("unresolved")]
        public async Task<IActionResult> GetUnresolvedReports()
        {
            var reports = await _reportingPlugin.GetUnresolvedReportsAsync();
            return Ok(reports);
        }
    }
} 
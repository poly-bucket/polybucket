using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Reports.Domain;

namespace PolyBucket.Api.Features.Reports.Queries
{
    [ApiController]
    [Route("api/reports")]
    [Authorize(Roles = "Admin")]
    public class GetUnresolvedReportsQueryController : ControllerBase
    {
        private readonly IReportingPlugin _reportingPlugin;

        public GetUnresolvedReportsQueryController(IReportingPlugin reportingPlugin)
        {
            _reportingPlugin = reportingPlugin;
        }

        [HttpGet("unresolved")]
        public async Task<IActionResult> GetUnresolvedReports()
        {
            var reports = await _reportingPlugin.GetUnresolvedReportsAsync();
            return Ok(reports);
        }
    }
} 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Reports.GetUnresolvedReports.Domain;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;

namespace PolyBucket.Api.Features.Reports.GetUnresolvedReports.Http
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    [RequirePermission(PermissionConstants.MODERATION_VIEW_REPORTS)]
    public class GetUnresolvedReportsController(IGetUnresolvedReportsService getUnresolvedReportsService) : ControllerBase
    {
        private readonly IGetUnresolvedReportsService _getUnresolvedReportsService = getUnresolvedReportsService;

        [HttpGet("unresolved")]
        public async Task<IActionResult> GetUnresolvedReports(CancellationToken cancellationToken = default)
        {
            var reports = await _getUnresolvedReportsService.GetUnresolvedReportsAsync(cancellationToken);
            return Ok(reports);
        }
    }
}

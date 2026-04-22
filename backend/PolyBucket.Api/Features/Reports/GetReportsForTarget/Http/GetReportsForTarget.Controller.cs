using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.GetReportsForTarget.Domain;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;

namespace PolyBucket.Api.Features.Reports.GetReportsForTarget.Http
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    [RequirePermission(PermissionConstants.MODERATION_VIEW_REPORTS)]
    public class GetReportsForTargetController(IGetReportsForTargetService getReportsForTargetService) : ControllerBase
    {
        private readonly IGetReportsForTargetService _getReportsForTargetService = getReportsForTargetService;

        [HttpGet("target/{targetId}")]
        public async Task<IActionResult> GetReportsForTarget(Guid targetId, [FromQuery] ReportType type, CancellationToken cancellationToken = default)
        {
            var reports = await _getReportsForTargetService.GetReportsForTargetAsync(type, targetId, cancellationToken);
            return Ok(reports);
        }
    }
}

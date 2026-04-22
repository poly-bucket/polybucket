using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using PolyBucket.Api.Features.Reports.ResolveReport.Domain;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;

namespace PolyBucket.Api.Features.Reports.ResolveReport.Http
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    [RequirePermission(PermissionConstants.MODERATION_HANDLE_REPORTS)]
    public class ResolveReportController(IResolveReportService resolveReportService) : ControllerBase
    {
        private readonly IResolveReportService _resolveReportService = resolveReportService;

        [HttpPost("{reportId}/resolve")]
        public async Task<IActionResult> ResolveReport(Guid reportId, [FromBody] ResolveReportRequest request, CancellationToken cancellationToken = default)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                throw new UnauthorizedAccessException("User ID not found in token"));

            await _resolveReportService.ResolveReportAsync(reportId, userId, request.Resolution, cancellationToken);
            return Ok();
        }
    }
}

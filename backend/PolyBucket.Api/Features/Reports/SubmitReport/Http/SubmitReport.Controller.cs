using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using PolyBucket.Api.Features.Reports.SubmitReport.Domain;

namespace PolyBucket.Api.Features.Reports.SubmitReport.Http
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class SubmitReportController(ISubmitReportService submitReportService) : ControllerBase
    {
        private readonly ISubmitReportService _submitReportService = submitReportService;

        [HttpPost]
        public async Task<IActionResult> SubmitReport([FromBody] SubmitReportRequest request, CancellationToken cancellationToken = default)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                throw new UnauthorizedAccessException("User ID not found in token"));

            var report = await _submitReportService.SubmitReportAsync(
                request.Type,
                request.TargetId,
                userId,
                request.Reason,
                request.Description,
                cancellationToken);

            return Ok(report);
        }
    }
}

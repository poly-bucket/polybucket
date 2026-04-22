using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.GetAllReports.Domain;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;

namespace PolyBucket.Api.Features.Reports.GetAllReports.Http
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    [RequirePermission(PermissionConstants.MODERATION_VIEW_REPORTS)]
    public class GetAllReportsController(IGetAllReportsService getAllReportsService) : ControllerBase
    {
        private readonly IGetAllReportsService _getAllReportsService = getAllReportsService;

        /// <summary>
        /// Get all reports with pagination and filtering options
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 20)</param>
        /// <param name="isResolved">Filter by resolution status (null for all)</param>
        /// <param name="type">Filter by report type (null for all)</param>
        /// <returns>Paginated list of reports</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ReportsResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetAllReports(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? isResolved = null,
            [FromQuery] ReportType? type = null,
            CancellationToken cancellationToken = default)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Invalid pagination parameters");
            }

            var reports = await _getAllReportsService.GetAllReportsAsync(page, pageSize, isResolved, type, cancellationToken);
            return Ok(reports);
        }
    }
}

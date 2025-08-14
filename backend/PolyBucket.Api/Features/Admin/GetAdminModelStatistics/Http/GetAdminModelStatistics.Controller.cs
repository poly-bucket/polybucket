using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Admin.GetAdminModelStatistics.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Admin.GetAdminModelStatistics.Http
{
    [Authorize]
    [ApiController]
    [Route("api/admin/models")]
    public class GetAdminModelStatisticsController : ControllerBase
    {
        private readonly IGetAdminModelStatisticsService _adminModelStatisticsService;
        private readonly ILogger<GetAdminModelStatisticsController> _logger;

        public GetAdminModelStatisticsController(
            IGetAdminModelStatisticsService adminModelStatisticsService,
            ILogger<GetAdminModelStatisticsController> logger)
        {
            _adminModelStatisticsService = adminModelStatisticsService;
            _logger = logger;
        }

        /// <summary>
        /// Get comprehensive model statistics for admin dashboard
        /// </summary>
        /// <returns>Admin model statistics including counts, file sizes, and distributions</returns>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(GetAdminModelStatisticsResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<GetAdminModelStatisticsResponse>> GetAdminModelStatistics(CancellationToken cancellationToken)
        {
            try
            {
                var response = await _adminModelStatisticsService.GetAdminModelStatisticsAsync(cancellationToken);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin model statistics");
                return StatusCode(500, "An error occurred while retrieving model statistics");
            }
        }
    }
}

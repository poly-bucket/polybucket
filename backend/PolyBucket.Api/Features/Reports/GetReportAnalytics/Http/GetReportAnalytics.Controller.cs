using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.Reports.GetReportAnalytics.Domain;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;

namespace PolyBucket.Api.Features.Reports.GetReportAnalytics.Http
{
    [ApiController]
    [Route("api/reports/analytics")]
    [Authorize]
    [RequirePermission(PermissionConstants.MODERATION_VIEW_REPORTS)]
    public class GetReportAnalyticsController(IGetReportAnalyticsService getReportAnalyticsService) : ControllerBase
    {
        private readonly IGetReportAnalyticsService _getReportAnalyticsService = getReportAnalyticsService;

        /// <summary>
        /// Get comprehensive reports analytics for the moderation dashboard
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ReportsAnalytics), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetReportsAnalytics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                return BadRequest("From date cannot be after to date");
            }

            var analytics = await _getReportAnalyticsService.GetReportsAnalyticsAsync(fromDate, toDate, cancellationToken);
            return Ok(analytics);
        }

        [HttpGet("top-models")]
        [ProducesResponseType(typeof(List<TopReportedItem>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetTopReportedModels([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
        {
            if (limit < 1 || limit > 100)
            {
                return BadRequest("Limit must be between 1 and 100");
            }

            var topModels = await _getReportAnalyticsService.GetTopReportedModelsAsync(limit, cancellationToken);
            return Ok(topModels);
        }

        [HttpGet("top-users")]
        [ProducesResponseType(typeof(List<TopReportedItem>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetTopReportedUsers([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
        {
            if (limit < 1 || limit > 100)
            {
                return BadRequest("Limit must be between 1 and 100");
            }

            var topUsers = await _getReportAnalyticsService.GetTopReportedUsersAsync(limit, cancellationToken);
            return Ok(topUsers);
        }

        [HttpGet("top-comments")]
        [ProducesResponseType(typeof(List<TopReportedItem>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetTopReportedComments([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
        {
            if (limit < 1 || limit > 100)
            {
                return BadRequest("Limit must be between 1 and 100");
            }

            var topComments = await _getReportAnalyticsService.GetTopReportedCommentsAsync(limit, cancellationToken);
            return Ok(topComments);
        }

        [HttpGet("trends")]
        [ProducesResponseType(typeof(List<ReportTrend>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetReportTrends(
            [FromQuery] string period = "daily",
            [FromQuery] int days = 30,
            CancellationToken cancellationToken = default)
        {
            if (days < 1 || days > 365)
            {
                return BadRequest("Days must be between 1 and 365");
            }

            if (!new[] { "daily", "weekly", "monthly" }.Contains(period.ToLower()))
            {
                return BadRequest("Period must be daily, weekly, or monthly");
            }

            var trends = await _getReportAnalyticsService.GetReportTrendsAsync(period, days, cancellationToken);
            return Ok(trends);
        }

        [HttpGet("moderator-activity")]
        [ProducesResponseType(typeof(List<ModeratorActivity>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetModeratorActivity(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                return BadRequest("From date cannot be after to date");
            }

            var activity = await _getReportAnalyticsService.GetModeratorActivityAsync(fromDate, toDate, cancellationToken);
            return Ok(activity);
        }
    }
}

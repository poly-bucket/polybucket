using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using PolyBucket.Api.Features.Reports.Domain;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;

namespace PolyBucket.Api.Features.Reports.Queries
{
    [ApiController]
    [Route("api/reports/analytics")]
    [Authorize]
    [RequirePermission(PermissionConstants.MODERATION_VIEW_REPORTS)]
    public class GetReportsAnalyticsController(IReportingPlugin reportingPlugin) : ControllerBase
    {
        private readonly IReportingPlugin _reportingPlugin = reportingPlugin;

        /// <summary>
        /// Get comprehensive reports analytics for the moderation dashboard
        /// </summary>
        /// <param name="fromDate">Start date for analytics (optional)</param>
        /// <param name="toDate">End date for analytics (optional)</param>
        /// <returns>Comprehensive reports analytics</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ReportsAnalytics), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetReportsAnalytics(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                return BadRequest("From date cannot be after to date");
            }

            var analytics = await _reportingPlugin.GetReportsAnalyticsAsync(fromDate, toDate);
            return Ok(analytics);
        }

        /// <summary>
        /// Get top reported models
        /// </summary>
        /// <param name="limit">Number of items to return (default: 10)</param>
        /// <returns>List of top reported models</returns>
        [HttpGet("top-models")]
        [ProducesResponseType(typeof(List<TopReportedItem>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetTopReportedModels([FromQuery] int limit = 10)
        {
            if (limit < 1 || limit > 100)
            {
                return BadRequest("Limit must be between 1 and 100");
            }

            var topModels = await _reportingPlugin.GetTopReportedModelsAsync(limit);
            return Ok(topModels);
        }

        /// <summary>
        /// Get top reported users
        /// </summary>
        /// <param name="limit">Number of items to return (default: 10)</param>
        /// <returns>List of top reported users</returns>
        [HttpGet("top-users")]
        [ProducesResponseType(typeof(List<TopReportedItem>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetTopReportedUsers([FromQuery] int limit = 10)
        {
            if (limit < 1 || limit > 100)
            {
                return BadRequest("Limit must be between 1 and 100");
            }

            var topUsers = await _reportingPlugin.GetTopReportedUsersAsync(limit);
            return Ok(topUsers);
        }

        /// <summary>
        /// Get top reported comments
        /// </summary>
        /// <param name="limit">Number of items to return (default: 10)</param>
        /// <returns>List of top reported comments</returns>
        [HttpGet("top-comments")]
        [ProducesResponseType(typeof(List<TopReportedItem>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetTopReportedComments([FromQuery] int limit = 10)
        {
            if (limit < 1 || limit > 100)
            {
                return BadRequest("Limit must be between 1 and 100");
            }

            var topComments = await _reportingPlugin.GetTopReportedCommentsAsync(limit);
            return Ok(topComments);
        }

        /// <summary>
        /// Get report trends over time
        /// </summary>
        /// <param name="period">Period type: daily, weekly, monthly (default: daily)</param>
        /// <param name="days">Number of days to look back (default: 30)</param>
        /// <returns>List of report trends</returns>
        [HttpGet("trends")]
        [ProducesResponseType(typeof(List<ReportTrend>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetReportTrends(
            [FromQuery] string period = "daily",
            [FromQuery] int days = 30)
        {
            if (days < 1 || days > 365)
            {
                return BadRequest("Days must be between 1 and 365");
            }

            if (!new[] { "daily", "weekly", "monthly" }.Contains(period.ToLower()))
            {
                return BadRequest("Period must be daily, weekly, or monthly");
            }

            var trends = await _reportingPlugin.GetReportTrendsAsync(period, days);
            return Ok(trends);
        }

        /// <summary>
        /// Get moderator activity statistics
        /// </summary>
        /// <param name="fromDate">Start date for activity (optional)</param>
        /// <param name="toDate">End date for activity (optional)</param>
        /// <returns>List of moderator activity</returns>
        [HttpGet("moderator-activity")]
        [ProducesResponseType(typeof(List<ModeratorActivity>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetModeratorActivity(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                return BadRequest("From date cannot be after to date");
            }

            var activity = await _reportingPlugin.GetModeratorActivityAsync(fromDate, toDate);
            return Ok(activity);
        }
    }
} 
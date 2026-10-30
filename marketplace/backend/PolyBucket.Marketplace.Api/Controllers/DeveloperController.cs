using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Marketplace.Api.Data;
using PolyBucket.Marketplace.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PolyBucket.Marketplace.Api.Controllers
{
    /// <summary>
    /// Controller for developer dashboard functionality
    /// </summary>
    /// <remarks>
    /// This controller provides endpoints for developers to manage their plugins, view analytics,
    /// and access developer-specific features. All endpoints require authentication.
    /// </remarks>
    [ApiController]
    [Route("api/developer")]
    [Authorize]
    [Produces("application/json")]
    public class DeveloperController : ControllerBase
    {
        private readonly MarketplaceDbContext _context;
        private readonly ILogger<DeveloperController> _logger;

        public DeveloperController(MarketplaceDbContext context, ILogger<DeveloperController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get developer statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<DeveloperStatsResponse>> GetStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var stats = await CalculateDeveloperStats(userId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting developer stats");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get developer's plugins
        /// </summary>
        [HttpGet("plugins")]
        public async Task<ActionResult<List<PluginSummaryResponse>>> GetPlugins()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var plugins = await _context.Plugins
                    .Include(p => p.Reviews)
                    .Where(p => p.AuthorId == userId)
                    .Select(p => new PluginSummaryResponse
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Version = p.Version,
                        Downloads = p.Downloads,
                        Rating = p.AverageRating,
                        ReviewCount = p.Reviews.Count,
                        Status = p.Status,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(plugins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting developer plugins");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get developer analytics data
        /// </summary>
        [HttpGet("analytics")]
        public async Task<ActionResult<DeveloperAnalyticsResponse>> GetAnalytics()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var analytics = await CalculateDeveloperAnalytics(userId);
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting developer analytics");
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task<DeveloperStatsResponse> CalculateDeveloperStats(string userId)
        {
            var plugins = await _context.Plugins
                .Include(p => p.Reviews)
                .Where(p => p.AuthorId == userId)
                .ToListAsync();

            var pluginIds = plugins.Select(p => p.Id).ToList();
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var monthlyDownloads = pluginIds.Count == 0
                ? 0
                : await _context.Downloads.CountAsync(d => pluginIds.Contains(d.PluginId) && d.DownloadedAt >= thirtyDaysAgo);

            var totalDownloads = plugins.Sum(p => p.Downloads);
            var averageRating = plugins.Any() ? plugins.Average(p => p.AverageRating) : 0;
            var totalReviews = plugins.Sum(p => p.Reviews.Count);

            var topPlugin = plugins
                .OrderByDescending(p => p.Downloads)
                .FirstOrDefault();

            return new DeveloperStatsResponse
            {
                TotalPlugins = plugins.Count,
                TotalDownloads = totalDownloads,
                AverageRating = averageRating,
                TotalReviews = totalReviews,
                MonthlyDownloads = monthlyDownloads,
                TopPlugin = topPlugin != null ? new TopPluginResponse
                {
                    Id = topPlugin.Id,
                    Name = topPlugin.Name,
                    Downloads = topPlugin.Downloads
                } : null
            };
        }

        private async Task<DeveloperAnalyticsResponse> CalculateDeveloperAnalytics(string userId)
        {
            var plugins = await _context.Plugins
                .Where(p => p.AuthorId == userId)
                .ToListAsync();

            var pluginIds = plugins.Select(p => p.Id).ToList();
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var downloadRows = pluginIds.Count == 0
                ? new List<PluginDownload>()
                : await _context.Downloads
                    .Where(d => pluginIds.Contains(d.PluginId) && d.DownloadedAt >= thirtyDaysAgo)
                    .ToListAsync();

            var downloads = downloadRows
                .GroupBy(d => d.DownloadedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new DateCountResponse
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Count = g.Count()
                })
                .ToList();

            var topPlugins = plugins
                .OrderByDescending(p => p.Downloads)
                .Take(5)
                .Select(p => new PluginDownloadsResponse
                {
                    Name = p.Name,
                    Downloads = p.Downloads
                })
                .ToList();

            return new DeveloperAnalyticsResponse
            {
                Downloads = downloads,
                TopPlugins = topPlugins
            };
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }

    // Response models
    public class DeveloperStatsResponse
    {
        public int TotalPlugins { get; set; }
        public int TotalDownloads { get; set; }
        public decimal TotalRevenue { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int MonthlyDownloads { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public TopPluginResponse? TopPlugin { get; set; }
    }

    public class TopPluginResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Downloads { get; set; }
    }

    public class PluginSummaryResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public int Downloads { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal Revenue { get; set; }
    }

    public class DeveloperAnalyticsResponse
    {
        public List<DateCountResponse> Downloads { get; set; } = new();
        public List<DateAmountResponse> Revenue { get; set; } = new();
        public List<CountryDownloadsResponse> TopCountries { get; set; } = new();
        public List<PluginDownloadsResponse> TopPlugins { get; set; } = new();
    }

    public class DateCountResponse
    {
        public string Date { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class DateAmountResponse
    {
        public string Date { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class CountryDownloadsResponse
    {
        public string Country { get; set; } = string.Empty;
        public int Downloads { get; set; }
    }

    public class PluginDownloadsResponse
    {
        public string Name { get; set; } = string.Empty;
        public int Downloads { get; set; }
    }

    public class PluginResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LongDescription { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string RepositoryUrl { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreatePluginRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LongDescription { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string RepositoryUrl { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;
    }

    public class UpdatePluginRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LongDescription { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string RepositoryUrl { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;
    }
}

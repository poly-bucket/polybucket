using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Marketplace.Api.Data;
using PolyBucket.Marketplace.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
                        UpdatedAt = p.UpdatedAt,
                        Revenue = p.Revenue
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

        /// <summary>
        /// Create a new plugin
        /// </summary>
        [HttpPost("plugins")]
        public async Task<ActionResult<PluginResponse>> CreatePlugin([FromBody] CreatePluginRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var plugin = new Plugin
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = request.Name,
                    Description = request.Description,
                    LongDescription = request.LongDescription,
                    Category = request.Category,
                    AuthorId = userId,
                    Version = request.Version,
                    RepositoryUrl = request.RepositoryUrl,
                    License = request.License,
                    Status = "draft",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Plugins.Add(plugin);
                await _context.SaveChangesAsync();

                var response = new PluginResponse
                {
                    Id = plugin.Id,
                    Name = plugin.Name,
                    Description = plugin.Description,
                    LongDescription = plugin.LongDescription,
                    Category = plugin.Category,
                    Author = plugin.Author?.UserName ?? "Unknown",
                    Version = plugin.Version,
                    RepositoryUrl = plugin.RepositoryUrl,
                    License = plugin.License,
                    Status = plugin.Status,
                    CreatedAt = plugin.CreatedAt,
                    UpdatedAt = plugin.UpdatedAt
                };

                return CreatedAtAction(nameof(GetPlugin), new { id = plugin.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating plugin");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get a specific plugin
        /// </summary>
        [HttpGet("plugins/{id}")]
        public async Task<ActionResult<PluginResponse>> GetPlugin(string id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var plugin = await _context.Plugins
                    .FirstOrDefaultAsync(p => p.Id == id && p.AuthorId == userId);

                if (plugin == null)
                {
                    return NotFound();
                }

                var response = new PluginResponse
                {
                    Id = plugin.Id,
                    Name = plugin.Name,
                    Description = plugin.Description,
                    LongDescription = plugin.LongDescription,
                    Category = plugin.Category,
                    Author = plugin.Author?.UserName ?? "Unknown",
                    Version = plugin.Version,
                    RepositoryUrl = plugin.RepositoryUrl,
                    License = plugin.License,
                    Status = plugin.Status,
                    CreatedAt = plugin.CreatedAt,
                    UpdatedAt = plugin.UpdatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plugin");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update a plugin
        /// </summary>
        [HttpPut("plugins/{id}")]
        public async Task<ActionResult<PluginResponse>> UpdatePlugin(string id, [FromBody] UpdatePluginRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var plugin = await _context.Plugins
                    .FirstOrDefaultAsync(p => p.Id == id && p.AuthorId == userId);

                if (plugin == null)
                {
                    return NotFound();
                }

                plugin.Name = request.Name;
                plugin.Description = request.Description;
                plugin.LongDescription = request.LongDescription;
                plugin.Category = request.Category;
                plugin.Version = request.Version;
                plugin.RepositoryUrl = request.RepositoryUrl;
                plugin.License = request.License;
                plugin.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var response = new PluginResponse
                {
                    Id = plugin.Id,
                    Name = plugin.Name,
                    Description = plugin.Description,
                    LongDescription = plugin.LongDescription,
                    Category = plugin.Category,
                    Author = plugin.Author?.UserName ?? "Unknown",
                    Version = plugin.Version,
                    RepositoryUrl = plugin.RepositoryUrl,
                    License = plugin.License,
                    Status = plugin.Status,
                    CreatedAt = plugin.CreatedAt,
                    UpdatedAt = plugin.UpdatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating plugin");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a plugin
        /// </summary>
        [HttpDelete("plugins/{id}")]
        public async Task<ActionResult> DeletePlugin(string id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var plugin = await _context.Plugins
                    .FirstOrDefaultAsync(p => p.Id == id && p.AuthorId == userId);

                if (plugin == null)
                {
                    return NotFound();
                }

                _context.Plugins.Remove(plugin);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting plugin");
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task<DeveloperStatsResponse> CalculateDeveloperStats(string userId)
        {
            var plugins = await _context.Plugins
                .Where(p => p.AuthorId == userId)
                .ToListAsync();

            var totalDownloads = plugins.Sum(p => p.Downloads);
            var totalRevenue = plugins.Sum(p => p.Revenue);
            var averageRating = plugins.Any() ? plugins.Average(p => p.AverageRating) : 0;
            var totalReviews = plugins.Sum(p => p.Reviews.Count);
            var monthlyDownloads = plugins.Sum(p => p.Downloads); // Simplified - would need date filtering
            var monthlyRevenue = plugins.Sum(p => p.Revenue); // Simplified - would need date filtering

            var topPlugin = plugins
                .OrderByDescending(p => p.Downloads)
                .FirstOrDefault();

            return new DeveloperStatsResponse
            {
                TotalPlugins = plugins.Count,
                TotalDownloads = totalDownloads,
                TotalRevenue = totalRevenue,
                AverageRating = averageRating,
                TotalReviews = totalReviews,
                MonthlyDownloads = monthlyDownloads,
                MonthlyRevenue = monthlyRevenue,
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
            // Simplified analytics - would need more complex date filtering and aggregation
            var plugins = await _context.Plugins
                .Where(p => p.AuthorId == userId)
                .ToListAsync();

            var downloads = new List<DateCountResponse>();
            var revenue = new List<DateAmountResponse>();
            var topCountries = new List<CountryDownloadsResponse>();
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
                Revenue = revenue,
                TopCountries = topCountries,
                TopPlugins = topPlugins
            };
        }

        private string? GetCurrentUserId()
        {
            // This would typically come from JWT token claims
            return User.FindFirst("sub")?.Value ?? User.FindFirst("user_id")?.Value;
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

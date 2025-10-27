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
    /// Controller for plugin reviews and ratings
    /// </summary>
    [ApiController]
    [Route("api/plugins/{pluginId}/reviews")]
    [Authorize]
    public class PluginReviewsController : ControllerBase
    {
        private readonly MarketplaceDbContext _context;
        private readonly ILogger<PluginReviewsController> _logger;

        public PluginReviewsController(MarketplaceDbContext context, ILogger<PluginReviewsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get reviews for a plugin
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PluginReviewsResponse>> GetReviews(string pluginId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var plugin = await _context.Plugins.FindAsync(pluginId);
                if (plugin == null)
                {
                    return NotFound("Plugin not found");
                }

                var reviews = await _context.PluginReviews
                    .Where(r => r.PluginId == pluginId)
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new PluginReviewResponse
                    {
                        Id = r.Id,
                        UserId = r.UserId,
                        Username = r.Username,
                        Rating = r.Rating,
                        Title = r.Title,
                        Content = r.Content,
                        CreatedAt = r.CreatedAt,
                        IsVerified = r.IsVerified,
                        HelpfulCount = r.HelpfulCount
                    })
                    .ToListAsync();

                var totalReviews = await _context.PluginReviews
                    .CountAsync(r => r.PluginId == pluginId);

                var averageRating = await _context.PluginReviews
                    .Where(r => r.PluginId == pluginId)
                    .AverageAsync(r => (double?)r.Rating) ?? 0;

                var ratingDistribution = await _context.PluginReviews
                    .Where(r => r.PluginId == pluginId)
                    .GroupBy(r => r.Rating)
                    .Select(g => new RatingDistributionResponse
                    {
                        Rating = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                return Ok(new PluginReviewsResponse
                {
                    Reviews = reviews,
                    TotalReviews = totalReviews,
                    AverageRating = averageRating,
                    RatingDistribution = ratingDistribution,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalReviews / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plugin reviews for plugin {PluginId}", pluginId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new review
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PluginReviewResponse>> CreateReview(string pluginId, [FromBody] CreateReviewRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var plugin = await _context.Plugins.FindAsync(pluginId);
                if (plugin == null)
                {
                    return NotFound("Plugin not found");
                }

                // Check if user already reviewed this plugin
                var existingReview = await _context.PluginReviews
                    .FirstOrDefaultAsync(r => r.PluginId == pluginId && r.UserId == userId);

                if (existingReview != null)
                {
                    return BadRequest("You have already reviewed this plugin");
                }

                var review = new PluginReview
                {
                    Id = Guid.NewGuid().ToString(),
                    PluginId = pluginId,
                    UserId = userId,
                    Username = GetCurrentUsername(),
                    Rating = request.Rating,
                    Title = request.Title,
                    Content = request.Content,
                    CreatedAt = DateTime.UtcNow,
                    IsVerified = false, // Would be set based on user verification status
                    HelpfulCount = 0
                };

                _context.PluginReviews.Add(review);

                // Update plugin average rating
                await UpdatePluginRating(pluginId);

                await _context.SaveChangesAsync();

                var response = new PluginReviewResponse
                {
                    Id = review.Id,
                    UserId = review.UserId,
                    Username = review.Username,
                    Rating = review.Rating,
                    Title = review.Title,
                    Content = review.Content,
                    CreatedAt = review.CreatedAt,
                    IsVerified = review.IsVerified,
                    HelpfulCount = review.HelpfulCount
                };

                return CreatedAtAction(nameof(GetReview), new { pluginId, reviewId = review.Id }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review for plugin {PluginId}", pluginId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get a specific review
        /// </summary>
        [HttpGet("{reviewId}")]
        public async Task<ActionResult<PluginReviewResponse>> GetReview(string pluginId, string reviewId)
        {
            try
            {
                var review = await _context.PluginReviews
                    .FirstOrDefaultAsync(r => r.Id == reviewId && r.PluginId == pluginId);

                if (review == null)
                {
                    return NotFound();
                }

                var response = new PluginReviewResponse
                {
                    Id = review.Id,
                    UserId = review.UserId,
                    Username = review.Username,
                    Rating = review.Rating,
                    Title = review.Title,
                    Content = review.Content,
                    CreatedAt = review.CreatedAt,
                    IsVerified = review.IsVerified,
                    HelpfulCount = review.HelpfulCount
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review {ReviewId} for plugin {PluginId}", reviewId, pluginId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update a review
        /// </summary>
        [HttpPut("{reviewId}")]
        public async Task<ActionResult<PluginReviewResponse>> UpdateReview(string pluginId, string reviewId, [FromBody] UpdateReviewRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var review = await _context.PluginReviews
                    .FirstOrDefaultAsync(r => r.Id == reviewId && r.PluginId == pluginId && r.UserId == userId);

                if (review == null)
                {
                    return NotFound();
                }

                review.Rating = request.Rating;
                review.Title = request.Title;
                review.Content = request.Content;
                review.UpdatedAt = DateTime.UtcNow;

                // Update plugin average rating
                await UpdatePluginRating(pluginId);

                await _context.SaveChangesAsync();

                var response = new PluginReviewResponse
                {
                    Id = review.Id,
                    UserId = review.UserId,
                    Username = review.Username,
                    Rating = review.Rating,
                    Title = review.Title,
                    Content = review.Content,
                    CreatedAt = review.CreatedAt,
                    IsVerified = review.IsVerified,
                    HelpfulCount = review.HelpfulCount
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review {ReviewId} for plugin {PluginId}", reviewId, pluginId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete a review
        /// </summary>
        [HttpDelete("{reviewId}")]
        public async Task<ActionResult> DeleteReview(string pluginId, string reviewId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var review = await _context.PluginReviews
                    .FirstOrDefaultAsync(r => r.Id == reviewId && r.PluginId == pluginId && r.UserId == userId);

                if (review == null)
                {
                    return NotFound();
                }

                _context.PluginReviews.Remove(review);

                // Update plugin average rating
                await UpdatePluginRating(pluginId);

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review {ReviewId} for plugin {PluginId}", reviewId, pluginId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Mark a review as helpful
        /// </summary>
        [HttpPost("{reviewId}/helpful")]
        public async Task<ActionResult> MarkHelpful(string pluginId, string reviewId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var review = await _context.PluginReviews
                    .FirstOrDefaultAsync(r => r.Id == reviewId && r.PluginId == pluginId);

                if (review == null)
                {
                    return NotFound();
                }

                // Check if user already marked this review as helpful
                var existingHelpful = await _context.ReviewHelpfuls
                    .FirstOrDefaultAsync(h => h.ReviewId == reviewId && h.UserId == userId);

                if (existingHelpful != null)
                {
                    return BadRequest("You have already marked this review as helpful");
                }

                var helpful = new ReviewHelpful
                {
                    Id = Guid.NewGuid().ToString(),
                    ReviewId = reviewId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ReviewHelpfuls.Add(helpful);

                // Update helpful count
                review.HelpfulCount++;

                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking review {ReviewId} as helpful", reviewId);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Remove helpful mark from a review
        /// </summary>
        [HttpDelete("{reviewId}/helpful")]
        public async Task<ActionResult> RemoveHelpful(string pluginId, string reviewId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var helpful = await _context.ReviewHelpfuls
                    .FirstOrDefaultAsync(h => h.ReviewId == reviewId && h.UserId == userId);

                if (helpful == null)
                {
                    return NotFound();
                }

                _context.ReviewHelpfuls.Remove(helpful);

                // Update helpful count
                var review = await _context.PluginReviews.FindAsync(reviewId);
                if (review != null)
                {
                    review.HelpfulCount--;
                }

                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing helpful mark from review {ReviewId}", reviewId);
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task UpdatePluginRating(string pluginId)
        {
            var plugin = await _context.Plugins.FindAsync(pluginId);
            if (plugin != null)
            {
                var averageRating = await _context.PluginReviews
                    .Where(r => r.PluginId == pluginId)
                    .AverageAsync(r => (double?)r.Rating) ?? 0;

                plugin.AverageRating = averageRating;
            }
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst("sub")?.Value ?? User.FindFirst("user_id")?.Value;
        }

        private string GetCurrentUsername()
        {
            return User.FindFirst("name")?.Value ?? User.FindFirst("username")?.Value ?? "Anonymous";
        }
    }

    // Response models
    public class PluginReviewsResponse
    {
        public List<PluginReviewResponse> Reviews { get; set; } = new();
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public List<RatingDistributionResponse> RatingDistribution { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class PluginReviewResponse
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsVerified { get; set; }
        public int HelpfulCount { get; set; }
    }

    public class RatingDistributionResponse
    {
        public int Rating { get; set; }
        public int Count { get; set; }
    }

    public class CreateReviewRequest
    {
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class UpdateReviewRequest
    {
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}

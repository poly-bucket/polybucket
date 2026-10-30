using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Marketplace.Api.Data;
using PolyBucket.Marketplace.Api.Models;

namespace PolyBucket.Marketplace.Api.Controllers;

[ApiController]
[Route("api/users")]
[Produces("application/json")]
public class UsersController(MarketplaceDbContext context) : ControllerBase
{
    /// <summary>
    /// Public profile and published plugins for a marketplace user.
    /// </summary>
    [HttpGet("{username}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PublicUserProfileDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<PublicUserProfileDto>> GetPublicProfile(string username)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.UserName == username ||
                u.GitHubUsername == username);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var plugins = await context.Plugins
            .Where(p => p.AuthorId == user.Id && p.IsActive && p.Status == "published")
            .Include(p => p.Reviews)
            .Include(p => p.PluginTags)
            .Include(p => p.Author)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();

        var summaries = plugins.Select(p => new PluginSummary
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Category = p.Category,
            Version = p.Version,
            Author = p.Author?.UserName ?? user.UserName ?? "Unknown",
            AuthorId = p.AuthorId,
            Downloads = p.Downloads,
            AverageRating = p.AverageRating,
            ReviewCount = p.Reviews.Count,
            IsVerified = p.IsVerified,
            IsFeatured = p.IsFeatured,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            Tags = p.PluginTags.Select(t => t.Name).ToList()
        }).ToList();

        return Ok(new PublicUserProfileDto
        {
            Username = user.UserName ?? user.GitHubUsername ?? username,
            DisplayName = user.GitHubDisplayName,
            AvatarUrl = user.GitHubAvatarUrl,
            Bio = user.Bio,
            GitHubProfileUrl = user.GitHubProfileUrl,
            Website = user.Website,
            CreatedAt = user.CreatedAt,
            PluginCount = summaries.Count,
            TotalDownloads = summaries.Sum(p => p.Downloads),
            Plugins = summaries
        });
    }
}

public class PublicUserProfileDto
{
    public string Username { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? GitHubProfileUrl { get; set; }
    public string? Website { get; set; }
    public DateTime CreatedAt { get; set; }
    public int PluginCount { get; set; }
    public int TotalDownloads { get; set; }
    public List<PluginSummary> Plugins { get; set; } = new();
}

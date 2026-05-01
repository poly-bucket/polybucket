using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PolyBucket.Api.Common;
using PolyBucket.Api.Data;

namespace PolyBucket.Api.Features.Authentication.Account.Http;

[ApiController]
[Route("api/auth/account")]
[Authorize]
public class ExportAccountController(PolyBucketDbContext context) : ControllerBase
{
    private readonly PolyBucketDbContext _context = context;

    /// <summary>
    /// Export a snapshot of profile and settings data for the signed-in user (JSON).
    /// </summary>
    [HttpGet("export")]
    [ProducesResponseType(typeof(AccountExportResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<AccountExportResponse>> Export(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindUserIdClaim();
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid authentication token" });
        }

        var user = await _context.Users
            .Include(u => u.Settings)
            .Include(u => u.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var response = new AccountExportResponse
        {
            ExportedAtUtc = DateTime.UtcNow,
            UserId = user.Id,
            Email = user.Email,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Bio = user.Bio,
            Country = user.Country,
            WebsiteUrl = user.WebsiteUrl,
            TwitterUrl = user.TwitterUrl,
            InstagramUrl = user.InstagramUrl,
            YouTubeUrl = user.YouTubeUrl,
            IsProfilePublic = user.IsProfilePublic,
            ShowEmail = user.ShowEmail,
            ShowLastLogin = user.ShowLastLogin,
            ShowStatistics = user.ShowStatistics,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            RoleName = user.Role?.Name,
            ModelCount = await _context.Models.CountAsync(m => m.CreatedById == userId, cancellationToken),
            CollectionCount = await _context.Collections.CountAsync(c => c.OwnerId == userId, cancellationToken),
            FavoriteCollectionCount = await _context.Collections.CountAsync(c => c.OwnerId == userId && c.Favorite, cancellationToken),
            RecentModels = await _context.Models
                .Where(m => m.CreatedById == userId)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new AccountExportModelSummaryDto
                {
                    ModelId = m.Id,
                    Name = m.Name,
                    CreatedAt = m.CreatedAt,
                    Privacy = m.Privacy.ToString(),
                })
                .Take(10)
                .ToListAsync(cancellationToken),
            RecentCollections = await _context.Collections
                .Where(c => c.OwnerId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new AccountExportCollectionSummaryDto
                {
                    CollectionId = c.Id,
                    Name = c.Name,
                    Visibility = c.Visibility.ToString(),
                    CreatedAt = c.CreatedAt,
                })
                .Take(10)
                .ToListAsync(cancellationToken),
            Settings = user.Settings == null
                ? null
                : new AccountExportSettingsDto
                {
                    Language = user.Settings.Language,
                    Theme = user.Settings.Theme,
                    EmailNotifications = user.Settings.EmailNotifications,
                    MeasurementSystem = user.Settings.MeasurementSystem,
                    TimeZone = user.Settings.TimeZone,
                },
        };

        return Ok(response);
    }
}

public class AccountExportResponse
{
    public DateTime ExportedAtUtc { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Bio { get; set; }
    public string? Country { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? YouTubeUrl { get; set; }
    public bool IsProfilePublic { get; set; }
    public bool ShowEmail { get; set; }
    public bool ShowLastLogin { get; set; }
    public bool ShowStatistics { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? RoleName { get; set; }
    public int ModelCount { get; set; }
    public int CollectionCount { get; set; }
    public int FavoriteCollectionCount { get; set; }
    public IReadOnlyList<AccountExportModelSummaryDto> RecentModels { get; set; } = [];
    public IReadOnlyList<AccountExportCollectionSummaryDto> RecentCollections { get; set; } = [];
    public AccountExportSettingsDto? Settings { get; set; }
}

public class AccountExportSettingsDto
{
    public string Language { get; set; } = string.Empty;
    public string Theme { get; set; } = string.Empty;
    public bool EmailNotifications { get; set; }
    public string MeasurementSystem { get; set; } = string.Empty;
    public string TimeZone { get; set; } = string.Empty;
}

public class AccountExportModelSummaryDto
{
    public Guid ModelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Privacy { get; set; } = string.Empty;
}

public class AccountExportCollectionSummaryDto
{
    public Guid CollectionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

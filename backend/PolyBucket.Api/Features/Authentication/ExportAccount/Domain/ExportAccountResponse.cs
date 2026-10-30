namespace PolyBucket.Api.Features.Authentication.ExportAccount.Domain;

public interface IExportAccountService
{
    Task<AccountExportResponse?> ExportAsync(Guid userId, CancellationToken cancellationToken);
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

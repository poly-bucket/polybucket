using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Marketplace.Api.Models
{
    public class PluginInstallation
    {
        public string Id { get; set; } = string.Empty;
        public string PluginId { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? Version { get; set; }
        public string InstallationMethod { get; set; } = string.Empty; // marketplace, direct, cli
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
        public DateTime InstalledAt { get; set; }

        // Navigation properties
        public Plugin Plugin { get; set; } = null!;
        public MarketplaceUser? User { get; set; }
    }
    public class PluginDownloadInfo
    {
        public string PluginId { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string Checksum { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string ReleaseNotes { get; set; } = string.Empty;
    }

    public class PluginInstallationRequest
    {
        public string PluginId { get; set; } = string.Empty;
        public string? Version { get; set; }
        public string? InstanceId { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
    }

    public class PluginInstallationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime InstalledAt { get; set; }
        public string? InstallationId { get; set; }
    }

    public class MarketplacePluginDetails
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LongDescription { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string RepositoryUrl { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public bool IsFeatured { get; set; }
        public int Downloads { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public string Author { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PluginVersion> Versions { get; set; } = new();
        public List<string> Tags { get; set; } = new();
    }

    public class MarketplacePluginsResponse
    {
        public List<MarketplacePluginDetails> Plugins { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    public class MarketplaceCategory
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PluginCount { get; set; }
    }
}

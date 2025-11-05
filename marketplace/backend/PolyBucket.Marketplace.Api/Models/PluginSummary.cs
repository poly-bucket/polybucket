using System.ComponentModel.DataAnnotations;

namespace PolyBucket.Marketplace.Api.Models
{
    /// <summary>
    /// Lightweight plugin summary for browsing and listing
    /// </summary>
    public class PluginSummary
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? AuthorId { get; set; }
        public int Downloads { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsVerified { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    /// <summary>
    /// Request model for plugin browsing with filters and pagination
    /// </summary>
    public class PluginBrowseRequest
    {
        public string? Search { get; set; }
        public string? Category { get; set; }
        public List<string> Tags { get; set; } = new();
        public string? SortBy { get; set; } = "downloads"; // downloads, rating, created, updated
        public string? SortOrder { get; set; } = "desc"; // asc, desc
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool? IsVerified { get; set; }
        public bool? IsFeatured { get; set; }
        public double? MinRating { get; set; }
    }

    /// <summary>
    /// Response model for plugin browsing with pagination metadata
    /// </summary>
    public class PluginBrowseResponse
    {
        public List<PluginSummary> Plugins { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    /// <summary>
    /// Available plugin categories
    /// </summary>
    public static class PluginCategories
    {
        public const string UI_COMPONENTS = "UI Components";
        public const string AUTHENTICATION = "Authentication";
        public const string DATA_VISUALIZATION = "Data Visualization";
        public const string INTEGRATIONS = "Integrations";
        public const string PRODUCTIVITY = "Productivity";
        public const string ANALYTICS = "Analytics";
        public const string THEMES = "Themes";
        public const string LOCALIZATION = "Localization";
        public const string OTHER = "Other";

        public static readonly List<string> All = new()
        {
            UI_COMPONENTS,
            AUTHENTICATION,
            DATA_VISUALIZATION,
            INTEGRATIONS,
            PRODUCTIVITY,
            ANALYTICS,
            THEMES,
            LOCALIZATION,
            OTHER
        };
    }

    /// <summary>
    /// Available sort options for plugin browsing
    /// </summary>
    public static class PluginSortOptions
    {
        public const string DOWNLOADS = "downloads";
        public const string RATING = "rating";
        public const string CREATED = "created";
        public const string UPDATED = "updated";
        public const string NAME = "name";

        public static readonly List<string> All = new()
        {
            DOWNLOADS,
            RATING,
            CREATED,
            UPDATED,
            NAME
        };
    }
}

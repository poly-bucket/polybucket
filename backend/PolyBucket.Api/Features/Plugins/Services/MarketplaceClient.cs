using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace PolyBucket.Api.Features.Plugins.Services
{
    /// <summary>
    /// Client for communicating with the PolyBucket Marketplace API
    /// </summary>
    public class MarketplaceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IMarketplaceBaseUrlResolver _baseUrlResolver;
        private readonly ILogger<MarketplaceClient> _logger;

        public MarketplaceClient(
            HttpClient httpClient,
            IMarketplaceBaseUrlResolver baseUrlResolver,
            ILogger<MarketplaceClient> logger)
        {
            _httpClient = httpClient;
            _baseUrlResolver = baseUrlResolver;
            _logger = logger;
        }

        private async Task<string> GetBaseUrlAsync(CancellationToken cancellationToken = default) =>
            await _baseUrlResolver.ResolveAsync(cancellationToken);

        /// <summary>
        /// Get available plugins from the marketplace
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="category">Filter by category</param>
        /// <param name="search">Search query</param>
        /// <returns>List of marketplace plugins</returns>
        public async Task<MarketplacePluginsResponse> GetPluginsAsync(
            int page = 1,
            int pageSize = 20,
            string? category = null,
            string? search = null,
            string? target = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var baseUrl = await GetBaseUrlAsync(cancellationToken);
                var queryParams = new List<string>
                {
                    $"page={page}",
                    $"pageSize={pageSize}"
                };

                if (!string.IsNullOrWhiteSpace(category))
                {
                    queryParams.Add($"category={Uri.EscapeDataString(category)}");
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    queryParams.Add($"search={Uri.EscapeDataString(search)}");
                }

                if (!string.IsNullOrWhiteSpace(target))
                {
                    queryParams.Add($"target={Uri.EscapeDataString(target)}");
                }

                var url = $"{baseUrl}/api/plugins/main-api?{string.Join("&", queryParams)}";
                
                _logger.LogInformation("Fetching plugins from marketplace: {Url}", url);
                
                var response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var plugins = JsonSerializer.Deserialize<MarketplacePluginsResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return plugins ?? new MarketplacePluginsResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching plugins from marketplace");
                throw new InvalidOperationException("Failed to fetch plugins from marketplace", ex);
            }
        }

        /// <summary>
        /// Get plugin details from the marketplace
        /// </summary>
        /// <param name="pluginId">Plugin ID</param>
        /// <returns>Plugin details</returns>
        public async Task<MarketplacePluginDetails?> GetPluginDetailsAsync(string pluginId, CancellationToken cancellationToken = default)
        {
            try
            {
                var baseUrl = await GetBaseUrlAsync(cancellationToken);
                var url = $"{baseUrl}/api/plugins/{Uri.EscapeDataString(pluginId)}/details";
                
                _logger.LogInformation("Fetching plugin details from marketplace: {Url}", url);
                
                var response = await _httpClient.GetAsync(url, cancellationToken);
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var plugin = JsonSerializer.Deserialize<MarketplacePluginDetails>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return plugin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching plugin details from marketplace for plugin {PluginId}", pluginId);
                throw new InvalidOperationException($"Failed to fetch plugin details for {pluginId}", ex);
            }
        }

        /// <summary>
        /// Get plugin download URL from the marketplace
        /// </summary>
        /// <param name="pluginId">Plugin ID</param>
        /// <param name="version">Plugin version</param>
        /// <returns>Download URL and metadata</returns>
        public async Task<PluginDownloadInfo?> GetPluginDownloadAsync(string pluginId, string? version = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var baseUrl = await GetBaseUrlAsync(cancellationToken);
                var url = $"{baseUrl}/api/plugins/{Uri.EscapeDataString(pluginId)}/download";
                
                if (!string.IsNullOrWhiteSpace(version))
                {
                    url += $"?version={Uri.EscapeDataString(version)}";
                }
                
                _logger.LogInformation("Getting plugin download info from marketplace: {Url}", url);
                
                var response = await _httpClient.GetAsync(url, cancellationToken);
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var downloadInfo = JsonSerializer.Deserialize<PluginDownloadInfo>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return downloadInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plugin download info from marketplace for plugin {PluginId}", pluginId);
                throw new InvalidOperationException($"Failed to get download info for plugin {pluginId}", ex);
            }
        }

        /// <summary>
        /// Get plugin categories from the marketplace
        /// </summary>
        /// <returns>List of categories</returns>
        public async Task<List<MarketplaceCategory>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var baseUrl = await GetBaseUrlAsync(cancellationToken);
                var url = $"{baseUrl}/api/plugins/categories/main-api";
                
                _logger.LogInformation("Fetching categories from marketplace: {Url}", url);
                
                var response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var categories = JsonSerializer.Deserialize<List<MarketplaceCategory>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return categories ?? new List<MarketplaceCategory>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories from marketplace");
                throw new InvalidOperationException("Failed to fetch categories from marketplace", ex);
            }
        }

        /// <summary>
        /// Record plugin installation for analytics
        /// </summary>
        /// <param name="pluginId">Plugin ID</param>
        /// <param name="version">Plugin version</param>
        /// <param name="userId">User ID who installed the plugin</param>
        public async Task RecordInstallationAsync(string pluginId, string version, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var baseUrl = await GetBaseUrlAsync(cancellationToken);
                var url = $"{baseUrl}/api/plugins/{Uri.EscapeDataString(pluginId)}/install";
                
                var installation = new PluginInstallationRequestDto
                {
                    PluginId = pluginId,
                    Version = version
                };

                var json = JsonSerializer.Serialize(installation);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                _logger.LogInformation("Recording plugin installation: {PluginId} v{Version} for user {UserId}", 
                    pluginId, version, userId);
                
                var response = await _httpClient.PostAsync(url, content, cancellationToken);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording plugin installation for {PluginId}", pluginId);
                // Don't throw - analytics failures shouldn't break the installation
            }
        }
    }

    // Data Transfer Objects
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

    public class MarketplacePlugin
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public int Downloads { get; set; }
        public double Rating { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public string RepositoryUrl { get; set; } = string.Empty;
        public string RepositoryBranch { get; set; } = "main";
        public string RepositorySubpath { get; set; } = string.Empty;
        public List<string> Targets { get; set; } = new();
        public bool IsVerified { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string License { get; set; } = string.Empty;
        public List<string> Screenshots { get; set; } = new();
        public string DocumentationUrl { get; set; } = string.Empty;
    }

    public class MarketplacePluginDetails : MarketplacePlugin
    {
        public string Readme { get; set; } = string.Empty;
        public List<MarketplacePluginVersion> Versions { get; set; } = new();
        public List<MarketplacePluginReview> Reviews { get; set; } = new();
        public MarketplacePluginStats Stats { get; set; } = new();
    }

    public class MarketplacePluginVersion
    {
        public string Version { get; set; } = string.Empty;
        public string ReleaseNotes { get; set; } = string.Empty;
        public DateTime ReleasedAt { get; set; }
        public bool IsLatest { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }

    public class MarketplacePluginReview
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsVerified { get; set; }
    }

    public class MarketplacePluginStats
    {
        public int TotalDownloads { get; set; }
        public int DownloadsThisMonth { get; set; }
        public int DownloadsThisWeek { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public DateTime LastDownloaded { get; set; }
    }

    public class MarketplaceCategory
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int PluginCount { get; set; }
        public int SortOrder { get; set; }
    }

    public class PluginDownloadInfo
    {
        public string PluginId { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string Checksum { get; set; } = string.Empty;
        public string ChecksumAlgorithm { get; set; } = string.Empty;
        public DateTime ReleasedAt { get; set; }
        public string ReleaseNotes { get; set; } = string.Empty;
    }

    internal class PluginInstallationRequestDto
    {
        public string PluginId { get; set; } = string.Empty;
        public string? Version { get; set; }
    }
}

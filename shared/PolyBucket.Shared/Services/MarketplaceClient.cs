using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PolyBucket.Contracts;

namespace PolyBucket.Shared.Services
{
    public class MarketplaceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MarketplaceClient> _logger;
        private readonly string _marketplaceBaseUrl;

        public MarketplaceClient(
            HttpClient httpClient,
            ILogger<MarketplaceClient> logger,
            string marketplaceBaseUrl)
        {
            _httpClient = httpClient;
            _logger = logger;
            _marketplaceBaseUrl = marketplaceBaseUrl.TrimEnd('/');
        }

        public async Task<List<MarketplacePlugin>> GetPluginsAsync(
            string? category = null,
            string? search = null,
            int page = 1,
            int limit = 20)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(category))
                    queryParams.Add($"category={Uri.EscapeDataString(category)}");
                if (!string.IsNullOrEmpty(search))
                    queryParams.Add($"search={Uri.EscapeDataString(search)}");
                queryParams.Add($"page={page}");
                queryParams.Add($"limit={limit}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var url = $"{_marketplaceBaseUrl}/api/plugins{queryString}";

                _logger.LogInformation("Fetching plugins from marketplace: {Url}", url);

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var plugins = JsonSerializer.Deserialize<List<MarketplacePlugin>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Successfully fetched {Count} plugins from marketplace", plugins?.Count ?? 0);
                return plugins ?? new List<MarketplacePlugin>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching plugins from marketplace");
                return new List<MarketplacePlugin>();
            }
        }

        public async Task<MarketplacePluginDetails?> GetPluginDetailsAsync(string pluginId)
        {
            try
            {
                var url = $"{_marketplaceBaseUrl}/api/plugins/{pluginId}";
                
                _logger.LogInformation("Fetching plugin details: {PluginId}", pluginId);

                var response = await _httpClient.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Plugin not found: {PluginId}", pluginId);
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var plugin = JsonSerializer.Deserialize<MarketplacePluginDetails>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Successfully fetched plugin details: {PluginId}", pluginId);
                return plugin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching plugin details: {PluginId}", pluginId);
                return null;
            }
        }

        public async Task<PluginInstallationResult> InstallPluginAsync(PluginInstallationRequest request)
        {
            try
            {
                var url = $"{_marketplaceBaseUrl}/api/plugins/{request.PluginId}/install";
                
                _logger.LogInformation("Installing plugin: {PluginId} for user: {UserId}", 
                    request.PluginId, request.UserId);

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PluginInstallationResult>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Plugin installation result: {Success} for {PluginId}", 
                    result?.Success, request.PluginId);

                return result ?? new PluginInstallationResult
                {
                    Success = false,
                    Message = "Failed to deserialize installation result"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error installing plugin: {PluginId}", request.PluginId);
                return new PluginInstallationResult
                {
                    Success = false,
                    Message = ex.Message,
                    PluginId = request.PluginId
                };
            }
        }

        public async Task<bool> RatePluginAsync(string pluginId, PluginRating rating)
        {
            try
            {
                var url = $"{_marketplaceBaseUrl}/api/plugins/{pluginId}/rate";
                
                _logger.LogInformation("Rating plugin: {PluginId} with rating: {Rating}", 
                    pluginId, rating.Rating);

                var json = JsonSerializer.Serialize(rating);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully rated plugin: {PluginId}", pluginId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rating plugin: {PluginId}", pluginId);
                return false;
            }
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            try
            {
                var url = $"{_marketplaceBaseUrl}/api/categories";
                
                _logger.LogInformation("Fetching categories from marketplace");

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var categories = JsonSerializer.Deserialize<List<string>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Successfully fetched {Count} categories from marketplace", 
                    categories?.Count ?? 0);

                return categories ?? new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories from marketplace");
                return new List<string>();
            }
        }

        public async Task<bool> RecordInstallationAsync(string pluginId, string userId, string instanceId)
        {
            try
            {
                var url = $"{_marketplaceBaseUrl}/api/analytics/installations";
                
                var installation = new
                {
                    PluginId = pluginId,
                    UserId = userId,
                    InstanceId = instanceId,
                    InstalledAt = DateTime.UtcNow
                };

                var json = JsonSerializer.Serialize(installation);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                _logger.LogInformation("Successfully recorded installation: {PluginId} for user: {UserId}", 
                    pluginId, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording installation: {PluginId}", pluginId);
                return false;
            }
        }
    }
}

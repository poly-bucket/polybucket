using Microsoft.EntityFrameworkCore;
using PolyBucket.Marketplace.Api.Data;
using PolyBucket.Marketplace.Api.Models;

namespace PolyBucket.Marketplace.Api.Services
{
    public class PluginService : IPluginService
    {
        private readonly MarketplaceDbContext _context;
        private readonly ILogger<PluginService> _logger;

        public PluginService(MarketplaceDbContext context, ILogger<PluginService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Plugin>> GetPluginsAsync(string? category = null, string? search = null)
        {
            try
            {
                var query = _context.Plugins
                    .Include(p => p.Versions)
                    .Where(p => p.IsActive);

                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(p => p.Category == category);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
                }

                var plugins = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} plugins", plugins.Count);
                return plugins;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving plugins");
                return new List<Plugin>();
            }
        }

        public async Task<Plugin?> GetPluginAsync(string id)
        {
            try
            {
                var plugin = await _context.Plugins
                    .Include(p => p.Versions)
                    .Include(p => p.Reviews)
                    .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

                if (plugin != null)
                {
                    _logger.LogInformation("Retrieved plugin: {PluginName}", plugin.Name);
                }

                return plugin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving plugin: {PluginId}", id);
                return null;
            }
        }

        public async Task<Plugin> CreatePluginAsync(Plugin plugin)
        {
            try
            {
                plugin.Id = Guid.NewGuid().ToString();
                plugin.CreatedAt = DateTime.UtcNow;
                plugin.UpdatedAt = DateTime.UtcNow;
                plugin.IsActive = true;

                _context.Plugins.Add(plugin);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created plugin: {PluginName} with ID: {PluginId}", plugin.Name, plugin.Id);
                return plugin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating plugin: {PluginName}", plugin.Name);
                throw;
            }
        }

        public async Task<Plugin> UpdatePluginAsync(Plugin plugin)
        {
            try
            {
                plugin.UpdatedAt = DateTime.UtcNow;
                _context.Plugins.Update(plugin);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated plugin: {PluginName} with ID: {PluginId}", plugin.Name, plugin.Id);
                return plugin;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating plugin: {PluginId}", plugin.Id);
                throw;
            }
        }

        public async Task<bool> DeletePluginAsync(string id)
        {
            try
            {
                var plugin = await _context.Plugins.FindAsync(id);
                if (plugin == null)
                {
                    return false;
                }

                plugin.IsActive = false;
                plugin.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Soft deleted plugin: {PluginName} with ID: {PluginId}", plugin.Name, id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting plugin: {PluginId}", id);
                return false;
            }
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            try
            {
                var categories = await _context.Plugins
                    .Where(p => p.IsActive)
                    .Select(p => p.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} categories", categories.Count);
                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return new List<string>();
            }
        }

        public async Task<PluginBrowseResponse> BrowsePluginsAsync(PluginBrowseRequest request)
        {
            try
            {
                var query = _context.Plugins
                    .Include(p => p.Reviews)
                    .Where(p => p.IsActive);

                // Apply search filter
                if (!string.IsNullOrEmpty(request.Search))
                {
                    var searchTerm = request.Search.ToLower();
                    query = query.Where(p => 
                        p.Name.ToLower().Contains(searchTerm) || 
                        p.Description.ToLower().Contains(searchTerm) ||
                        p.LongDescription.ToLower().Contains(searchTerm));
                }

                // Apply category filter
                if (!string.IsNullOrEmpty(request.Category))
                {
                    query = query.Where(p => p.Category == request.Category);
                }

                // Apply verification filter
                if (request.IsVerified.HasValue)
                {
                    query = query.Where(p => p.IsVerified == request.IsVerified.Value);
                }

                // Apply featured filter
                if (request.IsFeatured.HasValue)
                {
                    query = query.Where(p => p.IsFeatured == request.IsFeatured.Value);
                }

                // Apply minimum rating filter
                if (request.MinRating.HasValue)
                {
                    query = query.Where(p => p.AverageRating >= request.MinRating.Value);
                }

                // Apply sorting
                query = request.SortBy?.ToLower() switch
                {
                    "rating" => request.SortOrder == "asc" ? query.OrderBy(p => p.AverageRating) : query.OrderByDescending(p => p.AverageRating),
                    "created" => request.SortOrder == "asc" ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt),
                    "updated" => request.SortOrder == "asc" ? query.OrderBy(p => p.UpdatedAt) : query.OrderByDescending(p => p.UpdatedAt),
                    "name" => request.SortOrder == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
                    _ => request.SortOrder == "asc" ? query.OrderBy(p => p.Downloads) : query.OrderByDescending(p => p.Downloads)
                };

                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply pagination
                var plugins = await query
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(p => new PluginSummary
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Category = p.Category,
                        Version = p.Version,
                        Author = p.Author != null ? p.Author.UserName : "Unknown",
                        AuthorId = p.AuthorId,
                        Downloads = p.Downloads,
                        AverageRating = p.AverageRating,
                        ReviewCount = p.Reviews.Count,
                        IsVerified = p.IsVerified,
                        IsFeatured = p.IsFeatured,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        Tags = new List<string>() // TODO: Implement tags when tag system is added
                    })
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

                _logger.LogInformation("Retrieved {Count} plugins for browse request", plugins.Count);

                return new PluginBrowseResponse
                {
                    Plugins = plugins,
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalPages = totalPages,
                    HasNextPage = request.Page < totalPages,
                    HasPreviousPage = request.Page > 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error browsing plugins");
                return new PluginBrowseResponse();
            }
        }

        public async Task<List<string>> GetPopularTagsAsync(int limit = 20)
        {
            try
            {
                // For now, return mock popular tags
                // TODO: Implement actual tag system with database queries
                var popularTags = new List<string>
                {
                    "react", "typescript", "api", "dashboard", "charts", "forms", 
                    "tables", "notifications", "authentication", "ui-components",
                    "data-viz", "productivity", "analytics", "integrations", "themes",
                    "localization", "security", "performance", "mobile", "responsive"
                };

                return popularTags.Take(limit).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving popular tags");
                return new List<string>();
            }
        }

        public async Task<List<PluginSummary>> GetFeaturedPluginsAsync(int limit = 6)
        {
            try
            {
                var plugins = await _context.Plugins
                    .Where(p => p.IsActive && p.IsFeatured)
                    .Include(p => p.Reviews)
                    .OrderByDescending(p => p.Downloads)
                    .Take(limit)
                    .Select(p => new PluginSummary
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Category = p.Category,
                        Version = p.Version,
                        Author = p.Author != null ? p.Author.UserName : "Unknown",
                        AuthorId = p.AuthorId,
                        Downloads = p.Downloads,
                        AverageRating = p.AverageRating,
                        ReviewCount = p.Reviews.Count,
                        IsVerified = p.IsVerified,
                        IsFeatured = p.IsFeatured,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        Tags = new List<string>()
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} featured plugins", plugins.Count);
                return plugins;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving featured plugins");
                return new List<PluginSummary>();
            }
        }

        public async Task<List<PluginSummary>> GetTrendingPluginsAsync(int limit = 10)
        {
            try
            {
                // Trending plugins based on recent downloads and ratings
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                
                var plugins = await _context.Plugins
                    .Where(p => p.IsActive && p.CreatedAt >= thirtyDaysAgo)
                    .Include(p => p.Reviews)
                    .OrderByDescending(p => p.Downloads)
                    .ThenByDescending(p => p.AverageRating)
                    .Take(limit)
                    .Select(p => new PluginSummary
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Category = p.Category,
                        Version = p.Version,
                        Author = p.Author != null ? p.Author.UserName : "Unknown",
                        AuthorId = p.AuthorId,
                        Downloads = p.Downloads,
                        AverageRating = p.AverageRating,
                        ReviewCount = p.Reviews.Count,
                        IsVerified = p.IsVerified,
                        IsFeatured = p.IsFeatured,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        Tags = new List<string>()
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} trending plugins", plugins.Count);
                return plugins;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trending plugins");
                return new List<PluginSummary>();
            }
        }

        public async Task<PluginDownloadInfo?> GetPluginDownloadAsync(string pluginId, string? version = null)
        {
            try
            {
                _logger.LogInformation("GetPluginDownloadAsync called with pluginId: {PluginId}, version: {Version}", pluginId, version);
                
                var plugin = await _context.Plugins
                    .Include(p => p.Versions)
                    .FirstOrDefaultAsync(p => p.Id == pluginId && p.IsActive);

                _logger.LogInformation("Plugin found: {PluginFound}, PluginId: {PluginId}, IsActive: {IsActive}", 
                    plugin != null, plugin?.Id, plugin?.IsActive);

                if (plugin == null)
                {
                    _logger.LogWarning("Plugin not found or not active: {PluginId}", pluginId);
                    return null;
                }

                _logger.LogInformation("Plugin has {VersionCount} versions", plugin.Versions?.Count ?? 0);

                // Find the specific version or use the latest
                var targetVersion = version ?? plugin.Version;
                var pluginVersion = plugin.Versions
                    .Where(v => v.IsActive && v.Version == targetVersion)
                    .OrderByDescending(v => v.CreatedAt)
                    .FirstOrDefault();

                _logger.LogInformation("Target version: {TargetVersion}, Found version: {FoundVersion}", 
                    targetVersion, pluginVersion?.Version);

                if (pluginVersion == null)
                {
                    // If no specific version found, create a download URL based on the plugin
                    pluginVersion = new PluginVersion
                    {
                        Version = plugin.Version,
                        DownloadUrl = $"/api/plugins/{pluginId}/download",
                        ReleaseNotes = "Latest version"
                    };
                    _logger.LogInformation("Created fallback version with URL: {DownloadUrl}", pluginVersion.DownloadUrl);
                }

                return new PluginDownloadInfo
                {
                    PluginId = pluginId,
                    Version = pluginVersion.Version,
                    DownloadUrl = pluginVersion.DownloadUrl,
                    FileName = $"{plugin.Name}-{pluginVersion.Version}.zip",
                    FileSize = 0, // TODO: Calculate actual file size
                    Checksum = "", // TODO: Calculate checksum
                    CreatedAt = pluginVersion.CreatedAt,
                    ReleaseNotes = pluginVersion.ReleaseNotes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plugin download info for plugin {PluginId}", pluginId);
                return null;
            }
        }

        public async Task<MarketplacePluginDetails?> GetPluginDetailsAsync(string pluginId)
        {
            try
            {
                var plugin = await _context.Plugins
                    .Include(p => p.Versions)
                    .Include(p => p.Reviews)
                    .Include(p => p.Author)
                    .FirstOrDefaultAsync(p => p.Id == pluginId && p.IsActive);

                if (plugin == null)
                {
                    return null;
                }

                return new MarketplacePluginDetails
                {
                    Id = plugin.Id,
                    Name = plugin.Name,
                    Description = plugin.Description,
                    LongDescription = plugin.LongDescription,
                    Category = plugin.Category,
                    Version = plugin.Version,
                    RepositoryUrl = plugin.RepositoryUrl,
                    License = plugin.License,
                    IsVerified = plugin.IsVerified,
                    IsFeatured = plugin.IsFeatured,
                    Downloads = plugin.Downloads,
                    AverageRating = plugin.AverageRating,
                    ReviewCount = plugin.Reviews.Count,
                    Author = plugin.Author != null ? plugin.Author.UserName : "Unknown",
                    CreatedAt = plugin.CreatedAt,
                    UpdatedAt = plugin.UpdatedAt,
                    Versions = plugin.Versions.Where(v => v.IsActive).Select(v => new PluginVersion
                    {
                        Id = v.Id,
                        PluginId = v.PluginId,
                        Version = v.Version,
                        DownloadUrl = v.DownloadUrl,
                        ReleaseNotes = v.ReleaseNotes,
                        IsActive = v.IsActive,
                        CreatedAt = v.CreatedAt
                    }).ToList(),
                    Tags = new List<string>() // TODO: Implement tags when tag system is added
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plugin details for plugin {PluginId}", pluginId);
                return null;
            }
        }

        public async Task<PluginInstallationResponse> RecordInstallationAsync(PluginInstallationRequest request)
        {
            try
            {
                // Check if plugin exists
                var plugin = await _context.Plugins.FindAsync(request.PluginId);
                if (plugin == null)
                {
                    return new PluginInstallationResponse
                    {
                        Success = false,
                        Message = "Plugin not found",
                        InstalledAt = DateTime.UtcNow
                    };
                }

                // Record the installation in the database
                var installation = new PluginDownload
                {
                    Id = Guid.NewGuid().ToString(),
                    PluginId = request.PluginId,
                    InstanceId = request.InstanceId,
                    UserAgent = request.UserAgent,
                    IpAddress = request.IpAddress,
                    DownloadedAt = DateTime.UtcNow
                };

                _context.Downloads.Add(installation);

                // Update download count
                plugin.Downloads++;
                plugin.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Recorded installation for plugin {PluginId}", request.PluginId);

                return new PluginInstallationResponse
                {
                    Success = true,
                    Message = "Installation recorded successfully",
                    InstalledAt = installation.DownloadedAt,
                    InstallationId = installation.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording installation for plugin {PluginId}", request.PluginId);
                return new PluginInstallationResponse
                {
                    Success = false,
                    Message = "Failed to record installation",
                    InstalledAt = DateTime.UtcNow
                };
            }
        }

        public async Task<MarketplacePluginsResponse> GetPluginsForMainApiAsync(int page = 1, int pageSize = 20, string? category = null, string? search = null)
        {
            try
            {
                var query = _context.Plugins
                    .Include(p => p.Reviews)
                    .Include(p => p.Author)
                    .Where(p => p.IsActive);

                // Apply filters
                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(p => p.Category == category);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    var searchTerm = search.ToLower();
                    query = query.Where(p =>
                        p.Name.ToLower().Contains(searchTerm) ||
                        p.Description.ToLower().Contains(searchTerm));
                }

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination
                var plugins = await query
                    .OrderByDescending(p => p.Downloads)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new MarketplacePluginDetails
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        LongDescription = p.LongDescription,
                        Category = p.Category,
                        Version = p.Version,
                        RepositoryUrl = p.RepositoryUrl,
                        License = p.License,
                        IsVerified = p.IsVerified,
                        IsFeatured = p.IsFeatured,
                        Downloads = p.Downloads,
                        AverageRating = p.AverageRating,
                        ReviewCount = p.Reviews.Count,
                        Author = p.Author != null ? p.Author.UserName : "Unknown",
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        Versions = new List<PluginVersion>(),
                        Tags = new List<string>()
                    })
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return new MarketplacePluginsResponse
                {
                    Plugins = plugins,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    HasNextPage = page < totalPages,
                    HasPreviousPage = page > 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plugins for main API");
                return new MarketplacePluginsResponse();
            }
        }

        public async Task<List<MarketplaceCategory>> GetCategoriesForMainApiAsync()
        {
            try
            {
                var categories = await _context.Plugins
                    .Where(p => p.IsActive)
                    .GroupBy(p => p.Category)
                    .Select(g => new MarketplaceCategory
                    {
                        Id = g.Key,
                        Name = g.Key,
                        Description = $"Plugins in the {g.Key} category",
                        PluginCount = g.Count()
                    })
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories for main API");
                return new List<MarketplaceCategory>();
            }
        }
    }
}

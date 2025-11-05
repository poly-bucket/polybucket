using Microsoft.AspNetCore.Mvc;
using PolyBucket.Marketplace.Api.Models;
using PolyBucket.Marketplace.Api.Services;

namespace PolyBucket.Marketplace.Api.Controllers
{
    /// <summary>
    /// Controller for managing plugins in the marketplace
    /// </summary>
    /// <remarks>
    /// This controller provides endpoints for browsing, viewing, creating, updating, and managing plugins.
    /// It supports filtering, searching, pagination, and integration with the main PolyBucket API.
    /// </remarks>
    [ApiController]
    [Route("api/plugins")]
    [Produces("application/json")]
    public class PluginController : ControllerBase
    {
        private readonly IPluginService _pluginService;
        private readonly IFileService _fileService;
        private readonly ILogger<PluginController> _logger;

        public PluginController(
            IPluginService pluginService,
            IFileService fileService,
            ILogger<PluginController> logger)
        {
            _pluginService = pluginService;
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Get all plugins with optional filtering
        /// </summary>
        /// <param name="category">Filter plugins by category (optional)</param>
        /// <param name="search">Search term to filter plugins by name or description (optional)</param>
        /// <returns>List of plugins matching the criteria</returns>
        /// <response code="200">Returns the list of plugins</response>
        /// <response code="500">If an error occurs while retrieving plugins</response>
        /// <example>
        /// GET /api/plugins?category=UI Components&amp;search=chart
        /// </example>
        [HttpGet]
        [ProducesResponseType(typeof(List<Plugin>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<Plugin>>> GetPlugins(
            [FromQuery] string? category = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var plugins = await _pluginService.GetPluginsAsync(category, search);
                return Ok(plugins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plugins");
                return StatusCode(500, new { message = "Error retrieving plugins" });
            }
        }

        /// <summary>
        /// Browse plugins with advanced filtering, sorting, and pagination
        /// </summary>
        /// <param name="request">Browse request with filters, sorting, and pagination options</param>
        /// <returns>Paginated list of plugins matching the browse criteria</returns>
        /// <response code="200">Returns the paginated list of plugins</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="500">If an error occurs while browsing plugins</response>
        /// <example>
        /// POST /api/plugins/browse
        /// {
        ///   "search": "dashboard",
        ///   "category": "UI Components",
        ///   "sortBy": "downloads",
        ///   "sortOrder": "desc",
        ///   "page": 1,
        ///   "pageSize": 20
        /// }
        /// </example>
        [HttpPost("browse")]
        [ProducesResponseType(typeof(PluginBrowseResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PluginBrowseResponse>> BrowsePlugins([FromBody] PluginBrowseRequest request)
        {
            try
            {
                // Validate request
                if (request.Page < 1) request.Page = 1;
                if (request.PageSize < 1 || request.PageSize > 100) request.PageSize = 20;

                var response = await _pluginService.BrowsePluginsAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error browsing plugins");
                return StatusCode(500, new { message = "Error browsing plugins" });
            }
        }

        /// <summary>
        /// Get featured plugins
        /// </summary>
        [HttpGet("featured")]
        [ProducesResponseType(typeof(List<PluginSummary>), 200)]
        public async Task<ActionResult<List<PluginSummary>>> GetFeaturedPlugins([FromQuery] int limit = 6)
        {
            try
            {
                var plugins = await _pluginService.GetFeaturedPluginsAsync(limit);
                return Ok(plugins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured plugins");
                return StatusCode(500, new { message = "Error retrieving featured plugins" });
            }
        }

        /// <summary>
        /// Get trending plugins
        /// </summary>
        [HttpGet("trending")]
        [ProducesResponseType(typeof(List<PluginSummary>), 200)]
        public async Task<ActionResult<List<PluginSummary>>> GetTrendingPlugins([FromQuery] int limit = 10)
        {
            try
            {
                var plugins = await _pluginService.GetTrendingPluginsAsync(limit);
                return Ok(plugins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trending plugins");
                return StatusCode(500, new { message = "Error retrieving trending plugins" });
            }
        }

        /// <summary>
        /// Get popular tags
        /// </summary>
        [HttpGet("tags/popular")]
        [ProducesResponseType(typeof(List<string>), 200)]
        public async Task<ActionResult<List<string>>> GetPopularTags([FromQuery] int limit = 20)
        {
            try
            {
                var tags = await _pluginService.GetPopularTagsAsync(limit);
                return Ok(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular tags");
                return StatusCode(500, new { message = "Error retrieving popular tags" });
            }
        }

        /// <summary>
        /// Get a specific plugin by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Plugin), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Plugin>> GetPlugin(string id)
        {
            try
            {
                var plugin = await _pluginService.GetPluginAsync(id);
                if (plugin == null)
                {
                    return NotFound(new { message = "Plugin not found" });
                }

                return Ok(plugin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plugin: {PluginId}", id);
                return StatusCode(500, new { message = "Error retrieving plugin" });
            }
        }

        /// <summary>
        /// Create a new plugin
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Plugin), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Plugin>> CreatePlugin([FromBody] CreatePluginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest(new { message = "Plugin name is required" });
                }

                var plugin = new Plugin
                {
                    Name = request.Name,
                    Description = request.Description ?? string.Empty,
                    Category = request.Category ?? "Other",
                    RepositoryUrl = request.RepositoryUrl ?? string.Empty,
                    License = request.License ?? "MIT",
                    IsVerified = false,
                    IsFeatured = false,
                    IsActive = true
                };

                var createdPlugin = await _pluginService.CreatePluginAsync(plugin);
                return CreatedAtAction(nameof(GetPlugin), new { id = createdPlugin.Id }, createdPlugin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating plugin");
                return StatusCode(500, new { message = "Error creating plugin" });
            }
        }

        /// <summary>
        /// Upload plugin files
        /// </summary>
        [HttpPost("{id}/upload")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> UploadPluginFiles(string id, [FromForm] IFormFileCollection files)
        {
            try
            {
                var plugin = await _pluginService.GetPluginAsync(id);
                if (plugin == null)
                {
                    return NotFound(new { message = "Plugin not found" });
                }

                if (files == null || files.Count == 0)
                {
                    return BadRequest(new { message = "No files provided" });
                }

                var uploadedFiles = new List<string>();

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var filePath = await _fileService.SavePluginFileAsync(file, id.ToString());
                        uploadedFiles.Add(filePath);
                    }
                }

                _logger.LogInformation("Uploaded {Count} files for plugin: {PluginId}", uploadedFiles.Count, id);

                return Ok(new
                {
                    message = $"Successfully uploaded {uploadedFiles.Count} files",
                    files = uploadedFiles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading files for plugin: {PluginId}", id);
                return StatusCode(500, new { message = "Error uploading files" });
            }
        }

        /// <summary>
        /// Download plugin files
        /// </summary>
        [HttpGet("{id}/download")]
        [ProducesResponseType(typeof(PluginDownloadInfo), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PluginDownloadInfo>> DownloadPlugin(string id, [FromQuery] string? version = null)
        {
            try
            {
                var downloadInfo = await _pluginService.GetPluginDownloadAsync(id, version);
                if (downloadInfo == null)
                {
                    return NotFound(new { message = "Plugin not found" });
                }

                return Ok(downloadInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading plugin: {PluginId}", id);
                return StatusCode(500, new { message = "Error downloading plugin" });
            }
        }

        /// <summary>
        /// Update a plugin
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Plugin), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Plugin>> UpdatePlugin(string id, [FromBody] UpdatePluginRequest request)
        {
            try
            {
                var plugin = await _pluginService.GetPluginAsync(id);
                if (plugin == null)
                {
                    return NotFound(new { message = "Plugin not found" });
                }

                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    plugin.Name = request.Name;
                }

                if (!string.IsNullOrWhiteSpace(request.Description))
                {
                    plugin.Description = request.Description;
                }

                if (!string.IsNullOrWhiteSpace(request.Category))
                {
                    plugin.Category = request.Category;
                }

                if (!string.IsNullOrWhiteSpace(request.RepositoryUrl))
                {
                    plugin.RepositoryUrl = request.RepositoryUrl;
                }

                if (!string.IsNullOrWhiteSpace(request.License))
                {
                    plugin.License = request.License;
                }

                var updatedPlugin = await _pluginService.UpdatePluginAsync(plugin);
                return Ok(updatedPlugin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating plugin: {PluginId}", id);
                return StatusCode(500, new { message = "Error updating plugin" });
            }
        }

        /// <summary>
        /// Delete a plugin
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> DeletePlugin(string id)
        {
            try
            {
                var success = await _pluginService.DeletePluginAsync(id);
                if (!success)
                {
                    return NotFound(new { message = "Plugin not found" });
                }

                return Ok(new { message = "Plugin deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting plugin: {PluginId}", id);
                return StatusCode(500, new { message = "Error deleting plugin" });
            }
        }

        /// <summary>
        /// Get plugin categories
        /// </summary>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(List<string>), 200)]
        public async Task<ActionResult<List<string>>> GetCategories()
        {
            try
            {
                var categories = await _pluginService.GetCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                return StatusCode(500, new { message = "Error retrieving categories" });
            }
        }

        /// <summary>
        /// Get plugin details for main API integration
        /// </summary>
        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(MarketplacePluginDetails), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<MarketplacePluginDetails>> GetPluginDetails(string id)
        {
            try
            {
                var pluginDetails = await _pluginService.GetPluginDetailsAsync(id);
                if (pluginDetails == null)
                {
                    return NotFound(new { message = "Plugin not found" });
                }

                return Ok(pluginDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plugin details: {PluginId}", id);
                return StatusCode(500, new { message = "Error retrieving plugin details" });
            }
        }

        /// <summary>
        /// Record plugin installation
        /// </summary>
        [HttpPost("{id}/install")]
        [ProducesResponseType(typeof(PluginInstallationResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PluginInstallationResponse>> RecordInstallation(string id, [FromBody] PluginInstallationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { message = "Plugin ID is required" });
                }

                request.PluginId = id;
                var result = await _pluginService.RecordInstallationAsync(request);
                
                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording installation for plugin: {PluginId}", id);
                return StatusCode(500, new { message = "Error recording installation" });
            }
        }

        /// <summary>
        /// Get plugins for main API integration
        /// </summary>
        [HttpGet("main-api")]
        [ProducesResponseType(typeof(MarketplacePluginsResponse), 200)]
        public async Task<ActionResult<MarketplacePluginsResponse>> GetPluginsForMainApi(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? category = null,
            [FromQuery] string? search = null)
        {
            try
            {
                var response = await _pluginService.GetPluginsForMainApiAsync(page, pageSize, category, search);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plugins for main API");
                return StatusCode(500, new { message = "Error retrieving plugins" });
            }
        }

        /// <summary>
        /// Get categories for main API integration
        /// </summary>
        [HttpGet("categories/main-api")]
        [ProducesResponseType(typeof(List<MarketplaceCategory>), 200)]
        public async Task<ActionResult<List<MarketplaceCategory>>> GetCategoriesForMainApi()
        {
            try
            {
                var categories = await _pluginService.GetCategoriesForMainApiAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories for main API");
                return StatusCode(500, new { message = "Error retrieving categories" });
            }
        }
    }

}

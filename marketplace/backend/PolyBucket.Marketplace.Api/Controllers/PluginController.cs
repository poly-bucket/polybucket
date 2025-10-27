using Microsoft.AspNetCore.Mvc;
using PolyBucket.Marketplace.Api.Models;
using PolyBucket.Marketplace.Api.Services;

namespace PolyBucket.Marketplace.Api.Controllers
{
    [ApiController]
    [Route("api/plugins")]
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
        [HttpGet]
        [ProducesResponseType(typeof(List<Plugin>), 200)]
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
        [ProducesResponseType(typeof(FileResult), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> DownloadPlugin(string id)
        {
            try
            {
                var plugin = await _pluginService.GetPluginAsync(id);
                if (plugin == null)
                {
                    return NotFound(new { message = "Plugin not found" });
                }

                // For now, return a simple response
                // In a real implementation, you'd package the plugin files
                return Ok(new
                {
                    message = "Plugin download not yet implemented",
                    pluginId = id,
                    pluginName = plugin.Name
                });
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
    }

}

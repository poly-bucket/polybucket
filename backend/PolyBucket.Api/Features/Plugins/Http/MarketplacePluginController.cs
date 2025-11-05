using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Plugins.Services;
using PolyBucket.Api.Features.Plugins.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Plugins.Http
{
    /// <summary>
    /// Controller for marketplace plugin integration
    /// </summary>
    [ApiController]
    [Route("api/plugins/marketplace")]
    [Authorize]
    [ApiExplorerSettings(GroupName = "v1")]
    [Tags("Plugin Marketplace")]
    public class MarketplacePluginController(
        MarketplaceClient marketplaceClient,
        PluginInstallationService installationService,
        ILogger<MarketplacePluginController> logger) : ControllerBase
    {
        private readonly MarketplaceClient _marketplaceClient = marketplaceClient;
        private readonly PluginInstallationService _installationService = installationService;
        private readonly ILogger<MarketplacePluginController> _logger = logger;

        /// <summary>
        /// Get available plugins from the marketplace
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 20, max: 100)</param>
        /// <param name="category">Filter by category</param>
        /// <param name="search">Search query</param>
        /// <returns>List of marketplace plugins</returns>
        [HttpGet]
        [ProducesResponseType(typeof(MarketplacePluginsResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<MarketplacePluginsResponse>> GetMarketplacePlugins(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? category = null,
            [FromQuery] string? search = null)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var response = await _marketplaceClient.GetPluginsAsync(page, pageSize, category, search);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching marketplace plugins");
                return StatusCode(500, new { message = "Error fetching marketplace plugins" });
            }
        }

        /// <summary>
        /// Get plugin details from the marketplace
        /// </summary>
        /// <param name="pluginId">Plugin ID</param>
        /// <returns>Plugin details</returns>
        [HttpGet("{pluginId}")]
        [ProducesResponseType(typeof(MarketplacePluginDetails), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<MarketplacePluginDetails>> GetMarketplacePlugin(string pluginId)
        {
            try
            {
                var plugin = await _marketplaceClient.GetPluginDetailsAsync(pluginId);
                
                if (plugin == null)
                {
                    return NotFound(new { message = $"Plugin {pluginId} not found in marketplace" });
                }

                return Ok(plugin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching marketplace plugin {PluginId}", pluginId);
                return StatusCode(500, new { message = "Error fetching plugin details" });
            }
        }

        /// <summary>
        /// Get plugin categories from the marketplace
        /// </summary>
        /// <returns>List of categories</returns>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(List<MarketplaceCategory>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<MarketplaceCategory>>> GetMarketplaceCategories()
        {
            try
            {
                var categories = await _marketplaceClient.GetCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching marketplace categories");
                return StatusCode(500, new { message = "Error fetching categories" });
            }
        }

        /// <summary>
        /// Install a plugin from the marketplace
        /// </summary>
        /// <param name="pluginId">Plugin ID</param>
        /// <param name="version">Plugin version (optional, defaults to latest)</param>
        /// <returns>Installation result</returns>
        [HttpPost("{pluginId}/install")]
        [RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
        [ProducesResponseType(typeof(PluginInstallationResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PluginInstallationResult>> InstallFromMarketplace(
            string pluginId,
            [FromQuery] string? version = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pluginId))
                {
                    return BadRequest(new { message = "Plugin ID is required" });
                }

                // Get plugin details from marketplace
                var pluginDetails = await _marketplaceClient.GetPluginDetailsAsync(pluginId);
                if (pluginDetails == null)
                {
                    return NotFound(new { message = $"Plugin {pluginId} not found in marketplace" });
                }

                // Get download info
                var downloadInfo = await _marketplaceClient.GetPluginDownloadAsync(pluginId, version);
                if (downloadInfo == null)
                {
                    return NotFound(new { message = $"Download information not available for plugin {pluginId}" });
                }

                // Install the plugin
                var result = await _installationService.InstallFromUrlAsync(downloadInfo.DownloadUrl);

                // Record installation for analytics
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
                await _marketplaceClient.RecordInstallationAsync(pluginId, downloadInfo.Version, userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error installing plugin {PluginId} from marketplace", pluginId);
                return StatusCode(500, new { message = "Error installing plugin from marketplace" });
            }
        }

        /// <summary>
        /// Get plugin installation status
        /// </summary>
        /// <param name="pluginId">Plugin ID</param>
        /// <returns>Installation status</returns>
        [HttpGet("{pluginId}/status")]
        [ProducesResponseType(typeof(PluginInstallationStatus), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PluginInstallationStatus>> GetPluginInstallationStatus(string pluginId)
        {
            try
            {
                var installedPlugins = await _installationService.GetInstalledPluginsAsync();
                var isInstalled = installedPlugins.Any(p => p.Id == pluginId);

                var status = new PluginInstallationStatus
                {
                    PluginId = pluginId,
                    IsInstalled = isInstalled,
                    InstalledVersion = isInstalled ? installedPlugins.First(p => p.Id == pluginId).Version : null,
                    InstalledAt = isInstalled ? installedPlugins.First(p => p.Id == pluginId).InstalledAt : null
                };

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting installation status for plugin {PluginId}", pluginId);
                return StatusCode(500, new { message = "Error getting plugin installation status" });
            }
        }

    }

    public class PluginInstallationStatus
    {
        public string PluginId { get; set; } = string.Empty;
        public bool IsInstalled { get; set; }
        public string? InstalledVersion { get; set; }
        public DateTime? InstalledAt { get; set; }
    }
}

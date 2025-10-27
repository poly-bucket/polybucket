using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolyBucket.Api.Features.Plugins.Domain;
using PolyBucket.Api.Features.Plugins.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Plugins.Http
{
    [ApiController]
    [Route("api/plugins")]
    [Authorize]
    public class PluginManagementController : ControllerBase
    {
        private readonly PluginInstallationService _installationService;
        private readonly ILogger<PluginManagementController> _logger;

        public PluginManagementController(
            PluginInstallationService installationService,
            ILogger<PluginManagementController> logger)
        {
            _installationService = installationService;
            _logger = logger;
        }

        /// <summary>
        /// Get all installed plugins
        /// </summary>
        /// <returns>List of installed plugins</returns>
        [HttpGet("installed")]
        [ProducesResponseType(typeof(List<InstalledPlugin>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<InstalledPlugin>>> GetInstalledPlugins()
        {
            try
            {
                var plugins = await _installationService.GetInstalledPluginsAsync();
                return Ok(plugins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting installed plugins");
                return StatusCode(500, new { message = "Error retrieving installed plugins" });
            }
        }

        /// <summary>
        /// Install a plugin from URL
        /// </summary>
        /// <param name="request">Plugin installation request</param>
        /// <returns>Installation result</returns>
        [HttpPost("install")]
        [ProducesResponseType(typeof(PluginInstallationResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PluginInstallationResult>> InstallPlugin([FromBody] PluginInstallationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Source))
                {
                    return BadRequest(new { message = "Installation source is required" });
                }

                PluginInstallationResult result;

                switch (request.Source.ToLower())
                {
                    case "url":
                        if (string.IsNullOrWhiteSpace(request.Url))
                        {
                            return BadRequest(new { message = "URL is required for URL installation" });
                        }
                        result = await _installationService.InstallFromUrlAsync(request.Url);
                        break;

                    case "github":
                        if (string.IsNullOrWhiteSpace(request.Url))
                        {
                            return BadRequest(new { message = "Repository URL is required for GitHub installation" });
                        }
                        result = await _installationService.InstallFromGitHubAsync(request.Url, request.Branch);
                        break;

                    case "marketplace":
                        if (string.IsNullOrWhiteSpace(request.PluginId))
                        {
                            return BadRequest(new { message = "Plugin ID is required for marketplace installation" });
                        }
                        result = await _installationService.InstallFromMarketplaceAsync(request.PluginId, request.Version ?? "latest");
                        break;

                    default:
                        return BadRequest(new { message = "Invalid installation source. Must be 'url', 'github', or 'marketplace'" });
                }

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error installing plugin");
                return StatusCode(500, new { message = "Error installing plugin", error = ex.Message });
            }
        }

        /// <summary>
        /// Uninstall a plugin
        /// </summary>
        /// <param name="pluginId">Plugin ID to uninstall</param>
        /// <returns>Uninstallation result</returns>
        [HttpDelete("{pluginId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> UninstallPlugin(string pluginId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pluginId))
                {
                    return BadRequest(new { message = "Plugin ID is required" });
                }

                var success = await _installationService.UninstallPluginAsync(pluginId);
                
                if (success)
                {
                    return Ok(new { message = $"Plugin {pluginId} uninstalled successfully" });
                }
                else
                {
                    return NotFound(new { message = $"Plugin {pluginId} not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uninstalling plugin {PluginId}", pluginId);
                return StatusCode(500, new { message = "Error uninstalling plugin", error = ex.Message });
            }
        }

        /// <summary>
        /// Get marketplace plugins (placeholder for future implementation)
        /// </summary>
        /// <returns>List of marketplace plugins</returns>
        [HttpGet("marketplace")]
        [ProducesResponseType(typeof(List<PolyBucket.Api.Features.Plugins.Domain.MarketplacePlugin>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<PolyBucket.Api.Features.Plugins.Domain.MarketplacePlugin>>> GetMarketplacePlugins()
        {
            try
            {
                // TODO: Implement marketplace integration
                var plugins = new List<PolyBucket.Api.Features.Plugins.Domain.MarketplacePlugin>
                {
                    new PolyBucket.Api.Features.Plugins.Domain.MarketplacePlugin
                    {
                        Id = "dark-theme-plugin",
                        Name = "Dark Theme",
                        Version = "1.2.0",
                        Author = "PolyBucket Community",
                        Description = "Dark theme with customizable accent colors",
                        Type = "theme",
                        License = "MIT",
                        Keywords = new() { "theme", "dark", "ui", "customization" },
                        DownloadCount = 1250,
                        Rating = 4.8,
                        RepositoryUrl = "https://github.com/polybucket/dark-theme-plugin",
                        LastUpdated = DateTime.UtcNow.AddDays(-7),
                        IsInstalled = false
                    },
                    new PolyBucket.Api.Features.Plugins.Domain.MarketplacePlugin
                    {
                        Id = "discord-oauth-plugin",
                        Name = "Discord OAuth",
                        Version = "1.0.0",
                        Author = "PolyBucket Community",
                        Description = "Discord account integration and community features",
                        Type = "oauth",
                        License = "MIT",
                        Keywords = new() { "oauth", "discord", "social", "integration" },
                        DownloadCount = 890,
                        Rating = 4.6,
                        RepositoryUrl = "https://github.com/polybucket/discord-oauth-plugin",
                        LastUpdated = DateTime.UtcNow.AddDays(-14),
                        IsInstalled = false
                    }
                };

                return Ok(plugins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting marketplace plugins");
                return StatusCode(500, new { message = "Error retrieving marketplace plugins" });
            }
        }

        /// <summary>
        /// Get plugin health status
        /// </summary>
        /// <returns>Plugin health information</returns>
        [HttpGet("health")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> GetPluginHealth()
        {
            try
            {
                var installedPlugins = await _installationService.GetInstalledPluginsAsync();
                
                var health = new
                {
                    TotalPlugins = installedPlugins.Count,
                    ActivePlugins = installedPlugins.Count(p => p.Status == "active"),
                    InactivePlugins = installedPlugins.Count(p => p.Status == "inactive"),
                    ErrorPlugins = installedPlugins.Count(p => p.Status == "error"),
                    Plugins = installedPlugins.Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Version,
                        p.Status,
                        p.IsEnabled
                    })
                };

                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plugin health");
                return StatusCode(500, new { message = "Error retrieving plugin health" });
            }
        }
    }
}

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
    [Route("api/plugins/themes")]
    [Authorize]
    public class ThemePluginController : ControllerBase
    {
        private readonly ThemePluginService _themeService;
        private readonly ILogger<ThemePluginController> _logger;

        public ThemePluginController(
            ThemePluginService themeService,
            ILogger<ThemePluginController> logger)
        {
            _themeService = themeService;
            _logger = logger;
        }

        /// <summary>
        /// Get all active themes
        /// </summary>
        /// <returns>List of active themes</returns>
        [HttpGet("active")]
        [ProducesResponseType(typeof(List<ActiveTheme>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public ActionResult<List<ActiveTheme>> GetActiveThemes()
        {
            try
            {
                var activeThemes = _themeService.GetActiveThemes();
                return Ok(activeThemes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active themes");
                return StatusCode(500, new { message = "Error retrieving active themes" });
            }
        }

        /// <summary>
        /// Apply a theme from a plugin
        /// </summary>
        /// <param name="pluginId">Plugin ID containing the theme</param>
        /// <returns>Theme application result</returns>
        [HttpPost("{pluginId}/apply")]
        [ProducesResponseType(typeof(ThemeApplicationResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ThemeApplicationResult>> ApplyTheme(string pluginId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pluginId))
                {
                    return BadRequest(new { message = "Plugin ID is required" });
                }

                // TODO: Get theme plugin from plugin manager
                // For now, return a placeholder response
                return Ok(new ThemeApplicationResult
                {
                    Success = true,
                    Message = $"Theme from plugin {pluginId} applied successfully",
                    AppliedVariables = new Dictionary<string, string>
                    {
                        ["--primary-color"] = "#007bff",
                        ["--background-color"] = "#ffffff"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying theme from plugin {PluginId}", pluginId);
                return StatusCode(500, new { message = "Error applying theme", error = ex.Message });
            }
        }

        /// <summary>
        /// Remove a theme
        /// </summary>
        /// <param name="pluginId">Plugin ID containing the theme</param>
        /// <returns>Theme removal result</returns>
        [HttpDelete("{pluginId}")]
        [ProducesResponseType(typeof(ThemeApplicationResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ThemeApplicationResult>> RemoveTheme(string pluginId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pluginId))
                {
                    return BadRequest(new { message = "Plugin ID is required" });
                }

                var result = await _themeService.RemoveThemeAsync(pluginId);
                
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
                _logger.LogError(ex, "Error removing theme from plugin {PluginId}", pluginId);
                return StatusCode(500, new { message = "Error removing theme", error = ex.Message });
            }
        }

        /// <summary>
        /// Update theme settings
        /// </summary>
        /// <param name="pluginId">Plugin ID containing the theme</param>
        /// <param name="settings">Theme settings to apply</param>
        /// <returns>Theme update result</returns>
        [HttpPut("{pluginId}/settings")]
        [ProducesResponseType(typeof(ThemeApplicationResult), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ThemeApplicationResult>> UpdateThemeSettings(
            string pluginId, 
            [FromBody] ThemeSettings settings)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pluginId))
                {
                    return BadRequest(new { message = "Plugin ID is required" });
                }

                if (settings == null)
                {
                    return BadRequest(new { message = "Theme settings are required" });
                }

                var result = await _themeService.UpdateThemeSettingsAsync(pluginId, settings);
                
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
                _logger.LogError(ex, "Error updating theme settings for plugin {PluginId}", pluginId);
                return StatusCode(500, new { message = "Error updating theme settings", error = ex.Message });
            }
        }

        /// <summary>
        /// Get theme settings for a plugin
        /// </summary>
        /// <param name="pluginId">Plugin ID containing the theme</param>
        /// <returns>Theme settings</returns>
        [HttpGet("{pluginId}/settings")]
        [ProducesResponseType(typeof(ThemeSettings), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ThemeSettings>> GetThemeSettings(string pluginId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pluginId))
                {
                    return BadRequest(new { message = "Plugin ID is required" });
                }

                // TODO: Get theme settings from plugin
                // For now, return default settings
                var settings = new ThemeSettings
                {
                    PrimaryColor = "#007bff",
                    SecondaryColor = "#6c757d",
                    BackgroundColor = "#ffffff",
                    SurfaceColor = "#f8f9fa",
                    TextColor = "#212529",
                    TextMutedColor = "#6c757d",
                    BorderColor = "#dee2e6",
                    ShadowColor = "rgba(0, 0, 0, 0.1)",
                    EnableAnimations = true,
                    FontFamily = "system-ui, -apple-system, sans-serif",
                    BorderRadius = 4
                };

                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting theme settings for plugin {PluginId}", pluginId);
                return StatusCode(500, new { message = "Error retrieving theme settings", error = ex.Message });
            }
        }
    }
}

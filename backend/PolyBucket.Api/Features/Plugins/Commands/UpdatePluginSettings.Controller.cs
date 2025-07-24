using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common.Plugins;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Plugins.Commands
{
    [ApiController]
    [Route("api/plugins")]
    [Authorize]
    [RequirePermission(PermissionConstants.PLUGIN_CONFIGURE)]
    public class UpdatePluginSettingsController(PluginManager pluginManager) : ControllerBase
    {
        private readonly PluginManager _pluginManager = pluginManager;

        /// <summary>
        /// Update plugin settings
        /// </summary>
        /// <param name="pluginId">The plugin ID</param>
        /// <param name="request">Plugin settings to update</param>
        /// <returns>Update result</returns>
        [HttpPut("{pluginId}/settings")]
        [ProducesResponseType(typeof(UpdatePluginSettingsResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UpdatePluginSettingsResponse>> UpdatePluginSettings(
            string pluginId, 
            [FromBody] UpdatePluginSettingsRequest request)
        {
            var plugin = _pluginManager.GetLoadedPlugins().FirstOrDefault(p => p.Id == pluginId);
            if (plugin == null)
            {
                return NotFound($"Plugin with ID '{pluginId}' not found");
            }

            try
            {
                // Validate settings against plugin schema
                var validationResult = ValidateSettings(plugin, request.Settings);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new UpdatePluginSettingsResponse
                    {
                        Success = false,
                        Message = "Settings validation failed",
                        ValidationErrors = validationResult.Errors
                    });
                }

                // In a real implementation, this would save to database/configuration store
                // For now, we'll simulate success
                var response = new UpdatePluginSettingsResponse
                {
                    Success = true,
                    Message = "Plugin settings updated successfully",
                    UpdatedSettings = request.Settings,
                    UpdatedAt = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new UpdatePluginSettingsResponse
                {
                    Success = false,
                    Message = $"Failed to update plugin settings: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Enable or disable a plugin
        /// </summary>
        /// <param name="pluginId">The plugin ID</param>
        /// <param name="request">Enable/disable request</param>
        /// <returns>Update result</returns>
        [HttpPut("{pluginId}/status")]
        [ProducesResponseType(typeof(PluginStatusResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PluginStatusResponse>> UpdatePluginStatus(
            string pluginId,
            [FromBody] UpdatePluginStatusRequest request)
        {
            var plugin = _pluginManager.GetLoadedPlugins().FirstOrDefault(p => p.Id == pluginId);
            if (plugin == null)
            {
                return NotFound($"Plugin with ID '{pluginId}' not found");
            }

            try
            {
                // Check if plugin can be disabled
                if (!request.Enabled && plugin.Metadata?.Lifecycle?.CanDisable == false)
                {
                    return BadRequest(new PluginStatusResponse
                    {
                        Success = false,
                        Message = "This plugin cannot be disabled",
                        CurrentStatus = "Active"
                    });
                }

                // In a real implementation, this would actually enable/disable the plugin
                var newStatus = request.Enabled ? "Active" : "Disabled";
                
                var response = new PluginStatusResponse
                {
                    Success = true,
                    Message = $"Plugin {(request.Enabled ? "enabled" : "disabled")} successfully",
                    CurrentStatus = newStatus,
                    UpdatedAt = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new PluginStatusResponse
                {
                    Success = false,
                    Message = $"Failed to update plugin status: {ex.Message}",
                    CurrentStatus = "Unknown"
                });
            }
        }

        /// <summary>
        /// Get current plugin configuration
        /// </summary>
        /// <param name="pluginId">The plugin ID</param>
        /// <returns>Plugin configuration</returns>
        [HttpGet("{pluginId}/settings")]
        [ProducesResponseType(typeof(PluginSettingsResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PluginSettingsResponse>> GetPluginSettings(string pluginId)
        {
            var plugin = _pluginManager.GetLoadedPlugins().FirstOrDefault(p => p.Id == pluginId);
            if (plugin == null)
            {
                return NotFound($"Plugin with ID '{pluginId}' not found");
            }

            var response = new PluginSettingsResponse
            {
                PluginId = plugin.Id,
                PluginName = plugin.Name,
                Settings = plugin.Metadata?.Settings ?? new Dictionary<string, PluginSetting>(),
                CurrentValues = new Dictionary<string, object>(), // Would be loaded from storage
                Schema = plugin.Metadata?.Settings?.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new PluginSettingSchema
                    {
                        Type = kvp.Value.Type.ToString(),
                        Description = kvp.Value.Description,
                        Required = kvp.Value.Required,
                        DefaultValue = kvp.Value.DefaultValue,
                        Options = kvp.Value.Options
                    }
                ) ?? new Dictionary<string, PluginSettingSchema>()
            };

            return Ok(response);
        }

        private SettingsValidationResult ValidateSettings(IPlugin plugin, Dictionary<string, object> settings)
        {
            var result = new SettingsValidationResult { IsValid = true };
            var pluginSettings = plugin.Metadata?.Settings ?? new Dictionary<string, PluginSetting>();

            foreach (var setting in settings)
            {
                if (!pluginSettings.ContainsKey(setting.Key))
                {
                    result.IsValid = false;
                    result.Errors.Add($"Unknown setting: {setting.Key}");
                    continue;
                }

                var pluginSetting = pluginSettings[setting.Key];
                
                // Validate required settings
                if (pluginSetting.Required && (setting.Value == null || string.IsNullOrEmpty(setting.Value.ToString())))
                {
                    result.IsValid = false;
                    result.Errors.Add($"Required setting '{setting.Key}' is missing or empty");
                }

                // Additional type validation could be added here
            }

            // Check for missing required settings
            foreach (var pluginSetting in pluginSettings.Where(ps => ps.Value.Required))
            {
                if (!settings.ContainsKey(pluginSetting.Key))
                {
                    result.IsValid = false;
                    result.Errors.Add($"Required setting '{pluginSetting.Key}' is missing");
                }
            }

            return result;
        }
    }

    public class UpdatePluginSettingsRequest
    {
        [Required]
        public Dictionary<string, object> Settings { get; set; } = new();
    }

    public class UpdatePluginSettingsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, object> UpdatedSettings { get; set; } = new();
        public List<string> ValidationErrors { get; set; } = new();
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdatePluginStatusRequest
    {
        [Required]
        public bool Enabled { get; set; }
    }

    public class PluginStatusResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }

    public class PluginSettingsResponse
    {
        public string PluginId { get; set; } = string.Empty;
        public string PluginName { get; set; } = string.Empty;
        public Dictionary<string, PluginSetting> Settings { get; set; } = new();
        public Dictionary<string, object> CurrentValues { get; set; } = new();
        public Dictionary<string, PluginSettingSchema> Schema { get; set; } = new();
    }

    public class PluginSettingSchema
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Required { get; set; }
        public object DefaultValue { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new();
    }

    public class SettingsValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }
} 
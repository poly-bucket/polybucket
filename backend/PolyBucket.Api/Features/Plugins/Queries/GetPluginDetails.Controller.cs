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

namespace PolyBucket.Api.Features.Plugins.Queries
{
    [ApiController]
    [Route("api/plugins")]
    [Authorize]
    [RequirePermission(PermissionConstants.PLUGIN_MANAGE)]
    public class GetPluginDetailsController(PluginManager pluginManager) : ControllerBase
    {
        private readonly PluginManager _pluginManager = pluginManager;

        /// <summary>
        /// Get detailed information about a specific plugin
        /// </summary>
        /// <param name="pluginId">The plugin ID</param>
        /// <returns>Detailed plugin information</returns>
        [HttpGet("{pluginId}/details")]
        [ProducesResponseType(typeof(PluginDetailsResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PluginDetailsResponse>> GetPluginDetails(string pluginId)
        {
            var plugin = _pluginManager.GetLoadedPlugins().FirstOrDefault(p => p.Id == pluginId);
            if (plugin == null)
            {
                return NotFound($"Plugin with ID '{pluginId}' not found");
            }

            var details = new PluginDetailsResponse
            {
                Id = plugin.Id,
                Name = plugin.Name,
                Version = plugin.Version,
                Author = plugin.Author,
                Description = plugin.Description,
                IsLoaded = true,
                Metadata = plugin.Metadata,
                Components = plugin.FrontendComponents?.ToList() ?? new List<PluginComponent>(),
                LoadedAt = DateTime.UtcNow, // This would be tracked by PluginManager
                Status = "Active",
                Dependencies = plugin.Metadata?.Dependencies ?? new List<string>(),
                Hooks = plugin.FrontendComponents?.SelectMany(c => c.Hooks).ToList() ?? new List<PluginHook>(),
                Settings = plugin.Metadata?.Settings?.Values.ToList() ?? new List<PluginSetting>()
            };

            return Ok(details);
        }

        /// <summary>
        /// Get all plugins with their basic information and status
        /// </summary>
        /// <returns>List of all plugins</returns>
        [HttpGet("overview")]
        [ProducesResponseType(typeof(PluginOverviewResponse), 200)]
        public async Task<ActionResult<PluginOverviewResponse>> GetPluginsOverview()
        {
            var loadedPlugins = _pluginManager.GetLoadedPlugins();
            
            var plugins = loadedPlugins.Select(p => new PluginSummary
            {
                Id = p.Id,
                Name = p.Name,
                Version = p.Version,
                Author = p.Author,
                Description = p.Description,
                IsLoaded = true,
                Status = "Active",
                ComponentCount = p.FrontendComponents?.Count() ?? 0,
                HookCount = p.FrontendComponents?.SelectMany(c => c.Hooks).Count() ?? 0,
                SettingsCount = p.Metadata?.Settings?.Count ?? 0,
                HasPermissions = p.Metadata?.RequiredPermissions?.Any() ?? false
            }).ToList();

            var response = new PluginOverviewResponse
            {
                TotalPlugins = plugins.Count,
                ActivePlugins = plugins.Count(p => p.Status == "Active"),
                PluginSummaries = plugins,
                LastRefresh = DateTime.UtcNow
            };

            return Ok(response);
        }

        /// <summary>
        /// Get plugin hooks information for extension point management
        /// </summary>
        /// <returns>Plugin hooks grouped by extension points</returns>
        [HttpGet("hooks")]
        [ProducesResponseType(typeof(PluginHooksResponse), 200)]
        public async Task<ActionResult<PluginHooksResponse>> GetPluginHooks()
        {
            var loadedPlugins = _pluginManager.GetLoadedPlugins();
            var hooksByExtensionPoint = new Dictionary<string, List<PluginHookInfo>>();

            foreach (var plugin in loadedPlugins)
            {
                if (plugin.FrontendComponents != null)
                {
                    foreach (var component in plugin.FrontendComponents)
                    {
                        foreach (var hook in component.Hooks)
                        {
                            var extensionPoint = hook.ComponentId; // Use ComponentId as extension point
                            if (!hooksByExtensionPoint.ContainsKey(extensionPoint))
                            {
                                hooksByExtensionPoint[extensionPoint] = new List<PluginHookInfo>();
                            }

                            hooksByExtensionPoint[extensionPoint].Add(new PluginHookInfo
                            {
                                PluginId = plugin.Id,
                                PluginName = plugin.Name,
                                HookName = hook.HookName,
                                Priority = hook.Priority,
                                Configuration = hook.Config
                            });
                        }
                    }
                }
            }

            // Sort hooks by priority within each extension point
            foreach (var extensionPoint in hooksByExtensionPoint.Keys.ToList())
            {
                hooksByExtensionPoint[extensionPoint] = hooksByExtensionPoint[extensionPoint]
                    .OrderBy(h => h.Priority)
                    .ToList();
            }

            var response = new PluginHooksResponse
            {
                ExtensionPoints = hooksByExtensionPoint,
                TotalExtensionPoints = hooksByExtensionPoint.Count,
                TotalHooks = hooksByExtensionPoint.Values.SelectMany(h => h).Count()
            };

            return Ok(response);
        }
    }

    public class PluginDetailsResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsLoaded { get; set; }
        public PluginMetadata? Metadata { get; set; }
        public List<PluginComponent>? Components { get; set; }
        public DateTime LoadedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> Dependencies { get; set; } = new();
        public List<PluginHook> Hooks { get; set; } = new();
        public List<PluginSetting> Settings { get; set; } = new();
    }

    public class PluginOverviewResponse
    {
        public int TotalPlugins { get; set; }
        public int ActivePlugins { get; set; }
        public List<PluginSummary> PluginSummaries { get; set; } = new();
        public DateTime LastRefresh { get; set; }
    }

    public class PluginSummary
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsLoaded { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ComponentCount { get; set; }
        public int HookCount { get; set; }
        public int SettingsCount { get; set; }
        public bool HasPermissions { get; set; }
    }

    public class PluginHooksResponse
    {
        public Dictionary<string, List<PluginHookInfo>> ExtensionPoints { get; set; } = new();
        public int TotalExtensionPoints { get; set; }
        public int TotalHooks { get; set; }
    }

    public class PluginHookInfo
    {
        public string PluginId { get; set; } = string.Empty;
        public string PluginName { get; set; } = string.Empty;
        public string HookName { get; set; } = string.Empty;
        public int Priority { get; set; }
        public Dictionary<string, object>? Configuration { get; set; }
    }
} 
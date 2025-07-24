using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using PolyBucket.Api.Common.Plugins;
using System.Linq;

namespace PolyBucket.Api.Features.Plugins.Queries
{
    [ApiController]
    [Route("api/plugins")]
    [Authorize]
    public class GetPluginsController(PluginManager pluginManager) : ControllerBase
    {
        private readonly PluginManager _pluginManager = pluginManager;

        [HttpGet]
        public ActionResult<IEnumerable<PluginInfo>> GetPlugins()
        {
            var plugins = _pluginManager.GetLoadedPlugins();
            var pluginInfos = plugins.Select(p => new PluginInfo
            {
                Id = p.Id,
                Name = p.Name,
                Version = p.Version,
                Author = p.Author,
                Description = p.Description
            });
            return Ok(pluginInfos);
        }
    }

    public class PluginInfo
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Version { get; set; }
        public required string Author { get; set; }
        public required string Description { get; set; }
    }

    public class Plugin
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
} 
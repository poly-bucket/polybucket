using Core.Plugins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Plugins
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PluginsController : ControllerBase
    {
        private readonly PluginManager _pluginManager;

        public PluginsController(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;
        }

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

        [HttpPost("reload")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReloadPlugins()
        {
            await _pluginManager.UnloadAllPluginsAsync();
            await _pluginManager.InitializeAsync();
            return Ok();
        }
    }

    public class PluginInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
    }
} 
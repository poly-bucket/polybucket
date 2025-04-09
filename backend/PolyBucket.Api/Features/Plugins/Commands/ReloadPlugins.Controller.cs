using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PolyBucket.Api.Common.Plugins;

namespace PolyBucket.Api.Features.Plugins.Commands
{
    [ApiController]
    [Route("api/plugins")]
    [Authorize(Roles = "Admin")]
    public class ReloadPluginsController : ControllerBase
    {
        private readonly PluginManager _pluginManager;

        public ReloadPluginsController(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;
        }

        [HttpPost("reload")]
        public async Task<IActionResult> ReloadPlugins()
        {
            await _pluginManager.UnloadAllPluginsAsync();
            await _pluginManager.InitializeAsync();
            return Ok();
        }
    }
} 
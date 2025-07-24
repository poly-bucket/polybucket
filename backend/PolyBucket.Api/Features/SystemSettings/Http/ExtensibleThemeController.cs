using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common.Services;
using PolyBucket.Api.Common.Plugins;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.SystemSettings.Http
{
    [ApiController]
    [Route("api/system-settings/extensible-theme")]
    public class ExtensibleThemeController(IThemeManager themeManager) : ControllerBase
    {
        private readonly IThemeManager _themeManager = themeManager;

        /// <summary>
        /// Get all available themes from plugins
        /// </summary>
        /// <returns>List of available themes</returns>
        [HttpGet("themes")]
        [AllowAnonymous]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ThemeDefinition>))]
        public async Task<ActionResult<IEnumerable<ThemeDefinition>>> GetAvailableThemes()
        {
            var themes = await _themeManager.GetAllThemesAsync();
            return Ok(themes);
        }

        /// <summary>
        /// Get the currently active theme
        /// </summary>
        /// <returns>Active theme definition</returns>
        [HttpGet("active")]
        [AllowAnonymous]
        [ProducesResponseType(200, Type = typeof(ThemeDefinition))]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ThemeDefinition>> GetActiveTheme()
        {
            var theme = await _themeManager.GetActiveThemeAsync();
            if (theme == null)
            {
                return NotFound("No active theme found");
            }
            return Ok(theme);
        }

        /// <summary>
        /// Set the active theme
        /// </summary>
        /// <param name="themeId">Theme identifier</param>
        /// <returns>Success result</returns>
        [HttpPost("active/{themeId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult> SetActiveTheme(string themeId)
        {
            var success = await _themeManager.SetActiveThemeAsync(themeId);
            if (!success)
            {
                return BadRequest($"Failed to set theme: {themeId}");
            }
            return Ok(new { message = $"Theme {themeId} activated successfully" });
        }

        /// <summary>
        /// Get theme components (CSS, JS) for the active theme
        /// </summary>
        /// <returns>Theme components</returns>
        [HttpGet("components")]
        [AllowAnonymous]
        [ProducesResponseType(200, Type = typeof(ThemeComponents))]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ThemeComponents>> GetActiveThemeComponents()
        {
            var components = await _themeManager.GetActiveThemeComponentsAsync();
            if (components == null)
            {
                return NotFound("No theme components found");
            }
            return Ok(components);
        }

        /// <summary>
        /// Get theme components for a specific theme
        /// </summary>
        /// <param name="themeId">Theme identifier</param>
        /// <returns>Theme components</returns>
        [HttpGet("components/{themeId}")]
        [AllowAnonymous]
        [ProducesResponseType(200, Type = typeof(ThemeComponents))]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ThemeComponents>> GetThemeComponents(string themeId)
        {
            var components = await _themeManager.GetThemeComponentsAsync(themeId);
            if (components == null)
            {
                return NotFound($"Theme components not found for: {themeId}");
            }
            return Ok(components);
        }

        /// <summary>
        /// Get current theme configuration
        /// </summary>
        /// <returns>Current theme configuration</returns>
        [HttpGet("configuration")]
        [AllowAnonymous]
        [ProducesResponseType(200, Type = typeof(Dictionary<string, object>))]
        public async Task<ActionResult<Dictionary<string, object>>> GetCurrentConfiguration()
        {
            var configuration = await _themeManager.GetCurrentThemeConfigurationAsync();
            return Ok(configuration);
        }

        /// <summary>
        /// Update theme configuration
        /// </summary>
        /// <param name="configuration">New theme configuration</param>
        /// <returns>Update result</returns>
        [HttpPut("configuration")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200, Type = typeof(ThemeApplicationResult))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ThemeApplicationResult>> UpdateConfiguration([FromBody] Dictionary<string, object> configuration)
        {
            if (configuration == null)
            {
                return BadRequest("Configuration is required");
            }

            var result = await _themeManager.UpdateThemeConfigurationAsync(configuration);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Validate theme configuration
        /// </summary>
        /// <param name="themeId">Theme identifier</param>
        /// <param name="configuration">Configuration to validate</param>
        /// <returns>Validation result</returns>
        [HttpPost("validate/{themeId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200, Type = typeof(ThemeValidationResult))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ThemeValidationResult>> ValidateConfiguration(string themeId, [FromBody] Dictionary<string, object> configuration)
        {
            if (configuration == null)
            {
                return BadRequest("Configuration is required");
            }

            var result = await _themeManager.ValidateThemeConfigurationAsync(themeId, configuration);
            return Ok(result);
        }

        /// <summary>
        /// Reset theme to default configuration
        /// </summary>
        /// <returns>Reset result</returns>
        [HttpPost("reset")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200, Type = typeof(ThemeApplicationResult))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<ThemeApplicationResult>> ResetToDefault()
        {
            var result = await _themeManager.ResetThemeToDefaultAsync();
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Get theme plugins
        /// </summary>
        /// <returns>List of theme plugins</returns>
        [HttpGet("plugins")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<IThemePlugin>))]
        public ActionResult<IEnumerable<IThemePlugin>> GetThemePlugins()
        {
            var plugins = _themeManager.GetThemePlugins();
            return Ok(plugins);
        }
    }
} 
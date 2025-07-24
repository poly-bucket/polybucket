using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using PolyBucket.Api.Features.SystemSettings.UpdateThemeSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.GetThemeSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.Http
{
    [ApiController]
    [Route("api/system-settings/theme")]
    public class ThemeSettingsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Get the current theme settings
        /// </summary>
        /// <returns>Current theme configuration</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(200, Type = typeof(GetThemeSettingsResponse))]
        [ProducesResponseType(400)]
        public async Task<ActionResult<GetThemeSettingsResponse>> GetThemeSettings()
        {
            var query = new GetThemeSettingsQuery();
            var result = await _mediator.Send(query);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        /// <summary>
        /// Update the theme settings
        /// </summary>
        /// <param name="command">Theme settings configuration</param>
        /// <returns>Update result</returns>
        [HttpPut]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200, Type = typeof(UpdateThemeSettingsResponse))]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<ActionResult<UpdateThemeSettingsResponse>> UpdateThemeSettings([FromBody] UpdateThemeSettingsCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!command.IsValid(out var validationResults))
            {
                foreach (var validationResult in validationResults)
                {
                    ModelState.AddModelError("", validationResult.ErrorMessage ?? "Validation error");
                }
                return BadRequest(ModelState);
            }

            var result = await _mediator.Send(command);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }
    }
} 
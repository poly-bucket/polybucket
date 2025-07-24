using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using PolyBucket.Api.Features.SystemSettings.CheckFirstTimeSetup.Domain;
using PolyBucket.Api.Features.SystemSettings.UpdateSiteSettings.Domain;
using PolyBucket.Api.Features.SystemSettings.CompleteFirstTimeSetup.Domain;

namespace PolyBucket.Api.Features.SystemSettings.Http
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemSetupController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Check the current status of first-time setup
        /// </summary>
        /// <returns>Setup status information</returns>
        [HttpGet("status")]
        [AllowAnonymous]
        public async Task<ActionResult<CheckFirstTimeSetupResponse>> GetSetupStatus()
        {
            var query = new CheckFirstTimeSetupQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Update site settings during first-time setup
        /// </summary>
        /// <param name="command">Site settings configuration</param>
        /// <returns>Update result</returns>
        [HttpPost("site-settings")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UpdateSiteSettingsResponse>> UpdateSiteSettings([FromBody] UpdateSiteSettingsCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Custom validation for AdminContact
            if (!command.IsValid(out var validationResults))
            {
                foreach (var validationResult in validationResults)
                {
                    foreach (var memberName in validationResult.MemberNames)
                    {
                        ModelState.AddModelError(memberName, validationResult.ErrorMessage ?? "Validation failed");
                    }
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

        /// <summary>
        /// Complete the first-time setup process
        /// </summary>
        /// <returns>Completion result</returns>
        [HttpPost("complete")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CompleteFirstTimeSetupResponse>> CompleteSetup()
        {
            var command = new CompleteFirstTimeSetupCommand();
            var result = await _mediator.Send(command);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
} 
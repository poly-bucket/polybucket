using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using PolyBucket.Api.Features.Authentication.ChangePassword.Domain;

namespace PolyBucket.Api.Features.Authentication.ChangePassword.Http
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChangePasswordController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        /// <summary>
        /// Change user password (required for first-time admin setup)
        /// </summary>
        /// <param name="command">Password change request</param>
        /// <returns>Change result</returns>
        [HttpPost("change")]
        public async Task<ActionResult<ChangePasswordResponse>> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            if (!ModelState.IsValid)
            {
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
        /// Skip password change during first-time setup (Admin only)
        /// </summary>
        /// <returns>Skip result</returns>
        [HttpPost("skip")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ChangePasswordResponse>> SkipPasswordChange()
        {
            var handler = HttpContext.RequestServices.GetRequiredService<ChangePasswordCommandHandler>();
            var result = await handler.SkipPasswordChange(HttpContext.RequestAborted);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
} 
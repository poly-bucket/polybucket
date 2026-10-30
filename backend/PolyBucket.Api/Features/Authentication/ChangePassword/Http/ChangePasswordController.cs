using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using PolyBucket.Api.Common.Http;
using PolyBucket.Api.Features.Authentication.ChangePassword.Domain;

namespace PolyBucket.Api.Features.Authentication.ChangePassword.Http;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChangePasswordController(IMediator mediator) : ControllerBase
{
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

        command.Client = ClientRequestInfoMapper.From(HttpContext);
        var result = await mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

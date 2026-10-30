using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Authentication.SkipPasswordChange.Domain;

namespace PolyBucket.Api.Features.Authentication.SkipPasswordChange.Http;

[ApiController]
[Route("api/ChangePassword")]
[Authorize]
public class SkipPasswordChangeController(ISkipPasswordChangeService skipPasswordChangeService) : ControllerBase
{
    /// <summary>
    /// Skip password change during first-time setup (Admin only)
    /// </summary>
    /// <returns>Skip result</returns>
    [HttpPost("skip")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SkipPasswordChangeResponse>> SkipPasswordChange(CancellationToken cancellationToken)
    {
        var result = await skipPasswordChangeService.SkipPasswordChangeAsync(cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

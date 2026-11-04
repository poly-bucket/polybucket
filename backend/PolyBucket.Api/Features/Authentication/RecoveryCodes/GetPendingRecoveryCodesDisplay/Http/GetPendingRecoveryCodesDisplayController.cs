using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Authentication.RecoveryCodes.GetPendingRecoveryCodesDisplay.Domain;

namespace PolyBucket.Api.Features.Authentication.RecoveryCodes.GetPendingRecoveryCodesDisplay.Http;

[ApiController]
[Authorize]
[Route("api/auth/recovery-codes")]
public class GetPendingRecoveryCodesDisplayController(GetPendingRecoveryCodesDisplayQueryHandler handler) : ControllerBase
{
    /// <summary>
    /// Returns account recovery codes once for the bootstrap admin during first-time setup.
    /// </summary>
    /// <response code="200">Pending recovery codes for display.</response>
    /// <response code="404">No pending recovery codes are available for this user.</response>
    [HttpGet("pending-display")]
    [ProducesResponseType(typeof(GetPendingRecoveryCodesDisplayResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetPendingDisplay(CancellationToken cancellationToken)
    {
        var response = await handler.Handle(cancellationToken);
        if (response == null)
        {
            return NotFound();
        }

        return Ok(response);
    }
}

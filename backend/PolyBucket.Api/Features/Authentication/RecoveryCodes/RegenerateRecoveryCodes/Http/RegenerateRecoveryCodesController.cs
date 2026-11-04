using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Authentication.RecoveryCodes.RegenerateRecoveryCodes.Domain;

namespace PolyBucket.Api.Features.Authentication.RecoveryCodes.RegenerateRecoveryCodes.Http;

[ApiController]
[Authorize]
[Route("api/auth/recovery-codes")]
public class RegenerateRecoveryCodesController(RegenerateRecoveryCodesCommandHandler handler) : ControllerBase
{
    /// <summary>
    /// Generates a new set of account recovery codes and invalidates all previous codes.
    /// </summary>
    /// <response code="200">New recovery codes (shown once).</response>
    /// <response code="401">The caller is not authenticated.</response>
    [HttpPost("regenerate")]
    [ProducesResponseType(typeof(RegenerateRecoveryCodesResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Regenerate(CancellationToken cancellationToken)
    {
        var response = await handler.Handle(cancellationToken);
        return Ok(response);
    }
}

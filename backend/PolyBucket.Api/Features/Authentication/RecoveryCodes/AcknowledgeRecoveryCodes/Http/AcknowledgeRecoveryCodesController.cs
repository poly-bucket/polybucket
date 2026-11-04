using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Authentication.RecoveryCodes.AcknowledgeRecoveryCodes.Domain;

namespace PolyBucket.Api.Features.Authentication.RecoveryCodes.AcknowledgeRecoveryCodes.Http;

[ApiController]
[Authorize]
[Route("api/auth/recovery-codes")]
public class AcknowledgeRecoveryCodesController(AcknowledgeRecoveryCodesCommandHandler handler) : ControllerBase
{
    /// <summary>
    /// Confirms the user has saved their account recovery codes and clears the one-time display payload.
    /// </summary>
    /// <response code="204">Recovery codes were acknowledged.</response>
    [HttpPost("acknowledge")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Acknowledge(CancellationToken cancellationToken)
    {
        await handler.Handle(cancellationToken);
        return NoContent();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Authentication.GetRegistrationStatus.Domain;

namespace PolyBucket.Api.Features.Authentication.GetRegistrationStatus.Http;

[ApiController]
[Route("api/auth")]
public class GetRegistrationStatusController(IGetRegistrationStatusService getRegistrationStatusService) : ControllerBase
{
    /// <summary>
    /// Returns whether public user registration is enabled on this instance.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Registration availability flag</returns>
    [HttpGet("registration-status")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GetRegistrationStatusResponse), 200)]
    public async Task<IActionResult> GetRegistrationStatus(CancellationToken cancellationToken)
    {
        var result = await getRegistrationStatusService.GetRegistrationStatusAsync(cancellationToken);
        return Ok(new { allowUserRegistration = result.AllowUserRegistration });
    }
}

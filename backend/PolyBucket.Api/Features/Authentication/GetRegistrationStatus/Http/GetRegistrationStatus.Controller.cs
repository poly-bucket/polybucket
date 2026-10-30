using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Authentication.Services;
using System.Threading;
using System.Threading.Tasks;

namespace PolyBucket.Api.Features.Authentication.GetRegistrationStatus.Http;

[ApiController]
[Route("api/auth")]
public class GetRegistrationStatusController(IUserRegistrationGate registrationGate) : ControllerBase
{
    /// <summary>
    /// Returns whether public user registration is enabled on this instance.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Registration availability flag</returns>
    [HttpGet("registration-status")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetRegistrationStatus(CancellationToken cancellationToken)
    {
        var allowed = await registrationGate.IsRegistrationAllowedAsync(cancellationToken);
        return Ok(new { allowUserRegistration = allowed });
    }
}

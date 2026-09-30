using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PolyBucket.Api.Extensions;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Email.TestEmailConfiguration.Domain;

namespace PolyBucket.Api.Features.Email.TestEmailConfiguration.Http;

[Authorize]
[ApiController]
[Route("api/system-settings/email")]
[RequirePermission(PermissionConstants.ADMIN_SYSTEM_SETTINGS)]
public class TestEmailConfigurationController(ITestEmailConfigurationService service) : ControllerBase
{
    private readonly ITestEmailConfigurationService _service = service;

    /// <summary>
    /// Sends a test email with the saved settings and reports each delivery stage (configuration, DNS, connect, TLS, authentication, send).
    /// </summary>
    /// <remarks>A successful test is recorded and is required before email verification can be enforced.</remarks>
    /// <param name="request">The address that should receive the test email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The staged test result; check <c>success</c> to see whether delivery worked.</response>
    /// <response code="400">The test address is invalid.</response>
    [HttpPost("test")]
    [EnableRateLimiting(RequestSecurityServiceCollectionExtensions.AuthStrictPolicy)]
    [ProducesResponseType(typeof(EmailTestResult), 200)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(429)]
    public async Task<ActionResult<EmailTestResult>> TestEmailConfiguration(
        [FromBody] TestEmailConfigurationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ValidationProblemDetails(ModelState));
        }

        return Ok(await _service.TestAsync(request.TestEmailAddress, cancellationToken));
    }
}

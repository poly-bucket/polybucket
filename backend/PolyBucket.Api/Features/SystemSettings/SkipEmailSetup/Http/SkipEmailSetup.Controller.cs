using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.SystemSettings.SkipEmailSetup.Domain;

namespace PolyBucket.Api.Features.SystemSettings.SkipEmailSetup.Http;

[ApiController]
[Route("api/SystemSetup")]
public class SkipEmailSetupController(ISkipEmailSetupService skipEmailSetupService) : ControllerBase
{
    private static readonly string[] AdminRoleClaimTypes =
    [
        ClaimTypes.Role,
        "role",
        "roles",
        "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    ];

    private bool IsAdminRequest() =>
        User.Claims.Any(claim =>
            AdminRoleClaimTypes.Contains(claim.Type, StringComparer.OrdinalIgnoreCase)
            && string.Equals(claim.Value, "Admin", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Marks the optional email setup step as skipped during first-time setup.
    /// </summary>
    [HttpPost("skip-email")]
    [Authorize]
    [ProducesResponseType(typeof(SkipEmailSetupResponse), 200)]
    [ProducesResponseType(typeof(SkipEmailSetupResponse), 400)]
    public async Task<ActionResult<SkipEmailSetupResponse>> SkipEmailSetup(CancellationToken cancellationToken)
    {
        if (!IsAdminRequest())
        {
            return Forbid();
        }

        var result = await skipEmailSetupService.SkipEmailSetupAsync(new SkipEmailSetupCommand(), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

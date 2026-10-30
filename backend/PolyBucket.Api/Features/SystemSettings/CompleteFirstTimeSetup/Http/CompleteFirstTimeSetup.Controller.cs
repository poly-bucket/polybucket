using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.SystemSettings.CompleteFirstTimeSetup.Domain;

namespace PolyBucket.Api.Features.SystemSettings.CompleteFirstTimeSetup.Http;

[ApiController]
[Route("api/SystemSetup")]
public class CompleteFirstTimeSetupController(ICompleteFirstTimeSetupService completeFirstTimeSetupService) : ControllerBase
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
    /// Complete the first-time setup process
    /// </summary>
    /// <returns>Completion result</returns>
    [HttpPost("complete")]
    [Authorize]
    public async Task<ActionResult<CompleteFirstTimeSetupResponse>> CompleteSetup(CancellationToken cancellationToken)
    {
        if (!IsAdminRequest())
        {
            return Forbid();
        }

        var result = await completeFirstTimeSetupService.CompleteFirstTimeSetupAsync(new CompleteFirstTimeSetupCommand(), cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.SystemSettings.ApplyStorageSetup.Domain;

namespace PolyBucket.Api.Features.SystemSettings.ApplyStorageSetup.Http;

[ApiController]
[Route("api/SystemSetup")]
public class ApplyStorageSetupController(IApplyStorageSetupService applyStorageSetupService) : ControllerBase
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
    /// Restarts the API so object storage settings loaded from the database take effect.
    /// </summary>
    [HttpPost("apply-storage")]
    [Authorize]
    [ProducesResponseType(typeof(ApplyStorageSetupResponse), 202)]
    [ProducesResponseType(typeof(ApplyStorageSetupResponse), 400)]
    public async Task<ActionResult<ApplyStorageSetupResponse>> ApplyStorageSetup(CancellationToken cancellationToken)
    {
        if (!IsAdminRequest())
        {
            return Forbid();
        }

        var result = await applyStorageSetupService.ApplyStorageSetupAsync(new ApplyStorageSetupCommand(), cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Accepted(result);
    }
}

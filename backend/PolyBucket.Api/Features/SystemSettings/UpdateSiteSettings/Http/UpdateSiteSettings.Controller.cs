using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.SystemSettings.UpdateSiteSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.UpdateSiteSettings.Http;

[ApiController]
[Route("api/SystemSetup")]
public class UpdateSiteSettingsController(IUpdateSiteSettingsService updateSiteSettingsService) : ControllerBase
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
    /// Update site settings during first-time setup
    /// </summary>
    /// <param name="command">Site settings configuration</param>
    /// <returns>Update result</returns>
    [HttpPost("site-settings")]
    [Authorize]
    public async Task<ActionResult<UpdateSiteSettingsResponse>> UpdateSiteSettings([FromBody] UpdateSiteSettingsCommand command, CancellationToken cancellationToken)
    {
        if (!IsAdminRequest())
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!command.IsValid(out var validationResults))
        {
            foreach (var validationResult in validationResults)
            {
                foreach (var memberName in validationResult.MemberNames)
                {
                    ModelState.AddModelError(memberName, validationResult.ErrorMessage ?? "Validation failed");
                }
            }
            return BadRequest(ModelState);
        }

        var result = await updateSiteSettingsService.UpdateSiteSettingsAsync(command, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

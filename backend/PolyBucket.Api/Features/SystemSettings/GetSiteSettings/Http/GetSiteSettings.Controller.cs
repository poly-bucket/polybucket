using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.SystemSettings.GetSiteSettings.Domain;

namespace PolyBucket.Api.Features.SystemSettings.GetSiteSettings.Http;

[ApiController]
[Route("api/SystemSetup")]
public class GetSiteSettingsController(IGetSiteSettingsService getSiteSettingsService) : ControllerBase
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
    /// Get current site settings for administration
    /// </summary>
    /// <returns>Site settings configuration</returns>
    [HttpGet("site-settings")]
    [Authorize]
    [ProducesResponseType(200, Type = typeof(GetSiteSettingsResponse))]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<GetSiteSettingsResponse>> GetSiteSettings(CancellationToken cancellationToken)
    {
        if (!IsAdminRequest())
        {
            return Forbid();
        }

        var result = await getSiteSettingsService.GetSiteSettingsAsync(new GetSiteSettingsQuery(), cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

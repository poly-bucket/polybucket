using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Authentication.GetSiteAccess.Domain;

namespace PolyBucket.Api.Features.Authentication.GetSiteAccess.Http;

[ApiController]
[Route("api/auth")]
public class GetSiteAccessController(IGetSiteAccessService getSiteAccessService) : ControllerBase
{
    /// <summary>
    /// Returns public site access flags for browsing and model uploads.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Public browsing and upload policy flags</returns>
    [HttpGet("site-access")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GetSiteAccessResponse), 200)]
    public async Task<IActionResult> GetSiteAccess(CancellationToken cancellationToken)
    {
        var result = await getSiteAccessService.GetSiteAccessAsync(cancellationToken);
        return Ok(new
        {
            allowPublicBrowsing = result.AllowPublicBrowsing,
            requireLoginForUpload = result.RequireLoginForUpload,
            allowAnonymousModelUpload = result.AllowAnonymousModelUpload,
            defaultModelPrivacy = result.DefaultModelPrivacy
        });
    }
}
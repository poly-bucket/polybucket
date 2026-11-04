using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.Authentication.GetSiteAccess.Domain;

namespace PolyBucket.Api.Features.Authentication.GetSiteAccess.Http;

[ApiController]
[Route("api/auth")]
public class GetSiteAccessController(IGetSiteAccessService getSiteAccessService) : ControllerBase
{
    /// <summary>
    /// Returns whether anonymous users may browse the site without signing in.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Public browsing availability flag</returns>
    [HttpGet("site-access")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GetSiteAccessResponse), 200)]
    public async Task<IActionResult> GetSiteAccess(CancellationToken cancellationToken)
    {
        var result = await getSiteAccessService.GetSiteAccessAsync(cancellationToken);
        return Ok(new { allowPublicBrowsing = result.AllowPublicBrowsing });
    }
}

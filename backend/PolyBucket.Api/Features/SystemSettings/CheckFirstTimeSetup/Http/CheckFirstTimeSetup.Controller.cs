using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Features.SystemSettings.CheckFirstTimeSetup.Domain;

namespace PolyBucket.Api.Features.SystemSettings.CheckFirstTimeSetup.Http;

[ApiController]
[Route("api/SystemSetup")]
public class CheckFirstTimeSetupController(ICheckFirstTimeSetupService checkFirstTimeSetupService) : ControllerBase
{
    /// <summary>
    /// Check the current status of first-time setup
    /// </summary>
    /// <returns>Setup status information</returns>
    [HttpGet("status")]
    [AllowAnonymous]
    public async Task<ActionResult<CheckFirstTimeSetupResponse>> GetSetupStatus(CancellationToken cancellationToken)
    {
        var result = await checkFirstTimeSetupService.CheckFirstTimeSetupAsync(new CheckFirstTimeSetupQuery(), cancellationToken);
        return Ok(result);
    }
}

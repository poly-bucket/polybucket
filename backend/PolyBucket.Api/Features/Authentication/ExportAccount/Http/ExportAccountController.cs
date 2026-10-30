using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Features.Authentication.ExportAccount.Domain;

namespace PolyBucket.Api.Features.Authentication.ExportAccount.Http;

[ApiController]
[Route("api/auth/account")]
[Authorize]
public class ExportAccountController(IExportAccountService exportAccountService) : ControllerBase
{
    /// <summary>
    /// Export a snapshot of profile and settings data for the signed-in user (JSON).
    /// </summary>
    [HttpGet("export")]
    [ProducesResponseType(typeof(AccountExportResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<AccountExportResponse>> Export(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindUserIdClaim();
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Invalid authentication token" });
        }

        var response = await exportAccountService.ExportAsync(userId, cancellationToken);
        if (response == null)
        {
            return NotFound(new { message = "User not found" });
        }

        return Ok(response);
    }
}

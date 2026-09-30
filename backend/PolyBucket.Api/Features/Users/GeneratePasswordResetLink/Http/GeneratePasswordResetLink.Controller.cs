using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolyBucket.Api.Common;
using PolyBucket.Api.Common.Http;
using PolyBucket.Api.Features.ACL.Authorization;
using PolyBucket.Api.Features.ACL.Domain;
using PolyBucket.Api.Features.Users.GeneratePasswordResetLink.Domain;

namespace PolyBucket.Api.Features.Users.GeneratePasswordResetLink.Http;

[ApiController]
[Route("api/admin/users")]
[Authorize]
[RequirePermission(PermissionConstants.ADMIN_MANAGE_USERS)]
public class GeneratePasswordResetLinkController(IGeneratePasswordResetLinkService generatePasswordResetLinkService) : ControllerBase
{
    /// <summary>
    /// Creates a one-time password reset link for a user so an administrator can hand it over directly.
    /// </summary>
    /// <remarks>
    /// Intended for servers without working email. The link is valid for 24 hours, can be used once,
    /// and is recorded in the user audit log. It is returned only in this response and never stored in plain text.
    /// </remarks>
    /// <param name="userId">The user who needs to reset their password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The reset link. <c>url</c> is null when no public base URL is configured; use <c>path</c> instead.</response>
    /// <response code="401">The caller is not signed in.</response>
    /// <response code="403">The caller cannot manage users.</response>
    /// <response code="404">The user does not exist.</response>
    [HttpPost("{userId}/password-reset-link")]
    [ProducesResponseType(typeof(GeneratePasswordResetLinkResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GeneratePasswordResetLink([FromRoute] Guid userId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var currentUserId))
        {
            return Unauthorized("Invalid user token");
        }

        try
        {
            var result = await generatePasswordResetLinkService.GenerateAsync(userId, currentUserId, ClientRequestInfo.From(HttpContext), cancellationToken);
            Response.Headers.CacheControl = "no-store";
            return Ok(new GeneratePasswordResetLinkResponse
            {
                Path = result.Path,
                Url = result.Url,
                ExpiresAt = result.ExpiresAt
            });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

public class GeneratePasswordResetLinkResponse
{
    public string Path { get; set; } = string.Empty;
    public string? Url { get; set; }
    public DateTime ExpiresAt { get; set; }
}
